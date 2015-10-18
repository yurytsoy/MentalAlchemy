namespace audiotest
{
	partial class MainForm
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
			this.RecordBtn = new System.Windows.Forms.Button();
			this.StopBtn = new System.Windows.Forms.Button();
			this.OutputBox = new System.Windows.Forms.TextBox();
			this.OutputList = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// RecordBtn
			// 
			this.RecordBtn.Location = new System.Drawing.Point(12, 12);
			this.RecordBtn.Name = "RecordBtn";
			this.RecordBtn.Size = new System.Drawing.Size(75, 23);
			this.RecordBtn.TabIndex = 0;
			this.RecordBtn.Text = "Record";
			this.RecordBtn.UseVisualStyleBackColor = true;
			this.RecordBtn.Click += new System.EventHandler(this.RecordBtn_Click);
			// 
			// StopBtn
			// 
			this.StopBtn.Location = new System.Drawing.Point(104, 12);
			this.StopBtn.Name = "StopBtn";
			this.StopBtn.Size = new System.Drawing.Size(75, 23);
			this.StopBtn.TabIndex = 1;
			this.StopBtn.Text = "Stop";
			this.StopBtn.UseVisualStyleBackColor = true;
			this.StopBtn.Click += new System.EventHandler(this.StopBtn_Click);
			// 
			// OutputBox
			// 
			this.OutputBox.Location = new System.Drawing.Point(12, 41);
			this.OutputBox.Multiline = true;
			this.OutputBox.Name = "OutputBox";
			this.OutputBox.Size = new System.Drawing.Size(322, 209);
			this.OutputBox.TabIndex = 2;
			// 
			// OutputList
			// 
			this.OutputList.FormattingEnabled = true;
			this.OutputList.Location = new System.Drawing.Point(340, 41);
			this.OutputList.Name = "OutputList";
			this.OutputList.Size = new System.Drawing.Size(317, 212);
			this.OutputList.TabIndex = 3;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(669, 262);
			this.Controls.Add(this.OutputList);
			this.Controls.Add(this.OutputBox);
			this.Controls.Add(this.StopBtn);
			this.Controls.Add(this.RecordBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "MainForm";
			this.Text = "MainForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button RecordBtn;
		private System.Windows.Forms.Button StopBtn;
		private System.Windows.Forms.TextBox OutputBox;
		private System.Windows.Forms.ListBox OutputList;
	}
}

