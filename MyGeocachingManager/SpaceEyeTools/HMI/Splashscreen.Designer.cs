namespace SpaceEyeTools.HMI
{
    partial class Splashscreen
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
        	this.pBox = new System.Windows.Forms.PictureBox();
        	this.lblName = new System.Windows.Forms.Label();
        	this.lblVersion = new System.Windows.Forms.Label();
        	this.lblInfo = new System.Windows.Forms.Label();
        	this.progressBar = new System.Windows.Forms.ProgressBar();
        	this.lblExtraInfo = new System.Windows.Forms.Label();
        	((System.ComponentModel.ISupportInitialize)(this.pBox)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// pBox
        	// 
        	this.pBox.Location = new System.Drawing.Point(0, 0);
        	this.pBox.Name = "pBox";
        	this.pBox.Size = new System.Drawing.Size(638, 380);
        	this.pBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        	this.pBox.TabIndex = 0;
        	this.pBox.TabStop = false;
        	// 
        	// lblName
        	// 
        	this.lblName.AutoSize = true;
        	this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblName.Location = new System.Drawing.Point(12, 9);
        	this.lblName.Name = "lblName";
        	this.lblName.Size = new System.Drawing.Size(324, 55);
        	this.lblName.TabIndex = 1;
        	this.lblName.Text = "Product name";
        	this.lblName.Visible = false;
        	// 
        	// lblVersion
        	// 
        	this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblVersion.Location = new System.Drawing.Point(12, 335);
        	this.lblVersion.Name = "lblVersion";
        	this.lblVersion.Size = new System.Drawing.Size(616, 37);
        	this.lblVersion.TabIndex = 2;
        	this.lblVersion.Text = "Version x.y.z.a";
        	this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        	// 
        	// lblInfo
        	// 
        	this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblInfo.Location = new System.Drawing.Point(12, 274);
        	this.lblInfo.Name = "lblInfo";
        	this.lblInfo.Size = new System.Drawing.Size(616, 25);
        	this.lblInfo.TabIndex = 3;
        	this.lblInfo.Text = "foo fii fuu";
        	this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        	// 
        	// progressBar
        	// 
        	this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
        	this.progressBar.Location = new System.Drawing.Point(11, 312);
        	this.progressBar.Margin = new System.Windows.Forms.Padding(0);
        	this.progressBar.Name = "progressBar";
        	this.progressBar.Size = new System.Drawing.Size(616, 23);
        	this.progressBar.Step = 1;
        	this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
        	this.progressBar.TabIndex = 4;
        	// 
        	// lblExtraInfo
        	// 
        	this.lblExtraInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblExtraInfo.Location = new System.Drawing.Point(379, 12);
        	this.lblExtraInfo.Name = "lblExtraInfo";
        	this.lblExtraInfo.Size = new System.Drawing.Size(236, 25);
        	this.lblExtraInfo.TabIndex = 5;
        	this.lblExtraInfo.Text = "foo fii fuu";
        	this.lblExtraInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        	// 
        	// Splashscreen
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(638, 380);
        	this.ControlBox = false;
        	this.Controls.Add(this.lblExtraInfo);
        	this.Controls.Add(this.progressBar);
        	this.Controls.Add(this.lblInfo);
        	this.Controls.Add(this.lblVersion);
        	this.Controls.Add(this.lblName);
        	this.Controls.Add(this.pBox);
        	this.DoubleBuffered = true;
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        	this.MaximizeBox = false;
        	this.MaximumSize = new System.Drawing.Size(640, 382);
        	this.MinimizeBox = false;
        	this.MinimumSize = new System.Drawing.Size(638, 380);
        	this.Name = "Splashscreen";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "Splashscreen";
        	this.Load += new System.EventHandler(this.Splashscreen_Load);
        	((System.ComponentModel.ISupportInitialize)(this.pBox)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pBox;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblExtraInfo;
    }
}