﻿namespace SpaceEyeTools.HMI
{
    partial class MyMessageBox
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

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        	this.button1 = new System.Windows.Forms.Button();
        	this.pictureBox1 = new System.Windows.Forms.PictureBox();
        	this.textBox1 = new System.Windows.Forms.TextBox();
        	this.button2 = new System.Windows.Forms.Button();
        	this.pictureBox2 = new System.Windows.Forms.PictureBox();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// button1
        	// 
        	this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.button1.Location = new System.Drawing.Point(118, 191);
        	this.button1.Name = "button1";
        	this.button1.Size = new System.Drawing.Size(90, 23);
        	this.button1.TabIndex = 0;
        	this.button1.Text = "OK";
        	this.button1.UseVisualStyleBackColor = true;
        	this.button1.Click += new System.EventHandler(this.button1_Click);
        	// 
        	// pictureBox1
        	// 
        	this.pictureBox1.Location = new System.Drawing.Point(3, 78);
        	this.pictureBox1.Name = "pictureBox1";
        	this.pictureBox1.Size = new System.Drawing.Size(32, 32);
        	this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        	this.pictureBox1.TabIndex = 1;
        	this.pictureBox1.TabStop = false;
        	// 
        	// textBox1
        	// 
        	this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
        	this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.textBox1.Location = new System.Drawing.Point(41, 3);
        	this.textBox1.Multiline = true;
        	this.textBox1.Name = "textBox1";
        	this.textBox1.ReadOnly = true;
        	this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        	this.textBox1.Size = new System.Drawing.Size(386, 154);
        	this.textBox1.TabIndex = 2;
        	// 
        	// button2
        	// 
        	this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.button2.Location = new System.Drawing.Point(228, 191);
        	this.button2.Name = "button2";
        	this.button2.Size = new System.Drawing.Size(90, 23);
        	this.button2.TabIndex = 3;
        	this.button2.Text = "Cancel";
        	this.button2.UseVisualStyleBackColor = true;
        	this.button2.Click += new System.EventHandler(this.button2_Click);
        	// 
        	// pictureBox2
        	// 
        	this.pictureBox2.Location = new System.Drawing.Point(59, 102);
        	this.pictureBox2.Name = "pictureBox2";
        	this.pictureBox2.Size = new System.Drawing.Size(32, 32);
        	this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        	this.pictureBox2.TabIndex = 4;
        	this.pictureBox2.TabStop = false;
        	// 
        	// MyMessageBox
        	// 
        	this.AcceptButton = this.button1;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.CancelButton = this.button2;
        	this.ClientSize = new System.Drawing.Size(428, 226);
        	this.Controls.Add(this.textBox1);
        	this.Controls.Add(this.pictureBox2);
        	this.Controls.Add(this.button2);
        	this.Controls.Add(this.pictureBox1);
        	this.Controls.Add(this.button1);
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "MyMessageBox";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        	this.Text = "MyMessageBox";
        	this.SizeChanged += new System.EventHandler(this.MyMessageBox_SizeChanged);
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox pictureBox2;

        /// <summary>
        /// OK button
        /// </summary>
        public System.Windows.Forms.Button button1;

        /// <summary>
        /// Cancel button
        /// </summary>
        public System.Windows.Forms.Button button2;
    }
}