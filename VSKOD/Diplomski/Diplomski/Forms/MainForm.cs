using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace Diplomski.Forms
{
    public partial class MainForm : Form
    {
        private Button trenutnoDugme;
        public event EventHandler OdjaviSeKliknuto;
        private Panel panelPodaciPacijent;
        private Panel panelInformacije;
        private Panel panelPacijentiSekcija;
        private Panel panelPodesavanjaSekcija;
        private string trenutnaUlogaKorisnika = ""; // Čuvamo ulogu radi provere pristupa

        // ---- živa merenja (Dashboard) ----
        private int idRadnika;                       // postavlja se posle prijave (PostaviKorisnika) ili se nalazi u bazi
        private string korisnikIme, korisnikPrezime, korisnikUloga;   // za pronalazak ID-ja radnika u RADNICI
        private int aktivniPacijentId;               // 0 = pacijent nije izabran
        private long poslednjiMerenjeId;             // najveći ID iz MERENJA koji je već prikazan
        private DateTime poslednjePrimljeno = DateTime.MinValue;   // lokalno vreme prijema poslednjeg merenja
        private bool citamMerenja;                   // sprečava preklapanje upita
        private bool serverGreska, bazaGreska;
        private string statusServera = "", statusBaze = "";
        private Panel panelZivaMerenja, panelIstorijaMerenja;
        private Label lblSpo2Vrednost, lblPulsVrednost, lblStatusMerenja;
        private DataGridView dgvIstorijaMerenja;
        private readonly System.Windows.Forms.Timer timerMerenja = new System.Windows.Forms.Timer { Interval = 1000 };

        private static readonly Color BojaSpo2 = ColorTranslator.FromHtml("#4ecca3");
        private static readonly Color BojaPuls = ColorTranslator.FromHtml("#4ea8ec");
        private static readonly Color BojaStaro = ColorTranslator.FromHtml("#5A6577");
        private const int MaxRedovaIstorije = 500;

        private class MerenjeRed
        {
            public long Id;
            public DateTime Vreme;
            public int Spo2;
            public int Puls;
        }

        public MainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            lblVreme.Text = DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy.");

            foreach (Control ctrl in panel1.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.MouseEnter += NavButton_MouseEnter;
                    btn.MouseLeave += NavButton_MouseLeave;
                    btn.Click += NavButton_Click;
                }
            }
            btnOdjaviSe.Click += btnOdjaviSe_Click;

            timerMerenja.Tick += TimerMerenja_Tick;
            panelAktivniPacijent.VisibleChanged += (s, e) => OsveziVidljivostMerenja();
            this.FormClosing += (s, e) => ZatvoriSesiju();
        }

        public void PostaviKorisnika(string ime, string prezime, string uloga, int idRadnika = 0)
        {
            this.idRadnika = idRadnika;
            korisnikIme = ime;
            korisnikPrezime = prezime;
            korisnikUloga = uloga;
            lblImeKorisnika.Text = ime;
            lblPrezimeKorisnika.Text = prezime;
            lblTitulaKorisnika.Text = PrikaziNazivUloge(uloga);
            trenutnaUlogaKorisnika = uloga; // Pamtimo ulogu (npr. "ADMIN")
        }

        private string PrikaziNazivUloge(string ulogaIzBaze)
        {
            switch (ulogaIzBaze)
            {
                case "DOKTOR": return "Doktor";
                case "ADMIN": return "Administrator";
                case "MEDICINSKA SESTRA": return "Medicinska sestra";
                default: return ulogaIzBaze;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void NavButton_MouseEnter(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn != trenutnoDugme)
            {
                btn.BackColor = Color.FromArgb(35, 45, 60);
                btn.ForeColor = Color.FromArgb(0, 150, 255);
            }
        }

        private void NavButton_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn != trenutnoDugme)
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.White;
            }
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            // Provera za Podešavanja - samo Administrator može da pristupi
            if (btn == btnPodesavanje)
            {
                if (trenutnaUlogaKorisnika != "ADMIN")
                {
                    MessageBox.Show("Nemate privilegije administratora za pristup podešavanjima sistema.", "Ograničen pristup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Prekida izvršavanje, ne menja panel
                }
            }

            AktivirajDugme(sender);

            // Sakrij sve panele pre otvaranja novog
            if (panelPacijentiSekcija != null) panelPacijentiSekcija.Visible = false;
            if (panelPodesavanjaSekcija != null) panelPodesavanjaSekcija.Visible = false;

            if (btn == btnDashboard)
            {
                panelAktivniPacijent.Visible = true;
            }
            else if (btn == btnPacijenti)
            {
                panelAktivniPacijent.Visible = false;
                PrikaziPanelPacijenti();
            }
            else if (btn == btnPodesavanje)
            {
                panelAktivniPacijent.Visible = false;
                PrikaziPanelPodesavanja();
            }
            else
            {
                panelAktivniPacijent.Visible = false;
            }
        }

        private void AktivirajDugme(object btnSender)
        {
            if (btnSender != null)
            {
                if (trenutnoDugme != (Button)btnSender)
                {
                    DeaktivirajDugme();
                    trenutnoDugme = (Button)btnSender;
                    trenutnoDugme.BackColor = Color.FromArgb(35, 45, 60);
                    trenutnoDugme.ForeColor = Color.FromArgb(0, 150, 255);
                }
            }
        }

        private void DeaktivirajDugme()
        {
            if (trenutnoDugme != null)
            {
                trenutnoDugme.BackColor = Color.Transparent;
                trenutnoDugme.ForeColor = Color.White;
            }
        }

        private void btnOdjaviSe_Click(object sender, EventArgs e)
        {
            ZatvoriSesiju();
            OdjaviSeKliknuto?.Invoke(this, EventArgs.Empty);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblVreme.Text = DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy.");
        }

        private void btnPretraziPacijenta_Click(object sender, EventArgs e)
        {
            using (var searchForm = new PacijentSearchForm())
            {
                if (searchForm.ShowDialog() == DialogResult.OK)
                {
                    PrikaziPodatkePacijenta(searchForm);
                }
            }
        }

        private void PrikaziPodatkePacijenta(PacijentSearchForm searchForm)
        {
            if (panelPodaciPacijent != null) panelAktivniPacijent.Controls.Remove(panelPodaciPacijent);
            if (panelInformacije != null) panelAktivniPacijent.Controls.Remove(panelInformacije);

            var lblNaslov = new Label
            {
                Text = "AKTIVNI PACIJENT",
                ForeColor = ColorTranslator.FromHtml("#8B96A5"),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(15, 10),
                AutoSize = true,
                Name = "lblNaslovAktivniPacijent"
            };

            var lblInfoNaslov = new Label
            {
                Text = "INFORMACIJE",
                ForeColor = ColorTranslator.FromHtml("#8B96A5"),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(320, 10),
                AutoSize = true,
                Name = "lblInfoNaslov"
            };

            panelPodaciPacijent = new Panel
            {
                Location = new Point(15, 40),
                Size = new Size(280, 130),
                BackColor = panelAktivniPacijent.BackColor
            };

            AddLabelUPanel(panelPodaciPacijent, $"JMBG: {searchForm.OdabraniJmbg}", 0, 0);
            AddLabelUPanel(panelPodaciPacijent, $"Ime: {searchForm.OdabranoIme}", 0, 25);
            AddLabelUPanel(panelPodaciPacijent, $"Prezime: {searchForm.OdabranoPrezime}", 0, 50);
            AddLabelUPanel(panelPodaciPacijent, $"ID: {searchForm.OdabraniPacijentId}", 0, 75);

            panelInformacije = new Panel
            {
                Location = new Point(320, 40),
                Size = new Size(320, 130),
                BackColor = panelAktivniPacijent.BackColor
            };

            AddLabelUPanel(panelInformacije, $"Pol: {searchForm.OdabraniPol}", 0, 0);
            AddLabelUPanel(panelInformacije, $"Datum rođenja: {searchForm.OdabraniDatumRodjenja?.ToString("dd.MM.yyyy.") ?? "-"}", 0, 25);
            AddLabelUPanel(panelInformacije, $"Telefon: {searchForm.OdabraniTelefon}", 0, 50);
            AddLabelUPanel(panelInformacije, $"Adresa: {searchForm.OdabranaAdresa}", 0, 75);

            if (!panelAktivniPacijent.Controls.ContainsKey("lblNaslovAktivniPacijent"))
                panelAktivniPacijent.Controls.Add(lblNaslov);

            if (!panelAktivniPacijent.Controls.ContainsKey("lblInfoNaslov"))
                panelAktivniPacijent.Controls.Add(lblInfoNaslov);

            panelAktivniPacijent.Controls.Add(panelPodaciPacijent);
            panelAktivniPacijent.Controls.Add(panelInformacije);

            btnPretraziPacijenta.Text = "Promeni pacijenta";
            btnPretraziPacijenta.Location = new Point(panelAktivniPacijent.Width - btnPretraziPacijenta.Width - 20, 15);
            btnPretraziPacijenta.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            btnPretraziPacijenta.BringToFront();

            PostaviAktivnogPacijenta(Convert.ToInt32(searchForm.OdabraniPacijentId));
        }

        private void AddLabelUPanel(Panel roditelj, string tekst, int x, int y)
        {
            var lbl = new Label
            {
                Text = tekst,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(x, y),
                AutoSize = true
            };
            roditelj.Controls.Add(lbl);
        }

        // =====================================================================
        //  ŽIVA MERENJA SA UREĐAJA
        //  Izbor pacijenta -> aplikacija javi serveru ko je pacijent i ko je radnik
        //  (POST /api/sesija). Server upisuje merenja sa Nucleoa u tabelu MERENJA,
        //  a aplikacija ih jednom u sekundi čita iz baze: najnovije ide u velike
        //  brojeve gore, a svako novo merenje se dodaje na vrh tabele istorije.
        //  Adresa servera: App.config -> <appSettings><add key="ServerUrl" value="http://IP:5000" />
        // =====================================================================
        private static string ServerUrl()
        {
            string u = ConfigurationManager.AppSettings["ServerUrl"];
            if (string.IsNullOrWhiteSpace(u)) u = "http://localhost:5000";
            return u.Trim().TrimEnd('/');
        }

        /// <summary>HTTP poziv ka serveru. Vraća true samo za odgovor 2xx. Nikad ne baca izuzetak.</summary>
        private static bool PozoviServer(string metoda, string putanja, string json, int timeoutMs)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(ServerUrl() + putanja);
                req.Method = metoda;
                req.Timeout = timeoutMs;
                req.ReadWriteTimeout = timeoutMs;
                req.Proxy = null;                               // bez traženja proksija (inače zna da uspori prvi poziv)
                if (json != null)
                {
                    byte[] telo = Encoding.UTF8.GetBytes(json);
                    req.ContentType = "application/json";
                    req.ContentLength = telo.Length;
                    using (Stream st = req.GetRequestStream()) st.Write(telo, 0, telo.Length);
                }
                else if (metoda != "GET")
                {
                    req.ContentLength = 0;
                }
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    return (int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300;
                }
            }
            catch
            {
                return false;
            }
        }

        private async void PostaviAktivnogPacijenta(int pacijentId)
        {
            aktivniPacijentId = pacijentId;
            poslednjiMerenjeId = 0;
            poslednjePrimljeno = DateTime.MinValue;
            serverGreska = bazaGreska = false;
            statusServera = "Server: javljam pacijenta...";
            statusBaze = "";
            timerMerenja.Stop();

            NapraviPaneleMerenja();
            dgvIstorijaMerenja.Rows.Clear();
            lblSpo2Vrednost.Text = "--";
            lblPulsVrednost.Text = "--";
            OsveziVidljivostMerenja();
            OsveziStatus();

            // 0) ako prijava nije prosledila ID radnika, nadji ga u tabeli RADNICI (ime + prezime + uloga)
            if (idRadnika <= 0 && korisnikIme != null)
            {
                try
                {
                    idRadnika = await Task.Run(() => NadjiIdRadnika(korisnikIme, korisnikPrezime, korisnikUloga));
                }
                catch (Exception ex)
                {
                    bazaGreska = true;
                    statusBaze = "Baza: " + SkratiPoruku(ex.Message);
                }
                if (pacijentId != aktivniPacijentId) return;
            }

            // 1) javi serveru pacijenta i radnika (bez toga server odbija merenja)
            if (idRadnika <= 0)
            {
                serverGreska = true;
                statusServera = "ID radnika nije pronadjen u tabeli RADNICI - merenja se neće upisivati";
            }
            else
            {
                string json = "{\"id_pacijenta\":" + pacijentId + ",\"id_radnika\":" + idRadnika + "}";
                bool ok = await Task.Run(() => PozoviServer("POST", "/api/sesija", json, 3000));
                if (pacijentId != aktivniPacijentId) return;      // u međuvremenu izabran drugi pacijent
                serverGreska = !ok;
                statusServera = ok ? "Server: spreman, čekam merenja"
                                   : "Server nije dostupan (" + ServerUrl() + ") - merenja se neće upisivati";
            }
            OsveziStatus();

            // 2) istorija prethodnih merenja ovog pacijenta (poslednjih MaxRedovaIstorije)
            try
            {
                List<MerenjeRed> istorija = await Task.Run(() => UcitajMerenja(pacijentId, 0, MaxRedovaIstorije));
                if (pacijentId != aktivniPacijentId) return;
                foreach (MerenjeRed m in istorija)                 // stiglo je od najnovijeg ka najstarijem
                {
                    dgvIstorijaMerenja.Rows.Add(m.Vreme.ToString("dd.MM.yyyy HH:mm:ss"), m.Spo2, m.Puls);
                    if (m.Id > poslednjiMerenjeId) poslednjiMerenjeId = m.Id;
                }
                bazaGreska = false;
                statusBaze = "";
            }
            catch (Exception ex)
            {
                bazaGreska = true;
                statusBaze = "Baza: " + SkratiPoruku(ex.Message);
            }
            OsveziStatus();

            if (pacijentId == aktivniPacijentId) timerMerenja.Start();
        }

        private async void TimerMerenja_Tick(object sender, EventArgs e)
        {
            OsveziStatus();                                        // boja "staro" i poruke
            if (aktivniPacijentId <= 0 || citamMerenja) return;

            citamMerenja = true;
            int pac = aktivniPacijentId;
            long od = poslednjiMerenjeId;
            try
            {
                List<MerenjeRed> novi = await Task.Run(() => UcitajMerenja(pac, od, 0));
                if (pac == aktivniPacijentId)
                {
                    DodajNovaMerenja(novi);
                    bazaGreska = false;
                    statusBaze = "";
                }
            }
            catch (Exception ex)
            {
                bazaGreska = true;
                statusBaze = "Baza: " + SkratiPoruku(ex.Message);
            }
            finally
            {
                citamMerenja = false;
            }
            OsveziStatus();
        }

        /// <summary>novi: od najstarijeg ka najnovijem.</summary>
        private void DodajNovaMerenja(List<MerenjeRed> novi)
        {
            if (novi == null || novi.Count == 0) return;

            foreach (MerenjeRed m in novi)
            {
                dgvIstorijaMerenja.Rows.Insert(0, m.Vreme.ToString("dd.MM.yyyy HH:mm:ss"), m.Spo2, m.Puls);
                if (m.Id > poslednjiMerenjeId) poslednjiMerenjeId = m.Id;
            }
            while (dgvIstorijaMerenja.Rows.Count > MaxRedovaIstorije)
                dgvIstorijaMerenja.Rows.RemoveAt(dgvIstorijaMerenja.Rows.Count - 1);
            if (dgvIstorijaMerenja.Visible && dgvIstorijaMerenja.Rows.Count > 0)
            {
                try { dgvIstorijaMerenja.FirstDisplayedScrollingRowIndex = 0; }   // vrati prikaz na najnovije
                catch (InvalidOperationException) { }                              // tabela trenutno nema mesta za redove
            }

            MerenjeRed najnovije = novi[novi.Count - 1];
            lblSpo2Vrednost.Text = najnovije.Spo2.ToString();
            lblPulsVrednost.Text = najnovije.Puls.ToString();
            poslednjePrimljeno = DateTime.Now;
        }

        /// <summary>ID radnika iz tabele RADNICI. Vraća 0 ako radnik nije nađen ili ima više istih (ne pogađa).</summary>
        private int NadjiIdRadnika(string ime, string prezime, string uloga)
        {
            using (var conn = new OracleConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new OracleCommand(
                    "SELECT ID FROM RADNICI WHERE IME = :ime AND PREZIME = :prezime AND ULOGA = :uloga", conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add(new OracleParameter("ime", ime));
                    cmd.Parameters.Add(new OracleParameter("prezime", prezime));
                    cmd.Parameters.Add(new OracleParameter("uloga", uloga));
                    using (OracleDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return 0;
                        int id = Convert.ToInt32(r["ID"]);
                        return r.Read() ? 0 : id;            // drugi red = dvosmisleno, ne pogadjaj
                    }
                }
            }
        }

        private List<MerenjeRed> UcitajMerenja(int pacijentId, long odId, int limit)
        {
            var lista = new List<MerenjeRed>();
            using (var conn = new OracleConnection(GetConnectionString()))
            {
                conn.Open();
                string sql = limit > 0
                    ? "SELECT ID, VREME_MERENJA, SPO2, PULS FROM (" +
                      "SELECT ID, VREME_MERENJA, SPO2, PULS FROM MERENJA WHERE ID_PACIJENTA = :pac ORDER BY ID DESC) " +
                      "WHERE ROWNUM <= :lim"
                    : "SELECT ID, VREME_MERENJA, SPO2, PULS FROM MERENJA " +
                      "WHERE ID_PACIJENTA = :pac AND ID > :od ORDER BY ID";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add(new OracleParameter("pac", pacijentId));
                    if (limit > 0) cmd.Parameters.Add(new OracleParameter("lim", limit));
                    else cmd.Parameters.Add(new OracleParameter("od", odId));

                    using (OracleDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new MerenjeRed
                            {
                                Id = Convert.ToInt64(r["ID"]),
                                Vreme = r["VREME_MERENJA"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["VREME_MERENJA"]),
                                Spo2 = r["SPO2"] == DBNull.Value ? 0 : Convert.ToInt32(r["SPO2"]),
                                Puls = r["PULS"] == DBNull.Value ? 0 : Convert.ToInt32(r["PULS"])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        private void NapraviPaneleMerenja()
        {
            if (panelZivaMerenja != null) return;                  // već napravljeni, samo se prazne

            Color pozadina = ColorTranslator.FromHtml("#131A26");
            Color natpis = ColorTranslator.FromHtml("#8B96A5");

            // ---------- gornji panel: SpO2 i PULS velikim slovima ----------
            panelZivaMerenja = new Panel
            {
                Location = new Point(panelAktivniPacijent.Left, panelAktivniPacijent.Bottom + 10),
                Size = new Size(panelAktivniPacijent.Width, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = pozadina,
                Visible = false
            };

            var tabela = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = pozadina
            };
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabela.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            lblSpo2Vrednost = new Label
            {
                Dock = DockStyle.Fill,
                Text = "--",
                ForeColor = BojaSpo2,
                Font = new Font("Segoe UI", 56, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblPulsVrednost = new Label
            {
                Dock = DockStyle.Fill,
                Text = "--",
                ForeColor = BojaPuls,
                Font = new Font("Segoe UI", 56, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var kolonaSpo2 = new Panel { Dock = DockStyle.Fill, BackColor = pozadina };
            kolonaSpo2.Controls.Add(lblSpo2Vrednost);
            kolonaSpo2.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Text = "SpO2 (%)",
                ForeColor = natpis,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomCenter
            });

            var kolonaPuls = new Panel { Dock = DockStyle.Fill, BackColor = pozadina };
            kolonaPuls.Controls.Add(lblPulsVrednost);
            kolonaPuls.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Text = "PULS (bpm)",
                ForeColor = natpis,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomCenter
            });

            tabela.Controls.Add(kolonaSpo2, 0, 0);
            tabela.Controls.Add(kolonaPuls, 1, 0);

            lblStatusMerenja = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                ForeColor = natpis,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelZivaMerenja.Controls.Add(tabela);                 // Fill prvi, pa Bottom (isti redosled kao kod tvojih panela)
            panelZivaMerenja.Controls.Add(lblStatusMerenja);

            // ---------- donji panel: tabela istorije ----------
            panelIstorijaMerenja = new Panel
            {
                Location = new Point(panelAktivniPacijent.Left, panelZivaMerenja.Bottom + 10),
                Size = new Size(panelAktivniPacijent.Width, Math.Max(150, this.ClientSize.Height - panelZivaMerenja.Bottom - 10 - 40)),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = pozadina,
                Visible = false
            };

            dgvIstorijaMerenja = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = pozadina,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvIstorijaMerenja.DefaultCellStyle.BackColor = pozadina;
            dgvIstorijaMerenja.DefaultCellStyle.ForeColor = Color.White;
            dgvIstorijaMerenja.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#1E5FD9");
            dgvIstorijaMerenja.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#1A2231");
            dgvIstorijaMerenja.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvIstorijaMerenja.EnableHeadersVisualStyles = false;
            dgvIstorijaMerenja.GridColor = ColorTranslator.FromHtml("#1E2836");
            dgvIstorijaMerenja.Columns.Add("Vreme", "Vreme merenja");
            dgvIstorijaMerenja.Columns.Add("SpO2", "SpO2 (%)");
            dgvIstorijaMerenja.Columns.Add("Puls", "Puls (bpm)");

            panelIstorijaMerenja.Controls.Add(dgvIstorijaMerenja);

            this.Controls.Add(panelZivaMerenja);
            this.Controls.Add(panelIstorijaMerenja);
        }

        /// <summary>Paneli merenja se vide samo na Dashboard-u i samo kad je pacijent izabran.</summary>
        private void OsveziVidljivostMerenja()
        {
            bool vidi = aktivniPacijentId > 0 && panelAktivniPacijent.Visible;
            if (panelZivaMerenja != null) panelZivaMerenja.Visible = vidi;
            if (panelIstorijaMerenja != null) panelIstorijaMerenja.Visible = vidi;
        }

        private void OsveziStatus()
        {
            if (lblStatusMerenja == null) return;

            // brojevi koji nisu osvežavani 10 s postaju sivi (da se staro merenje ne uzme za trenutno)
            bool staro = poslednjePrimljeno != DateTime.MinValue &&
                         (DateTime.Now - poslednjePrimljeno).TotalSeconds > 10;
            lblSpo2Vrednost.ForeColor = staro ? BojaStaro : BojaSpo2;
            lblPulsVrednost.ForeColor = staro ? BojaStaro : BojaPuls;

            string tekst = statusServera;
            if (statusBaze != "") tekst += "   |   " + statusBaze;
            if (poslednjePrimljeno != DateTime.MinValue)
                tekst += "   |   Poslednje merenje: " + poslednjePrimljeno.ToString("HH:mm:ss") + (staro ? " (nema novih)" : "");

            lblStatusMerenja.Text = tekst;
            lblStatusMerenja.ForeColor = (serverGreska || bazaGreska)
                ? ColorTranslator.FromHtml("#FF6B6B")
                : ColorTranslator.FromHtml("#8B96A5");
        }

        private static string SkratiPoruku(string poruka)
        {
            if (string.IsNullOrEmpty(poruka)) return "greška";
            int kraj = poruka.IndexOf('\n');
            if (kraj > 0) poruka = poruka.Substring(0, kraj);
            return poruka.Length > 80 ? poruka.Substring(0, 80) + "..." : poruka;
        }

        /// <summary>Zatvara sesiju na serveru (odjava ili zatvaranje programa). Čeka najviše 1 s.</summary>
        private void ZatvoriSesiju()
        {
            timerMerenja.Stop();
            if (aktivniPacijentId > 0)
            {
                aktivniPacijentId = 0;
                PozoviServer("DELETE", "/api/sesija", null, 1000);
            }
            OsveziVidljivostMerenja();
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;
        }

        private void PrikaziPanelPacijenti()
        {
            if (panelPacijentiSekcija != null)
            {
                panelPacijentiSekcija.Visible = true;
                panelPacijentiSekcija.BringToFront();
                return;
            }

            panelPacijentiSekcija = new Panel
            {
                Location = panelAktivniPacijent.Location,
                Size = new Size(panelAktivniPacijent.Width, this.ClientSize.Height - panelAktivniPacijent.Top - 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = this.BackColor
            };

            var txtPretragaPacijenti = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = ColorTranslator.FromHtml("#1A2231"),
                ForeColor = ColorTranslator.FromHtml("#8B96A5"),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                Text = "Pretraži pacijenta...",
                Margin = new Padding(15, 0, 0, 10)
            };

            string placeholderTekst = "Pretraži pacijenta...";

            var dgvSviPacijenti = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = ColorTranslator.FromHtml("#131A26"),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvSviPacijenti.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#131A26");
            dgvSviPacijenti.DefaultCellStyle.ForeColor = Color.White;
            dgvSviPacijenti.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#1E5FD9");
            dgvSviPacijenti.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#1A2231");
            dgvSviPacijenti.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSviPacijenti.GridColor = ColorTranslator.FromHtml("#1E2836");

            void UcitajSvePacijente(string pretraga)
            {
                if (pretraga == placeholderTekst)
                {
                    pretraga = "";
                }

                DataTable dt = new DataTable();
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT ID AS ""ID_pacijenta"", IME AS ""Ime"", IME_RODITELJA AS ""Ime roditelja"",  
                                            PREZIME AS ""Prezime"", JMBG AS ""JMBG"",  
                                            DATUM_RODJENJA AS ""Datum rođenja"", ADRESA AS ""Adresa"",  
                                            TELEFON AS ""Broj telefona""
                                     FROM PACIJENTI
                                     WHERE UPPER(IME) LIKE UPPER(:p) OR UPPER(PREZIME) LIKE UPPER(:p) OR JMBG LIKE :p
                                     ORDER BY PREZIME, IME";
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("p", "%" + pretraga + "%"));
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
                dgvSviPacijenti.DataSource = dt;
            }

            txtPretragaPacijenti.GotFocus += (s, e) =>
            {
                if (txtPretragaPacijenti.Text == placeholderTekst)
                {
                    txtPretragaPacijenti.Text = "";
                    txtPretragaPacijenti.ForeColor = Color.White;
                }
            };

            txtPretragaPacijenti.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPretragaPacijenti.Text))
                {
                    txtPretragaPacijenti.Text = placeholderTekst;
                    txtPretragaPacijenti.ForeColor = ColorTranslator.FromHtml("#8B96A5");
                    UcitajSvePacijente("");
                }
            };

            txtPretragaPacijenti.TextChanged += (s, e) =>
            {
                if (txtPretragaPacijenti.Text != placeholderTekst)
                {
                    UcitajSvePacijente(txtPretragaPacijenti.Text.Trim());
                }
            };

            dgvSviPacijenti.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int pacijentId = Convert.ToInt32(dgvSviPacijenti.Rows[e.RowIndex].Cells["ID_pacijenta"].Value);
                using (var detaljiForm = new PacijentDetaljiForm(pacijentId))
                {
                    detaljiForm.ShowDialog();
                }
            };

            UcitajSvePacijente("");

            panelPacijentiSekcija.Controls.Add(dgvSviPacijenti);
            panelPacijentiSekcija.Controls.Add(txtPretragaPacijenti);

            this.Controls.Add(panelPacijentiSekcija);
            panelPacijentiSekcija.BringToFront();
        }

        private void PrikaziPanelPodesavanja()
        {
            if (panelPodesavanjaSekcija != null)
            {
                panelPodesavanjaSekcija.Visible = true;
                panelPodesavanjaSekcija.BringToFront();
                return;
            }

            panelPodesavanjaSekcija = new Panel
            {
                Location = panelAktivniPacijent.Location,
                Size = new Size(panelAktivniPacijent.Width, this.ClientSize.Height - panelAktivniPacijent.Top - 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = this.BackColor
            };

            // Glavni naslov sekcije
            var lblNaslov = new Label
            {
                Text = "Podešavanje Wi-Fi / MQTT komunikacije",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 25),
                AutoSize = true
            };
            panelPodesavanjaSekcija.Controls.Add(lblNaslov);

            // GroupBox okvir za kontrole
            var groupBoxConfig = new GroupBox
            {
                Text = " Parametri mrežne povezanosti ESP-01 modula ",
                ForeColor = ColorTranslator.FromHtml("#8B96A5"),
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 80),
                Size = new Size(520, 320)
            };

            // IP Adresa Servera
            var lblIp = new Label { Text = "IP Adresa Servera:", ForeColor = Color.White, Location = new Point(30, 45), AutoSize = true };
            var txtIp = new TextBox { Text = "192.168.1.100", Location = new Point(30, 70), Width = 440, BackColor = ColorTranslator.FromHtml("#1A2231"), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };

            // Port
            var lblPort = new Label { Text = "Port:", ForeColor = Color.White, Location = new Point(30, 110), AutoSize = true };
            var txtPort = new TextBox { Text = "8080", Location = new Point(30, 135), Width = 440, BackColor = ColorTranslator.FromHtml("#1A2231"), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };

            // Wi-Fi SSID
            var lblSsid = new Label { Text = "Wi-Fi SSID (Mreža):", ForeColor = Color.White, Location = new Point(30, 175), AutoSize = true };
            var txtSsid = new TextBox { Text = "MojiUredjaji_WiFi", Location = new Point(30, 200), Width = 440, BackColor = ColorTranslator.FromHtml("#1A2231"), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };

            // Wi-Fi Lozinka
            var lblPass = new Label { Text = "Wi-Fi Lozinka:", ForeColor = Color.White, Location = new Point(30, 240), AutoSize = true };
            var txtPass = new TextBox { Text = "********", UseSystemPasswordChar = true, Location = new Point(30, 265), Width = 440, BackColor = ColorTranslator.FromHtml("#1A2231"), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };

            groupBoxConfig.Controls.Add(lblIp);
            groupBoxConfig.Controls.Add(txtIp);
            groupBoxConfig.Controls.Add(lblPort);
            groupBoxConfig.Controls.Add(txtPort);
            groupBoxConfig.Controls.Add(lblSsid);
            groupBoxConfig.Controls.Add(txtSsid);
            groupBoxConfig.Controls.Add(lblPass);
            groupBoxConfig.Controls.Add(txtPass);

            panelPodesavanjaSekcija.Controls.Add(groupBoxConfig);

            // Dugme za čuvanje
            var btnSacuvaj = new Button
            {
                Text = "Sačuvaj izmene",
                Location = new Point(30, 420),
                Size = new Size(160, 40),
                BackColor = ColorTranslator.FromHtml("#1E5FD9"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSacuvaj.FlatAppearance.BorderSize = 0;

            btnSacuvaj.Click += (s, e) =>
            {
                MessageBox.Show("Mrežna podešavanja su uspešno sačuvana!", "Uspeh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            panelPodesavanjaSekcija.Controls.Add(btnSacuvaj);

            this.Controls.Add(panelPodesavanjaSekcija);
            panelPodesavanjaSekcija.BringToFront();
        }

        private void NavButton_Click(object sender, MouseEventArgs e)
        {

        }
    }
}