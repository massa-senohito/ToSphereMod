namespace BlenderModifier
{
  partial class SphereModForm
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
			this.components = new System.ComponentModel.Container();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FactorBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.RadiusBox = new System.Windows.Forms.TextBox();
			this.OffsetBox = new System.Windows.Forms.TextBox();
			this.BoneBox = new System.Windows.Forms.TextBox();
			this.FactorBar = new System.Windows.Forms.TrackBar();
			this.RadiusBar = new System.Windows.Forms.TrackBar();
			this.UIAlphaBar = new System.Windows.Forms.TrackBar();
			this.UIAlpha = new System.Windows.Forms.Label();
			this.MorphNameBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.modFormModelBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.label5 = new System.Windows.Forms.Label();
			this.XScale = new System.Windows.Forms.TextBox();
			this.ZScale = new System.Windows.Forms.TextBox();
			this.YScale = new System.Windows.Forms.TextBox();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FactorBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RadiusBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UIAlphaBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.modFormModelBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.updateToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(429, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
			this.saveToolStripMenuItem.Text = "Save";
			// 
			// loadToolStripMenuItem
			// 
			this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
			this.loadToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
			this.loadToolStripMenuItem.Text = "Load";
			// 
			// updateToolStripMenuItem
			// 
			this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
			this.updateToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
			this.updateToolStripMenuItem.Text = "Update";
			this.updateToolStripMenuItem.Click += new System.EventHandler(this.updateToolStripMenuItem_Click);
			// 
			// FactorBox
			// 
			this.FactorBox.Location = new System.Drawing.Point(65, 31);
			this.FactorBox.Name = "FactorBox";
			this.FactorBox.Size = new System.Drawing.Size(100, 19);
			this.FactorBox.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 34);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 12);
			this.label1.TabIndex = 3;
			this.label1.Text = "Factor";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 110);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 12);
			this.label2.TabIndex = 5;
			this.label2.Text = "Radius";
			// 
			// RadiusBox
			// 
			this.RadiusBox.Location = new System.Drawing.Point(65, 107);
			this.RadiusBox.Name = "RadiusBox";
			this.RadiusBox.Size = new System.Drawing.Size(100, 19);
			this.RadiusBox.TabIndex = 4;
			// 
			// OffsetBox
			// 
			this.OffsetBox.Location = new System.Drawing.Point(132, 286);
			this.OffsetBox.Name = "OffsetBox";
			this.OffsetBox.Size = new System.Drawing.Size(285, 19);
			this.OffsetBox.TabIndex = 6;
			this.OffsetBox.Text = "0,0,0";
			// 
			// BoneBox
			// 
			this.BoneBox.Location = new System.Drawing.Point(26, 286);
			this.BoneBox.Name = "BoneBox";
			this.BoneBox.Size = new System.Drawing.Size(100, 19);
			this.BoneBox.TabIndex = 7;
			this.BoneBox.Text = "0";
			// 
			// FactorBar
			// 
			this.FactorBar.Location = new System.Drawing.Point(65, 56);
			this.FactorBar.Maximum = 100;
			this.FactorBar.Name = "FactorBar";
			this.FactorBar.Size = new System.Drawing.Size(104, 45);
			this.FactorBar.TabIndex = 8;
			// 
			// RadiusBar
			// 
			this.RadiusBar.Location = new System.Drawing.Point(65, 139);
			this.RadiusBar.Maximum = 1000;
			this.RadiusBar.Name = "RadiusBar";
			this.RadiusBar.Size = new System.Drawing.Size(104, 45);
			this.RadiusBar.TabIndex = 9;
			// 
			// UIAlphaBar
			// 
			this.UIAlphaBar.Location = new System.Drawing.Point(65, 185);
			this.UIAlphaBar.Maximum = 100;
			this.UIAlphaBar.Name = "UIAlphaBar";
			this.UIAlphaBar.Size = new System.Drawing.Size(104, 45);
			this.UIAlphaBar.TabIndex = 10;
			// 
			// UIAlpha
			// 
			this.UIAlpha.AutoSize = true;
			this.UIAlpha.Location = new System.Drawing.Point(12, 185);
			this.UIAlpha.Name = "UIAlpha";
			this.UIAlpha.Size = new System.Drawing.Size(45, 12);
			this.UIAlpha.TabIndex = 11;
			this.UIAlpha.Text = "UIAlpha";
			// 
			// MorphNameBox
			// 
			this.MorphNameBox.Location = new System.Drawing.Point(132, 251);
			this.MorphNameBox.Name = "MorphNameBox";
			this.MorphNameBox.Size = new System.Drawing.Size(285, 19);
			this.MorphNameBox.TabIndex = 12;
			this.MorphNameBox.Text = "MorphName";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(26, 251);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(65, 12);
			this.label3.TabIndex = 13;
			this.label3.Text = "morphName";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.ForeColor = System.Drawing.Color.Red;
			this.label4.Location = new System.Drawing.Point(192, 217);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(35, 12);
			this.label4.TabIndex = 14;
			this.label4.Text = "label4";
			// 
			// modFormModelBindingSource
			// 
			this.modFormModelBindingSource.DataSource = typeof(SharpDXTest.ModFormModel);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(26, 329);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(32, 12);
			this.label5.TabIndex = 15;
			this.label5.Text = "scale";
			// 
			// XScale
			// 
			this.XScale.Location = new System.Drawing.Point(91, 322);
			this.XScale.Name = "XScale";
			this.XScale.Size = new System.Drawing.Size(103, 19);
			this.XScale.TabIndex = 16;
			this.XScale.Text = "0";
			// 
			// ZScale
			// 
			this.ZScale.Location = new System.Drawing.Point(309, 322);
			this.ZScale.Name = "ZScale";
			this.ZScale.Size = new System.Drawing.Size(103, 19);
			this.ZScale.TabIndex = 17;
			this.ZScale.Text = "0";
			// 
			// YScale
			// 
			this.YScale.Location = new System.Drawing.Point(200, 322);
			this.YScale.Name = "YScale";
			this.YScale.Size = new System.Drawing.Size(103, 19);
			this.YScale.TabIndex = 18;
			this.YScale.Text = "0";
			// 
			// SphereModForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(429, 369);
			this.Controls.Add(this.YScale);
			this.Controls.Add(this.ZScale);
			this.Controls.Add(this.XScale);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.MorphNameBox);
			this.Controls.Add(this.UIAlpha);
			this.Controls.Add(this.UIAlphaBar);
			this.Controls.Add(this.RadiusBar);
			this.Controls.Add(this.FactorBar);
			this.Controls.Add(this.BoneBox);
			this.Controls.Add(this.OffsetBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.RadiusBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.FactorBox);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "SphereModForm";
			this.Text = "SphereModForm";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FactorBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RadiusBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UIAlphaBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.modFormModelBindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;
    private System.Windows.Forms.TextBox FactorBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox RadiusBox;
    private System.Windows.Forms.TextBox OffsetBox;
    private System.Windows.Forms.TextBox BoneBox;
    private System.Windows.Forms.TrackBar FactorBar;
    private System.Windows.Forms.TrackBar RadiusBar;
    private System.Windows.Forms.TrackBar UIAlphaBar;
    private System.Windows.Forms.Label UIAlpha;
		private System.Windows.Forms.TextBox MorphNameBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.BindingSource modFormModelBindingSource;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox XScale;
		private System.Windows.Forms.TextBox ZScale;
		private System.Windows.Forms.TextBox YScale;
	}
}