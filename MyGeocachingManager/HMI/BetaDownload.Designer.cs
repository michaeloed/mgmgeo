namespace MyGeocachingManager.HMI
{
    partial class BetaDownload
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
            this.listViewBeta = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // listViewBeta
            // 
            this.listViewBeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewBeta.Location = new System.Drawing.Point(0, 0);
            this.listViewBeta.Name = "listViewBeta";
            this.listViewBeta.Size = new System.Drawing.Size(502, 257);
            this.listViewBeta.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listViewBeta.TabIndex = 0;
            this.listViewBeta.UseCompatibleStateImageBehavior = false;
            this.listViewBeta.View = System.Windows.Forms.View.Details;
            this.listViewBeta.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewBeta_MouseDoubleClick);
            // 
            // BetaDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 257);
            this.Controls.Add(this.listViewBeta);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BetaDownload";
            this.Text = "#BetaDownload";
            this.Load += new System.EventHandler(this.BetaDownload_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewBeta;
    }
}