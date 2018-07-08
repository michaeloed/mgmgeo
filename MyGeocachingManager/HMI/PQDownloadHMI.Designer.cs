namespace MyGeocachingManager
{
    partial class PQDownloadHMI
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
        	this.listView1pqdownloader = new System.Windows.Forms.ListView();
        	this.menuStrip1 = new System.Windows.Forms.MenuStrip();
        	this.fMenuLiveDownloadPQToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.openPQFolderToolStripMenuItempqmgr = new System.Windows.Forms.ToolStripMenuItem();
        	this.checkAllToolStripMenuItempqmgr = new System.Windows.Forms.ToolStripMenuItem();
        	this.uncheckAllToolStripMenuItempqmgr = new System.Windows.Forms.ToolStripMenuItem();
        	this.downloadAllToolStripMenuItempqmgr = new System.Windows.Forms.ToolStripMenuItem();
        	this.downloadUpdatedToolStripMenuItempqmgr = new System.Windows.Forms.ToolStripMenuItem();
        	this.menuStrip1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// listView1pqdownloader
        	// 
        	this.listView1pqdownloader.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
        	this.listView1pqdownloader.CheckBoxes = true;
        	this.listView1pqdownloader.FullRowSelect = true;
        	this.listView1pqdownloader.Location = new System.Drawing.Point(12, 27);
        	this.listView1pqdownloader.Name = "listView1pqdownloader";
        	this.listView1pqdownloader.Size = new System.Drawing.Size(755, 312);
        	this.listView1pqdownloader.TabIndex = 0;
        	this.listView1pqdownloader.UseCompatibleStateImageBehavior = false;
        	this.listView1pqdownloader.View = System.Windows.Forms.View.Details;
        	// 
        	// menuStrip1
        	// 
        	this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fMenuLiveDownloadPQToolStripMenuItem});
        	this.menuStrip1.Location = new System.Drawing.Point(0, 0);
        	this.menuStrip1.Name = "menuStrip1";
        	this.menuStrip1.Size = new System.Drawing.Size(779, 24);
        	this.menuStrip1.TabIndex = 9;
        	this.menuStrip1.Text = "menuStrip1";
        	// 
        	// fMenuLiveDownloadPQToolStripMenuItem
        	// 
        	this.fMenuLiveDownloadPQToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.openPQFolderToolStripMenuItempqmgr,
			this.checkAllToolStripMenuItempqmgr,
			this.uncheckAllToolStripMenuItempqmgr,
			this.downloadAllToolStripMenuItempqmgr,
			this.downloadUpdatedToolStripMenuItempqmgr});
        	this.fMenuLiveDownloadPQToolStripMenuItem.Name = "fMenuLiveDownloadPQToolStripMenuItem";
        	this.fMenuLiveDownloadPQToolStripMenuItem.Size = new System.Drawing.Size(147, 20);
        	this.fMenuLiveDownloadPQToolStripMenuItem.Text = "FMenuLiveDownloadPQ";
        	// 
        	// openPQFolderToolStripMenuItempqmgr
        	// 
        	this.openPQFolderToolStripMenuItempqmgr.Name = "openPQFolderToolStripMenuItempqmgr";
        	this.openPQFolderToolStripMenuItempqmgr.Size = new System.Drawing.Size(175, 22);
        	this.openPQFolderToolStripMenuItempqmgr.Text = "#Open PQ folder";
        	this.openPQFolderToolStripMenuItempqmgr.Click += new System.EventHandler(this.openPQFolderToolStripMenuItem_Click);
        	// 
        	// checkAllToolStripMenuItempqmgr
        	// 
        	this.checkAllToolStripMenuItempqmgr.Name = "checkAllToolStripMenuItempqmgr";
        	this.checkAllToolStripMenuItempqmgr.Size = new System.Drawing.Size(175, 22);
        	this.checkAllToolStripMenuItempqmgr.Text = "Check all";
        	this.checkAllToolStripMenuItempqmgr.Click += new System.EventHandler(this.checkAllToolStripMenuItem_Click);
        	// 
        	// uncheckAllToolStripMenuItempqmgr
        	// 
        	this.uncheckAllToolStripMenuItempqmgr.Name = "uncheckAllToolStripMenuItempqmgr";
        	this.uncheckAllToolStripMenuItempqmgr.Size = new System.Drawing.Size(175, 22);
        	this.uncheckAllToolStripMenuItempqmgr.Text = "Uncheck all";
        	this.uncheckAllToolStripMenuItempqmgr.Click += new System.EventHandler(this.uncheckAllToolStripMenuItem_Click);
        	// 
        	// downloadAllToolStripMenuItempqmgr
        	// 
        	this.downloadAllToolStripMenuItempqmgr.Name = "downloadAllToolStripMenuItempqmgr";
        	this.downloadAllToolStripMenuItempqmgr.Size = new System.Drawing.Size(175, 22);
        	this.downloadAllToolStripMenuItempqmgr.Text = "Download all";
        	this.downloadAllToolStripMenuItempqmgr.Click += new System.EventHandler(this.downloadAllToolStripMenuItem_Click);
        	// 
        	// downloadUpdatedToolStripMenuItempqmgr
        	// 
        	this.downloadUpdatedToolStripMenuItempqmgr.Name = "downloadUpdatedToolStripMenuItempqmgr";
        	this.downloadUpdatedToolStripMenuItempqmgr.Size = new System.Drawing.Size(175, 22);
        	this.downloadUpdatedToolStripMenuItempqmgr.Text = "Download updated";
        	this.downloadUpdatedToolStripMenuItempqmgr.Click += new System.EventHandler(this.downloadUpdatedToolStripMenuItem_Click);
        	// 
        	// PQDownloadHMI
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(779, 351);
        	this.Controls.Add(this.listView1pqdownloader);
        	this.Controls.Add(this.menuStrip1);
        	this.MainMenuStrip = this.menuStrip1;
        	this.Name = "PQDownloadHMI";
        	this.Text = "PQ Downloader";
        	this.Load += new System.EventHandler(this.PQDownloadHMILoad);
        	this.menuStrip1.ResumeLayout(false);
        	this.menuStrip1.PerformLayout();
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1pqdownloader;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fMenuLiveDownloadPQToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkAllToolStripMenuItempqmgr;
        private System.Windows.Forms.ToolStripMenuItem uncheckAllToolStripMenuItempqmgr;
        private System.Windows.Forms.ToolStripMenuItem downloadAllToolStripMenuItempqmgr;
        private System.Windows.Forms.ToolStripMenuItem downloadUpdatedToolStripMenuItempqmgr;
        private System.Windows.Forms.ToolStripMenuItem openPQFolderToolStripMenuItempqmgr;
    }
}