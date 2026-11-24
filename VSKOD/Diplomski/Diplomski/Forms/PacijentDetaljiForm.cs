using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Diplomski.Forms
{
    public partial class PacijentDetaljiForm : Form
    {
        private Label lblID;
        private Label lblJMBG;
        private Label lblIme;
        private Label lblImeRoditelja;
        private Label lblPrezime;
        private Label lblDatumRodjenja;
        private Label lblAdresa;
        private Label lblTelefon;
        private readonly int pacijentID;
        private DateTimePicker dtpOd;
        private DateTimePicker dtpDo;
        private Button btnPrikazi;
        private DataGridView dgv;
        private Label lblSazetak;

        private static readonly Color BojaPozadina = Color.FromArgb(31, 41, 55);

        private const int SPO2_ALARM = 92;   
        private const int PULS_MIN = 50;
        private const int PULS_MAX = 110;

        public PacijentDetaljiForm(int id)
        {
            InitializeComponent();
            this.ClientSize = new Size(960, 760);
            pacijentID = id;
            this.BackColor = Color.FromArgb(19, 26, 38);
            KreirajPodatkePacijenta();
            KreirajFilterDatuma();
            KreirajRezultate();
            UcitajPodatkePacijenta();
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;
        }

        private void KreirajPodatkePacijenta()
        {
            Panel panelInfo = new Panel();
            panelInfo.Location = new Point(20, 20);
            panelInfo.Size = new Size(920, 180);
            panelInfo.BorderStyle = BorderStyle.FixedSingle;
            panelInfo.BackColor = Color.FromArgb(31, 41, 55);
            panelInfo.BorderStyle = BorderStyle.None;

            Controls.Add(panelInfo);

            PictureBox pbSlika = new PictureBox();
            pbSlika.Location = new Point(15, 15);
            pbSlika.Size = new Size(120, 140);
            pbSlika.BorderStyle = BorderStyle.FixedSingle;
            pbSlika.SizeMode = PictureBoxSizeMode.Zoom;
            try
            {
                pbSlika.Image = Properties.Resources.user;
            }
            catch { }

            panelInfo.Controls.Add(pbSlika);

            Font naslov = new Font("Segoe UI", 10, FontStyle.Bold);
            Font vrednost = new Font("Segoe UI", 10);

            int x1 = 170;
            int x2 = 520;
            int y = 20;
            int razmak = 30;

            // LEVA KOLONA
            panelInfo.Controls.Add(new Label() { Text = "ID:", Location = new Point(x1, y), AutoSize = true, ForeColor = Color.White, Font = naslov });
            lblID = new Label() { Location = new Point(x1 + 120, y), AutoSize = true, ForeColor = Color.Gainsboro, Font = vrednost };
            panelInfo.Controls.Add(lblID);

            y += razmak;
            panelInfo.Controls.Add(new Label() { Text = "Ime:", Location = new Point(x1, y), AutoSize = true, ForeColor = Color.White, Font = naslov });
            lblIme = new Label() { Location = new Point(x1 + 120, y), AutoSize = true, ForeColor = Color.Gainsboro, Font = vrednost };
            panelInfo.Controls.Add(lblIme);

            y += razmak;
            panelInfo.Controls.Add(new Label() { Text = "Ime roditelja:", Location = new Point(x1, y), AutoSize = true, ForeColor = Color.White, Font = naslov });
            lblImeRoditelja = new Label() { Location = new Point(x1 + 120, y), AutoSize = true, ForeColor = Color.Gainsboro, Font = vrednost };
            panelInfo.Controls.Add(lblImeRoditelja);

            y += razmak;
            panelInfo.Controls.Add(new Label() { Text = "Prezime:", Location = new Point(x1, y), AutoSize = true, ForeColor = Color.White, Font = naslov });
            lblPrezime = new Label() { Location = new Point(x1 + 120, y), AutoSize = true, ForeColor = Color.Gainsboro, Font = vrednost };
            panelInfo.Controls.Add(lblPrezime);

            // DESNA KOLONA
            y = 20;
            panelInfo.Controls.Add(new Label() { Text = "JMBG:", Location = new Point(x2, y), AutoSize = true, ForeColor = Color.White, Font = naslov });
            lblJMBG = new Label() { Location = new Point(x2 + 140, y), AutoSize = true, ForeColor = Color.Gainsboro, Font = vrednost };
            panelInfo.Controls.Add(lblJMBG);

            y += razmak;
            panelInfo.Controls.Add(new Label() { Text = "Datum rođenja:", Location = new Point(x2, y), AutoSize = true, ForeColor = Color.White, Font = naslov });
            lblDatumRodjenja = new Label() { Location = new Point(x2 + 140, y), AutoSize = true, ForeColor = Color.Gainsboro, Font = vrednost };
            panelInfo.Controls.Add(lblDatumRodjenja);

            y += razmak;
            panelInfo.Controls.Add(new Label() { Text = "Adresa:", Location = new Point(x2, y), AutoSize = true, ForeColor = Color.White, Font = naslov });
            lblAdresa = new Label() { Location = new Point(x2 + 140, y), AutoSize = true, ForeColor = Color.Gainsboro, Font = vrednost };
            panelInfo.Controls.Add(lblAdresa);

            y += razmak;
            panelInfo.Controls.Add(new Label() { Text = "Telefon:", Location = new Point(x2, y), AutoSize = true, ForeColor = Color.White, Font = naslov });
            lblTelefon = new Label() { Location = new Point(x2 + 140, y), AutoSize = true, ForeColor = Color.Gainsboro, Font = vrednost };
            panelInfo.Controls.Add(lblTelefon);
        }

        private void UcitajPodatkePacijenta()
        {
            using (OracleConnection conn = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            ID, 
                            IME, 
                            IME_RODITELJA, 
                            PREZIME, 
                            JMBG, 
                            DATUM_RODJENJA, 
                            ADRESA, 
                            TELEFON 
                        FROM PACIJENTI 
                        WHERE ID = :id";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":id", OracleDbType.Int32).Value = pacijentID;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblID.Text = reader["ID"]?.ToString() ?? "";
                                lblIme.Text = reader["IME"]?.ToString() ?? "";
                                lblImeRoditelja.Text = reader["IME_RODITELJA"]?.ToString() ?? "";
                                lblPrezime.Text = reader["PREZIME"]?.ToString() ?? "";
                                lblJMBG.Text = reader["JMBG"]?.ToString() ?? "";
                                lblAdresa.Text = reader["ADRESA"]?.ToString() ?? "";
                                lblTelefon.Text = reader["TELEFON"]?.ToString() ?? "";

                                if (reader["DATUM_RODJENJA"] != DBNull.Value && reader["DATUM_RODJENJA"] != null)
                                {
                                    lblDatumRodjenja.Text = Convert.ToDateTime(reader["DATUM_RODJENJA"]).ToString("dd.MM.yyyy.");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Greška pri učitavanju podataka iz baze: " + ex.Message, "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void KreirajFilterDatuma()
        {
            Panel panelFilter = new Panel();
            panelFilter.Location = new Point(20, 215);
            panelFilter.Size = new Size(920, 55);
            panelFilter.BorderStyle = BorderStyle.FixedSingle;
            panelFilter.BackColor = Color.FromArgb(31, 41, 55);
            panelFilter.BorderStyle = BorderStyle.None;

            Controls.Add(panelFilter);

            Label lblOd = new Label();
            lblOd.Text = "Od:";
            lblOd.ForeColor = Color.White;
            lblOd.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblOd.Location = new Point(20, 18);
            lblOd.AutoSize = true;
            panelFilter.Controls.Add(lblOd);

            dtpOd = new DateTimePicker();
            dtpOd.Format = DateTimePickerFormat.Custom;
            dtpOd.CustomFormat = "dd.MM.yyyy.";
            dtpOd.Location = new Point(60, 15);
            dtpOd.Size = new Size(120, 25);
            dtpOd.Value = DateTime.Today.AddDays(-7);
            panelFilter.Controls.Add(dtpOd);

            Label lblDo = new Label();
            lblDo.Text = "Do:";
            lblDo.ForeColor = Color.White;
            lblDo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblDo.Location = new Point(220, 18);
            lblDo.AutoSize = true;
            panelFilter.Controls.Add(lblDo);

            dtpDo = new DateTimePicker();
            dtpDo.Format = DateTimePickerFormat.Custom;
            dtpDo.CustomFormat = "dd.MM.yyyy.";
            dtpDo.Location = new Point(260, 15);
            dtpDo.Size = new Size(120, 25);
            dtpDo.Value = DateTime.Today;
            panelFilter.Controls.Add(dtpDo);

            btnPrikazi = new Button();
            btnPrikazi.Text = "Prikaži";
            btnPrikazi.Location = new Point(420, 13);
            btnPrikazi.Size = new Size(100, 30);
            btnPrikazi.BackColor = Color.FromArgb(37, 99, 235);
            btnPrikazi.ForeColor = Color.White;
            btnPrikazi.FlatStyle = FlatStyle.Flat;
            btnPrikazi.FlatAppearance.BorderSize = 0;
            btnPrikazi.Cursor = Cursors.Hand;
            btnPrikazi.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnPrikazi.Click += BtnPrikazi_Click;

            panelFilter.Controls.Add(btnPrikazi);
        }

        private void KreirajRezultate()
        {
            lblSazetak = new Label
            {
                Location = new Point(20, 282),
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10),
                Text = "Izaberi period i klikni Prikaži."
            };
            Controls.Add(lblSazetak);

            dgv = new DataGridView
            {
                Location = new Point(20, 315),
                Size = new Size(920, 420),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = BojaPozadina,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(55, 65, 81),
                EnableHeadersVisualStyles = false
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(17, 24, 39);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.DefaultCellStyle.BackColor = BojaPozadina;
            dgv.DefaultCellStyle.ForeColor = Color.Gainsboro;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
            Controls.Add(dgv);
        }

        private void BtnPrikazi_Click(object sender, EventArgs e)
        {
            DateTime od = dtpOd.Value.Date;
            DateTime doo = dtpDo.Value.Date.AddDays(1);   

            if (od >= doo)
            {
                MessageBox.Show("Datum 'Od' mora biti pre datuma 'Do'.", "Upozorenje",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable dt = new DataTable();

            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                using (OracleCommand cmd = new OracleCommand(@"
                    SELECT VREME_MERENJA, SPO2, PULS
                    FROM   MERENJA
                    WHERE  ID_PACIJENTA = :id
                      AND  VREME_MERENJA >= :od
                      AND  VREME_MERENJA <  :doo
                    ORDER  BY VREME_MERENJA DESC", conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("id", OracleDbType.Int32).Value = pacijentID;
                    cmd.Parameters.Add("od", OracleDbType.TimeStamp).Value = od;
                    cmd.Parameters.Add("doo", OracleDbType.TimeStamp).Value = doo;

                    conn.Open();
                    using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                        da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri učitavanju merenja: " + ex.Message, "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dt.Rows.Count == 0)
            {
                dgv.DataSource = null;
                lblSazetak.Text = "Nema merenja u izabranom periodu.";
                return;
            }

            // tabela
            dgv.DataSource = dt;
            dgv.Columns["VREME_MERENJA"].HeaderText = "Vreme merenja";
            dgv.Columns["VREME_MERENJA"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            dgv.Columns["SPO2"].HeaderText = "SpO2 (%)";
            dgv.Columns["PULS"].HeaderText = "Puls (bpm)";

            // alarm
            int brojAlarma = 0;
            foreach (DataGridViewRow red in dgv.Rows)
            {
                int spo2 = Convert.ToInt32(red.Cells["SPO2"].Value);
                int puls = Convert.ToInt32(red.Cells["PULS"].Value);
                if (spo2 < SPO2_ALARM || puls < PULS_MIN || puls > PULS_MAX)
                {
                    red.DefaultCellStyle.BackColor = Color.FromArgb(127, 29, 29);
                    red.DefaultCellStyle.ForeColor = Color.White;
                    brojAlarma++;
                }
            }

            // sažetak
            lblSazetak.Text =
                $"Merenja: {dt.Rows.Count} | " +
                $"SpO2 min/prosek/max: {dt.Compute("MIN(SPO2)", "")}/" +
                $"{Convert.ToDouble(dt.Compute("AVG(SPO2)", "")):0.0}/{dt.Compute("MAX(SPO2)", "")} | " +
                $"Puls prosek: {Convert.ToDouble(dt.Compute("AVG(PULS)", "")):0} | " +
                $"Alarmi: {brojAlarma}";
        }
    }
}