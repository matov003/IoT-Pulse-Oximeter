using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace Diplomski.UserControls
{
    public partial class LoginControl : UserControl
    {
        // Event koji se okida kad je prijava uspešna
        public event EventHandler<LoginEventArgs> LoginUspesan;

        public LoginControl()
        {
            InitializeComponent();
            txtKorisnickoIme.KeyDown += textBox1_KeyDown;
            txtLozinka.KeyDown += textBox1_KeyDown;
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = txtKorisnickoIme.Text.Trim();
            string password = txtLozinka.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Molimo unesite korisničko ime i lozinku.", "Greška",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int idRadnika = 0;
                string hashIzBaze = null;
                string imeRadnika = null;
                string prezimeRadnika = null;
                string ulogaRadnika = null;

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ID, LOZINKA, IME, PREZIME, ULOGA FROM RADNICI WHERE USERNAME = :username";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("username", username));

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                idRadnika = Convert.ToInt32(reader["ID"]);
                                hashIzBaze = reader["LOZINKA"].ToString();
                                imeRadnika = reader["IME"].ToString();
                                prezimeRadnika = reader["PREZIME"].ToString();
                                ulogaRadnika = reader["ULOGA"].ToString();
                            }
                        }
                    }
                }

                if (hashIzBaze == null)
                {
                    MessageBox.Show("Pogrešno korisničko ime ili lozinka.", "Neuspešna prijava",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtLozinka.Clear();
                    txtLozinka.Focus();
                    return;
                }

                bool lozinkaTacna = BCrypt.Net.BCrypt.Verify(password, hashIzBaze);

                if (lozinkaTacna)
                {
                    LoginUspesan?.Invoke(this, new LoginEventArgs(username, imeRadnika, prezimeRadnika, ulogaRadnika, idRadnika));
                }
                else
                {
                    MessageBox.Show("Pogrešno korisničko ime ili lozinka.", "Neuspešna prijava",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtLozinka.Clear();
                    txtLozinka.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom povezivanja sa bazom: " + ex.Message, "Greška konekcije",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.Clear(this.BackColor);
            Rectangle rect = new Rectangle(0, 0, panel1.Width - 1, panel1.Height - 1);
            int radius = 14;
            int d = radius * 2;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                using (SolidBrush fill = new SolidBrush(ColorTranslator.FromHtml("#1E2836")))
                {
                    e.Graphics.FillPath(fill, path);
                }
                using (Pen p = new Pen(ColorTranslator.FromHtml("#1E2836"), 1.5f))
                {
                    e.Graphics.DrawPath(p, path);
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                button1_Click(sender, e);
            }
        }

        private void chkPrikaziLozinku_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPrikaziLozinku.Checked)
            {
                txtLozinka.PasswordChar = '\0';
            }
            else
            {
                txtLozinka.PasswordChar = '*';
            }
        }
    }


    public class LoginEventArgs : EventArgs
    {
        public string Username { get; }
        public string Ime { get; }
        public string Prezime { get; }
        public string Uloga { get; }
        public int IdRadnika { get; }

        public LoginEventArgs(string username, string ime, string prezime, string uloga, int idRadnika)
            : this(username, ime, prezime, uloga)
        {
            IdRadnika = idRadnika;
        }

        public LoginEventArgs(string username, string ime, string prezime, string uloga)
        {
            Username = username;
            Ime = ime;
            Prezime = prezime;
            Uloga = uloga;
        }
    }
}