using Diplomski.Forms;
using Diplomski.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Diplomski
{
    public partial class Form1 : Form
    {
        private LoginControl loginEkran;
        private MainForm mainForm;

        public Form1()
        {
            InitializeComponent();
            this.Shown += (s, e) => this.WindowState = FormWindowState.Maximized;
            PrikaziLogin();
        }

        private void PrikaziLogin()
        {
            loginEkran = new LoginControl();
            this.Controls.Add(loginEkran);
            loginEkran.BringToFront();
            loginEkran.LoginUspesan += LoginControl_LoginUspesan;
            this.Resize += (sender, e) =>
            {
                if (loginEkran != null && this.Controls.Contains(loginEkran))
                {
                    loginEkran.Location = new Point(
                        (this.ClientSize.Width - loginEkran.Width) / 2,
                        (this.ClientSize.Height - loginEkran.Height) / 2
                    );
                }
            };
            this.OnResize(EventArgs.Empty);
        }

        private void LoginControl_LoginUspesan(object sender, LoginEventArgs e)
        {
            this.Controls.Remove(loginEkran);
            loginEkran.Dispose();

            mainForm = new MainForm();
            mainForm.PostaviKorisnika(e.Ime, e.Prezime, e.Uloga);
            mainForm.OdjaviSeKliknuto += MainForm_OdjaviSeKliknuto;

            mainForm.Show();
            this.Hide();
        }

        private void MainForm_OdjaviSeKliknuto(object sender, EventArgs e)
        {
            mainForm.OdjaviSeKliknuto -= MainForm_OdjaviSeKliknuto;
            mainForm.Close();
            mainForm.Dispose();

            this.Show();
            PrikaziLogin();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

       
    }
}