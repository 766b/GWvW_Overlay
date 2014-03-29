namespace GWvW_Overlay
{
    partial class ColorDisplayApplet
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.line1 = new System.Windows.Forms.Label();
            this.line2 = new System.Windows.Forms.Label();
            this.line3 = new System.Windows.Forms.Label();
            this.line4 = new System.Windows.Forms.Label();
            this.line5 = new System.Windows.Forms.Label();
            this.line6 = new System.Windows.Forms.Label();
            this.tabs = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // line1
            // 
            this.line1.AutoSize = true;
            this.line1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.line1.ForeColor = System.Drawing.Color.White;
            this.line1.Location = new System.Drawing.Point(3, 28);
            this.line1.Name = "line1";
            this.line1.Size = new System.Drawing.Size(42, 20);
            this.line1.TabIndex = 1;
            this.line1.Text = "line1";
            // 
            // label2
            // 
            this.line2.AutoSize = true;
            this.line2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.line2.ForeColor = System.Drawing.Color.White;
            this.line2.Location = new System.Drawing.Point(3, 63);
            this.line2.Name = "line2";
            this.line2.Size = new System.Drawing.Size(42, 20);
            this.line2.TabIndex = 2;
            this.line2.Text = "line2";
            // 
            // label3
            // 
            this.line3.AutoSize = true;
            this.line3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.line3.ForeColor = System.Drawing.Color.White;
            this.line3.Location = new System.Drawing.Point(3, 98);
            this.line3.Name = "line3";
            this.line3.Size = new System.Drawing.Size(42, 20);
            this.line3.TabIndex = 3;
            this.line3.Text = "line3";
            // 
            // label4
            // 
            this.line4.AutoSize = true;
            this.line4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.line4.ForeColor = System.Drawing.Color.White;
            this.line4.Location = new System.Drawing.Point(3, 133);
            this.line4.Name = "line4";
            this.line4.Size = new System.Drawing.Size(42, 20);
            this.line4.TabIndex = 4;
            this.line4.Text = "line4";
            // 
            // label5
            // 
            this.line5.AutoSize = true;
            this.line5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.line5.ForeColor = System.Drawing.Color.White;
            this.line5.Location = new System.Drawing.Point(3, 168);
            this.line5.Name = "line5";
            this.line5.Size = new System.Drawing.Size(42, 20);
            this.line5.TabIndex = 5;
            this.line5.Text = "line5";
            // 
            // label6
            // 
            this.line6.AutoSize = true;
            this.line6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.line6.ForeColor = System.Drawing.Color.White;
            this.line6.Location = new System.Drawing.Point(3, 203);
            this.line6.Name = "line6";
            this.line6.Size = new System.Drawing.Size(42, 20);
            this.line6.TabIndex = 6;
            this.line6.Text = "line6";
            // 
            // tabs
            // 
            this.tabs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabs.Location = new System.Drawing.Point(0, 3);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(320, 22);
            this.tabs.TabIndex = 7;
            // 
            // ColorDisplayApplet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(57)))));
            this.Controls.Add(this.line6);
            this.Controls.Add(this.line5);
            this.Controls.Add(this.line4);
            this.Controls.Add(this.line3);
            this.Controls.Add(this.line2);
            this.Controls.Add(this.line1);
            this.Controls.Add(this.tabs);
            this.Name = "ColorDisplayApplet";
            this.Size = new System.Drawing.Size(320, 240);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label line1;
        private System.Windows.Forms.Label line2;
        private System.Windows.Forms.Label line3;
        private System.Windows.Forms.Label line4;
        private System.Windows.Forms.Label line5;
        private System.Windows.Forms.Label line6;
        private System.Windows.Forms.TabControl tabs;

    }
}
