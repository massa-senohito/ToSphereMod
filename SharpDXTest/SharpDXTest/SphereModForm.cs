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
      FactorBar.ValueChanged += FactorBar_ValueChanged;
      FactorBar.Value = 30;
      RadiusBar.ValueChanged += RadiusBar_ValueChanged;
      RadiusBar.Value = 200;
      UIAlphaBar.Value = 100;
    }

    private void RadiusBar_ValueChanged(object sender, EventArgs e)
    {
      Radius = RadiusBar.Value * 0.01f; ;
    }

    private void FactorBar_ValueChanged(object sender, EventArgs e)
    {
      Factor = FactorBar.Value * 0.01f;
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

    public void SetAlphaBarChanged(EventHandler f)
    {
      UIAlphaBar.ValueChanged += f;
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
      private set
      {
        FactorBox.Text = value.ToString();
      }
    }
    public float Radius
    {
      get
      {
        var success = float.TryParse(RadiusBox.Text, out float result);
        return result;
      }
      private set
      {
        RadiusBox.Text = value.ToString();
      }
    }

    public float Alpha
    {
      get
      {
        var v = UIAlphaBar.Value;
        return v * 0.01f;
      }
    }

    private void updateToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OnUpdate?.Invoke(sender, e);
    }
  }
}

