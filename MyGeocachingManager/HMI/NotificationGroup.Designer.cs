/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 12/08/2016
 * Time: 10:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 using SpaceEyeTools.EXControls;
namespace MyGeocachingManager.HMI
{
	partial class NotificationGroup
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private EXListView lvGCNListGroup;
		private System.Windows.Forms.Button btnGCNGDelete;
		private System.Windows.Forms.Button btnGCNGToggle;
		private System.Windows.Forms.Button btnGCNGUpdate;
		private System.Windows.Forms.Button btnGCNGMap;
		
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
			this.lvGCNListGroup = new EXListView();
			this.btnGCNGDelete = new System.Windows.Forms.Button();
			this.btnGCNGToggle = new System.Windows.Forms.Button();
			this.btnGCNGUpdate = new System.Windows.Forms.Button();
			this.btnGCNGMap = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lvGCNListGroup
			// 
			this.lvGCNListGroup.ControlPadding = 4;
			this.lvGCNListGroup.FullRowSelect = true;
			this.lvGCNListGroup.GridLines = true;
			this.lvGCNListGroup.Location = new System.Drawing.Point(12, 79);
			this.lvGCNListGroup.Name = "lvGCNListGroup";
			this.lvGCNListGroup.OwnerDraw = true;
			this.lvGCNListGroup.Size = new System.Drawing.Size(613, 516);
			this.lvGCNListGroup.TabIndex = 2;
			this.lvGCNListGroup.UseCompatibleStateImageBehavior = false;
			this.lvGCNListGroup.View = System.Windows.Forms.View.Details;
			// 
			// btnGCNGDelete
			// 
			this.btnGCNGDelete.Location = new System.Drawing.Point(12, 12);
			this.btnGCNGDelete.Name = "btnGCNGDelete";
			this.btnGCNGDelete.Size = new System.Drawing.Size(278, 23);
			this.btnGCNGDelete.TabIndex = 3;
			this.btnGCNGDelete.Text = "btnGCNGDelete";
			this.btnGCNGDelete.UseVisualStyleBackColor = true;
			this.btnGCNGDelete.Click += new System.EventHandler(this.BtnGCNGDeleteClick);
			// 
			// btnGCNGToggle
			// 
			this.btnGCNGToggle.Location = new System.Drawing.Point(12, 41);
			this.btnGCNGToggle.Name = "btnGCNGToggle";
			this.btnGCNGToggle.Size = new System.Drawing.Size(278, 23);
			this.btnGCNGToggle.TabIndex = 4;
			this.btnGCNGToggle.Text = "btnGCNGToggle";
			this.btnGCNGToggle.UseVisualStyleBackColor = true;
			this.btnGCNGToggle.Click += new System.EventHandler(this.BtnGCNGToggleClick);
			// 
			// btnGCNGUpdate
			// 
			this.btnGCNGUpdate.Location = new System.Drawing.Point(347, 12);
			this.btnGCNGUpdate.Name = "btnGCNGUpdate";
			this.btnGCNGUpdate.Size = new System.Drawing.Size(278, 23);
			this.btnGCNGUpdate.TabIndex = 5;
			this.btnGCNGUpdate.Text = "btnGCNGUpdate";
			this.btnGCNGUpdate.UseVisualStyleBackColor = true;
			this.btnGCNGUpdate.Click += new System.EventHandler(this.BtnGCNGUpdateClick);
			// 
			// btnGCNGMap
			// 
			this.btnGCNGMap.Location = new System.Drawing.Point(347, 41);
			this.btnGCNGMap.Name = "btnGCNGMap";
			this.btnGCNGMap.Size = new System.Drawing.Size(278, 23);
			this.btnGCNGMap.TabIndex = 6;
			this.btnGCNGMap.Text = "btnGCNGMap";
			this.btnGCNGMap.UseVisualStyleBackColor = true;
			this.btnGCNGMap.Click += new System.EventHandler(this.BtnGCNGMapClick);
			// 
			// NotificationGroup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(637, 612);
			this.Controls.Add(this.btnGCNGMap);
			this.Controls.Add(this.btnGCNGUpdate);
			this.Controls.Add(this.btnGCNGToggle);
			this.Controls.Add(this.btnGCNGDelete);
			this.Controls.Add(this.lvGCNListGroup);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NotificationGroup";
			this.Text = "NotificationGroup";
			this.ResumeLayout(false);

	        this.btnGCNGDelete.TabIndex = 3;
			this.btnGCNGDelete.Text = "btnGCNGDelete";
			this.btnGCNGDelete.UseVisualStyleBackColor = true;
			// 
			// btnGCNGToggle
			// 
			this.btnGCNGToggle.Location = new System.Drawing.Point(12, 41);
			this.btnGCNGToggle.Name = "btnGCNGToggle";
			this.btnGCNGToggle.Size = new System.Drawing.Size(278, 23);
			this.btnGCNGToggle.TabIndex = 4;
			this.btnGCNGToggle.Text = "btnGCNGToggle";
			this.btnGCNGToggle.UseVisualStyleBackColor = true;
			// 
			// btnGCNGUpdate
			// 
			this.btnGCNGUpdate.Location = new System.Drawing.Point(347, 12);
			this.btnGCNGUpdate.Name = "btnGCNGUpdate";
			this.btnGCNGUpdate.Size = new System.Drawing.Size(278, 23);
			this.btnGCNGUpdate.TabIndex = 5;
			this.btnGCNGUpdate.Text = "btnGCNGUpdate";
			this.btnGCNGUpdate.UseVisualStyleBackColor = true;
			// 
			// btnGCNGMap
			// 
			this.btnGCNGMap.Location = new System.Drawing.Point(347, 41);
			this.btnGCNGMap.Name = "btnGCNGMap";
			this.btnGCNGMap.Size = new System.Drawing.Size(278, 23);
			this.btnGCNGMap.TabIndex = 6;
			this.btnGCNGMap.Text = "btnGCNGMap";
			this.btnGCNGMap.UseVisualStyleBackColor = true;
			// 
			// NotificationGroup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(637, 612);
			this.Controls.Add(this.btnGCNGMap);
			this.Controls.Add(this.btnGCNGUpdate);
			this.Controls.Add(this.btnGCNGToggle);
			this.Controls.Add(this.btnGCNGDelete);
			this.Controls.Add(this.lvGCNListGroup);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NotificationGroup";
			this.Text = "NotificationGroup";
			this.ResumeLayout(false);

		}
	}
}
