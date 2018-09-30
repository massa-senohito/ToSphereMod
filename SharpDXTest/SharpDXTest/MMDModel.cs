using BlenderModifier;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpHelper;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vert = SharpHelper.TexturedVertex;

namespace SharpDXTest
{
  public class Material
  {
    //;Material,材質名,材質名(英),拡散色_R,拡散色_G,拡散色_B,拡散色_A(非透過度),反射色_R,反射色_G,反射色_B,反射強度,環境色_R,環境色_G,環境色_B,両面描画(0/1),地面影(0/1),セルフ影マップ(0/1),セルフ影(0/1),頂点色(0/1),描画(0:Tri/1:Point/2:Line),エッジ(0/1),エッジサイズ,エッジ色_R,エッジ色_G,エッジ色_B,エッジ色_A,テクスチャパス,スフィアテクスチャパス,スフィアモード(0:無効/1:乗算/2:加算/3:サブテクスチャ),Toonテクスチャパス,メモ
    //Material,"スカート腕ヘドフォン","en",1,1,1,1,0,0,0,50,0.4,0.4,0.4,1,1,1,1,0,0,1,0.6,0.3,0.2,0.4,0.6,"k_huku1.png","body00_s.bmp",2,"toon_defo.bmp",""
    //;Face,親材質名,面Index,頂点Index1,頂点Index2,頂点Index3
    //Face,"スカート腕ヘドフォン",0,858,840,855
    public string Name;
    public string TexName;
    public IEnumerable<Vert> Vertice;
    public IEnumerable<int[]> Faces;
    public int[] FlattenFace;
    public Material(string line)
    {
      var csv = line.Split(',');
      Name = csv[1];
      TexName = csv[26];
      TexName = TexName.Replace("\"", "");
    }

    private IEnumerable<Vert> GetVertex(Vert[] vert)
    {
      foreach (var i in Faces)
      {
        yield return vert[i[0]];
        yield return vert[i[1]];
        yield return vert[i[2]];
      }
    }

    public static IEnumerable<Material> MakeFromCSV(IEnumerable<string> lines,Dictionary<string,List<string>> dictionary,Vert[] verts)
    {
      foreach (var item in lines)
      {
        var mat = new Material(item);
        var facecsv = dictionary[mat.Name];
        mat.Faces = Util.ParseFaceCSVAll(facecsv);
        mat.Vertice = mat.GetVertex(verts);
        mat.FlattenFace = mat.Faces.SelectMany(x => x).ToArray();
        yield return mat;
      }
    }
    public override string ToString()
    {
      return Name + " : " + TexName;
    }
  }
  public class MMDModel : System.IDisposable
  {
    public Vert[] Vertice;
    public int[] Index;
    public Face[] Faces;
    public IEnumerable<Material> Materials;
    SphereCast cast;

    SharpMesh GetSharpMesh(SharpDevice device)
    {
      var mesh = new SharpMesh(device);
      List<int> indices = new List<int>();
      List<Face> faces = new List<Face>();
      int icount = 0;
      foreach (var item in Materials)
      {
        indices.AddRange(item.FlattenFace);
        int faceCount = item.FlattenFace.Count();
        mesh.SubSets.Add(new SharpSubSet()
        {
          IndexCount = faceCount,
          StartIndex = icount,
          DiffuseMap = device.LoadTextureFromFile(item.TexName)
        });

        for (var i = icount; i < icount+faceCount; i += 3)
        {
          Face item1 = new Face(Vertice[indices[i]].Position, Vertice[indices[i + 1]].Position, Vertice[indices[i + 2]].Position, item.Name);
          //Debug.WriteLine("first " + item1.TriString);
          faces.Add(item1);

        }
        icount += faceCount;
      }
      Index = indices.ToArray();
      Faces = faces.ToArray();
      //Faces = new Face[Index.Length / 3];
      //for (int i = 0; i < Index.Length; i += 3)
      //{
      //  Face face = new Face(Vertice[Index[i]].Position, Vertice[Index[i + 1]].Position, Vertice[Index[i + 2]].Position, "");
      //    Debug.WriteLine("second " + face.TriString);
      //  Faces[i / 3] = face;
      //}
      // 毎フレーム送るにはおもすぎる
      ModelStr = Faces.Select(f => f.TriString).ConcatStr();
      mesh.SetOnly(Vertice, Index);
      return mesh;
    }

    public MMDModel(string path)
    {
      var lines = File.ReadAllLines(path);
      var gr = lines.GroupBy(l => l.Split(',')[0]);
      var gs = gr.Where(g => !g.Key.Contains(";")).ToDictionary(s=>s.Key,g=>g.ToList());

      Vertice = ParseCSV(gs["Vertex"]).ToArray();
      var faceGr = gs["Face"].GroupBy(s => s.Split(',')[1]).ToDictionary(s=>s.Key,g=>g.ToList());
      Materials = Material.MakeFromCSV(gs["Material"], faceGr , Vertice );

      cast = new SphereCast(Matrix.Zero);
    }

    SharpMesh Mesh;
    Buffer Buffer;
    SharpShader Shader;
    VertexShaderStage VertexShader;
    PixelShaderStage PixelShader;

    public void LoadTexture(SharpDevice device)
    {

      Mesh = GetSharpMesh(device);

      //init shader
      Shader = new SharpShader(device, "../../HLSLModel.txt",
          new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
          new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
          });
      Buffer = Shader.CreateBuffer<Matrix>();
      VertexShader = device.DeviceContext.VertexShader;
      PixelShader = device.DeviceContext.PixelShader;

    }
    public void Update(SharpDevice device, Matrix worldViewProjection)
    {
      //apply shader
      Shader.Apply();

      //apply constant buffer to shader
      VertexShader.SetConstantBuffer(0, Buffer);

      device.UpdateData<Matrix>(Buffer, worldViewProjection);
      Mesh.Begin();
      for (int i = 0; i < Mesh.SubSets.Count; i++)
      {
        device.DeviceContext.PixelShader.SetShaderResource(0, Mesh.SubSets[i].DiffuseMap);
        //set texture
        Mesh.Draw(i);
      }

    }
    public string ModelStr
    {
      get; private set;

    }
    public static IEnumerable<TexturedVertex> ParseCSV(IEnumerable<string> lines)
    {
      //; Vertex,頂点Index,位置_x,位置_y,位置_z,法線_x,法線_y,法線_z,エッジ倍率,UV_u,UV_v,追加UV1_x,追加UV1_y,追加UV1_z,追加UV1_w,追加UV2_x,追加UV2_y,追加UV2_z,追加UV2_w,追加UV3_x,追加UV3_y,追加UV3_z,追加UV3_w,追加UV4_x,追加UV4_y,追加UV4_z,追加UV4_w,ウェイト変形タイプ(0:BDEF1 / 1:BDEF2 / 2:BDEF4 / 3:SDEF / 4:QDEF),ウェイト1_ボーン名,ウェイト1_ウェイト値,ウェイト2_ボーン名,ウェイト2_ウェイト値,ウェイト3_ボーン名,ウェイト3_ウェイト値,ウェイト4_ボーン名,ウェイト4_ウェイト値,C_x,C_y,C_z,R0_x,R0_y,R0_z,R1_x,R1_y,R1_z
      //   Vertex,0,0.3916405,16.48059,-0.7562667,0.383015,0.4676141,-0.7966408,1,0.8393391,0.7603291,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"上半身2",1,"",0,"",0,"",0,0,0,0,0,0,0,0,0,0

      foreach (var item in lines)
      {
        var csv = item.Split(',');
        yield return new Vert(new Vector3(csv[2].Float(), csv[3].Float(), csv[4].Float()), new Vector2(csv[9].Float(), csv[10].Float()));
      }
    }

    public void ToSphere(Vector3 pos)
    {
      cast.Offset = pos;
      var vs = Vertice.Select(v => v.Position).ToArray();
      var vd = cast.GetSpereUntilEnd(vs);
      for (int i = 0; i < vd.Length; i++)
      {
        Vertice[i].Position = vd[i];
      }
      Mesh.SetOnly(Vertice, Index.ToArray());
    }

    public IEnumerable<HitResult> HitPos(RayWrap ray)
    {
      foreach (var item in Faces)
      {
        var res = ray.IntersectFace(item);
        if (res.IsHit)
        {
          yield return res;
        }
      }
    }

    #region IDisposable Support
    private bool disposedValue = false; // 重複する呼び出しを検出するには

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          Mesh.Dispose();
          VertexShader.Dispose();
          PixelShader.Dispose();
          Buffer.Dispose();
          Shader.Dispose();
        }

        // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
        // TODO: 大きなフィールドを null に設定します。

        disposedValue = true;
      }
    }

    // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
    // ~MMDModel() {
    //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
    //   Dispose(false);
    // }

    // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
    public void Dispose()
    {
      // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
      Dispose(true);
      // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
      // GC.SuppressFinalize(this);
    }
    #endregion
  }

}
