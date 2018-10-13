using System;
using System.Collections;
using System.Windows.Forms;
using SharpDX;
using SharpDXTest;

public class Mouse
{
  public Vector2 LastClickedPos
  {
    get;
    private set;
  }
  public Vector2 Position
  {
    get;
    private set;
  }
  public Vector2 LastPosition
  {
    get;
    private set;
  }
  public bool Clicked
  {
    get;
    private set;
  }
  public bool RightClicked
  {
    get;
    private set;
  }

  public Vector3 WorldPosition
  {
    get;
    private set;
  }
  public Vector3 LastClickedWorldPos
  {
    get;
    private set;
  }
  public Vector2 Delta
  {
    get;
    private set;
  }

  public bool IsMoved
  {
    get;
    private set;
  }

  public int WheelDelta
  {
    get;
    private set;
  }

  public void OnMouseMove(MouseEventArgs e , RayWrap startRay )
  {
      IsMoved = true;
      Clicked = e.Button == MouseButtons.Left;
      RightClicked = e.Button == MouseButtons.Right;
      if (Clicked)
      {
        LastClickedPos = Position;
        LastClickedWorldPos = WorldPosition;
      }
      else
      {

      }
      Vector2 screenPos = new Vector2(e.X, e.Y);
      Position = screenPos;
      WorldPosition = startRay.To;
    WheelDelta = Math.Sign(e.Delta);
  }
  public void Update()
  {
    //WorldPosition = wpos;
    Delta = Position - LastPosition;
    LastPosition = Position;
    IsMoved = false;
    WheelDelta = 0;
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

  static float FOV = 30;
  public Matrix Projection
  {
    get;
    private set;
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
    Projection = Matrix.PerspectiveFovLH(FOV.Rad(), 1, 1F, 10000.0F);
    debug = new VDBDebugger();
  }

  void Start()
  {

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

  void SetPosition(Vector3 pos)
  {

    Position = pos;
    Forward = Target - Position;
    Forward.Normalize();
  }
  Vector3 TowardTarget()
  {
    var lastPosn = Position;
    var targetPosn = Target;
    var vecPos = (targetPosn - lastPosn);
    vecPos.Normalize();
    return vecPos;
  }
  
  public void Update(Mouse mouse, float delta)
  {

    var mousePosn = mouse.Position;
    var mouseBtn = mouse.RightClicked;
    if (mouse.WheelDelta != 0)
    {
      distance -= mouse.WheelDelta;
      UpdatePosition(mouse);
    }
    // ボタンが押されていないと lastMousePosition = nullになる
    if (mouseBtn)
    {
      if (lastMousePosition.HasValue)
      {
        UpdatePosition(mouse);
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

  private void UpdatePosition(Mouse mouse)
  {
    SetPosition(CartesianPos(mouse) + Target);
    Vector3 up = Vector3.Up;

    View = Matrix.LookAtLH(Position, Target, up);
  }

  float Theta = 180;
  float Phi = 284;
  Vector3 CartesianPos(Mouse mouse)
  {

    Theta = Theta.AddDeg( mouse.Delta.X);
    Phi =  Phi.AddDeg(mouse.Delta.Y);
    var t = Theta.Rad();
    var p = Phi.Rad();
    var x = distance * Ma.Sin(p) * Ma.Sin(t);
    var y = distance * Ma.Cos(p);
    var z = distance * Ma.Sin(p) * Ma.Cos(t);
    var temp = new Vector3(x, y, z);
    return temp;
  }

}