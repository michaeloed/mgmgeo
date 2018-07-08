/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 09/09/2016
 * Time: 14:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 using SpaceEyeTools.EXControls;
  
namespace MyGeocachingManager.HMI
{
	partial class PQCheckHMI
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button btnGCNCreate;
		private System.Windows.Forms.Button btnGCNCancel;
		private EXListView lvGCNGrid;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox4;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox5;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBox6;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textBox7;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textBox8;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnGCNCreate = new System.Windows.Forms.Button();
			this.btnGCNCancel = new System.Windows.Forms.Button();
			this.lvGCNGrid = new EXListView();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.textBox7 = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textBox8 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnGCNCreate
			// 
			this.btnGCNCreate.Location = new System.Drawing.Point(529, 584);
			this.btnGCNCreate.Name = "btnGCNCreate";
			this.btnGCNCreate.Size = new System.Drawing.Size(75, 23);
			this.btnGCNCreate.TabIndex = 16;
			this.btnGCNCreate.Text = "Appliquer";
			this.btnGCNCreate.UseVisualStyleBackColor = true;
			this.btnGCNCreate.Click += new System.EventHandler(this.BtnGCNCreateClick);
			// 
			// btnGCNCancel
			// 
			this.btnGCNCancel.Location = new System.Drawing.Point(12, 584);
			this.btnGCNCancel.Name = "btnGCNCancel";
			this.btnGCNCancel.Size = new System.Drawing.Size(75, 23);
			this.btnGCNCancel.TabIndex = 15;
			this.btnGCNCancel.Text = "Fermer";
			this.btnGCNCancel.UseVisualStyleBackColor = true;
			this.btnGCNCancel.Click += new System.EventHandler(this.BtnGCNCancelClick);
			// 
			// lvGCNGrid
			// 
			this.lvGCNGrid.ControlPadding = 4;
			this.lvGCNGrid.FullRowSelect = true;
			this.lvGCNGrid.GridLines = true;
			this.lvGCNGrid.Location = new System.Drawing.Point(12, 96);
			this.lvGCNGrid.Name = "lvGCNGrid";
			this.lvGCNGrid.OwnerDraw = true;
			this.lvGCNGrid.Size = new System.Drawing.Size(592, 364);
			this.lvGCNGrid.TabIndex = 14;
			this.lvGCNGrid.UseCompatibleStateImageBehavior = false;
			this.lvGCNGrid.View = System.Windows.Forms.View.Details;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(23, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 23);
			this.label1.TabIndex = 17;
			this.label1.Text = "Dimanche";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(23, 36);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(54, 20);
			this.textBox1.TabIndex = 18;
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(86, 36);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(54, 20);
			this.textBox2.TabIndex = 20;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(86, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(54, 23);
			this.label2.TabIndex = 19;
			this.label2.Text = "Lundi";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(149, 36);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(54, 20);
			this.textBox3.TabIndex = 22;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(149, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(54, 23);
			this.label3.TabIndex = 21;
			this.label3.Text = "Mardi";
			// 
			// textBox4
			// 
			this.textBox4.Location = new System.Drawing.Point(212, 36);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(54, 20);
			this.textBox4.TabIndex = 24;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(212, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(54, 23);
			this.label4.TabIndex = 23;
			this.label4.Text = "Mercredi";
			// 
			// textBox5
			// 
			this.textBox5.Location = new System.Drawing.Point(275, 36);
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(54, 20);
			this.textBox5.TabIndex = 26;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(275, 9);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(54, 23);
			this.label5.TabIndex = 25;
			this.label5.Text = "Jeudi";
			// 
			// textBox6
			// 
			this.textBox6.Location = new System.Drawing.Point(338, 36);
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(54, 20);
			this.textBox6.TabIndex = 28;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(338, 9);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(54, 23);
			this.label6.TabIndex = 27;
			this.label6.Text = "Vendredi";
			// 
			// textBox7
			// 
			this.textBox7.Location = new System.Drawing.Point(401, 36);
			this.textBox7.Name = "textBox7";
			this.textBox7.Size = new System.Drawing.Size(54, 20);
			this.textBox7.TabIndex = 30;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(401, 9);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(54, 23);
			this.label7.TabIndex = 29;
			this.label7.Text = "Samedi";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(23, 70);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(306, 23);
			this.label8.TabIndex = 31;
			this.label8.Text = "heure serveur";
			// 
			// textBox8
			// 
			this.textBox8.Location = new System.Drawing.Point(12, 466);
			this.textBox8.Multiline = true;
			this.textBox8.Name = "textBox8";
			this.textBox8.ReadOnly = true;
			this.textBox8.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBox8.Size = new System.Drawing.Size(592, 112);
			this.textBox8.TabIndex = 32;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(361, 584);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 33;
			this.button1.Text = "Recharger";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.Button1Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(191, 584);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 34;
			this.button2.Text = "Supprimer";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.Button2Click);
			// 
			// PQCheckHMI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(616, 618);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBox8);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textBox7);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textBox6);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.textBox5);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textBox4);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textBox3);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnGCNCreate);
			this.Controls.Add(this.btnGCNCancel);
			this.Controls.Add(this.lvGCNGrid);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PQCheckHMI";
			this.Text = "PQCheckHMI";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
