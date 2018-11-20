namespace SharpDXTest
{
	partial class LatticeForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			ResourceDispose( );
			if ( disposing && ( components != null ) )
			{
				components.Dispose( );
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Lat1 = new System.Windows.Forms.TextBox();
			this.Lat2 = new System.Windows.Forms.TextBox();
			this.Lat0 = new System.Windows.Forms.TextBox();
			this.Lat4 = new System.Windows.Forms.TextBox();
			this.Lat5 = new System.Windows.Forms.TextBox();
			this.Lat3 = new System.Windows.Forms.TextBox();
			this.Lat7 = new System.Windows.Forms.TextBox();
			this.Lat8 = new System.Windows.Forms.TextBox();
			this.Lat6 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// Lat1
			// 
			this.Lat1.Location = new System.Drawing.Point(121, 12);
			this.Lat1.Name = "Lat1";
			this.Lat1.Size = new System.Drawing.Size(103, 19);
			this.Lat1.TabIndex = 28;
			this.Lat1.Text = "0";
			// 
			// Lat2
			// 
			this.Lat2.Location = new System.Drawing.Point(230, 12);
			this.Lat2.Name = "Lat2";
			this.Lat2.Size = new System.Drawing.Size(103, 19);
			this.Lat2.TabIndex = 27;
			this.Lat2.Text = "0";
			// 
			// Lat0
			// 
			this.Lat0.Location = new System.Drawing.Point(12, 12);
			this.Lat0.Name = "Lat0";
			this.Lat0.Size = new System.Drawing.Size(103, 19);
			this.Lat0.TabIndex = 26;
			this.Lat0.Text = "0";
			// 
			// Lat4
			// 
			this.Lat4.Location = new System.Drawing.Point(121, 37);
			this.Lat4.Name = "Lat4";
			this.Lat4.Size = new System.Drawing.Size(103, 19);
			this.Lat4.TabIndex = 25;
			this.Lat4.Text = "1";
			// 
			// Lat5
			// 
			this.Lat5.Location = new System.Drawing.Point(230, 37);
			this.Lat5.Name = "Lat5";
			this.Lat5.Size = new System.Drawing.Size(103, 19);
			this.Lat5.TabIndex = 24;
			this.Lat5.Text = "1";
			// 
			// Lat3
			// 
			this.Lat3.Location = new System.Drawing.Point(12, 37);
			this.Lat3.Name = "Lat3";
			this.Lat3.Size = new System.Drawing.Size(103, 19);
			this.Lat3.TabIndex = 23;
			this.Lat3.Text = "1";
			// 
			// Lat7
			// 
			this.Lat7.Location = new System.Drawing.Point(121, 62);
			this.Lat7.Name = "Lat7";
			this.Lat7.Size = new System.Drawing.Size(103, 19);
			this.Lat7.TabIndex = 31;
			this.Lat7.Text = "1";
			// 
			// Lat8
			// 
			this.Lat8.Location = new System.Drawing.Point(230, 62);
			this.Lat8.Name = "Lat8";
			this.Lat8.Size = new System.Drawing.Size(103, 19);
			this.Lat8.TabIndex = 30;
			this.Lat8.Text = "1";
			// 
			// Lat6
			// 
			this.Lat6.Location = new System.Drawing.Point(12, 62);
			this.Lat6.Name = "Lat6";
			this.Lat6.Size = new System.Drawing.Size(103, 19);
			this.Lat6.TabIndex = 29;
			this.Lat6.Text = "1";
			// 
			// LatticeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(410, 172);
			this.Controls.Add(this.Lat7);
			this.Controls.Add(this.Lat8);
			this.Controls.Add(this.Lat6);
			this.Controls.Add(this.Lat1);
			this.Controls.Add(this.Lat2);
			this.Controls.Add(this.Lat0);
			this.Controls.Add(this.Lat4);
			this.Controls.Add(this.Lat5);
			this.Controls.Add(this.Lat3);
			this.Name = "LatticeForm";
			this.Text = "LatticeForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox Lat1;
		private System.Windows.Forms.TextBox Lat2;
		private System.Windows.Forms.TextBox Lat0;
		private System.Windows.Forms.TextBox Lat4;
		private System.Windows.Forms.TextBox Lat5;
		private System.Windows.Forms.TextBox Lat3;
		private System.Windows.Forms.TextBox Lat7;
		private System.Windows.Forms.TextBox Lat8;
		private System.Windows.Forms.TextBox Lat6;
	}
}