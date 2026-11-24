namespace Diplomski.Forms
{
    partial class PacijentSearchForm
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
            this.dgvPacijenti = new System.Windows.Forms.DataGridView();
            this.txtPretraga = new System.Windows.Forms.TextBox();
            this.btnDodajNovogPacijenta = new System.Windows.Forms.Button();
            this.btnDodajPacijenta = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPacijenti)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvPacijenti
            // 
            this.dgvPacijenti.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPacijenti.Location = new System.Drawing.Point(15, 50);
            this.dgvPacijenti.Name = "dgvPacijenti";
            this.dgvPacijenti.RowHeadersWidth = 51;
            this.dgvPacijenti.RowTemplate.Height = 24;
            this.dgvPacijenti.Size = new System.Drawing.Size(580, 349);
            this.dgvPacijenti.TabIndex = 0;
            // 
            // txtPretraga
            // 
            this.txtPretraga.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPretraga.Location = new System.Drawing.Point(15, 14);
            this.txtPretraga.Name = "txtPretraga";
            this.txtPretraga.Size = new System.Drawing.Size(580, 30);
            this.txtPretraga.TabIndex = 1;
            // 
            // btnDodajNovogPacijenta
            // 
            this.btnDodajNovogPacijenta.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnDodajNovogPacijenta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(15)))), ((int)(((byte)(23)))));
            this.btnDodajNovogPacijenta.FlatAppearance.BorderSize = 0;
            this.btnDodajNovogPacijenta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDodajNovogPacijenta.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDodajNovogPacijenta.ForeColor = System.Drawing.Color.Cornsilk;
            this.btnDodajNovogPacijenta.Location = new System.Drawing.Point(245, 585);
            this.btnDodajNovogPacijenta.Name = "btnDodajNovogPacijenta";
            this.btnDodajNovogPacijenta.Size = new System.Drawing.Size(260, 33);
            this.btnDodajNovogPacijenta.TabIndex = 2;
            this.btnDodajNovogPacijenta.Text = "Dodaj novog pacijenta";
            this.btnDodajNovogPacijenta.UseVisualStyleBackColor = false;
            // 
            // btnDodajPacijenta
            // 
            this.btnDodajPacijenta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(34)))), ((int)(((byte)(49)))));
            this.btnDodajPacijenta.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDodajPacijenta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(150)))), ((int)(((byte)(165)))));
            this.btnDodajPacijenta.Location = new System.Drawing.Point(172, 405);
            this.btnDodajPacijenta.Name = "btnDodajPacijenta";
            this.btnDodajPacijenta.Size = new System.Drawing.Size(256, 53);
            this.btnDodajPacijenta.TabIndex = 3;
            this.btnDodajPacijenta.Text = "Dodaj pacijenta";
            this.btnDodajPacijenta.UseVisualStyleBackColor = false;
            this.btnDodajPacijenta.Click += new System.EventHandler(this.btnDodajPacijenta_Click);
            // 
            // PacijentSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 521);
            this.Controls.Add(this.btnDodajPacijenta);
            this.Controls.Add(this.btnDodajNovogPacijenta);
            this.Controls.Add(this.txtPretraga);
            this.Controls.Add(this.dgvPacijenti);
            this.Name = "PacijentSearchForm";
            this.Text = "PacijentSearchForm";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPacijenti)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvPacijenti;
        private System.Windows.Forms.TextBox txtPretraga;
        private System.Windows.Forms.Button btnDodajNovogPacijenta;
        private System.Windows.Forms.Button btnDodajPacijenta;
    }
}