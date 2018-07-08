/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 10/08/2016
 * Time: 09:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpaceEyeTools.EXControls;

namespace MyGeocachingManager.HMI
{
	partial class NotificationCreation
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private EXListView lvGCNGrid;
		private System.Windows.Forms.Label lblGCNNom;
		private System.Windows.Forms.TextBox tbGCNName;
		private System.Windows.Forms.TextBox tbGCNRadius;
		private System.Windows.Forms.Label lblGCNRadius;
		private System.Windows.Forms.Label lblGCNEmail;
		private System.Windows.Forms.Label lblGCNGrid;
		private System.Windows.Forms.TextBox tbGCNCenter;
		private System.Windows.Forms.Label lblGCNCentre;
		private System.Windows.Forms.Button btnGCNMap;
		private System.Windows.Forms.TextBox tbGCNAllCoords;
		private System.Windows.Forms.Button btnGCNCancel;
		private System.Windows.Forms.Button btnGCNCreate;
		private System.Windows.Forms.ComboBox cbGCNEmails;
		
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
			this.lvGCNGrid = new EXListView();
			this.lblGCNNom = new System.Windows.Forms.Label();
			this.tbGCNName = new System.Windows.Forms.TextBox();
			this.tbGCNRadius = new System.Windows.Forms.TextBox();
			this.lblGCNRadius = new System.Windows.Forms.Label();
			this.lblGCNEmail = new System.Windows.Forms.Label();
			this.lblGCNGrid = new System.Windows.Forms.Label();
			this.tbGCNCenter = new System.Windows.Forms.TextBox();
			this.lblGCNCentre = new System.Windows.Forms.Label();
			this.btnGCNMap = new System.Windows.Forms.Button();
			this.tbGCNAllCoords = new System.Windows.Forms.TextBox();
			this.btnGCNCancel = new System.Windows.Forms.Button();
			this.btnGCNCreate = new System.Windows.Forms.Button();
			this.cbGCNEmails = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// lvGCNGrid
			// 
			this.lvGCNGrid.ControlPadding = 4;
			this.lvGCNGrid.FullRowSelect = true;
			this.lvGCNGrid.GridLines = true;
			this.lvGCNGrid.Location = new System.Drawing.Point(119, 111);
			this.lvGCNGrid.Name = "lvGCNGrid";
			this.lvGCNGrid.OwnerDraw = true;
			this.lvGCNGrid.Size = new System.Drawing.Size(376, 372);
			this.lvGCNGrid.TabIndex = 1;
			this.lvGCNGrid.UseCompatibleStateImageBehavior = false;
			this.lvGCNGrid.View = System.Windows.Forms.View.Details;
			// 
			// lblGCNNom
			// 
			this.lblGCNNom.Location = new System.Drawing.Point(13, 515);
			this.lblGCNNom.Name = "lblGCNNom";
			this.lblGCNNom.Size = new System.Drawing.Size(178, 23);
			this.lblGCNNom.TabIndex = 2;
			this.lblGCNNom.Text = "#nom";
			// 
			// tbGCNName
			// 
			this.tbGCNName.Location = new System.Drawing.Point(197, 515);
			this.tbGCNName.Name = "tbGCNName";
			this.tbGCNName.Size = new System.Drawing.Size(264, 20);
			this.tbGCNName.TabIndex = 3;
			this.tbGCNName.Text = "MGMN";
			// 
			// tbGCNRadius
			// 
			this.tbGCNRadius.Location = new System.Drawing.Point(197, 489);
			this.tbGCNRadius.Name = "tbGCNRadius";
			this.tbGCNRadius.Size = new System.Drawing.Size(264, 20);
			this.tbGCNRadius.TabIndex = 5;
			this.tbGCNRadius.Text = "20";
			// 
			// lblGCNRadius
			// 
			this.lblGCNRadius.Location = new System.Drawing.Point(13, 489);
			this.lblGCNRadius.Name = "lblGCNRadius";
			this.lblGCNRadius.Size = new System.Drawing.Size(178, 23);
			this.lblGCNRadius.TabIndex = 4;
			this.lblGCNRadius.Text = "#rayon";
			// 
			// lblGCNEmail
			// 
			this.lblGCNEmail.Location = new System.Drawing.Point(12, 542);
			this.lblGCNEmail.Name = "lblGCNEmail";
			this.lblGCNEmail.Size = new System.Drawing.Size(178, 23);
			this.lblGCNEmail.TabIndex = 6;
			this.lblGCNEmail.Text = "#email";
			// 
			// lblGCNGrid
			// 
			this.lblGCNGrid.Location = new System.Drawing.Point(13, 85);
			this.lblGCNGrid.Name = "lblGCNGrid";
			this.lblGCNGrid.Size = new System.Drawing.Size(353, 23);
			this.lblGCNGrid.TabIndex = 7;
			this.lblGCNGrid.Text = "#type";
			// 
			// tbGCNCenter
			// 
			this.tbGCNCenter.Location = new System.Drawing.Point(118, 9);
			this.tbGCNCenter.Name = "tbGCNCenter";
			this.tbGCNCenter.Size = new System.Drawing.Size(218, 20);
			this.tbGCNCenter.TabIndex = 9;
			// 
			// lblGCNCentre
			// 
			this.lblGCNCentre.Location = new System.Drawing.Point(12, 9);
			this.lblGCNCentre.Name = "lblGCNCentre";
			this.lblGCNCentre.Size = new System.Drawing.Size(100, 23);
			this.lblGCNCentre.TabIndex = 8;
			this.lblGCNCentre.Text = "#centre";
			// 
			// btnGCNMap
			// 
			this.btnGCNMap.Location = new System.Drawing.Point(342, 6);
			this.btnGCNMap.Name = "btnGCNMap";
			this.btnGCNMap.Size = new System.Drawing.Size(24, 24);
			this.btnGCNMap.TabIndex = 10;
			this.btnGCNMap.UseVisualStyleBackColor = true;
			this.btnGCNMap.Click += new System.EventHandler(this.BtnGCNMapClick);
			// 
			// tbGCNAllCoords
			// 
			this.tbGCNAllCoords.Location = new System.Drawing.Point(118, 35);
			this.tbGCNAllCoords.Multiline = true;
			this.tbGCNAllCoords.Name = "tbGCNAllCoords";
			this.tbGCNAllCoords.ReadOnly = true;
			this.tbGCNAllCoords.Size = new System.Drawing.Size(248, 49);
			this.tbGCNAllCoords.TabIndex = 11;
			// 
			// btnGCNCancel
			// 
			this.btnGCNCancel.Location = new System.Drawing.Point(13, 599);
			this.btnGCNCancel.Name = "btnGCNCancel";
			this.btnGCNCancel.Size = new System.Drawing.Size(75, 23);
			this.btnGCNCancel.TabIndex = 12;
			this.btnGCNCancel.Text = "#cancel";
			this.btnGCNCancel.UseVisualStyleBackColor = true;
			this.btnGCNCancel.Click += new System.EventHandler(this.BtnGCNCancelClick);
			// 
			// btnGCNCreate
			// 
			this.btnGCNCreate.Location = new System.Drawing.Point(420, 599);
			this.btnGCNCreate.Name = "btnGCNCreate";
			this.btnGCNCreate.Size = new System.Drawing.Size(75, 23);
			this.btnGCNCreate.TabIndex = 13;
			this.btnGCNCreate.Text = "#valider";
			this.btnGCNCreate.UseVisualStyleBackColor = true;
			this.btnGCNCreate.Click += new System.EventHandler(this.BtnGCNCreateClick);
			// 
			// cbGCNEmails
			// 
			this.cbGCNEmails.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbGCNEmails.FormattingEnabled = true;
			this.cbGCNEmails.Location = new System.Drawing.Point(197, 542);
			this.cbGCNEmails.Name = "cbGCNEmails";
			this.cbGCNEmails.Size = new System.Drawing.Size(264, 21);
			this.cbGCNEmails.TabIndex = 14;
			// 
			// NotificationCreation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(512, 634);
			this.Controls.Add(this.cbGCNEmails);
			this.Controls.Add(this.btnGCNCreate);
			this.Controls.Add(this.btnGCNCancel);
			this.Controls.Add(this.tbGCNAllCoords);
			this.Controls.Add(this.btnGCNMap);
			this.Controls.Add(this.tbGCNCenter);
			this.Controls.Add(this.lblGCNCentre);
			this.Controls.Add(this.lblGCNGrid);
			this.Controls.Add(this.lblGCNEmail);
			this.Controls.Add(this.tbGCNRadius);
			this.Controls.Add(this.lblGCNRadius);
			this.Controls.Add(this.tbGCNName);
			this.Controls.Add(this.lblGCNNom);
			this.Controls.Add(this.lvGCNGrid);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NotificationCreation";
			this.Text = "NotificationCreation";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
