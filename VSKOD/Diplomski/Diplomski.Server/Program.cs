using Oracle.ManagedDataAccess.Client;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connStr = app.Configuration["Oracle:ConnectionString"]
                 ?? Environment.GetEnvironmentVariable("ORACLE_CONN")
                 ?? "";
const string IZVOR_UREDJAJA = "UREDJAJ";

// IZMENA 1: Postavljamo podrazumevane vrednosti (npr. 1 i 1)
// kako bi slanje radilo odmah, čak i ako se ne pozove POST /api/sesija pre toga.
int? aktivniPacijent = 1, aktivniRadnik = 1;

int poslednjaVrednost = 0;
int poslednjiSpo2 = 0;
int poslednjiPuls = 0;
DateTime poslednjeVreme = DateTime.Now;

app.Use(async (ctx, next) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {ctx.Connection.RemoteIpAddress}  {ctx.Request.Method} {ctx.Request.Path}{ctx.Request.QueryString}");
    await next();
});

app.MapGet("/", () => "Server radi!");

var statLock = new object();
var primljeniN = new HashSet<int>();
int duplikata = 0, najveciN = 0;

// POST /api/sesija
app.MapPost("/api/sesija", (Sesija s) =>
{
    if (s.id_pacijenta <= 0 || s.id_radnika <= 0)
        return Results.BadRequest("id_pacijenta i id_radnika moraju biti > 0");

    lock (statLock)
    {
        aktivniPacijent = s.id_pacijenta;
        aktivniRadnik = s.id_radnika;
        primljeniN.Clear(); duplikata = 0; najveciN = 0;
    }
    Console.WriteLine($"=== SESIJA: pacijent={s.id_pacijenta}  radnik={s.id_radnika} ===");
    return Results.Ok(new { ok = true });
});

// DELETE /api/sesija
app.MapDelete("/api/sesija", () =>
{
    lock (statLock) { aktivniPacijent = null; aktivniRadnik = null; }
    Console.WriteLine("=== SESIJA ZATVORENA ===");
    return Results.Ok(new { ok = true });
});

app.MapGet("/api/sesija", () =>
{
    lock (statLock) { return Results.Ok(new { id_pacijenta = aktivniPacijent, id_radnika = aktivniRadnik }); }
});

// ---- Nucleo salje: POST /api/merenja {"n":12,"spo2":97,"hr":72} ----
app.MapPost("/api/merenja", (Merenje m) =>
{
    if (m.spo2 < 0 || m.spo2 > 100 || m.hr < 0 || m.hr > 300)
        return Results.BadRequest("Vrednost van opsega");

    // IZMENA 2: Ažuriramo vrednosti za web sajt /prikaz ČIM podatak stigne!
    // Tako će sajt uvek odmah prikazati vrednosti bez obzira na sesiju ili bazu.
    poslednjiSpo2 = m.spo2;
    poslednjiPuls = m.hr;
    poslednjeVreme = DateTime.Now;

    lock (statLock)
    {
        if (aktivniPacijent is null || aktivniRadnik is null)
        {
            Console.WriteLine($"ODBIJENO n={m.n}: nema aktivne sesije (prvo podesi pacijenta i radnika u aplikaciji)");
            return Results.Json(new { ok = false, greska = "nema aktivne sesije" }, statusCode: 409);
        }

        if (m.n == 1 && najveciN > 1)
        {
            Console.WriteLine("--- nova serija (n=1), brojaci resetovani ---");
            primljeniN.Clear(); duplikata = 0; najveciN = 0;
        }

        if (primljeniN.Contains(m.n))
        {
            duplikata++;
            Console.WriteLine($"POST n={m.n} (DUPLIKAT, vec upisano)  duplikata={duplikata}");
            return Results.Text("{\"ok\":true}", "application/json");
        }

        try
        {
            using var con = new OracleConnection(connStr);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.BindByName = true;
            cmd.CommandText =
                "INSERT INTO MERENJA (ID_PACIJENTA, ID_RADNIKA, VREME_MERENJA, SPO2, PULS, PWM_CRVENA, PWM_IR, IZVOR) " +
                "VALUES (:pac, :rad, SYSTIMESTAMP, :spo2, :puls, NULL, NULL, :izvor)";
            cmd.Parameters.Add("pac", OracleDbType.Int32).Value = aktivniPacijent.Value;
            cmd.Parameters.Add("rad", OracleDbType.Int32).Value = aktivniRadnik.Value;
            cmd.Parameters.Add("spo2", OracleDbType.Int32).Value = m.spo2;
            cmd.Parameters.Add("puls", OracleDbType.Int32).Value = m.hr;
            cmd.Parameters.Add("izvor", OracleDbType.Varchar2).Value = IZVOR_UREDJAJA;
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GRESKA BAZE n={m.n}: {ex.Message}");
            return Results.Json(new { ok = false, greska = "baza" }, statusCode: 500);
        }

        primljeniN.Add(m.n);
        if (m.n > najveciN) najveciN = m.n;

        Console.WriteLine($"UPISANO n={m.n}  pacijent={aktivniPacijent} radnik={aktivniRadnik}  SpO2={m.spo2}%  Puls={m.hr} bpm" +
                          $"  | upisano={primljeniN.Count}  izgubljeno={najveciN - primljeniN.Count}  duplikata={duplikata}");
    }
    return Results.Text("{\"ok\":true}", "application/json");
});

// statistika prijema
app.MapGet("/api/merenja/statistika", () =>
{
    lock (statLock)
    {
        return Results.Ok(new
        {
            primljeno = primljeniN.Count,
            izgubljeno = najveciN - primljeniN.Count,
            duplikata,
            najveciN
        });
    }
});

app.MapGet("/api/merenja/test", (string? val) =>
{
    if (int.TryParse(val, out int broj))
    {
        poslednjaVrednost = broj;
        poslednjeVreme = DateTime.Now;
    }
    Console.WriteLine($"Stigla vrednost: {val} u {DateTime.Now:HH:mm:ss.fff}");
    return Results.Ok($"Primljeno: {val}");
});

app.MapGet("/api/merenja/unos", (int? spo2, int? hr) =>
{
    if (spo2 is null || hr is null)
        return Results.BadRequest("Nedostaje spo2 ili hr");

    poslednjiSpo2 = spo2.Value;
    poslednjiPuls = hr.Value;
    poslednjeVreme = DateTime.Now;
    Console.WriteLine($"Merenje: SpO2={spo2}%  Puls={hr} bpm  u {DateTime.Now:HH:mm:ss.fff}");
    return Results.Ok($"Primljeno: SpO2={spo2}, Puls={hr}");
});

app.MapGet("/api/merenja/poslednja", () =>
{
    return Results.Ok(new
    {
        vrednost = poslednjaVrednost,
        spo2 = poslednjiSpo2,
        puls = poslednjiPuls,
        vreme = poslednjeVreme.ToString("HH:mm:ss.fff")
    });
});

app.MapGet("/prikaz", () => Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Pulsni oksimetar</title>
    <style>
        body { font-family: Arial; text-align: center; background: #1a1a2e; color: white; padding-top: 80px; }
        .red { display: flex; justify-content: center; gap: 100px; }
        h1 { font-size: 110px; margin: 0; color: #4ecca3; }
        h2 { font-size: 22px; margin: 0; color: #aaa; font-weight: normal; }
        p { font-size: 20px; color: #aaa; }
    </style>
</head>
<body>
    <div class='red'>
        <div><h1 id='spo2'>--</h1><h2>SpO2 (%)</h2></div>
        <div><h1 id='puls'>--</h1><h2>Puls (bpm)</h2></div>
    </div>
    <p id='vreme'>Cekam podatke...</p>
    <script>
        setInterval(async () => {
            const res = await fetch('/api/merenja/poslednja');
            const data = await res.json();
            document.getElementById('spo2').innerText = data.spo2;
            document.getElementById('puls').innerText = data.puls;
            document.getElementById('vreme').innerText = 'Poslednje primljeno: ' + data.vreme;
        }, 500);
    </script>
</body>
</html>
", "text/html"));

static string ProveriBazu(string cs)
{
    if (string.IsNullOrWhiteSpace(cs))
        throw new Exception("konekcioni string nije podesen (Oracle:ConnectionString u appsettings.json ili promenljiva ORACLE_CONN)");
    using var testCon = new OracleConnection(cs);
    testCon.Open();
    using var testCmd = testCon.CreateCommand();
    testCmd.CommandText = "SELECT USER || ' @ ' || SYS_CONTEXT('USERENV','SERVICE_NAME') FROM DUAL";
    return testCmd.ExecuteScalar()?.ToString() ?? "?";
}

try { Console.WriteLine($"BAZA: POVEZANO  ({ProveriBazu(connStr)})"); }
catch (Exception ex) { Console.WriteLine($"BAZA: NIJE POVEZANO - {ex.Message}"); }

app.MapGet("/api/baza", () =>
{
    try { return Results.Ok(new { povezano = true, korisnik_servis = ProveriBazu(connStr) }); }
    catch (Exception ex) { return Results.Json(new { povezano = false, greska = ex.Message }, statusCode: 503); }
});

app.Run("http://0.0.0.0:5000");

record Merenje(int n, int spo2, int hr);
record Sesija(int id_pacijenta, int id_radnika);
