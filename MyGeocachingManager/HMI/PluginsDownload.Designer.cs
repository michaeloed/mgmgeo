namespace MyGeocachingManager.HMI
{
    partial class PluginsDownload
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
            this.listViewPluginsDownload = new System.Windows.Forms.ListView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fMenuPluginsIHMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLocalPluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLocalPluginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewPluginsDownload
            // 
            this.listViewPluginsDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewPluginsDownload.FullRowSelect = true;
            this.listViewPluginsDownload.Location = new System.Drawing.Point(0, 24);
            this.listViewPluginsDownload.Name = "listViewPluginsDownload";
            this.listViewPluginsDownload.Size = new System.Drawing.Size(773, 233);
            this.listViewPluginsDownload.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listViewPluginsDownload.TabIndex = 0;
            this.listViewPluginsDownload.UseCompatibleStateImageBehavior = false;
            this.listViewPluginsDownload.View = System.Windows.Forms.View.Details;
            this.listViewPluginsDownload.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewBeta_MouseDoubleClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fMenuPluginsIHMToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(773, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fMenuPluginsIHMToolStripMenuItem
            // 
            this.fMenuPluginsIHMToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteLocalPluginsToolStripMenuItem,
            this.openLocalPluginToolStripMenuItem});
            this.fMenuPluginsIHMToolStripMenuItem.Name = "fMenuPluginsIHMToolStripMenuItem";
            this.fMenuPluginsIHMToolStripMenuItem.Size = new System.Drawing.Size(118, 20);
            this.fMenuPluginsIHMToolStripMenuItem.Text = "FMenuPluginsIHM";
            // 
            // deleteLocalPluginsToolStripMenuItem
            // 
            this.deleteLocalPluginsToolStripMenuItem.Name = "deleteLocalPluginsToolStripMenuItem";
            this.deleteLocalPluginsToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.deleteLocalPluginsToolStripMenuItem.Text = "#Delete local plugins";
            this.deleteLocalPluginsToolStripMenuItem.Click += new System.EventHandler(this.deleteLocalPluginsToolStripMenuItem_Click);
            // 
            // openLocalPluginToolStripMenuItem
            // 
            this.openLocalPluginToolStripMenuItem.Name = "openLocalPluginToolStripMenuItem";
            this.openLocalPluginToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.openLocalPluginToolStripMenuItem.Text = "#Open local plugin rep";
            this.openLocalPluginToolStripMenuItem.Click += new System.EventHandler(this.openLocalPluginToolStripMenuItem_Click);
            // 
            // PluginsDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(773, 257);
            this.Controls.Add(this.listViewPluginsDownload);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginsDownload";
            this.Text = "#PluginsDownload";
            this.Load += new System.EventHandler(this.PluginsDownload_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewPluginsDownload;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fMenuPluginsIHMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteLocalPluginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLocalPluginToolStripMenuItem;
    }
}