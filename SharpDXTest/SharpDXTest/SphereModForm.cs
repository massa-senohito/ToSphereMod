using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using V3 = SharpDX.Vector3;

namespace BlenderModifier
{
  public partial class SphereModForm : Form
  {
    public EventHandler OnUpdate;

    public SphereModForm()
    {
      InitializeComponent();
      FactorBox.Text = "0.3";
      RadiusBox.Text = "2";
    }

    public void SetFactorBoxChanged(EventHandler f)
    {
      FactorBox.TextChanged += f;
    }

    public void SetRadiusBoxChanged(EventHandler f)
    {
      RadiusBox.TextChanged += f;
    }

    public void SetOffsetBoxChanged(EventHandler f)
    {
      OffsetBox.TextChanged += f;
    }

    public V3 GetOffset()
    {
      var tmp = OffsetBox.Text;
      var arr = tmp.Split(',');
      var flArr = arr.Select(float.Parse).ToArray();
      return new V3(flArr[0], flArr[1], flArr[2]);
    }
    public int BoneID
    {
      get
      {
        var success = int.TryParse(BoneBox.Text, out int result);
        if (success)
        {
          return result;
        }
        else
        {
          return 0;
        }
      }
    }
    public float Factor
    {
      get
      {
        var success = float.TryParse(FactorBox.Text, out float result);
        return result;
      }
    }
    public float Radius
    {
      get
      {
        var success = float.TryParse(RadiusBox.Text, out float result);
        return result;
      }
    }
    public void AddDebug(string message)
    {
      textBox1.AppendText(message + "\n");
    }

    private void updateToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OnUpdate?.Invoke(sender, e);
    }
  }
}

