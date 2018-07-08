namespace MyGeocachingManager
{
    partial class FieldNotesHMI
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
        	this.lvCachesfn = new System.Windows.Forms.ListView();
        	this.fnbtnMoveDown = new System.Windows.Forms.Button();
        	this.fnbtnMoveUp = new System.Windows.Forms.Button();
        	this.menuStrip1 = new System.Windows.Forms.MenuStrip();
        	this.importExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.templateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.defineDefaultLogTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.deleteDefaultLogTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
        	this.defineTtemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.applyTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.informationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.completeEmptyCachesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.displaySelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.displayLogStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.selectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.modifyLogSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.copyFieldNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.delSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.logSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.tBsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.travelTBOnSelectedCachesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.statusStrip1 = new System.Windows.Forms.StatusStrip();
        	this.tsSelStatus = new System.Windows.Forms.ToolStripStatusLabel();
        	this.menuStrip1.SuspendLayout();
        	this.statusStrip1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// lvCachesfn
        	// 
        	this.lvCachesfn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
        	this.lvCachesfn.FullRowSelect = true;
        	this.lvCachesfn.GridLines = true;
        	this.lvCachesfn.HideSelection = false;
        	this.lvCachesfn.Location = new System.Drawing.Point(13, 40);
        	this.lvCachesfn.Name = "lvCachesfn";
        	this.lvCachesfn.Size = new System.Drawing.Size(1172, 443);
        	this.lvCachesfn.TabIndex = 0;
        	this.lvCachesfn.UseCompatibleStateImageBehavior = false;
        	this.lvCachesfn.View = System.Windows.Forms.View.Details;
        	this.lvCachesfn.SelectedIndexChanged += new System.EventHandler(this.LvCachesfnSelectedIndexChanged);
        	this.lvCachesfn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LvCachesfnMouseClick);
        	this.lvCachesfn.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvCaches_MouseDoubleClick);
        	// 
        	// fnbtnMoveDown
        	// 
        	this.fnbtnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.fnbtnMoveDown.Image = global::MyGeocachingManager.Properties.Resources.Down;
        	this.fnbtnMoveDown.Location = new System.Drawing.Point(1191, 301);
        	this.fnbtnMoveDown.Name = "fnbtnMoveDown";
        	this.fnbtnMoveDown.Size = new System.Drawing.Size(40, 40);
        	this.fnbtnMoveDown.TabIndex = 7;
        	this.fnbtnMoveDown.UseVisualStyleBackColor = true;
        	this.fnbtnMoveDown.Click += new System.EventHandler(this.fnbtnMoveDown_Click);
        	// 
        	// fnbtnMoveUp
        	// 
        	this.fnbtnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.fnbtnMoveUp.Image = global::MyGeocachingManager.Properties.Resources.Up;
        	this.fnbtnMoveUp.Location = new System.Drawing.Point(1191, 210);
        	this.fnbtnMoveUp.Name = "fnbtnMoveUp";
        	this.fnbtnMoveUp.Size = new System.Drawing.Size(40, 40);
        	this.fnbtnMoveUp.TabIndex = 6;
        	this.fnbtnMoveUp.UseVisualStyleBackColor = true;
        	this.fnbtnMoveUp.Click += new System.EventHandler(this.fnbtnMoveUp_Click);
        	// 
        	// menuStrip1
        	// 
        	this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.importExportToolStripMenuItem,
			this.templateToolStripMenuItem,
			this.informationToolStripMenuItem,
			this.selectionToolStripMenuItem,
			this.tBsToolStripMenuItem});
        	this.menuStrip1.Location = new System.Drawing.Point(0, 0);
        	this.menuStrip1.Name = "menuStrip1";
        	this.menuStrip1.Size = new System.Drawing.Size(1243, 24);
        	this.menuStrip1.TabIndex = 12;
        	this.menuStrip1.Text = "menuStrip1";
        	// 
        	// importExportToolStripMenuItem
        	// 
        	this.importExportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.loadToolStripMenuItem,
			this.saveToolStripMenuItem});
        	this.importExportToolStripMenuItem.Name = "importExportToolStripMenuItem";
        	this.importExportToolStripMenuItem.Size = new System.Drawing.Size(100, 20);
        	this.importExportToolStripMenuItem.Text = "#Import/Export";
        	// 
        	// loadToolStripMenuItem
        	// 
        	this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
        	this.loadToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
        	this.loadToolStripMenuItem.Text = "#Load";
        	this.loadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItemClick);
        	// 
        	// saveToolStripMenuItem
        	// 
        	this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        	this.saveToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
        	this.saveToolStripMenuItem.Text = "#Save";
        	this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItemClick);
        	// 
        	// templateToolStripMenuItem
        	// 
        	this.templateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.defineDefaultLogTextToolStripMenuItem,
			this.deleteDefaultLogTextToolStripMenuItem,
			this.toolStripSeparator1,
			this.defineTtemplateToolStripMenuItem,
			this.applyTemplateToolStripMenuItem});
        	this.templateToolStripMenuItem.Name = "templateToolStripMenuItem";
        	this.templateToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
        	this.templateToolStripMenuItem.Text = "#Template";
        	// 
        	// defineDefaultLogTextToolStripMenuItem
        	// 
        	this.defineDefaultLogTextToolStripMenuItem.Name = "defineDefaultLogTextToolStripMenuItem";
        	this.defineDefaultLogTextToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.defineDefaultLogTextToolStripMenuItem.Text = "#DefineDefaultLogText";
        	this.defineDefaultLogTextToolStripMenuItem.Click += new System.EventHandler(this.DefineDefaultLogTextToolStripMenuItemClick);
        	// 
        	// deleteDefaultLogTextToolStripMenuItem
        	// 
        	this.deleteDefaultLogTextToolStripMenuItem.Name = "deleteDefaultLogTextToolStripMenuItem";
        	this.deleteDefaultLogTextToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.deleteDefaultLogTextToolStripMenuItem.Text = "#DeleteDefaultLogText";
        	this.deleteDefaultLogTextToolStripMenuItem.Click += new System.EventHandler(this.DeleteDefaultLogTextToolStripMenuItemClick);
        	// 
        	// toolStripSeparator1
        	// 
        	this.toolStripSeparator1.Name = "toolStripSeparator1";
        	this.toolStripSeparator1.Size = new System.Drawing.Size(192, 6);
        	// 
        	// defineTtemplateToolStripMenuItem
        	// 
        	this.defineTtemplateToolStripMenuItem.Name = "defineTtemplateToolStripMenuItem";
        	this.defineTtemplateToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.defineTtemplateToolStripMenuItem.Text = "#DefineTemplate";
        	this.defineTtemplateToolStripMenuItem.Click += new System.EventHandler(this.DefineTtemplateToolStripMenuItemClick);
        	// 
        	// applyTemplateToolStripMenuItem
        	// 
        	this.applyTemplateToolStripMenuItem.Name = "applyTemplateToolStripMenuItem";
        	this.applyTemplateToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.applyTemplateToolStripMenuItem.Text = "#ApplyTemplate";
        	this.applyTemplateToolStripMenuItem.Click += new System.EventHandler(this.ApplyTemplateToolStripMenuItemClick);
        	// 
        	// informationToolStripMenuItem
        	// 
        	this.informationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.completeEmptyCachesToolStripMenuItem,
			this.displaySelectedToolStripMenuItem,
			this.displayLogStatusToolStripMenuItem});
        	this.informationToolStripMenuItem.Name = "informationToolStripMenuItem";
        	this.informationToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
        	this.informationToolStripMenuItem.Text = "#Information";
        	// 
        	// completeEmptyCachesToolStripMenuItem
        	// 
        	this.completeEmptyCachesToolStripMenuItem.Name = "completeEmptyCachesToolStripMenuItem";
        	this.completeEmptyCachesToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
        	this.completeEmptyCachesToolStripMenuItem.Text = "#CompleteEmptyCaches";
        	this.completeEmptyCachesToolStripMenuItem.Click += new System.EventHandler(this.CompleteEmptyCachesToolStripMenuItemClick);
        	// 
        	// displaySelectedToolStripMenuItem
        	// 
        	this.displaySelectedToolStripMenuItem.Name = "displaySelectedToolStripMenuItem";
        	this.displaySelectedToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
        	this.displaySelectedToolStripMenuItem.Text = "#DisplaySelected";
        	this.displaySelectedToolStripMenuItem.Click += new System.EventHandler(this.DisplaySelectedToolStripMenuItemClick);
        	// 
        	// displayLogStatusToolStripMenuItem
        	// 
        	this.displayLogStatusToolStripMenuItem.Name = "displayLogStatusToolStripMenuItem";
        	this.displayLogStatusToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
        	this.displayLogStatusToolStripMenuItem.Text = "#DisplayLogStatus";
        	this.displayLogStatusToolStripMenuItem.Click += new System.EventHandler(this.DisplayLogStatusToolStripMenuItemClick);
        	// 
        	// selectionToolStripMenuItem
        	// 
        	this.selectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.modifyLogSelectedToolStripMenuItem,
			this.copyFieldNotesToolStripMenuItem,
			this.delSelectedToolStripMenuItem,
			this.logSelectedToolStripMenuItem});
        	this.selectionToolStripMenuItem.Name = "selectionToolStripMenuItem";
        	this.selectionToolStripMenuItem.Size = new System.Drawing.Size(97, 20);
        	this.selectionToolStripMenuItem.Text = "#Management";
        	// 
        	// modifyLogSelectedToolStripMenuItem
        	// 
        	this.modifyLogSelectedToolStripMenuItem.Name = "modifyLogSelectedToolStripMenuItem";
        	this.modifyLogSelectedToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
        	this.modifyLogSelectedToolStripMenuItem.Text = "#ModifyLogSelected";
        	this.modifyLogSelectedToolStripMenuItem.Click += new System.EventHandler(this.ModifyLogSelectedToolStripMenuItemClick);
        	// 
        	// copyFieldNotesToolStripMenuItem
        	// 
        	this.copyFieldNotesToolStripMenuItem.Name = "copyFieldNotesToolStripMenuItem";
        	this.copyFieldNotesToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
        	this.copyFieldNotesToolStripMenuItem.Text = "#CopyFieldNotes";
        	this.copyFieldNotesToolStripMenuItem.Click += new System.EventHandler(this.CopyFieldNotesToolStripMenuItemClick);
        	// 
        	// delSelectedToolStripMenuItem
        	// 
        	this.delSelectedToolStripMenuItem.Name = "delSelectedToolStripMenuItem";
        	this.delSelectedToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
        	this.delSelectedToolStripMenuItem.Text = "#DelSelected";
        	this.delSelectedToolStripMenuItem.Click += new System.EventHandler(this.DelSelectedToolStripMenuItemClick);
        	// 
        	// logSelectedToolStripMenuItem
        	// 
        	this.logSelectedToolStripMenuItem.Name = "logSelectedToolStripMenuItem";
        	this.logSelectedToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
        	this.logSelectedToolStripMenuItem.Text = "#LogSelected";
        	this.logSelectedToolStripMenuItem.Click += new System.EventHandler(this.LogSelectedToolStripMenuItemClick);
        	// 
        	// tBsToolStripMenuItem
        	// 
        	this.tBsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.travelTBOnSelectedCachesToolStripMenuItem});
        	this.tBsToolStripMenuItem.Name = "tBsToolStripMenuItem";
        	this.tBsToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
        	this.tBsToolStripMenuItem.Text = "#TBs";
        	// 
        	// travelTBOnSelectedCachesToolStripMenuItem
        	// 
        	this.travelTBOnSelectedCachesToolStripMenuItem.Name = "travelTBOnSelectedCachesToolStripMenuItem";
        	this.travelTBOnSelectedCachesToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
        	this.travelTBOnSelectedCachesToolStripMenuItem.Text = "#TravelTBOnSelectedCaches";
        	this.travelTBOnSelectedCachesToolStripMenuItem.Click += new System.EventHandler(this.TravelTBOnSelectedCachesToolStripMenuItemClick);
        	// 
        	// statusStrip1
        	// 
        	this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tsSelStatus});
        	this.statusStrip1.Location = new System.Drawing.Point(0, 486);
        	this.statusStrip1.Name = "statusStrip1";
        	this.statusStrip1.Size = new System.Drawing.Size(1243, 22);
        	this.statusStrip1.TabIndex = 13;
        	this.statusStrip1.Text = "statusStrip1";
        	// 
        	// tsSelStatus
        	// 
        	this.tsSelStatus.Name = "tsSelStatus";
        	this.tsSelStatus.Size = new System.Drawing.Size(70, 17);
        	this.tsSelStatus.Text = "#tsSelStatus";
        	// 
        	// FieldNotesHMI
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(1243, 508);
        	this.Controls.Add(this.statusStrip1);
        	this.Controls.Add(this.fnbtnMoveDown);
        	this.Controls.Add(this.fnbtnMoveUp);
        	this.Controls.Add(this.lvCachesfn);
        	this.Controls.Add(this.menuStrip1);
        	this.MainMenuStrip = this.menuStrip1;
        	this.Name = "FieldNotesHMI";
        	this.Text = "#FieldNotesHMI";
        	this.menuStrip1.ResumeLayout(false);
        	this.menuStrip1.PerformLayout();
        	this.statusStrip1.ResumeLayout(false);
        	this.statusStrip1.PerformLayout();
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvCachesfn;
        private System.Windows.Forms.Button fnbtnMoveUp;
        private System.Windows.Forms.Button fnbtnMoveDown;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem templateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defineTtemplateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyTemplateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem informationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displaySelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayLogStatusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modifyLogSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyFieldNotesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem delSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tBsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem travelTBOnSelectedCachesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem completeEmptyCachesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defineDefaultLogTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteDefaultLogTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsSelStatus;
    }
}