namespace MyGeocachingManager
{
    partial class FavManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FavManager));
            this.favmgrbutton1 = new System.Windows.Forms.Button();
            this.favmgrbutton2 = new System.Windows.Forms.Button();
            this.favmgrbtnDelete = new System.Windows.Forms.Button();
            this.favmgrbtnSave = new System.Windows.Forms.Button();
            this.favmgrbtnLoad = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.favmgrtextBox1 = new System.Windows.Forms.TextBox();
            this.favmgrtextBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.favmgrbutton3 = new System.Windows.Forms.Button();
            this.favmgrlistView1 = new System.Windows.Forms.ListView();
            this.favmgrbtnEdit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // favmgrbutton1
            // 
            this.favmgrbutton1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.favmgrbutton1.Location = new System.Drawing.Point(229, 327);
            this.favmgrbutton1.Name = "favmgrbutton1";
            this.favmgrbutton1.Size = new System.Drawing.Size(75, 23);
            this.favmgrbutton1.TabIndex = 0;
            this.favmgrbutton1.Text = "#validate";
            this.favmgrbutton1.UseVisualStyleBackColor = true;
            this.favmgrbutton1.Click += new System.EventHandler(this.button1_Click);
            // 
            // favmgrbutton2
            // 
            this.favmgrbutton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.favmgrbutton2.Location = new System.Drawing.Point(318, 327);
            this.favmgrbutton2.Name = "favmgrbutton2";
            this.favmgrbutton2.Size = new System.Drawing.Size(75, 23);
            this.favmgrbutton2.TabIndex = 1;
            this.favmgrbutton2.Text = "#cancel";
            this.favmgrbutton2.UseVisualStyleBackColor = true;
            this.favmgrbutton2.Click += new System.EventHandler(this.button2_Click);
            // 
            // favmgrbtnDelete
            // 
            this.favmgrbtnDelete.Image = ((System.Drawing.Image)(resources.GetObject("favmgrbtnDelete.Image")));
            this.favmgrbtnDelete.Location = new System.Drawing.Point(395, 7);
            this.favmgrbtnDelete.Name = "favmgrbtnDelete";
            this.favmgrbtnDelete.Size = new System.Drawing.Size(22, 22);
            this.favmgrbtnDelete.TabIndex = 45;
            this.favmgrbtnDelete.UseVisualStyleBackColor = true;
            this.favmgrbtnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // favmgrbtnSave
            // 
            this.favmgrbtnSave.Image = ((System.Drawing.Image)(resources.GetObject("favmgrbtnSave.Image")));
            this.favmgrbtnSave.Location = new System.Drawing.Point(367, 7);
            this.favmgrbtnSave.Name = "favmgrbtnSave";
            this.favmgrbtnSave.Size = new System.Drawing.Size(22, 22);
            this.favmgrbtnSave.TabIndex = 44;
            this.favmgrbtnSave.UseVisualStyleBackColor = true;
            this.favmgrbtnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // favmgrbtnLoad
            // 
            this.favmgrbtnLoad.Image = ((System.Drawing.Image)(resources.GetObject("favmgrbtnLoad.Image")));
            this.favmgrbtnLoad.Location = new System.Drawing.Point(339, 7);
            this.favmgrbtnLoad.Name = "favmgrbtnLoad";
            this.favmgrbtnLoad.Size = new System.Drawing.Size(22, 22);
            this.favmgrbtnLoad.TabIndex = 43;
            this.favmgrbtnLoad.UseVisualStyleBackColor = true;
            this.favmgrbtnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 46;
            this.label1.Text = "Name";
            // 
            // favmgrtextBox1
            // 
            this.favmgrtextBox1.Location = new System.Drawing.Point(110, 9);
            this.favmgrtextBox1.Name = "favmgrtextBox1";
            this.favmgrtextBox1.ReadOnly = true;
            this.favmgrtextBox1.Size = new System.Drawing.Size(191, 20);
            this.favmgrtextBox1.TabIndex = 47;
            // 
            // favmgrtextBox2
            // 
            this.favmgrtextBox2.Location = new System.Drawing.Point(110, 35);
            this.favmgrtextBox2.Name = "favmgrtextBox2";
            this.favmgrtextBox2.ReadOnly = true;
            this.favmgrtextBox2.Size = new System.Drawing.Size(191, 20);
            this.favmgrtextBox2.TabIndex = 49;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(22, 13);
            this.label2.TabIndex = 48;
            this.label2.Text = "Lat";
            // 
            // favmgrbutton3
            // 
            this.favmgrbutton3.Image = ((System.Drawing.Image)(resources.GetObject("favmgrbutton3.Image")));
            this.favmgrbutton3.Location = new System.Drawing.Point(311, 35);
            this.favmgrbutton3.Name = "favmgrbutton3";
            this.favmgrbutton3.Size = new System.Drawing.Size(20, 20);
            this.favmgrbutton3.TabIndex = 52;
            this.favmgrbutton3.UseVisualStyleBackColor = true;
            this.favmgrbutton3.Click += new System.EventHandler(this.button3_Click);
            // 
            // favmgrlistView1
            // 
            this.favmgrlistView1.Location = new System.Drawing.Point(12, 61);
            this.favmgrlistView1.Name = "favmgrlistView1";
            this.favmgrlistView1.Size = new System.Drawing.Size(381, 260);
            this.favmgrlistView1.TabIndex = 53;
            this.favmgrlistView1.UseCompatibleStateImageBehavior = false;
            this.favmgrlistView1.View = System.Windows.Forms.View.List;
            this.favmgrlistView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            // 
            // favmgrbtnEdit
            // 
            this.favmgrbtnEdit.Image = ((System.Drawing.Image)(resources.GetObject("favmgrbtnEdit.Image")));
            this.favmgrbtnEdit.Location = new System.Drawing.Point(311, 7);
            this.favmgrbtnEdit.Name = "favmgrbtnEdit";
            this.favmgrbtnEdit.Size = new System.Drawing.Size(22, 22);
            this.favmgrbtnEdit.TabIndex = 54;
            this.favmgrbtnEdit.UseVisualStyleBackColor = true;
            this.favmgrbtnEdit.Click += new System.EventHandler(this.favmgrbtnEdit_Click);
            // 
            // FavManager
            // 
            this.AcceptButton = this.favmgrbutton1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.favmgrbutton2;
            this.ClientSize = new System.Drawing.Size(426, 364);
            this.Controls.Add(this.favmgrbtnEdit);
            this.Controls.Add(this.favmgrlistView1);
            this.Controls.Add(this.favmgrbutton3);
            this.Controls.Add(this.favmgrtextBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.favmgrtextBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.favmgrbtnDelete);
            this.Controls.Add(this.favmgrbtnSave);
            this.Controls.Add(this.favmgrbtnLoad);
            this.Controls.Add(this.favmgrbutton2);
            this.Controls.Add(this.favmgrbutton1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
            this.Name = "FavManager";
            this.Text = "FavManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FavManager_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        /// <summary>
        /// First button (OK)
        /// </summary>
        public System.Windows.Forms.Button favmgrbutton1;

        /// <summary>
        /// Second button (Cancel)
        /// </summary>
        public System.Windows.Forms.Button favmgrbutton2;
        private System.Windows.Forms.Button favmgrbtnDelete;
        private System.Windows.Forms.Button favmgrbtnSave;
        private System.Windows.Forms.Button favmgrbtnLoad;

        /// <summary>
        /// First label (Name)
        /// </summary>
        public System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox favmgrtextBox1;
        private System.Windows.Forms.TextBox favmgrtextBox2;

        /// <summary>
        /// Second label (Latitude)
        /// </summary>
        public System.Windows.Forms.Label label2;

        /// <summary>
        /// Third button (center on map)
        /// </summary>
        public System.Windows.Forms.Button favmgrbutton3;
        private System.Windows.Forms.ListView favmgrlistView1;
        private System.Windows.Forms.Button favmgrbtnEdit;
    }
}