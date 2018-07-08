using SpaceEyeTools.EXControls;

namespace SpaceEyeTools.HMI
{
    partial class ConfigEditor
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
            this.elvConfigcfgeditor = new SpaceEyeTools.EXControls.EXListView();
            this.SuspendLayout();
            // 
            // elvConfigcfgeditor
            // 
            this.elvConfigcfgeditor.ControlPadding = 4;
            this.elvConfigcfgeditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elvConfigcfgeditor.FullRowSelect = true;
            this.elvConfigcfgeditor.Location = new System.Drawing.Point(0, 0);
            this.elvConfigcfgeditor.Name = "elvConfigcfgeditor";
            this.elvConfigcfgeditor.OwnerDraw = true;
            this.elvConfigcfgeditor.Size = new System.Drawing.Size(619, 519);
            this.elvConfigcfgeditor.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.elvConfigcfgeditor.TabIndex = 0;
            this.elvConfigcfgeditor.UseCompatibleStateImageBehavior = false;
            this.elvConfigcfgeditor.View = System.Windows.Forms.View.Details;
            // 
            // ConfigEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 519);
            this.Controls.Add(this.elvConfigcfgeditor);
            this.Name = "ConfigEditor";
            this.Text = "ConfigEditor";
            this.Load += new System.EventHandler(this.ConfigEditor_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private global::SpaceEyeTools.EXControls.EXListView elvConfigcfgeditor;
    }
}