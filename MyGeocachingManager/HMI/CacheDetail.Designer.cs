namespace MyGeocachingManager
{
    partial class CacheDetail
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
            this.tabControlCD = new MyGeocachingManager.TabControlEx(_daddy);
            this.SuspendLayout();
            // 
            // tabControlCD
            // 
            this.tabControlCD.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlCD.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControlCD.Location = new System.Drawing.Point(-1, 0);
            this.tabControlCD.Name = "tabControlCD";
            this.tabControlCD.Padding = new System.Drawing.Point(22, 3);
            this.tabControlCD.SelectedIndex = 0;
            this.tabControlCD.ShowToolTips = true;
            this.tabControlCD.Size = new System.Drawing.Size(1013, 731);
            this.tabControlCD.TabIndex = 0;
            this.tabControlCD.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tabControlCD_MouseDoubleClick);
            // 
            // CacheDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.tabControlCD);
            this.Name = "CacheDetail";
            this.Text = "Cache Detail";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CacheDetail_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion


        /// <summary>
        /// Custom tab control element, refers. to TabControlEx
        /// </summary>
        public MyGeocachingManager.TabControlEx tabControlCD;
    }
}