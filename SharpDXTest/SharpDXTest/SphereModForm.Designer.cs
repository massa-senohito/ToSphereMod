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
      this.menuStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.FactorBar)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.RadiusBar)).BeginInit();
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
      this.menuStrip1.Size = new System.Drawing.Size(284, 24);
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
      this.label2.Location = new System.Drawing.Point(12, 128);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(40, 12);
      this.label2.TabIndex = 5;
      this.label2.Text = "Radius";
      // 
      // RadiusBox
      // 
      this.RadiusBox.Location = new System.Drawing.Point(65, 125);
      this.RadiusBox.Name = "RadiusBox";
      this.RadiusBox.Size = new System.Drawing.Size(100, 19);
      this.RadiusBox.TabIndex = 4;
      // 
      // OffsetBox
      // 
      this.OffsetBox.Location = new System.Drawing.Point(65, 250);
      this.OffsetBox.Name = "OffsetBox";
      this.OffsetBox.Size = new System.Drawing.Size(100, 19);
      this.OffsetBox.TabIndex = 6;
      this.OffsetBox.Text = "0,0,0";
      // 
      // BoneBox
      // 
      this.BoneBox.Location = new System.Drawing.Point(65, 286);
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
      this.RadiusBar.Location = new System.Drawing.Point(65, 150);
      this.RadiusBar.Maximum = 1000;
      this.RadiusBar.Name = "RadiusBar";
      this.RadiusBar.Size = new System.Drawing.Size(104, 45);
      this.RadiusBar.TabIndex = 9;
      // 
      // SphereModForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 330);
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
  }
}