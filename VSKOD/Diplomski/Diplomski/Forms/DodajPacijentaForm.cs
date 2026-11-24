using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace Diplomski.Forms
{
    public partial class DodajPacijentaForm : Form
    {
        public int NoviPacijentId { get; private set; }
        public string NoviJmbg { get; private set; }
        public string NovoIme { get; private set; }
        public string NovoPrezime { get; private set; }
        public string NoviPol { get; private set; }
        public DateTime? NoviDatumRodjenja { get; private set; }
        public string NoviTelefon { get; private set; }
        public string NovaAdresa { get; private set; }

        public DodajPacijentaForm()
        {
            InitializeComponent();

            this.BackColor = ColorTranslator.FromHtml("#0B0F17");
            this.Text = "Dodaj novog pacijenta";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            
            cmbPol.Items.Clear();
            cmbPol.Items.Add("Muški");
            cmbPol.Items.Add("Ženski");
            cmbPol.DropDownStyle = ComboBoxStyle.DropDownList;

           
            dtpDatumRodjenja.Value = DateTime.Now.AddYears(-30);
            dtpDatumRodjenja.Format = DateTimePickerFormat.Custom;
            dtpDatumRodjenja.CustomFormat = "dd.MM.yyyy";

            btnSacuvaj.BackColor = ColorTranslator.FromHtml("#1E5FD9");
            btnSacuvaj.ForeColor = Color.White;
            btnSacuvaj.FlatStyle = FlatStyle.Flat;
            btnSacuvaj.FlatAppearance.BorderSize = 0;

            btnOtkazi.BackColor = ColorTranslator.FromHtml("#1A2231");
            btnOtkazi.ForeColor = ColorTranslator.FromHtml("#8B96A5");
            btnOtkazi.FlatStyle = FlatStyle.Flat;
            btnOtkazi.FlatAppearance.BorderSize = 0;

            btnSacuvaj.Click += BtnSacuvaj_Click;
            btnOtkazi.Click += BtnOtkazi_Click;
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;
        }

        private void BtnSacuvaj_Click(object sender, EventArgs e)
        {
            string jmbg = txtJmbg.Text.Trim();
            string ime = txtIme.Text.Trim();
            string imeRoditelja = txtImeRoditelja.Text.Trim();
            string prezime = txtPrezime.Text.Trim();
            string pol = cmbPol.Text;
            DateTime datumRodjenja = dtpDatumRodjenja.Value;
            string telefon = txtTelefon.Text.Trim();
            string adresa = txtAdresa.Text.Trim();

            
            if (string.IsNullOrEmpty(jmbg) || string.IsNullOrEmpty(ime) || string.IsNullOrEmpty(prezime))
            {
                MessageBox.Show("JMBG, Ime i Prezime su obavezni podaci.", "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (jmbg.Length != 13 || !long.TryParse(jmbg, out _))
            {
                MessageBox.Show("JMBG mora imati tačno 13 cifara.", "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(pol))
            {
                MessageBox.Show("Molimo izaberite pol pacijenta.", "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    
                    string proveraQuery = "SELECT COUNT(*) FROM PACIJENTI WHERE JMBG = :jmbg";
                    using (OracleCommand proveraCmd = new OracleCommand(proveraQuery, conn))
                    {
                        proveraCmd.Parameters.Add(new OracleParameter("jmbg", jmbg));
                        int broj = Convert.ToInt32(proveraCmd.ExecuteScalar());

                        if (broj > 0)
                        {
                            MessageBox.Show("Pacijent sa ovim JMBG-om već postoji u bazi.", "Greška",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string insertQuery = @"INSERT INTO PACIJENTI 
                                            (JMBG, IME, IME_RODITELJA, PREZIME, POL, DATUM_RODJENJA, TELEFON, ADRESA)
                                            VALUES 
                                            (:jmbg, :ime, :imeRoditelja, :prezime, :pol, :datumRodjenja, :telefon, :adresa)
                                            RETURNING ID INTO :noviId";

                    using (OracleCommand cmd = new OracleCommand(insertQuery, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("jmbg", jmbg));
                        cmd.Parameters.Add(new OracleParameter("ime", ime));
                        cmd.Parameters.Add(new OracleParameter("imeRoditelja", string.IsNullOrEmpty(imeRoditelja) ? (object)DBNull.Value : imeRoditelja));
                        cmd.Parameters.Add(new OracleParameter("prezime", prezime));
                        cmd.Parameters.Add(new OracleParameter("pol", pol));
                        cmd.Parameters.Add(new OracleParameter("datumRodjenja", datumRodjenja));
                        cmd.Parameters.Add(new OracleParameter("telefon", string.IsNullOrEmpty(telefon) ? (object)DBNull.Value : telefon));
                        cmd.Parameters.Add(new OracleParameter("adresa", string.IsNullOrEmpty(adresa) ? (object)DBNull.Value : adresa));

                        OracleParameter noviIdParam = new OracleParameter("noviId", OracleDbType.Int32, ParameterDirection.Output);
                        cmd.Parameters.Add(noviIdParam);

                        cmd.ExecuteNonQuery();

                        NoviPacijentId = Convert.ToInt32(((Oracle.ManagedDataAccess.Types.OracleDecimal)noviIdParam.Value).Value);
                    }
                }

                
                NoviJmbg = jmbg;
                NovoIme = ime;
                NovoPrezime = prezime;
                NoviPol = pol;
                NoviDatumRodjenja = datumRodjenja;
                NoviTelefon = string.IsNullOrEmpty(telefon) ? "-" : telefon;
                NovaAdresa = string.IsNullOrEmpty(adresa) ? "-" : adresa;

                MessageBox.Show("Pacijent je uspešno dodat.", "Uspeh",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom dodavanja pacijenta: " + ex.Message, "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOtkazi_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
