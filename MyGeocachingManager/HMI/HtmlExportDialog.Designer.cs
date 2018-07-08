namespace MyGeocachingManager
{
    partial class HtmlExportDialog
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
            this.radioButton2htmlexport = new System.Windows.Forms.RadioButton();
            this.button2 = new System.Windows.Forms.Button();
            this.radioButton1htmlexport = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButton3htmlexport = new System.Windows.Forms.RadioButton();
            this.radioButton4htmlexport = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButton2htmlexport
            // 
            this.radioButton2htmlexport.AutoSize = true;
            this.radioButton2htmlexport.Checked = true;
            this.radioButton2htmlexport.Location = new System.Drawing.Point(6, 37);
            this.radioButton2htmlexport.Name = "radioButton2htmlexport";
            this.radioButton2htmlexport.Size = new System.Drawing.Size(85, 17);
            this.radioButton2htmlexport.TabIndex = 1;
            this.radioButton2htmlexport.TabStop = true;
            this.radioButton2htmlexport.Text = "radioButton2";
            this.radioButton2htmlexport.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(208, 179);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // radioButton1htmlexport
            // 
            this.radioButton1htmlexport.AutoSize = true;
            this.radioButton1htmlexport.Location = new System.Drawing.Point(6, 19);
            this.radioButton1htmlexport.Name = "radioButton1htmlexport";
            this.radioButton1htmlexport.Size = new System.Drawing.Size(85, 17);
            this.radioButton1htmlexport.TabIndex = 0;
            this.radioButton1htmlexport.Text = "radioButton1";
            this.radioButton1htmlexport.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton2htmlexport);
            this.groupBox1.Controls.Add(this.radioButton1htmlexport);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(271, 68);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(127, 179);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButton3htmlexport);
            this.groupBox2.Controls.Add(this.radioButton4htmlexport);
            this.groupBox2.Location = new System.Drawing.Point(12, 86);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(271, 68);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // radioButton3htmlexport
            // 
            this.radioButton3htmlexport.AutoSize = true;
            this.radioButton3htmlexport.Location = new System.Drawing.Point(6, 37);
            this.radioButton3htmlexport.Name = "radioButton3htmlexport";
            this.radioButton3htmlexport.Size = new System.Drawing.Size(85, 17);
            this.radioButton3htmlexport.TabIndex = 1;
            this.radioButton3htmlexport.Text = "radioButton3";
            this.radioButton3htmlexport.UseVisualStyleBackColor = true;
            // 
            // radioButton4htmlexport
            // 
            this.radioButton4htmlexport.AutoSize = true;
            this.radioButton4htmlexport.Checked = true;
            this.radioButton4htmlexport.Location = new System.Drawing.Point(6, 19);
            this.radioButton4htmlexport.Name = "radioButton4htmlexport";
            this.radioButton4htmlexport.Size = new System.Drawing.Size(85, 17);
            this.radioButton4htmlexport.TabIndex = 0;
            this.radioButton4htmlexport.TabStop = true;
            this.radioButton4htmlexport.Text = "radioButton4";
            this.radioButton4htmlexport.UseVisualStyleBackColor = true;
            // 
            // HtmlExportDialog
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(295, 214);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
            this.Name = "HtmlExportDialog";
            this.Text = "HtmlExportDialog";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion


        /// <summary>
        /// Show cache description : no
        /// </summary>
        public System.Windows.Forms.RadioButton radioButton2htmlexport;

        /// <summary>
        /// Cancel button
        /// </summary>
        public System.Windows.Forms.Button button2;

        /// <summary>
        /// Show cache description : yes
        /// </summary>
        public System.Windows.Forms.RadioButton radioButton1htmlexport;

        /// <summary>
        /// Show cache description
        /// </summary>
        public System.Windows.Forms.GroupBox groupBox1;

        /// <summary>
        /// OK button
        /// </summary>
        public System.Windows.Forms.Button button1;

        /// <summary>
        /// Create a map of caches
        /// </summary>
        public System.Windows.Forms.GroupBox groupBox2;

        /// <summary>
        /// Create a map of caches : no
        /// </summary>
        public System.Windows.Forms.RadioButton radioButton3htmlexport;

        /// <summary>
        /// Create a map of caches : yes
        /// </summary>
        public System.Windows.Forms.RadioButton radioButton4htmlexport;
    }
}