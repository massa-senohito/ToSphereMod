using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{
  public class VDBDebugger :IDisposable
  {
    TcpClient Tcp;
    NetworkStream Ns;
    List< string> Strings;
    public VDBDebugger()
    {
      string ipOrHost = "127.0.0.1";
      //string ipOrHost = "localhost";
      int port = 10000;
      Strings = new List<string>();
      try
      {
        //TcpClientを作成し、サーバーと接続する
        Tcp = new TcpClient(ipOrHost, port);
        Ns = Tcp.GetStream();

      }
      catch (Exception e)
      {

      }
    }

    public void Send(string s)
    {
      //Debug.Write(s);
      if (s == null || s == "" ) return;
      Encoding enc = Encoding.UTF8;
      try
      {
        byte[] sendBytes = enc.GetBytes(s);
        Ns?.Write(sendBytes, 0, sendBytes.Length);
      }
      catch(Exception e)
      {

      }
    }

    public static string vdb_triangle(float x0, float y0, float z0, float x1, float y1, float z1, float x2, float y2, float z2)
    {
      return String.Format("t {0} {1} {2} {3} {4} {5} {6} {7} {8}\n", x0, y0, z0, x1, y1, z1, x2, y2, z2);
    }
    public static string vdb_point(float x, float y, float z)
    {
      //  var line = vdb_line(x - 5, y - 5, z - 5, x + 5, y + 5, z + 5) 
       //              + vdb_line(x-5,y-5,z+5,x+5,y+5,z-5);
      var line = String.Format("p {0} {1} {2}\n", x, y, z);
      return line;
    }
    public static string vdb_line(float x0, float y0, float z0, float x1, float y1, float z1)
    {
      return String.Format("l {0} {1} {2} {3} {4} {5}\n", x0, y0, z0, x1, y1, z1);
    }
    public static string vdb_color(float x0, float y0, float z0, float x1, float y1, float z1)
    {
      return String.Format("c {0} {1} {2}\n", x0, y0, z0);
    }
    int vdb_intern(string str)
    {
      for (int i = 0; i < Strings.Count; i++)
      {
        if (Strings[i] == str)
        {
          Send(String.Format("s {0} {1}\n", i, str));
          return i;
        }
      }
      Strings.Add(str);
      int key = Strings.Count;
      Send(String.Format("s {0} {1}\n", key, str));
      return key;
    }
    public void vdb_label(string s)
    {
      int key = vdb_intern(s);
      Send($"g {key}\n");
    }

    public void Dispose()
    {
      Ns.Close();
      Tcp.Close();
    }
  }

  public class Camera
  {
    Vector3 PosLocal = new Vector3(0, 30, 70);
    Vector3 To = new Vector3(0, 0, 0);

    Matrix CamAtt = Matrix.Identity;

    Vector3 MoveDir;
    float ZAngle = 0;
    float ZOffset;
    float Dist;
    Matrix View;

    public Camera(Vector3 pos , Vector3 to )
    {
      PosLocal = pos - to;
      To = to;
      View = Matrix.LookAtLH(pos, To, Vector3.UnitY);
          View.Decompose(out Vector3 sc1, out Quaternion ro1, out Vector3 tr1);
          var rr1 = ro1.EulerAngle();
      CamAtt = ro1.RotMat();
    }

    public Matrix GetView() { return View; }

    public void Move(Vector3 vector)
    {
      MoveDir.X += vector.X;
      MoveDir.Y += vector.Y;
      ZOffset += vector.Z;
    }
#if true
    public void Update()
    {
      // 1
      var rot = //new Quaternion(Vector3.UnitZ,(float)Math.PI);
        new Vector3(0, 0, ZAngle).QuatFromEuler();
      var rr = rot.EulerAngle();
      Matrix.RotationQuaternion(ref rot, out Matrix rotMat);
      CamAtt *= rotMat;
      // 2 カメラの移動方向を求めるベクトル
      Matrix.Translation(ref MoveDir, out Matrix dMat);
      var dl = dMat * CamAtt;
      if (!dl.TranslationVector.IsZero)
      {
        var rotAxis = dl.TranslationVector.Cross(CamAtt.Forward);
        float angleUnit = 0.02f;
        var trans = new Quaternion(rotAxis, angleUnit);
        var tt = trans.EulerAngle();
        var transQ = trans.RotMat();
            var campos = PosLocal.TransMat() * transQ;
        // ここまでローカルとしてはあってる
        PosLocal = campos.TranslationVector;
        
      }


      View = Matrix.LookAtLH(PosLocal + To, To, Vector3.UnitY);
    }
#else
    public void Update()
    {
      // 1
      var rot = //new Quaternion(Vector3.UnitZ,(float)Math.PI);
        new Vector3(0, 0, ZAngle).QuatFromEuler();
      var rr = rot.EulerAngle();
      Matrix.RotationQuaternion(ref rot, out Matrix rotMat);
      CamAtt *= rotMat;
      // 2 カメラの移動方向を求めるベクトル
      Matrix.Translation(ref MoveDir, out Matrix dMat);
      var dl = dMat * CamAtt;
      if (!dl.TranslationVector.IsZero)
      {
        var rotAxis = dl.TranslationVector.Cross(CamAtt.Forward);
        float angleUnit = 0.02f;
        var trans = new Quaternion(rotAxis, angleUnit);
        var tt = trans.EulerAngle();
        var transQ = trans.RotMat();
        {
          CamAtt.Decompose(out Vector3 sc1, out Quaternion ro1, out Vector3 tr1);
          var rr1 = ro1.EulerAngle();
        }
       // local
        var campos = PosLocal.TransMat() * transQ;
        // ここまでローカルとしてはあってる
        PosLocal = campos.TranslationVector;
        //3
        Vector3 z = -PosLocal;
        z.Normalize();
        var y = CamAtt.Up;
        var x = y.Cross(z);
        x.Normalize();
        y = z.Cross(y);
        y.Normalize();
        CamAtt.Right = x;
        CamAtt.Up = y;
        CamAtt.Forward = z;
        {
          CamAtt.Decompose(out Vector3 sc1, out Quaternion ro1, out Vector3 tr1);
          var rr1 = ro1.EulerAngle();
        }
      }
      // 4
      Dist = PosLocal.Length();
      if(Dist - ZOffset > 0)
      {
        var z = -PosLocal;
        z.Normalize();
        PosLocal += ZOffset * z;
        Dist -= ZOffset;
      }
      // 5 ワールド更新
      var posW = PosLocal + To;

      View = CamAtt;
      {
        View.Decompose(out Vector3 sc2, out Quaternion ro2, out Vector3 tr2);
        var rr2 = ro2.EulerAngle();
      }
      View.TranslationVector = posW;
      {
        View.Decompose(out Vector3 sc2, out Quaternion ro2, out Vector3 tr2);
        var rr2 = ro2.EulerAngle();
      }
      View.M44 = 1.0f;
      View.Invert();
      {
        View.Decompose(out Vector3 sc2, out Quaternion ro2, out Vector3 tr2);
        var rr2 = ro2.EulerAngle();
      }

      ZOffset = 0;
      ZAngle = 0;
      MoveDir = Vector3.Zero;
      /*
       * D3DXVec3Cross( &X, &Y, &Z );
D3DXVec3Normalize( &X, &X );

D3DXVec3Cross( &Y, &Z, &X );
D3DXVec3Normalize( &Y, &Y );

D3DXMatrixIdentity( CamMat );
memcpy( CamMat.m[0], &X, sizeof( D3DXVECTOR3 ) );
memcpy( CamMat.m[1], &Y, sizeof( D3DXVECTOR3 ) );
memcpy( CamMat.m[2], &Z, sizeof( D3DXVECTOR3 ) );

　これで、カメラの新しい姿勢と位置が定まりました。しかし、カメラの奥行き方向も考慮したいはずです。これは簡単で、新しい位置を原点方向にオフセットすれば良いんです。原点方向はカメラのZ軸ですから、
D3DXVECTOR3* CamZAxis = (D3DXVECTOR3*)CamMat.m[2];
CamPos += Offset_Z * (*CamZAxis);
       */
    }
#endif
  }

  public static class Ma
  {
    public static float Sin(float f)
    {
      return (float)Math.Sin(f);
    }
    public static float ASin(float f)
    {
      return (float)Math.Asin(f);
    }
    public static float Cos(float f)
    {
      return (float)Math.Cos(f);
    }
    public static float ACos(float f)
    {
      return (float)Math.Acos(f);
    }

    public static float Abs(this float f)
    {
      return Math.Abs(f);
    }

    public static float Rad(this float f)
    {
      return f / 180.0f * (float)Math.PI;
    }
    public static float Deg(this float f)
    {
      return f / (float)Math.PI * 180.0f;
    }

    public static float AddDeg(this float deg,float delta)
    {
      float added = deg + delta;
      if(added < 0)
      {
        added = -added;
        return 360.0f - added % 360.0f;
      }
      return added % 360.0f;
    }

    public static float Clamp(this float v , float min , float max)
    {
      return Math.Min(Math.Max(v, min), max);
    }

    public static float AddDegClamp(this float deg,float delta)
    {
      float added = deg + delta;

      return added.Clamp(180.5f , 359.5f);
    }

  }

  public static class Util
  {
    public const float ZeroTolerancef = 1e-06f;
    public static void ArrayFullCopy(this Array array , Array target)
    {
      Array.Copy(array, target, array.Length);
    }



    public static bool IsNearlyEqual(this float v,float other , float eps = float.Epsilon)
    {
      return (v - other).Abs() < eps;
    }

    public static bool IsNearlyZero(this float v , float eps = float.Epsilon)
    {
      return v.IsNearlyEqual(0.0f,eps);
    }


    public static float Float(this string s)
    {
      return float.Parse(s);
    }
    public static int Int(this string s)
    {
      return int.Parse(s);
    }

    public static void DebugWrite(this string s)
    {
      Debug.WriteLine(Platform.Program.FpsCounter.FrameCount + " " + s);
    }

    public static IEnumerable <Vector3> ParseCSV( IEnumerable<string> lines)
    {
      //Vertex,0,-5,-5,-5 ,-1
      foreach (var item in lines)
      {
        var csv = item.Split(',');
        yield return new Vector3(csv[2].Float(), csv[3].Float(), csv[4].Float());
      }
    }

    public static int[] ParseFaceCSV(string line)
    {

      var csv = line.Split(',');
      return new int[] { csv[3].Int(), csv[4].Int(), csv[5].Int() };
    }

    public static IEnumerable< int[]> ParseFaceCSVAll( IEnumerable<string> lines)
    {
      //Face,"箱",1,2,3,0
      foreach (var item in lines)
      {
        yield return ParseFaceCSV(item);
      }
    }

    public static string ConcatStr<T>(this IEnumerable<T> items)
    {
      StringBuilder builder = new StringBuilder();
      //return items.Aggregate("", (acc, i) => acc + i );
      foreach (var item in items)
      {
        builder.Append(item);
      }
      return builder.ToString();
    }
		public static void WriteFile( this IEnumerable<string> line , string path )
		{
			File.WriteAllLines( path , line );
		}

  }

  public class HitResult
  {
    public bool IsHit
    {
      get;
    }
    public Vector3 HitPosition
    {
      get;
    }
    public string Info
    {
      get;
    }

    public HitResult(bool isHit, Vector3 pos)
    {
      IsHit = isHit;
      HitPosition = pos;
    }
    public HitResult( Vector3 pos , string info)
    {
      IsHit = true;
      HitPosition = pos;
      Info = info;
    }
    public static HitResult Null
    {
      get
      {
        return new HitResult(false, Vector3.Zero);
      }
    }
  }

  public class RayWrap
  {
    public Vector3 From
    {
      get;
      private set;
    }
    public Vector3 To
    {
      get;
      private set;
    }

    float Length;

    public Vector3 Dir
    {
      get;
      private set;
    }

    Ray Ray;

    public RayWrap(Vector3 from , Vector3 to)
    {
      From = from;
      To = to;
      var vec = (to - from);
      Length = vec.Length();
      vec.Normalize();
      Dir = vec;
      Ray = new Ray(From, Dir);
    }

    public RayWrap(Ray ray,float length = 100)
    {
      From = ray.Position;
      Dir  = ray.Direction;
      Length = length;
      Ray = ray;
      To = From + Dir * Length;
    }

    public string RayStr
    {
      get
      {
       return  VDBDebugger.vdb_line(From.X, From.Y, From.Z, To.X, To.Y, To.Z);
      }
    }
    public void Extend(float len)
    {
      Length += len;
      To = From + Dir * Length;
    }

    public HitResult IntersectFace(Face face)
    {
      var p1 = face.P1;
      var p2 = face.P2;
      var p3 = face.P3;
      var intersects = Ray.Intersects(ref p1, ref p2, ref p3, out float dist);
      var isIn = dist < Length;
      if (intersects && true)
      {
        return new HitResult(From + Dir * dist,face.MatName.Replace("\"",""));
      }
      return HitResult.Null;

    }
 
  }

  public class Face
  {
    public string MatName
    {
      get;
    }
    public Vector3 P1
    {
      get;
      private set;
    }
    public Vector3 P2
    {
      get;
      private set;
    }
    public Vector3 P3
    {
      get;
      private set;
    }
    public Vector3 P1O
    {
      get;
      private set;
    }
    public Vector3 P2O
    {
      get;
      private set;
    }
    public Vector3 P3O
    {
      get;
      private set;
    }

    public Vector3 AB
    {
      get;
      private set;
    }
    public Vector3 BC
    {
      get;
      private set;
    }
    public Vector3 CA
    {
      get;
      private set;
    }
    public Vector3 BaryCentric;
    public float AverageZ
    {
      get
      {
        return P1.Z + P2.Z + P3.Z;
      }
    }
    public void SetP1(Vector3 loc)
    {
      var delta = loc - P1;
      P1 -= loc;
      P2 += delta;
      P3 += delta;
      ReculcEdge();
    }

    public Face(Vector3 p1,Vector3 p2 ,Vector3 p3,string matName)
    {
      P1 = p1;
      P2 = p2;
      P3 = p3;
      P1O = p1;
      P2O = p2;
      P3O = p3;
      var n1 = P1 - P2;
      var n2 = P1 - P3;
      n1.Normalize();
      n2.Normalize();
      Normal = Vector3.Cross(n2, n1);
      BaryCentric = (P1 + P2 + P3) / 3.0f;
      ReculcEdge();
      MatName = matName;
    }

    private void ReculcEdge()
    {
      AB = P2 - P1;
      BC = P3 - P2;
      CA = P1 - P3;
    }

    public void Update(Matrix matrix)
    {
      P1 = matrix.TransByMat(P1O).ToV3();
      P2 = matrix.TransByMat(P2O).ToV3();
      P3 = matrix.TransByMat(P3O).ToV3();
      ReculcEdge();

    }

    public string TriString
    {
      get
      {
        return VDBDebugger.vdb_triangle(
          P1.X, P1.Y, P1.Z,
          P2.X, P2.Y, P2.Z,
          P3.X, P3.Y, P3.Z
          );
      }
    }

    public Vector3 Normal
    {
      get;
    }
    public override string ToString()
    {
      return P1.ToString() + " " + P2.ToString() + " " + P3.ToString();
    }

  }
  
}
