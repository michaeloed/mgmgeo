namespace SpaceEyeTools.HMI
{
    partial class CoordConvHMI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CoordConvHMI));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLatLonDDD = new System.Windows.Forms.TextBox();
            this.tbLatLonDDMMM = new System.Windows.Forms.TextBox();
            this.button1coordconv = new System.Windows.Forms.Button();
            this.button2coordconv = new System.Windows.Forms.Button();
            this.button3coordconv = new System.Windows.Forms.Button();
            this.tbLatLonDDMMSSS = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(194, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Lat Lon : DD.DDDDDD";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(269, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Lat Lon : N/S/E/W DD° MM.MMM";
            // 
            // tbLatLonDDD
            // 
            this.tbLatLonDDD.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLatLonDDD.Location = new System.Drawing.Point(16, 32);
            this.tbLatLonDDD.Name = "tbLatLonDDD";
            this.tbLatLonDDD.Size = new System.Drawing.Size(249, 26);
            this.tbLatLonDDD.TabIndex = 4;
            // 
            // tbLatLonDDMMM
            // 
            this.tbLatLonDDMMM.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLatLonDDMMM.Location = new System.Drawing.Point(16, 93);
            this.tbLatLonDDMMM.Name = "tbLatLonDDMMM";
            this.tbLatLonDDMMM.Size = new System.Drawing.Size(249, 26);
            this.tbLatLonDDMMM.TabIndex = 8;
            // 
            // button1coordconv
            // 
            this.button1coordconv.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1coordconv.Image = ((System.Drawing.Image)(resources.GetObject("button1coordconv.Image")));
            this.button1coordconv.Location = new System.Drawing.Point(271, 24);
            this.button1coordconv.Name = "button1coordconv";
            this.button1coordconv.Size = new System.Drawing.Size(40, 40);
            this.button1coordconv.TabIndex = 12;
            this.button1coordconv.UseVisualStyleBackColor = true;
            this.button1coordconv.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2coordconv
            // 
            this.button2coordconv.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2coordconv.Image = ((System.Drawing.Image)(resources.GetObject("button2coordconv.Image")));
            this.button2coordconv.Location = new System.Drawing.Point(271, 87);
            this.button2coordconv.Name = "button2coordconv";
            this.button2coordconv.Size = new System.Drawing.Size(40, 40);
            this.button2coordconv.TabIndex = 13;
            this.button2coordconv.UseVisualStyleBackColor = true;
            this.button2coordconv.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3coordconv
            // 
            this.button3coordconv.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3coordconv.Image = ((System.Drawing.Image)(resources.GetObject("button3coordconv.Image")));
            this.button3coordconv.Location = new System.Drawing.Point(271, 153);
            this.button3coordconv.Name = "button3coordconv";
            this.button3coordconv.Size = new System.Drawing.Size(40, 40);
            this.button3coordconv.TabIndex = 19;
            this.button3coordconv.UseVisualStyleBackColor = true;
            this.button3coordconv.Click += new System.EventHandler(this.button3_Click);
            // 
            // tbLatLonDDMMSSS
            // 
            this.tbLatLonDDMMSSS.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLatLonDDMMSSS.Location = new System.Drawing.Point(16, 158);
            this.tbLatLonDDMMSSS.Name = "tbLatLonDDMMSSS";
            this.tbLatLonDDMMSSS.Size = new System.Drawing.Size(249, 26);
            this.tbLatLonDDMMSSS.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(12, 130);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(296, 20);
            this.label9.TabIndex = 14;
            this.label9.Text = "Lat Lon : N/S/E/W DD° MM\' SS.SSS";
            // 
            // CoordConvHMI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 210);
            this.Controls.Add(this.button3coordconv);
            this.Controls.Add(this.tbLatLonDDMMSSS);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.button2coordconv);
            this.Controls.Add(this.button1coordconv);
            this.Controls.Add(this.tbLatLonDDMMM);
            this.Controls.Add(this.tbLatLonDDD);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
            this.Name = "CoordConvHMI";
            this.Text = "#Coord converter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbLatLonDDD;
        private System.Windows.Forms.TextBox tbLatLonDDMMM;
        private System.Windows.Forms.Button button1coordconv;
        private System.Windows.Forms.Button button2coordconv;
        private System.Windows.Forms.Button button3coordconv;
        private System.Windows.Forms.TextBox tbLatLonDDMMSSS;
        private System.Windows.Forms.Label label9;
    }
}

