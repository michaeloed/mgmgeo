namespace MyGeocachingManager.HMI
{
    partial class Diagnostics
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
            this.lvDiagnostics = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // lvDiagnostics
            // 
            this.lvDiagnostics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvDiagnostics.FullRowSelect = true;
            this.lvDiagnostics.Location = new System.Drawing.Point(0, 0);
            this.lvDiagnostics.Name = "lvDiagnostics";
            this.lvDiagnostics.Size = new System.Drawing.Size(719, 667);
            this.lvDiagnostics.TabIndex = 0;
            this.lvDiagnostics.UseCompatibleStateImageBehavior = false;
            this.lvDiagnostics.View = System.Windows.Forms.View.Details;
            this.lvDiagnostics.DoubleClick += new System.EventHandler(this.lvDiagnostics_DoubleClick);
            // 
            // Diagnostics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 667);
            this.Controls.Add(this.lvDiagnostics);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "Diagnostics";
            this.Text = "#Diagnostics";
            this.Load += new System.EventHandler(this.Diagnostics_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvDiagnostics;
    }
}