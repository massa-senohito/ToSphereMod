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
			this.YRot = new System.Windows.Forms.TextBox();
			this.ZRot = new System.Windows.Forms.TextBox();
			this.Lat0 = new System.Windows.Forms.TextBox();
			this.YScale = new System.Windows.Forms.TextBox();
			this.ZScale = new System.Windows.Forms.TextBox();
			this.XScale = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// YRot
			// 
			this.YRot.Location = new System.Drawing.Point(121, 12);
			this.YRot.Name = "YRot";
			this.YRot.Size = new System.Drawing.Size(103, 19);
			this.YRot.TabIndex = 28;
			this.YRot.Text = "0";
			// 
			// ZRot
			// 
			this.ZRot.Location = new System.Drawing.Point(230, 12);
			this.ZRot.Name = "ZRot";
			this.ZRot.Size = new System.Drawing.Size(103, 19);
			this.ZRot.TabIndex = 27;
			this.ZRot.Text = "0";
			// 
			// Lat0
			// 
			this.Lat0.Location = new System.Drawing.Point(12, 12);
			this.Lat0.Name = "Lat0";
			this.Lat0.Size = new System.Drawing.Size(103, 19);
			this.Lat0.TabIndex = 26;
			this.Lat0.Text = "0";
			// 
			// YScale
			// 
			this.YScale.Location = new System.Drawing.Point(121, 37);
			this.YScale.Name = "YScale";
			this.YScale.Size = new System.Drawing.Size(103, 19);
			this.YScale.TabIndex = 25;
			this.YScale.Text = "1";
			// 
			// ZScale
			// 
			this.ZScale.Location = new System.Drawing.Point(230, 37);
			this.ZScale.Name = "ZScale";
			this.ZScale.Size = new System.Drawing.Size(103, 19);
			this.ZScale.TabIndex = 24;
			this.ZScale.Text = "1";
			// 
			// XScale
			// 
			this.XScale.Location = new System.Drawing.Point(12, 37);
			this.XScale.Name = "XScale";
			this.XScale.Size = new System.Drawing.Size(103, 19);
			this.XScale.TabIndex = 23;
			this.XScale.Text = "1";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(121, 62);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(103, 19);
			this.textBox1.TabIndex = 31;
			this.textBox1.Text = "1";
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(230, 62);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(103, 19);
			this.textBox2.TabIndex = 30;
			this.textBox2.Text = "1";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(12, 62);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(103, 19);
			this.textBox3.TabIndex = 29;
			this.textBox3.Text = "1";
			// 
			// LatticeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(410, 172);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textBox3);
			this.Controls.Add(this.YRot);
			this.Controls.Add(this.ZRot);
			this.Controls.Add(this.Lat0);
			this.Controls.Add(this.YScale);
			this.Controls.Add(this.ZScale);
			this.Controls.Add(this.XScale);
			this.Name = "LatticeForm";
			this.Text = "LatticeForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox YRot;
		private System.Windows.Forms.TextBox ZRot;
		private System.Windows.Forms.TextBox Lat0;
		private System.Windows.Forms.TextBox YScale;
		private System.Windows.Forms.TextBox ZScale;
		private System.Windows.Forms.TextBox XScale;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.TextBox textBox3;
	}
}