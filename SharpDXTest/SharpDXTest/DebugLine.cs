using SharpHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{
  class DebugLine:MMDModel
  {
    List<int> Depth = new List<int>();
    List<int> Near = new List<int>();

    public DebugLine(string path):base(path)
    {
    }

    void Ind()
    {
      for (int i = 0; i < Vertice.Length; i++)
      {
        if (Vertice[i].Position.Z < -20)
        {
          Depth.Add(i);
        }
        else
        {
          Near.Add(i);
        }
      }
    }

    public void AfterLoaded()
    {
      Ind();
    }

    public void OnClicked(Mouse mouse,RayWrap ray)
    {
      for (int i = 0; i < Depth.Count; i++)
      {

        Vertice[Depth[i]].Position = ray.To;
      }
      foreach (var i in Near)
      {
        //i.SetPosition(ray.From);
      }
      
      Mesh.SetOnly(Vertice, Index.ToArray());
    }
  }
}
