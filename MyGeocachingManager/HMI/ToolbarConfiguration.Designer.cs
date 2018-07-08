/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 15/09/2016
 * Time: 11:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MyGeocachingManager.HMI
{
	partial class ToolbarConfiguration
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button tbbtnMoveDown;
		private System.Windows.Forms.Button tbbtnMoveUp;
		private System.Windows.Forms.Button tbbtnAdd;
		private System.Windows.Forms.Button tbbtnRemove;
		private System.Windows.Forms.Button tbbtnCancel;
		private System.Windows.Forms.Button tbbtnSave;
		private System.Windows.Forms.ListView tblvAvail;
		private System.Windows.Forms.ListView tblvAdded;
		
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
			this.tbbtnMoveDown = new System.Windows.Forms.Button();
			this.tbbtnMoveUp = new System.Windows.Forms.Button();
			this.tbbtnAdd = new System.Windows.Forms.Button();
			this.tbbtnRemove = new System.Windows.Forms.Button();
			this.tbbtnCancel = new System.Windows.Forms.Button();
			this.tbbtnSave = new System.Windows.Forms.Button();
			this.tblvAvail = new System.Windows.Forms.ListView();
			this.tblvAdded = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// tbbtnMoveDown
			// 
			this.tbbtnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tbbtnMoveDown.Image = global::MyGeocachingManager.Properties.Resources.Down;
			this.tbbtnMoveDown.Location = new System.Drawing.Point(686, 292);
			this.tbbtnMoveDown.Name = "tbbtnMoveDown";
			this.tbbtnMoveDown.Size = new System.Drawing.Size(40, 40);
			this.tbbtnMoveDown.TabIndex = 9;
			this.tbbtnMoveDown.UseVisualStyleBackColor = true;
			this.tbbtnMoveDown.Click += new System.EventHandler(this.TbbtnMoveDownClick);
			// 
			// tbbtnMoveUp
			// 
			this.tbbtnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tbbtnMoveUp.Image = global::MyGeocachingManager.Properties.Resources.Up;
			this.tbbtnMoveUp.Location = new System.Drawing.Point(686, 152);
			this.tbbtnMoveUp.Name = "tbbtnMoveUp";
			this.tbbtnMoveUp.Size = new System.Drawing.Size(40, 40);
			this.tbbtnMoveUp.TabIndex = 8;
			this.tbbtnMoveUp.UseVisualStyleBackColor = true;
			this.tbbtnMoveUp.Click += new System.EventHandler(this.TbbtnMoveUpClick);
			// 
			// tbbtnAdd
			// 
			this.tbbtnAdd.Image = global::MyGeocachingManager.Properties.Resources.Right;
			this.tbbtnAdd.Location = new System.Drawing.Point(323, 251);
			this.tbbtnAdd.Name = "tbbtnAdd";
			this.tbbtnAdd.Size = new System.Drawing.Size(40, 40);
			this.tbbtnAdd.TabIndex = 11;
			this.tbbtnAdd.UseVisualStyleBackColor = true;
			this.tbbtnAdd.Click += new System.EventHandler(this.TbbtnAddClick);
			// 
			// tbbtnRemove
			// 
			this.tbbtnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tbbtnRemove.Image = global::MyGeocachingManager.Properties.Resources.Left;
			this.tbbtnRemove.Location = new System.Drawing.Point(323, 185);
			this.tbbtnRemove.Name = "tbbtnRemove";
			this.tbbtnRemove.Size = new System.Drawing.Size(40, 40);
			this.tbbtnRemove.TabIndex = 10;
			this.tbbtnRemove.UseVisualStyleBackColor = true;
			this.tbbtnRemove.Click += new System.EventHandler(this.TbbtnRemoveClick);
			// 
			// tbbtnCancel
			// 
			this.tbbtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.tbbtnCancel.Location = new System.Drawing.Point(12, 476);
			this.tbbtnCancel.Name = "tbbtnCancel";
			this.tbbtnCancel.Size = new System.Drawing.Size(106, 57);
			this.tbbtnCancel.TabIndex = 12;
			this.tbbtnCancel.Text = "#cancel";
			this.tbbtnCancel.UseVisualStyleBackColor = true;
			this.tbbtnCancel.Click += new System.EventHandler(this.TbbtnCancelClick);
			// 
			// tbbtnSave
			// 
			this.tbbtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.tbbtnSave.Location = new System.Drawing.Point(620, 476);
			this.tbbtnSave.Name = "tbbtnSave";
			this.tbbtnSave.Size = new System.Drawing.Size(106, 57);
			this.tbbtnSave.TabIndex = 13;
			this.tbbtnSave.Text = "#save";
			this.tbbtnSave.UseVisualStyleBackColor = true;
			this.tbbtnSave.Click += new System.EventHandler(this.TbbtnSaveClick);
			// 
			// tblvAvail
			// 
			this.tblvAvail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left)));
			this.tblvAvail.FullRowSelect = true;
			this.tblvAvail.GridLines = true;
			this.tblvAvail.HideSelection = false;
			this.tblvAvail.Location = new System.Drawing.Point(12, 12);
			this.tblvAvail.Name = "tblvAvail";
			this.tblvAvail.Size = new System.Drawing.Size(306, 448);
			this.tblvAvail.TabIndex = 14;
			this.tblvAvail.UseCompatibleStateImageBehavior = false;
			this.tblvAvail.View = System.Windows.Forms.View.Details;
			// 
			// tblvAdded
			// 
			this.tblvAdded.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tblvAdded.FullRowSelect = true;
			this.tblvAdded.GridLines = true;
			this.tblvAdded.HideSelection = false;
			this.tblvAdded.Location = new System.Drawing.Point(370, 12);
			this.tblvAdded.Name = "tblvAdded";
			this.tblvAdded.Size = new System.Drawing.Size(306, 448);
			this.tblvAdded.TabIndex = 15;
			this.tblvAdded.UseCompatibleStateImageBehavior = false;
			this.tblvAdded.View = System.Windows.Forms.View.Details;
			// 
			// ToolbarConfiguration
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(739, 546);
			this.Controls.Add(this.tblvAdded);
			this.Controls.Add(this.tblvAvail);
			this.Controls.Add(this.tbbtnSave);
			this.Controls.Add(this.tbbtnCancel);
			this.Controls.Add(this.tbbtnAdd);
			this.Controls.Add(this.tbbtnRemove);
			this.Controls.Add(this.tbbtnMoveDown);
			this.Controls.Add(this.tbbtnMoveUp);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToolbarConfiguration";
			this.Text = "ToolbarConfiguration";
			this.ResumeLayout(false);

		}
	}
}
