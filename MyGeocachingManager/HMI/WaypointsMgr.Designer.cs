namespace MyGeocachingManager.HMI
{
    partial class WaypointsMgr
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
            this.lvWaypointsMgrView = new System.Windows.Forms.ListView();
            this.btnWaypointMgrAdd = new System.Windows.Forms.Button();
            this.btnWaypointMgrDel = new System.Windows.Forms.Button();
            this.btnWaypointMgrEdit = new System.Windows.Forms.Button();
            this.btnCartoDisplay = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lvWaypointsMgrView
            // 
            this.lvWaypointsMgrView.FullRowSelect = true;
            this.lvWaypointsMgrView.Location = new System.Drawing.Point(13, 13);
            this.lvWaypointsMgrView.MultiSelect = false;
            this.lvWaypointsMgrView.Name = "lvWaypointsMgrView";
            this.lvWaypointsMgrView.Size = new System.Drawing.Size(789, 305);
            this.lvWaypointsMgrView.TabIndex = 0;
            this.lvWaypointsMgrView.UseCompatibleStateImageBehavior = false;
            this.lvWaypointsMgrView.View = System.Windows.Forms.View.Details;
            this.lvWaypointsMgrView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvWaypointsMgrView_MouseDoubleClick);
            // 
            // btnWaypointMgrAdd
            // 
            this.btnWaypointMgrAdd.Location = new System.Drawing.Point(77, 325);
            this.btnWaypointMgrAdd.Name = "btnWaypointMgrAdd";
            this.btnWaypointMgrAdd.Size = new System.Drawing.Size(181, 23);
            this.btnWaypointMgrAdd.TabIndex = 1;
            this.btnWaypointMgrAdd.Text = "WaypointMgrAdd";
            this.btnWaypointMgrAdd.UseVisualStyleBackColor = true;
            this.btnWaypointMgrAdd.Click += new System.EventHandler(this.btnWaypointMgrAdd_Click);
            // 
            // btnWaypointMgrDel
            // 
            this.btnWaypointMgrDel.Location = new System.Drawing.Point(284, 325);
            this.btnWaypointMgrDel.Name = "btnWaypointMgrDel";
            this.btnWaypointMgrDel.Size = new System.Drawing.Size(181, 23);
            this.btnWaypointMgrDel.TabIndex = 2;
            this.btnWaypointMgrDel.Text = "WaypointMgrDel";
            this.btnWaypointMgrDel.UseVisualStyleBackColor = true;
            this.btnWaypointMgrDel.Click += new System.EventHandler(this.btnWaypointMgrDel_Click);
            // 
            // btnWaypointMgrEdit
            // 
            this.btnWaypointMgrEdit.Location = new System.Drawing.Point(495, 325);
            this.btnWaypointMgrEdit.Name = "btnWaypointMgrEdit";
            this.btnWaypointMgrEdit.Size = new System.Drawing.Size(181, 23);
            this.btnWaypointMgrEdit.TabIndex = 3;
            this.btnWaypointMgrEdit.Text = "WaypointMgrEdit";
            this.btnWaypointMgrEdit.UseVisualStyleBackColor = true;
            this.btnWaypointMgrEdit.Click += new System.EventHandler(this.btnWaypointMgrEdit_Click);
            // 
            // btnCartoDisplay
            // 
            this.btnCartoDisplay.Location = new System.Drawing.Point(12, 324);
            this.btnCartoDisplay.Name = "btnCartoDisplay";
            this.btnCartoDisplay.Size = new System.Drawing.Size(24, 24);
            this.btnCartoDisplay.TabIndex = 52;
            this.btnCartoDisplay.UseVisualStyleBackColor = true;
            this.btnCartoDisplay.Click += new System.EventHandler(this.btnCartoDisplay_Click);
            // 
            // WaypointsMgr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 360);
            this.Controls.Add(this.btnCartoDisplay);
            this.Controls.Add(this.btnWaypointMgrEdit);
            this.Controls.Add(this.btnWaypointMgrDel);
            this.Controls.Add(this.btnWaypointMgrAdd);
            this.Controls.Add(this.lvWaypointsMgrView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
            this.Name = "WaypointsMgr";
            this.Text = "#WaypointsMgr";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvWaypointsMgrView;
        private System.Windows.Forms.Button btnWaypointMgrAdd;
        private System.Windows.Forms.Button btnWaypointMgrDel;
        private System.Windows.Forms.Button btnWaypointMgrEdit;
        private System.Windows.Forms.Button btnCartoDisplay;
    }
}