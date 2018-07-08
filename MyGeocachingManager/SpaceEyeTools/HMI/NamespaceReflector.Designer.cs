namespace SpaceEyeTools.HMI
{
    partial class NamespaceReflector
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
            this.treeViewTypes = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeViewTypes
            // 
            this.treeViewTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewTypes.FullRowSelect = true;
            this.treeViewTypes.Location = new System.Drawing.Point(0, 0);
            this.treeViewTypes.Name = "treeViewTypes";
            this.treeViewTypes.ShowNodeToolTips = true;
            this.treeViewTypes.Size = new System.Drawing.Size(444, 492);
            this.treeViewTypes.TabIndex = 0;
            // 
            // NamespaceReflector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 492);
            this.Controls.Add(this.treeViewTypes);
            this.Name = "NamespaceReflector";
            this.Text = "#MGMReflector";
            this.Load += new System.EventHandler(this.MGMReflector_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewTypes;
    }
}