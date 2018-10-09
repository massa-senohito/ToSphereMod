using System;
using System.Collections;

using SharpDX;
using SharpDXTest;

public class Mouse
{
  public Vector2 LastClickedPos;
  public Vector2 Position;
  public Vector2 LastPosition
  {
    get;
    private set;
  }
  public bool Clicked;
  public bool RightClicked;
  public Vector3 WorldPosition;
  public Vector3 LastClickedWorldPos;
  public Vector2 Delta
  {
    get;
    private set;
  }
  public bool IsMoved;
  public void Update()
  {
    //WorldPosition = wpos;
    Delta = Position - LastPosition;
    LastPosition = Position;
    IsMoved = false;
  }
}
public class TrackBallCamera
{

  public float distance = 15f;
  VDBDebugger debug;
  public float Multi = 0.8f; // distance of the virtual trackball.
  public Vector3 Forward;
  public Vector3 Target;
  public Vector3 Position;
  public Matrix View;
  public Matrix GetView()
  {
    return View;
  }
  private Vector2? lastMousePosition;
  // Use this for initialization

  public TrackBallCamera(Vector3 pos, Vector3 target)
  {
    Target = target;
    Position = pos;
    var dist = Vector3.Distance(Position, Target);
    distance = dist;
    View = Matrix.LookAtLH(pos, target, Vector3.UnitY);
    debug = new VDBDebugger();
  }

  void Start()
  {
    // ターゲットから15離れた位置まで自身を移動
    //var startPosn = (this.transform.position - target.transform.position).normalized * distance;
    //var position = startPosn + target.transform.position;
    //transform.position = position;
    //transform.LookAt(target.transform.position);

  }
  public static Vector3 Transform(Vector3 vec, Quaternion quat)

  {

    Vector3 result;

    Transform(ref vec, ref quat, out result);

    return result;

  }
  public static void Transform(ref Vector3 vec, ref Quaternion quat, out Vector3 result)

  {

    // Since vec.W == 0, we can optimize quat * vec * quat^-1 as follows:

    // vec + 2.0 * cross(quat.xyz, cross(quat.xyz, vec) + quat.w * vec)

    Vector3 xyz = quat.Axis, temp, temp2;

    Vector3.Cross(ref xyz, ref vec, out temp);

    Vector3.Multiply(ref vec, quat.W, out temp2);

    Vector3.Add(ref temp, ref temp2, out temp);

    Vector3.Cross(ref xyz, ref temp, out temp);

    Vector3.Multiply(ref temp, 2, out temp);

    Vector3.Add(ref vec, ref temp, out result);

  }
  public static Vector3 opA(Quaternion rotation, Vector3 point)
  {
    float num = rotation.X * 2f;
    float num2 = rotation.Y * 2f;
    float num3 = rotation.Z * 2f;
    float num4 = rotation.X * num;
    float num5 = rotation.Y * num2;
    float num6 = rotation.Z * num3;
    float num7 = rotation.X * num2;
    float num8 = rotation.X * num3;
    float num9 = rotation.Y * num3;
    float num10 = rotation.W * num;
    float num11 = rotation.W * num2;
    float num12 = rotation.W * num3;
    Vector3 result = new Vector3();
    result.X = (1f - (num5 + num6)) * point.X + (num7 - num12) * point.Y + (num8 + num11) * point.Z;
    result.Y = (num7 + num12) * point.X + (1f - (num4 + num6)) * point.Y + (num9 - num10) * point.Z;
    result.Z = (num8 - num11) * point.X + (num9 + num10) * point.Y + (1f - (num4 + num5)) * point.Z;
    return result;
  }

  void SetPosition(Vector3 pos)
  {

    Position = pos;
    Forward = Target - Position;
    Forward.Normalize();
  }

  public void Update(Mouse mouse, float delta)
  {
#if false
    var mousePosn = Input.mousePosition;

    var mouseBtn = Input.GetMouseButton(0);
    // ボタンが押されていないと lastMousePosition = nullになる
    if (mouseBtn)
    {
      if (lastMousePosition.HasValue)
      {
        // we are moving from here
        var lastPosn = this.transform.position;
        var targetPosn = target.transform.position;

        // we have traced out this distance on a sphere from lastPosn
        /*
				var rotation = TrackBall(
										lastMousePosition.Value.x, 
										lastMousePosition.Value.y,
										mousePosn.x,
										mousePosn.y );
				*/
        var rotation = FigureOutAxisAngleRotation(lastMousePosition.Value, mousePosn);

        var vecPos = (targetPosn - lastPosn).normalized * -distance;

        this.transform.position = rotation * vecPos + targetPosn;
        this.transform.LookAt(targetPosn);

        lastMousePosition = mousePosn;
      }
      else
      {
        lastMousePosition = mousePosn;
      }
    }
    else
    {
      lastMousePosition = null;
    }
#endif

    var mousePosn = mouse.Position;

    var mouseBtn = mouse.RightClicked;
    // ボタンが押されていないと lastMousePosition = nullになる
    if (mouseBtn)
    {
      if (lastMousePosition.HasValue)
      {
        // we are moving from here
        var lastPosn = Position;
        var targetPosn = Target;

        var rotation = FigureOutAxisAngleRotation(mouse);
        //new Quaternion(Vector3.UnitY*0.1f, 0.9f);
          //Vector3.UnitY.QuatFromEuler();//* 1000 *  delta;
        //rotation.Normalize();
        var vecPos = (targetPosn - lastPosn);
        vecPos.Normalize();
        var dir = new Vector3(vecPos.X, vecPos.Y, vecPos.Z);
        vecPos = vecPos * -distance;
        var translationVector = opA(rotation, vecPos); //(Transform(vecPos, rotation) );

        SetPosition( translationVector + targetPosn );
          //CartesianPos(mouse);
        var tDir = (Target - Position);
        //Console.WriteLine(": {0} ", rotation.EulerAngle());
        tDir.Normalize();
        Vector3 up = Vector3.Up;//tDir.Dot(Vector3.Up) < 0 ? Vector3.Down :Vector3.Up ;

        View = Matrix.LookAtLH(Position, targetPosn, up);
        //Console.WriteLine(": {0} ", up);
        //Tutorial16.Program.debug.Send(Position.DebugStr());
        lastMousePosition = mousePosn;
      }
      else
      {
        lastMousePosition = mousePosn;
      }
    }
    else
    {
      lastMousePosition = null;
    }
  }
  float Theta;
  float Phi;
  Vector3 CartesianPos(Mouse mouse)
  {

    Theta += mouse.Delta.X;
    Phi += mouse.Delta.Y;
    var t = Ma.ASin(0.2f);
    var p = 0;
    var x = distance * Ma.Sin(p) * Ma.Sin(t);
    var y = distance * Ma.Cos(p);
    var z = distance * Ma.Sin(p) * Ma.Cos(t);
    var temp = new Vector3(x, y, z);
    Console.WriteLine(temp);
    return temp;
  }

  Quaternion FigureOutAxisAngleRotation(Mouse mouse)
  {
#if false
    if (lastMousePosn.X == mousePosn.X && lastMousePosn.Y == mousePosn.Y)
      return Quaternion.Identity;

    Vector3 near = new Vector3(0, 0, Camera.main.nearClipPlane);

    Vector3 p1 = Camera.main.ScreenToWorldPoint(lastMousePosn + near);
    Vector3 p2 = Camera.main.ScreenToWorldPoint(mousePosn + near);

    //WriteLine("## {0} {1}", p1,p2);
    var axisOfRotation = Vector3.Cross(p2, p1);

    var twist = (p2 - p1).magnitude / (2.0f * virtualTrackballDistance);

    if (twist > 1.0f)
      twist = 1.0f;
    if (twist < -1.0f)
      twist = -1.0f;

    var phi = (2.0f * Mathf.Asin(twist)) * 180 / Mathf.PI;

    //WriteLine("AA: {0} angle: {1}",axisOfRotation, phi);

    return Quaternion.AngleAxis(phi, axisOfRotation);
#endif
    var mousePosn = mouse.Position;
    if (!mouse.IsMoved)
    {
      return Quaternion.Identity;
    }
#if false 
    Vector3 p1 = lastMousePosition.Value - Position;
    Vector3 p2 = mouse.WorldPosition - Position;

    // 移動距離はマウスの2次元距離にしないとカメラ回転の影響を
    //Console.WriteLine(" {0} : {1}", p1, p2);
    var axisOfRotation = Vector3.Cross(p1, p2);
    axisOfRotation.Normalize();
    //axisOfRotation = -axisOfRotation;
#else
    Vector2 p1 = lastMousePosition.Value;
    Vector2 p2 = mouse.Position;
    var axisOfRotation2D = p1.Normal( p2);
    var axisOfRotation = new Vector3(axisOfRotation2D.X, -axisOfRotation2D.Y,0);
#endif
    float moveLen = (mouse.Delta).Length();

    var twist = moveLen * Multi;

   Console.WriteLine("AA: {0} ", twist);

    Vector3 vector3 = (axisOfRotation * twist);

    //var rotation = new Vector3(-mouse.Delta.Y, mouse.Delta.X, 0);
    //Console.WriteLine(" {0} ", rotation);
    //return rotation.QuatFromEuler();

    return
      vector3.QuatFromEuler();
  }


}