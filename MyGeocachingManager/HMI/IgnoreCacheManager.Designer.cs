namespace MyGeocachingManager
{
    partial class IgnoreCacheManager
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
            this.btnRemoveFromIgnoreList = new System.Windows.Forms.Button();
            this.lvIgnoredCaches = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // btnRemoveFromIgnoreList
            // 
            this.btnRemoveFromIgnoreList.Location = new System.Drawing.Point(12, 2);
            this.btnRemoveFromIgnoreList.Name = "btnRemoveFromIgnoreList";
            this.btnRemoveFromIgnoreList.Size = new System.Drawing.Size(381, 23);
            this.btnRemoveFromIgnoreList.TabIndex = 1;
            this.btnRemoveFromIgnoreList.Text = "#removefromignorelist";
            this.btnRemoveFromIgnoreList.UseVisualStyleBackColor = true;
            this.btnRemoveFromIgnoreList.Click += new System.EventHandler(this.btnRemoveFromIgnoreList_Click);
            // 
            // lvIgnoredCaches
            // 
            this.lvIgnoredCaches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvIgnoredCaches.FullRowSelect = true;
            this.lvIgnoredCaches.Location = new System.Drawing.Point(12, 31);
            this.lvIgnoredCaches.Name = "lvIgnoredCaches";
            this.lvIgnoredCaches.Size = new System.Drawing.Size(753, 301);
            this.lvIgnoredCaches.TabIndex = 2;
            this.lvIgnoredCaches.UseCompatibleStateImageBehavior = false;
            this.lvIgnoredCaches.View = System.Windows.Forms.View.Details;
            // 
            // IgnoreCacheManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(777, 344);
            this.Controls.Add(this.lvIgnoredCaches);
            this.Controls.Add(this.btnRemoveFromIgnoreList);
            this.Name = "IgnoreCacheManager";
            this.Text = "#IgnoreCacheManager";
            this.Load += new System.EventHandler(this.IgnoreCacheManager_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRemoveFromIgnoreList;
        private System.Windows.Forms.ListView lvIgnoredCaches;
    }
}