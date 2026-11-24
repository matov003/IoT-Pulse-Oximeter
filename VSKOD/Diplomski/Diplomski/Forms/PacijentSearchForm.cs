using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace Diplomski.Forms
{
    public partial class PacijentSearchForm : Form
    {
        public int OdabraniPacijentId { get; private set; }
        public string OdabraniJmbg { get; private set; }
        public string OdabranoIme { get; private set; }
        public string OdabranoPrezime { get; private set; }
        public string OdabraniPol { get; private set; }
        public DateTime? OdabraniDatumRodjenja { get; private set; }
        public string OdabraniTelefon { get; private set; }
        public string OdabranaAdresa { get; private set; }

        public PacijentSearchForm()
        {
            InitializeComponent();

            this.BackColor = ColorTranslator.FromHtml("#0B0F17");
            this.Text = "Pretraga pacijenata";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            PodesiIzgledGrida();
            UcitajPacijente("");

            txtPretraga.TextChanged += TxtPretraga_TextChanged;
            dgvPacijenti.CellDoubleClick += DgvPacijenti_CellDoubleClick;
            btnDodajPacijenta.Click += btnDodajPacijenta_Click;
        }

        private void PodesiIzgledGrida()
        {
            dgvPacijenti.BackgroundColor = ColorTranslator.FromHtml("#131A26");
            dgvPacijenti.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#131A26");
            dgvPacijenti.DefaultCellStyle.ForeColor = Color.White;
            dgvPacijenti.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#1E5FD9");
            dgvPacijenti.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#1A2231");
            dgvPacijenti.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPacijenti.GridColor = ColorTranslator.FromHtml("#1E2836");
            dgvPacijenti.BorderStyle = BorderStyle.None;
            dgvPacijenti.RowHeadersVisible = false;
            dgvPacijenti.AllowUserToAddRows = false;
            dgvPacijenti.ReadOnly = true;
            dgvPacijenti.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPacijenti.MultiSelect = false;
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;
        }

        private void UcitajPacijente(string pretraga)
        {
            DataTable dt = new DataTable();

            using (OracleConnection conn = new OracleConnection(GetConnectionString()))
            {
                conn.Open();

                string query = @"SELECT ID, JMBG, IME, PREZIME, POL, DATUM_RODJENJA, TELEFON, ADRESA 
                                  FROM PACIJENTI 
                                  WHERE UPPER(IME) LIKE UPPER(:pretraga) 
                                     OR UPPER(PREZIME) LIKE UPPER(:pretraga)
                                     OR JMBG LIKE :pretraga
                                  ORDER BY PREZIME, IME";

                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("pretraga", "%" + pretraga + "%"));

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            dgvPacijenti.DataSource = dt;

            if (dgvPacijenti.Columns.Contains("ID"))
                dgvPacijenti.Columns["ID"].Visible = false;

            if (dgvPacijenti.Columns.Contains("JMBG"))
            {
                dgvPacijenti.Columns["JMBG"].HeaderText = "JMBG";
                dgvPacijenti.Columns["JMBG"].DisplayIndex = 0;
            }
            if (dgvPacijenti.Columns.Contains("IME"))
            {
                dgvPacijenti.Columns["IME"].HeaderText = "Ime";
                dgvPacijenti.Columns["IME"].DisplayIndex = 1;
            }
            if (dgvPacijenti.Columns.Contains("PREZIME"))
            {
                dgvPacijenti.Columns["PREZIME"].HeaderText = "Prezime";
                dgvPacijenti.Columns["PREZIME"].DisplayIndex = 2;
            }

            if (dgvPacijenti.Columns.Contains("POL"))
                dgvPacijenti.Columns["POL"].Visible = false;
            if (dgvPacijenti.Columns.Contains("DATUM_RODJENJA"))
                dgvPacijenti.Columns["DATUM_RODJENJA"].Visible = false;
            if (dgvPacijenti.Columns.Contains("TELEFON"))
                dgvPacijenti.Columns["TELEFON"].Visible = false;
            if (dgvPacijenti.Columns.Contains("ADRESA"))
                dgvPacijenti.Columns["ADRESA"].Visible = false;

            dgvPacijenti.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void TxtPretraga_TextChanged(object sender, EventArgs e)
        {
            UcitajPacijente(txtPretraga.Text.Trim());
        }

        private void DgvPacijenti_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow red = dgvPacijenti.Rows[e.RowIndex];

            OdabraniPacijentId = Convert.ToInt32(red.Cells["ID"].Value);
            OdabraniJmbg = red.Cells["JMBG"].Value.ToString();
            OdabranoIme = red.Cells["IME"].Value.ToString();
            OdabranoPrezime = red.Cells["PREZIME"].Value.ToString();
            OdabraniPol = red.Cells["POL"].Value?.ToString() ?? "-";
            OdabraniDatumRodjenja = red.Cells["DATUM_RODJENJA"].Value != DBNull.Value
                ? (DateTime?)red.Cells["DATUM_RODJENJA"].Value : null;
            OdabraniTelefon = red.Cells["TELEFON"].Value?.ToString() ?? "-";
            OdabranaAdresa = red.Cells["ADRESA"].Value?.ToString() ?? "-";

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

       

        private void btnDodajPacijenta_Click(object sender, EventArgs e)
        {
            using (var dodajForm = new DodajPacijentaForm())
            {
                if (dodajForm.ShowDialog() == DialogResult.OK)
                {                  
                    OdabraniPacijentId = dodajForm.NoviPacijentId;
                    OdabraniJmbg = dodajForm.NoviJmbg;
                    OdabranoIme = dodajForm.NovoIme;
                    OdabranoPrezime = dodajForm.NovoPrezime;
                    OdabraniPol = dodajForm.NoviPol;
                    OdabraniDatumRodjenja = dodajForm.NoviDatumRodjenja;
                    OdabraniTelefon = dodajForm.NoviTelefon;
                    OdabranaAdresa = dodajForm.NovaAdresa;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
    }
}