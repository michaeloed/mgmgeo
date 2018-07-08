namespace MyGeocachingManager
{
    partial class TagManager
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
            this.listView1tagmgr = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDeletetagmgr = new System.Windows.Forms.Button();
            this.btnAddtagmgr = new System.Windows.Forms.Button();
            this.button2tagmgr = new System.Windows.Forms.Button();
            this.button1tagmgr = new System.Windows.Forms.Button();
            this.comboBox1tagmgr = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // listView1tagmgr
            // 
            this.listView1tagmgr.FullRowSelect = true;
            this.listView1tagmgr.Location = new System.Drawing.Point(15, 41);
            this.listView1tagmgr.Name = "listView1tagmgr";
            this.listView1tagmgr.Size = new System.Drawing.Size(289, 158);
            this.listView1tagmgr.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listView1tagmgr.TabIndex = 66;
            this.listView1tagmgr.UseCompatibleStateImageBehavior = false;
            this.listView1tagmgr.View = System.Windows.Forms.View.List;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 59;
            this.label1.Text = "#Tag";
            // 
            // btnDeletetagmgr
            // 
            this.btnDeletetagmgr.Location = new System.Drawing.Point(318, 106);
            this.btnDeletetagmgr.Name = "btnDeletetagmgr";
            this.btnDeletetagmgr.Size = new System.Drawing.Size(22, 22);
            this.btnDeletetagmgr.TabIndex = 58;
            this.btnDeletetagmgr.Text = "-";
            this.btnDeletetagmgr.UseVisualStyleBackColor = true;
            this.btnDeletetagmgr.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAddtagmgr
            // 
            this.btnAddtagmgr.Location = new System.Drawing.Point(318, 13);
            this.btnAddtagmgr.Name = "btnAddtagmgr";
            this.btnAddtagmgr.Size = new System.Drawing.Size(22, 22);
            this.btnAddtagmgr.TabIndex = 56;
            this.btnAddtagmgr.Text = "+";
            this.btnAddtagmgr.UseVisualStyleBackColor = true;
            this.btnAddtagmgr.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // button2tagmgr
            // 
            this.button2tagmgr.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2tagmgr.Location = new System.Drawing.Point(229, 205);
            this.button2tagmgr.Name = "button2tagmgr";
            this.button2tagmgr.Size = new System.Drawing.Size(75, 22);
            this.button2tagmgr.TabIndex = 55;
            this.button2tagmgr.Text = "button2";
            this.button2tagmgr.UseVisualStyleBackColor = true;
            // 
            // button1tagmgr
            // 
            this.button1tagmgr.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1tagmgr.Location = new System.Drawing.Point(140, 205);
            this.button1tagmgr.Name = "button1tagmgr";
            this.button1tagmgr.Size = new System.Drawing.Size(75, 22);
            this.button1tagmgr.TabIndex = 54;
            this.button1tagmgr.Text = "button1";
            this.button1tagmgr.UseVisualStyleBackColor = true;
            this.button1tagmgr.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox1tagmgr
            // 
            this.comboBox1tagmgr.FormattingEnabled = true;
            this.comboBox1tagmgr.Location = new System.Drawing.Point(132, 13);
            this.comboBox1tagmgr.Name = "comboBox1tagmgr";
            this.comboBox1tagmgr.Size = new System.Drawing.Size(172, 21);
            this.comboBox1tagmgr.Sorted = true;
            this.comboBox1tagmgr.TabIndex = 68;
            // 
            // TagManager
            // 
            this.AcceptButton = this.btnAddtagmgr;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2tagmgr;
            this.ClientSize = new System.Drawing.Size(354, 239);
            this.Controls.Add(this.comboBox1tagmgr);
            this.Controls.Add(this.listView1tagmgr);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnDeletetagmgr);
            this.Controls.Add(this.btnAddtagmgr);
            this.Controls.Add(this.button2tagmgr);
            this.Controls.Add(this.button1tagmgr);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
            this.Name = "TagManager";
            this.Text = "TagManager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1tagmgr;

        /// <summary>
        /// Label of text field
        /// </summary>
        public System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDeletetagmgr;
        private System.Windows.Forms.Button btnAddtagmgr;

        /// <summary>
        /// Cancel button
        /// </summary>
        public System.Windows.Forms.Button button2tagmgr;

        /// <summary>
        /// OK button
        /// </summary>
        public System.Windows.Forms.Button button1tagmgr;
        private System.Windows.Forms.ComboBox comboBox1tagmgr;
    }
}