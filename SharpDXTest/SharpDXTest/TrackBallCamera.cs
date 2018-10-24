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

	public bool MiddleClicked
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

	public void OnMouseMove( MouseEventArgs e , RayWrap startRay )
	{
		IsMoved = true;
		Clicked = e.Button == MouseButtons.Left;
		RightClicked = e.Button == MouseButtons.Right;
		MiddleClicked = e.Button == MouseButtons.Middle;
		if ( Clicked )
		{
			LastClickedPos = Position;
			LastClickedWorldPos = WorldPosition;
		}
		else
		{

		}
		Vector2 screenPos = new Vector2( e.X , e.Y );
		Position = screenPos;
		WorldPosition = startRay.To;
		WheelDelta = Math.Sign( e.Delta );
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
	Matrix LookAt( Vector3 up )
	{
		Vector3 right = Forward.Cross( up );
		right.Normalize( );
		Vector3 recalcUp = right.Cross( Forward );
		recalcUp.Normalize( );
		return
		  Matrix.LookAtLH( Position , Target , recalcUp );
	}

	public float Distance = 15f;
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

	public TrackBallCamera( Vector3 pos , Vector3 target )
	{
		Target = target;
		Position = pos;
		float dist = Vector3.Distance( Position , Target );
		Distance = dist;
		View = Matrix.LookAtLH( pos , target , Vector3.UnitY );
		debug = new VDBDebugger( );
	}

	public static Vector3 Transform( Vector3 vec , Quaternion quat )

	{

		Vector3 result;

		Transform( ref vec , ref quat , out result );

		return result;

	}

	public static void Transform( ref Vector3 vec , ref Quaternion quat , out Vector3 result )

	{

		// Since vec.W == 0, we can optimize quat * vec * quat^-1 as follows:

		// vec + 2.0 * cross(quat.xyz, cross(quat.xyz, vec) + quat.w * vec)

		Vector3 xyz = quat.Axis, temp, temp2;

		Vector3.Cross( ref xyz , ref vec , out temp );

		Vector3.Multiply( ref vec , quat.W , out temp2 );

		Vector3.Add( ref temp , ref temp2 , out temp );

		Vector3.Cross( ref xyz , ref temp , out temp );

		Vector3.Multiply( ref temp , 2 , out temp );

		Vector3.Add( ref vec , ref temp , out result );

	}

	void SetPosition( Vector3 pos )
	{

		Position = pos;
		Forward = Target - Position;
		Forward.Normalize( );
	}

	Vector3 TowardTarget()
	{
		Vector3 lastPosn = Position;
		Vector3 targetPosn = Target;
		Vector3 vecPos = ( targetPosn - lastPosn );
		vecPos.Normalize( );
		return vecPos;
	}

	public void OnResize( float ratio )
	{
		Projection = Matrix.PerspectiveFovLH( FOV.Rad( ) , ratio , 1.0F , 100.0F );
	}

	public void Update( Mouse mouse , float delta )
	{

		Vector2 mousePosn = mouse.Position;
		bool mouseBtn = mouse.RightClicked;
		if ( mouse.WheelDelta != 0 )
		{
			Distance -= mouse.WheelDelta;
			Distance = Distance.Clamp( 1 , 200 );
			UpdateRotation( mouse );
		}
		// ボタンが押されていないと lastMousePosition = nullになる
		if ( mouseBtn )
		{
			if ( lastMousePosition.HasValue )
			{
				UpdateRotation( mouse );
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

		if ( mouse.MiddleClicked )
		{
			Vector3 mouseDelta = new Vector3( mouse.Delta.X , mouse.Delta.Y , 0 ) * 0.01f;
			Vector3 right = Forward.Cross( Vector3.UnitY );
			Vector3 recalcUp = right.Cross( Forward );
			Vector3 xVec = right * mouseDelta.X;
			Vector3 yVec = recalcUp * mouseDelta.Y;
			Vector3 movVec = xVec + yVec;
			Position += movVec;
			Target += movVec;
			SetPosition( Position );

			Vector3 up = Vector3.Up;
			View = LookAt( up );
		}

	}

	private void UpdateRotation( Mouse mouse )
	{
		SetPosition( SphericalPos( mouse ) + Target );
		Vector3 up = Vector3.Up;

		View = LookAt( up );
	}

	float Theta = 180;
	float Phi = 284;

	Vector3 SphericalPos( Mouse mouse )
	{

		Theta = Theta.AddDeg( mouse.Delta.X );
		Phi = Phi.AddDegClamp( mouse.Delta.Y );
		// Util.DebugWrite(Phi.ToString());
		float t = Theta.Rad( );
		float p = Phi.Rad( );
		float x = Distance * Util.Sin( p ) * Util.Sin( t );
		float y = Distance * Util.Cos( p );
		float z = Distance * Util.Sin( p ) * Util.Cos( t );
		Vector3 temp = new Vector3( x , y , z );
		return temp;
	}

}