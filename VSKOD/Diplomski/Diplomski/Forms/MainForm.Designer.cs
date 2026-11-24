namespace Diplomski.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlLeftMenu = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnOdjaviSe = new System.Windows.Forms.Button();
            this.lblPrezimeKorisnika = new System.Windows.Forms.Label();
            this.lblImeKorisnika = new System.Windows.Forms.Label();
            this.lblTitulaKorisnika = new System.Windows.Forms.Label();
            this.iconPictureBox1 = new FontAwesome.Sharp.IconPictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnPodesavanje = new System.Windows.Forms.Button();
            this.btnPacijenti = new System.Windows.Forms.Button();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblVreme = new System.Windows.Forms.Label();
            this.lblVerzija = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panelAktivniPacijent = new System.Windows.Forms.Panel();
            this.btnPretraziPacijenta = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnlLeftMenu.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox1)).BeginInit();
            this.panel3.SuspendLayout();
            this.panelAktivniPacijent.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(1);
            this.panel1.Size = new System.Drawing.Size(1791, 45);
            this.panel1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Transparent;
            this.button1.Dock = System.Windows.Forms.DockStyle.Right;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(243)))));
            this.button1.Location = new System.Drawing.Point(1747, 1);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(43, 43);
            this.button1.TabIndex = 1;
            this.button1.Text = "X";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(243)))));
            this.label1.Location = new System.Drawing.Point(52, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(458, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "IoT Pulsni Oksimetar – Sistem za Praćenje Pacijenata";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(26)))), ((int)(((byte)(38)))));
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = global::Diplomski.Properties.Resources.heart_rate__1_;
            this.pictureBox1.Location = new System.Drawing.Point(1, 1);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Padding = new System.Windows.Forms.Padding(1);
            this.pictureBox1.Size = new System.Drawing.Size(43, 43);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // pnlLeftMenu
            // 
            this.pnlLeftMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlLeftMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(54)))));
            this.pnlLeftMenu.Controls.Add(this.panel2);
            this.pnlLeftMenu.Controls.Add(this.btnPodesavanje);
            this.pnlLeftMenu.Controls.Add(this.btnPacijenti);
            this.pnlLeftMenu.Controls.Add(this.btnDashboard);
            this.pnlLeftMenu.Controls.Add(this.label2);
            this.pnlLeftMenu.Location = new System.Drawing.Point(0, 52);
            this.pnlLeftMenu.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.pnlLeftMenu.Name = "pnlLeftMenu";
            this.pnlLeftMenu.Size = new System.Drawing.Size(320, 778);
            this.pnlLeftMenu.TabIndex = 2;
            this.pnlLeftMenu.MouseEnter += new System.EventHandler(this.NavButton_MouseEnter);
            this.pnlLeftMenu.MouseLeave += new System.EventHandler(this.NavButton_MouseLeave);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.panel2.Controls.Add(this.btnOdjaviSe);
            this.panel2.Controls.Add(this.lblPrezimeKorisnika);
            this.panel2.Controls.Add(this.lblImeKorisnika);
            this.panel2.Controls.Add(this.lblTitulaKorisnika);
            this.panel2.Controls.Add(this.iconPictureBox1);
            this.panel2.Controls.Add(this.label4);
            this.panel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.panel2.Location = new System.Drawing.Point(15, 578);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(290, 186);
            this.panel2.TabIndex = 3;
            // 
            // btnOdjaviSe
            // 
            this.btnOdjaviSe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(50)))), ((int)(((byte)(74)))));
            this.btnOdjaviSe.FlatAppearance.BorderSize = 0;
            this.btnOdjaviSe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOdjaviSe.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOdjaviSe.Location = new System.Drawing.Point(5, 136);
            this.btnOdjaviSe.Name = "btnOdjaviSe";
            this.btnOdjaviSe.Size = new System.Drawing.Size(275, 37);
            this.btnOdjaviSe.TabIndex = 9;
            this.btnOdjaviSe.Text = "ODJAVI ME";
            this.btnOdjaviSe.UseVisualStyleBackColor = false;
            this.btnOdjaviSe.Click += new System.EventHandler(this.btnOdjaviSe_Click);
            // 
            // lblPrezimeKorisnika
            // 
            this.lblPrezimeKorisnika.AutoSize = true;
            this.lblPrezimeKorisnika.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrezimeKorisnika.ForeColor = System.Drawing.Color.White;
            this.lblPrezimeKorisnika.Location = new System.Drawing.Point(109, 97);
            this.lblPrezimeKorisnika.Name = "lblPrezimeKorisnika";
            this.lblPrezimeKorisnika.Size = new System.Drawing.Size(91, 25);
            this.lblPrezimeKorisnika.TabIndex = 8;
            this.lblPrezimeKorisnika.Text = "Marković";
            // 
            // lblImeKorisnika
            // 
            this.lblImeKorisnika.AutoSize = true;
            this.lblImeKorisnika.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImeKorisnika.ForeColor = System.Drawing.Color.White;
            this.lblImeKorisnika.Location = new System.Drawing.Point(109, 70);
            this.lblImeKorisnika.Name = "lblImeKorisnika";
            this.lblImeKorisnika.Size = new System.Drawing.Size(72, 25);
            this.lblImeKorisnika.TabIndex = 6;
            this.lblImeKorisnika.Text = "Marko ";
            // 
            // lblTitulaKorisnika
            // 
            this.lblTitulaKorisnika.AutoSize = true;
            this.lblTitulaKorisnika.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulaKorisnika.ForeColor = System.Drawing.Color.White;
            this.lblTitulaKorisnika.Location = new System.Drawing.Point(109, 35);
            this.lblTitulaKorisnika.Name = "lblTitulaKorisnika";
            this.lblTitulaKorisnika.Size = new System.Drawing.Size(58, 25);
            this.lblTitulaKorisnika.TabIndex = 3;
            this.lblTitulaKorisnika.Text = "Lekar";
            // 
            // iconPictureBox1
            // 
            this.iconPictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.iconPictureBox1.ErrorImage = ((System.Drawing.Image)(resources.GetObject("iconPictureBox1.ErrorImage")));
            this.iconPictureBox1.IconChar = FontAwesome.Sharp.IconChar.UserNurse;
            this.iconPictureBox1.IconColor = System.Drawing.Color.White;
            this.iconPictureBox1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconPictureBox1.IconSize = 76;
            this.iconPictureBox1.Location = new System.Drawing.Point(15, 35);
            this.iconPictureBox1.Name = "iconPictureBox1";
            this.iconPictureBox1.Size = new System.Drawing.Size(76, 110);
            this.iconPictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconPictureBox1.TabIndex = 4;
            this.iconPictureBox1.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(11, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 20);
            this.label4.TabIndex = 4;
            this.label4.Text = "KORISNIK";
            // 
            // btnPodesavanje
            // 
            this.btnPodesavanje.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(54)))));
            this.btnPodesavanje.FlatAppearance.BorderSize = 0;
            this.btnPodesavanje.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            this.btnPodesavanje.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            this.btnPodesavanje.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPodesavanje.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPodesavanje.ForeColor = System.Drawing.Color.White;
            this.btnPodesavanje.Image = global::Diplomski.Properties.Resources.settings_24x24;
            this.btnPodesavanje.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPodesavanje.Location = new System.Drawing.Point(15, 153);
            this.btnPodesavanje.Margin = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.btnPodesavanje.Name = "btnPodesavanje";
            this.btnPodesavanje.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.btnPodesavanje.Size = new System.Drawing.Size(290, 48);
            this.btnPodesavanje.TabIndex = 8;
            this.btnPodesavanje.Text = "   Podešavanja";
            this.btnPodesavanje.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPodesavanje.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPodesavanje.UseVisualStyleBackColor = false;
            this.btnPodesavanje.Click += new System.EventHandler(this.NavButton_Click);
            this.btnPodesavanje.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NavButton_Click);
            this.btnPodesavanje.MouseEnter += new System.EventHandler(this.NavButton_MouseEnter);
            this.btnPodesavanje.MouseLeave += new System.EventHandler(this.NavButton_MouseLeave);
            // 
            // btnPacijenti
            // 
            this.btnPacijenti.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(54)))));
            this.btnPacijenti.FlatAppearance.BorderSize = 0;
            this.btnPacijenti.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            this.btnPacijenti.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            this.btnPacijenti.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPacijenti.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPacijenti.ForeColor = System.Drawing.Color.White;
            this.btnPacijenti.Image = global::Diplomski.Properties.Resources.user_24x24;
            this.btnPacijenti.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPacijenti.Location = new System.Drawing.Point(15, 105);
            this.btnPacijenti.Margin = new System.Windows.Forms.Padding(15, 0, 25, 0);
            this.btnPacijenti.Name = "btnPacijenti";
            this.btnPacijenti.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.btnPacijenti.Size = new System.Drawing.Size(280, 48);
            this.btnPacijenti.TabIndex = 4;
            this.btnPacijenti.Text = "   Pacijenti";
            this.btnPacijenti.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPacijenti.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPacijenti.UseVisualStyleBackColor = false;
            this.btnPacijenti.Click += new System.EventHandler(this.NavButton_Click);
            this.btnPacijenti.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NavButton_Click);
            this.btnPacijenti.MouseEnter += new System.EventHandler(this.NavButton_MouseEnter);
            this.btnPacijenti.MouseLeave += new System.EventHandler(this.NavButton_MouseLeave);
            // 
            // btnDashboard
            // 
            this.btnDashboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(54)))));
            this.btnDashboard.FlatAppearance.BorderSize = 0;
            this.btnDashboard.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            this.btnDashboard.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            this.btnDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashboard.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDashboard.ForeColor = System.Drawing.Color.White;
            this.btnDashboard.Image = global::Diplomski.Properties.Resources.home_24x24;
            this.btnDashboard.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDashboard.Location = new System.Drawing.Point(15, 57);
            this.btnDashboard.Margin = new System.Windows.Forms.Padding(15, 0, 15, 0);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.btnDashboard.Size = new System.Drawing.Size(290, 48);
            this.btnDashboard.TabIndex = 3;
            this.btnDashboard.Text = "   Dashboard";
            this.btnDashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDashboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDashboard.UseVisualStyleBackColor = false;
            this.btnDashboard.Click += new System.EventHandler(this.NavButton_Click);
            this.btnDashboard.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NavButton_Click);
            this.btnDashboard.MouseEnter += new System.EventHandler(this.NavButton_MouseEnter);
            this.btnDashboard.MouseLeave += new System.EventHandler(this.NavButton_MouseLeave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.label2.Location = new System.Drawing.Point(15, 17);
            this.label2.Margin = new System.Windows.Forms.Padding(15, 21, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "GLAVNI MENI";
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.lblVreme);
            this.panel3.Controls.Add(this.lblVerzija);
            this.panel3.Location = new System.Drawing.Point(3, 836);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1788, 29);
            this.panel3.TabIndex = 3;
            // 
            // lblVreme
            // 
            this.lblVreme.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVreme.AutoSize = true;
            this.lblVreme.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVreme.ForeColor = System.Drawing.Color.White;
            this.lblVreme.Location = new System.Drawing.Point(1598, 4);
            this.lblVreme.Name = "lblVreme";
            this.lblVreme.Size = new System.Drawing.Size(125, 23);
            this.lblVreme.TabIndex = 5;
            this.lblVreme.Text = "datum+vreme";
            // 
            // lblVerzija
            // 
            this.lblVerzija.AutoSize = true;
            this.lblVerzija.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVerzija.ForeColor = System.Drawing.Color.White;
            this.lblVerzija.Location = new System.Drawing.Point(853, 4);
            this.lblVerzija.Name = "lblVerzija";
            this.lblVerzija.Size = new System.Drawing.Size(108, 23);
            this.lblVerzija.TabIndex = 4;
            this.lblVerzija.Text = "Verzija 1.0.0";
            this.lblVerzija.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // panelAktivniPacijent
            // 
            this.panelAktivniPacijent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAktivniPacijent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(54)))));
            this.panelAktivniPacijent.Controls.Add(this.btnPretraziPacijenta);
            this.panelAktivniPacijent.Location = new System.Drawing.Point(335, 52);
            this.panelAktivniPacijent.Name = "panelAktivniPacijent";
            this.panelAktivniPacijent.Size = new System.Drawing.Size(1442, 176);
            this.panelAktivniPacijent.TabIndex = 5;
            this.panelAktivniPacijent.Visible = false;
            // 
            // btnPretraziPacijenta
            // 
            this.btnPretraziPacijenta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnPretraziPacijenta.FlatAppearance.BorderSize = 0;
            this.btnPretraziPacijenta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPretraziPacijenta.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPretraziPacijenta.ForeColor = System.Drawing.Color.White;
            this.btnPretraziPacijenta.Image = global::Diplomski.Properties.Resources.add_user;
            this.btnPretraziPacijenta.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPretraziPacijenta.Location = new System.Drawing.Point(22, 17);
            this.btnPretraziPacijenta.Name = "btnPretraziPacijenta";
            this.btnPretraziPacijenta.Size = new System.Drawing.Size(204, 44);
            this.btnPretraziPacijenta.TabIndex = 1;
            this.btnPretraziPacijenta.Text = "Pretraži pacijenta";
            this.btnPretraziPacijenta.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPretraziPacijenta.UseVisualStyleBackColor = false;
            this.btnPretraziPacijenta.Click += new System.EventHandler(this.btnPretraziPacijenta_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(26)))), ((int)(((byte)(38)))));
            this.ClientSize = new System.Drawing.Size(1794, 865);
            this.Controls.Add(this.panelAktivniPacijent);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.pnlLeftMenu);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.Text = "MainForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnlLeftMenu.ResumeLayout(false);
            this.pnlLeftMenu.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox1)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panelAktivniPacijent.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlLeftMenu;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button btnPacijenti;
        private System.Windows.Forms.Button btnPodesavanje;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTitulaKorisnika;
        private FontAwesome.Sharp.IconPictureBox iconPictureBox1;
        private System.Windows.Forms.Label lblImeKorisnika;
        private System.Windows.Forms.Label lblPrezimeKorisnika;
        private System.Windows.Forms.Button btnOdjaviSe;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblVerzija;
        private System.Windows.Forms.Label lblVreme;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel panelAktivniPacijent;
        private System.Windows.Forms.Button btnPretraziPacijenta;
    }
}