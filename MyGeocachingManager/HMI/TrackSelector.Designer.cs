namespace MyGeocachingManager.HMI
{
    partial class TrackSelector
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
            this.trackBarStarttrksel = new System.Windows.Forms.TrackBar();
            this.trackBarEndtrksel = new System.Windows.Forms.TrackBar();
            this.labelStart = new System.Windows.Forms.Label();
            this.labelEnd = new System.Windows.Forms.Label();
            this.labelTxtStart = new System.Windows.Forms.Label();
            this.labelTxtEnd = new System.Windows.Forms.Label();
            this.pnGraph = new MyGeocachingManager.HMI.MyPanel();
            this.lblSpeedMoy = new System.Windows.Forms.Label();
            this.btnConfiguretrksel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStarttrksel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarEndtrksel)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBarStarttrksel
            // 
            this.trackBarStarttrksel.AutoSize = false;
            this.trackBarStarttrksel.Location = new System.Drawing.Point(233, 16);
            this.trackBarStarttrksel.Name = "trackBarStarttrksel";
            this.trackBarStarttrksel.Size = new System.Drawing.Size(329, 30);
            this.trackBarStarttrksel.TabIndex = 0;
            this.trackBarStarttrksel.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarStarttrksel.Scroll += new System.EventHandler(this.trackBarStart_Scroll);
            // 
            // trackBarEndtrksel
            // 
            this.trackBarEndtrksel.AutoSize = false;
            this.trackBarEndtrksel.Location = new System.Drawing.Point(233, 52);
            this.trackBarEndtrksel.Name = "trackBarEndtrksel";
            this.trackBarEndtrksel.Size = new System.Drawing.Size(329, 30);
            this.trackBarEndtrksel.TabIndex = 1;
            this.trackBarEndtrksel.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarEndtrksel.Scroll += new System.EventHandler(this.trackBarEnd_Scroll);
            // 
            // labelStart
            // 
            this.labelStart.AutoSize = true;
            this.labelStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStart.Location = new System.Drawing.Point(92, 16);
            this.labelStart.Name = "labelStart";
            this.labelStart.Size = new System.Drawing.Size(46, 17);
            this.labelStart.TabIndex = 2;
            this.labelStart.Text = "label1";
            // 
            // labelEnd
            // 
            this.labelEnd.AutoSize = true;
            this.labelEnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEnd.Location = new System.Drawing.Point(92, 52);
            this.labelEnd.Name = "labelEnd";
            this.labelEnd.Size = new System.Drawing.Size(46, 17);
            this.labelEnd.TabIndex = 3;
            this.labelEnd.Text = "label2";
            // 
            // labelTxtStart
            // 
            this.labelTxtStart.AutoSize = true;
            this.labelTxtStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTxtStart.Location = new System.Drawing.Point(12, 16);
            this.labelTxtStart.Name = "labelTxtStart";
            this.labelTxtStart.Size = new System.Drawing.Size(46, 17);
            this.labelTxtStart.TabIndex = 4;
            this.labelTxtStart.Text = "#Start";
            // 
            // labelTxtEnd
            // 
            this.labelTxtEnd.AutoSize = true;
            this.labelTxtEnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTxtEnd.Location = new System.Drawing.Point(12, 52);
            this.labelTxtEnd.Name = "labelTxtEnd";
            this.labelTxtEnd.Size = new System.Drawing.Size(41, 17);
            this.labelTxtEnd.TabIndex = 5;
            this.labelTxtEnd.Text = "#End";
            // 
            // pnGraph
            // 
            this.pnGraph.Location = new System.Drawing.Point(15, 118);
            this.pnGraph.Name = "pnGraph";
            this.pnGraph.Size = new System.Drawing.Size(547, 173);
            this.pnGraph.TabIndex = 6;
            // 
            // lblSpeedMoy
            // 
            this.lblSpeedMoy.AutoSize = true;
            this.lblSpeedMoy.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeedMoy.Location = new System.Drawing.Point(12, 88);
            this.lblSpeedMoy.Name = "lblSpeedMoy";
            this.lblSpeedMoy.Size = new System.Drawing.Size(83, 17);
            this.lblSpeedMoy.TabIndex = 7;
            this.lblSpeedMoy.Text = "#SpeedMoy";
            // 
            // btnConfiguretrksel
            // 
            this.btnConfiguretrksel.Location = new System.Drawing.Point(539, 82);
            this.btnConfiguretrksel.Name = "btnConfiguretrksel";
            this.btnConfiguretrksel.Size = new System.Drawing.Size(23, 23);
            this.btnConfiguretrksel.TabIndex = 8;
            this.btnConfiguretrksel.UseVisualStyleBackColor = true;
            this.btnConfiguretrksel.Click += new System.EventHandler(this.btnConfigure_Click);
            // 
            // TrackSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 303);
            this.Controls.Add(this.btnConfiguretrksel);
            this.Controls.Add(this.lblSpeedMoy);
            this.Controls.Add(this.pnGraph);
            this.Controls.Add(this.labelTxtEnd);
            this.Controls.Add(this.labelTxtStart);
            this.Controls.Add(this.labelEnd);
            this.Controls.Add(this.labelStart);
            this.Controls.Add(this.trackBarEndtrksel);
            this.Controls.Add(this.trackBarStarttrksel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
            this.Name = "TrackSelector";
            this.Text = "#TrackSelector";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.TrackSelector_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStarttrksel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarEndtrksel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar trackBarStarttrksel;
        private System.Windows.Forms.TrackBar trackBarEndtrksel;
        private System.Windows.Forms.Label labelStart;
        private System.Windows.Forms.Label labelEnd;
        private System.Windows.Forms.Label labelTxtStart;
        private System.Windows.Forms.Label labelTxtEnd;
        private MyGeocachingManager.HMI.MyPanel pnGraph;
        private System.Windows.Forms.Label lblSpeedMoy;
        private System.Windows.Forms.Button btnConfiguretrksel;
    }
}