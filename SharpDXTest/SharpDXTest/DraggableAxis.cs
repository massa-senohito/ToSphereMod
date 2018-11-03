using g3;
using SharpDX;
using SharpDXTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector3f = SharpDX.Vector3;

namespace SharpDXTest
{
	public enum ClickedAxis
	{
		X,
		Y,
		Z,
		XRot,
		YRot,
		ZRot,
		None,
	}

	class ClickState
	{
		f3.AxisTranslationWidget XAxis = new f3.AxisTranslationWidget( 0 );
		f3.AxisTranslationWidget YAxis = new f3.AxisTranslationWidget( 1 );
		f3.AxisTranslationWidget ZAxis = new f3.AxisTranslationWidget( 2 );

		public f3.AxisRotationWidget XRot = new f3.AxisRotationWidget( 0 );
		f3.AxisRotationWidget YRot = new f3.AxisRotationWidget( 1 );
		f3.AxisRotationWidget ZRot = new f3.AxisRotationWidget( 2 );

		public ClickedAxis Axis;
		public bool IsDragging;
		public MMDModel Parent;
		public f3.IWidget Widget;

		public ClickState( MMDModel parent , ClickedAxis axis )
		{
			Axis = axis;
			Parent = parent;
		}

		public void EndClick()
		{
			Axis = ClickedAxis.None;
			IsDragging = false;
		}

		public void Update( HitResult res , ClickedAxis axis )
		{
			Axis = axis;
			IsDragging = true;
			switch ( axis )
			{
				case ClickedAxis.X:
					Widget = XAxis;
					break;
				case ClickedAxis.XRot:
					Widget = XRot;
					break;

				case ClickedAxis.Y:
					Widget = YAxis;
					break;
				case ClickedAxis.YRot:
					Widget = YRot;
					break;

				case ClickedAxis.Z:
					Widget = ZAxis;
					break;
				case ClickedAxis.ZRot:
					Widget = ZRot;
					break;

				case ClickedAxis.None:
					break;
				default:
					break;
			}
			//Axis.ToString( ).DebugWrite( );
			Widget.BeginCapture( Parent , res );
		}

		public void Update( RayWrap ray )
		{
			Widget.UpdateCapture( Parent , ray );
		}


	}
	internal class DraggableAxis : MMDModel
	{

		ClickState state;

		public DraggableAxis( string path ) : base( path )
		{
			state = new ClickState( this , ClickedAxis.None );

		}

		public Vector3 RotXFrame
		{
			get
			{
				return state.XRot.raycastFrame.Origin;
			}
		}

		// hitpos からrayがくる
		public void OnClicked( Mouse mouse , RayWrap ray )
		{
			if ( !mouse.Clicked )
			{
				state.EndClick( );
				return;
			}
			//Util.DebugWrite( $"DraggingState {state.IsDragging}" );
			if ( state.IsDragging )
			{
				//Util.DebugWrite( $"Dragging {state.Axis}" );
				Dragging( ray );
				return;
			}

			foreach ( Face item in Faces )
			{
				HitResult res = ray.IntersectFace( item );
				if ( res.IsHit )
				{
					// red redHead
					// redRot
					if ( res.Info == "red" || res.Info == "redHead" )
					{
						state.Update( res , ClickedAxis.X );
					}
					if ( res.Info == "redRot" )
					{
						state.Update( res , ClickedAxis.XRot );
					}

					if ( res.Info == "green" || res.Info == "greenHead" )
					{
						state.Update( res , ClickedAxis.Y );
					}
					if ( res.Info == "greenRot" )
					{
						state.Update( res , ClickedAxis.YRot );
					}

					if ( res.Info == "blue" || res.Info == "blueHead" )
					{
						state.Update( res , ClickedAxis.Z );
					}
					if ( res.Info == "blueRot" )
					{
						state.Update( res , ClickedAxis.ZRot );
					}

					//Util.DebugWrite( $"start {res.Info}" );
				}

			}

		}

		void Dragging( RayWrap ray )
		{
			state.Update( ray );
		}
	}
}

#if true

namespace f3
{
	public enum CoordSpace
	{
		ObjectCoords,
		WorldCoords,
	}

	public static class F3Util
	{
		public static Frame3f GetGameObjectFrame( this MMDModel go , CoordSpace eSpace )
		{
			if ( eSpace == CoordSpace.WorldCoords )
				return new Frame3f( go.Position , go.Rotation );
			else if ( eSpace == CoordSpace.ObjectCoords )
			{

				//return new Frame3f(go.transform.localPosition, go.transform.localRotation);
				//Util.DebugWrite("ObjectCoords" + go.Rotation.ToString( ) );
				return new Frame3f( go.Position , go.Rotation );
			}
			else
				throw new ArgumentException( "not possible without refernce to scene!" );
		}

		public static void SetLocalFrame( this MMDModel go , Frame3f frame , CoordSpace eSpace )
		{
			go.Position = frame.Origin;
			go.Rotation = frame.Rotation;
		}

	}

	public interface IWidget
	{

		bool BeginCapture( MMDModel target , HitResult hit );
		bool UpdateCapture( MMDModel target , RayWrap worldRay );
		bool EndCapture( MMDModel target );
	}

	// AxisRotationWidget.cs
	public class AxisRotationWidget : IWidget
    {
		int nRotationAxis;

        public bool EnableSnapping = true;
        public float SnapIncrementDeg = 5.0f;

        public Func<Frame3f, int, float, float> AbsoluteAngleConstraintF = null;
        public Func<Frame3f, int, float, float> DeltaAngleConstraintF = null;


        public AxisRotationWidget(int nFrameAxis)
		{
			nRotationAxis = nFrameAxis;
		}

		// stored frames from target used during click-drag interaction
		Frame3f rotateFrameL;		// local-space frame 
		Frame3f rotateFrameW;		// world-space frame
		Vector3f rotateAxisW;		// world-space axis we are rotating around (redundant...)

		// computed values during interaction
		public Frame3f raycastFrame;		// camera-facing plane containing translateAxisW
		float fRotateStartAngle;

		public bool BeginCapture(MMDModel target, HitResult hit)
		{
			// save local and world frames
			rotateFrameL = target.GetGameObjectFrame(CoordSpace.ObjectCoords);
			rotateFrameW = target.GetGameObjectFrame(CoordSpace.WorldCoords);
			rotateAxisW = rotateFrameW.GetAxis (nRotationAxis);

			// save angle of hitpos in 2D plane perp to rotateAxis, so we can find delta-angle later
			Vector3f vWorldHitPos = hit.HitPosition;
			Vector3f dv = vWorldHitPos - rotateFrameW.Origin;
			int iX = (nRotationAxis + 1) % 3;
			int iY = (nRotationAxis + 2) % 3;
			float fX = Vector3f.Dot( dv, rotateFrameW.GetAxis(iX) );
			float fY = Vector3f.Dot( dv, rotateFrameW.GetAxis(iY) );
			fRotateStartAngle = (float)Math.Atan2 (fY, fX);

			// construct plane we will ray-intersect with in UpdateCapture()
			raycastFrame = new Frame3f( vWorldHitPos, rotateAxisW );
            if (EnableSnapping) {
                //enable_circle_indicator(true);
            }

            return true;
		}

		public bool UpdateCapture(MMDModel target, RayWrap worldRay)
		{
			int normalAxis = 2;
			if ( nRotationAxis == 2 )
			{
				normalAxis = 1;
			}

			// ray-hit with plane perpendicular to rotateAxisW
			var planeHitWOpt = raycastFrame.RayPlaneIntersection(worldRay.From, worldRay.Dir, normalAxis);
			if ( !planeHitWOpt.HasValue )
			{
				return false;
			}
			var planeHitW = planeHitWOpt.Value;

			// find angle of hitpos in 2D plane perp to rotateAxis, and compute delta-angle
			Vector3f dv = planeHitW - rotateFrameW.Origin;
			int iX = (nRotationAxis + 1) % 3;
			int iY = (nRotationAxis + 2) % 3;
			float fX = Vector3.Dot( dv, rotateFrameW.GetAxis(iX) );
			float fY = Vector3.Dot( dv, rotateFrameW.GetAxis(iY) );

			Util.DebugWrite( "dv " + dv.ToString( ) );
			Util.DebugWrite( "planeHitW " + planeHitW.ToString( ) );
			Util.DebugWrite( "rotateFrameW " + rotateFrameW.Origin.ToString( ) );

			float fNewAngle = (float)Math.Atan2 (fY, fX);

			//if ( AbsoluteAngleConstraintF != null )
			//	fNewAngle = AbsoluteAngleConstraintF( rotateFrameL , nRotationAxis , fNewAngle );

            float fDeltaAngle = (fNewAngle - fRotateStartAngle);
            //if (DeltaAngleConstraintF != null)
            //    fDeltaAngle = DeltaAngleConstraintF(rotateFrameL, nRotationAxis, fDeltaAngle);
			Util.DebugWrite( "fDeltaAngle " + fDeltaAngle );

			fDeltaAngle *= 0.03f;

            bool on_snap = false;
            if (EnableSnapping) {
                //double dist = (planeHitW - rotateFrameW.Origin).Length();
                //on_snap = Math.Abs(dist - gizmoRadiusW) < gizmoRadiusW * 0.15f;
                //if (on_snap)
                //    fDeltaAngle = (float)Snapping.SnapToIncrement(fDeltaAngle, SnapIncrementDeg * MathUtil.Deg2Radf);
                //enable_snap_indicator(true);
                //update_snap_indicator(-fDeltaAngle, on_snap);
            }

			// 軸を中心に回るターゲットのための新しいフレームを作る
			Vector3f rotateAxisL = rotateFrameL.GetAxis( nRotationAxis );
			Quaternion q = Quaternion.RotationAxis(rotateAxisL , fDeltaAngle.Deg() );
			Frame3f newFrame = rotateFrameL;
			newFrame.Rotation = q * newFrame.Rotation;		// order matters here!

			// update target
			target.SetLocalFrame (newFrame, CoordSpace.ObjectCoords);

			if ( EnableSnapping )
			{
                //update_circle_indicator(on_snap);
			}

            return true;
		}

        public bool EndCapture(MMDModel target)
        {
            if (EnableSnapping) {
                //enable_circle_indicator(false);
                //enable_snap_indicator(false);
            }

            return true;
        }


        public void Disconnect()
        {
            //if (circle_indicator != null)
            //    circle_indicator.Destroy();
            //if (snap_indicator != null)
            //    circle_indicator.Destroy();
            //RootGameObject.Destroy();
        }


        //static double VisibilityThresh = Math.Cos(65 * MathUtil.Deg2Rad);
        //public override bool CheckVisibility(ref Frame3f curFrameW, ref Vector3d eyePosW)
        //{
        //    Vector3d axis = curFrameW.GetAxis(nRotationAxis);
        //    Vector3d eyevec = (eyePosW - curFrameW.Origin).Normalized;
        //    double dot = axis.Dot(eyevec);
        //    return Math.Abs(dot) > VisibilityThresh;
        //}


        //static Vector3d[] diagonals = new Vector3d[3] {
        //    (-Vector3d.AxisX+Vector3d.AxisZ).Normalized,
        //    (Vector3d.AxisX+Vector3d.AxisZ).Normalized,
        //    (Vector3d.AxisX-Vector3d.AxisZ).Normalized,
        //};


        //fLineSetGameObject circle_indicator = null;
        //void enable_circle_indicator(bool enable)
        //{
        //    if (enable == false && circle_indicator == null)
        //        return;
        //    if (enable && circle_indicator == null) {
        //        LineSet lines = new LineSet();
        //        lines.UseFixedNormal = true;
        //        lines.FixedNormal = Vector3f.AxisY;
        //        DCurve3 curve = new DCurve3(Polygon2d.MakeCircle(gizmoInitialRadius, 64),0,2);
        //        lines.Curves.Add(curve);
        //        lines.Width = 1.0f;
        //        lines.WidthType = LineWidthType.Pixel;
        //        lines.Segments.Add(
        //            new Segment3d(Vector3d.Zero, gizmoInitialRadius * diagonals[nRotationAxis] ));
        //        lines.Color = new Colorf(0.2f);
        //        circle_indicator = new fLineSetGameObject(new GameObject(), lines, "circle");
        //        circle_indicator.SetLayer(FPlatform.WidgetOverlayLayer, true);

        //        circle_indicator.SetLocalRotation(Quaternionf.FromTo(Vector3f.AxisY, Frame3f.Identity.GetAxis(nRotationAxis)));
        //        RootGameObject.AddChild(circle_indicator, false);
        //    }
        //    circle_indicator.SetVisible(enable);
        //}
        //void update_circle_indicator(bool on_snap)
        //{
        //    if (circle_indicator != null)
        //        circle_indicator.SafeUpdateLines((lines) => {
        //            lines.Color.a = (on_snap) ? 1.0f : 0.5f;
        //        });
        //}


        //fLineSetGameObject snap_indicator = null;
        //void enable_snap_indicator(bool enable)
        //{
        //    if (enable == false && snap_indicator == null)
        //        return;
        //    if (enable && snap_indicator == null) {
        //        LineSet lines = new LineSet();
        //        lines.UseFixedNormal = true;
        //        lines.FixedNormal = Vector3f.AxisY;
        //        int n = 360 / (int)SnapIncrementDeg;
        //        int n45 = 45 / (int)SnapIncrementDeg;
        //        int n90 = 90 / (int)SnapIncrementDeg;
        //        double r = gizmoInitialRadius;
        //        double r2 = gizmoInitialRadius * 1.05;
        //        double r45 = gizmoInitialRadius * 1.10;
        //        double r90 = gizmoInitialRadius * 1.175;
        //        for ( int k = 0; k < n; ++k ) {
        //            float angle = ((float)k / (float)n) * MathUtil.TwoPIf;
        //            double x = Math.Cos(angle), y = Math.Sin(angle);
        //            Vector3d v = new Vector3d(x, 0, y); v.Normalize();
        //            double far_r = ((k + n45) % n90) == 0 ? r90 :
        //                (((k + n45) % n45) == 0 ? r45 : r2);
        //            lines.Segments.Add(new Segment3d(r * v, far_r * v));
        //        }
        //        lines.Width = 1.0f;
        //        lines.WidthType = LineWidthType.Pixel;
        //        lines.Color = new Colorf(0.2f);
        //        snap_indicator = new fLineSetGameObject(new GameObject(), lines, "indicator");
        //        snap_indicator.SetLayer(FPlatform.WidgetOverlayLayer, true);
                

        //        RootGameObject.AddChild(snap_indicator, false);
        //    }
        //    snap_indicator.SetVisible(enable);
        //}
        //void update_snap_indicator(float fAngle, bool on_snap)
        //{
        //    Quaternionf planeRotation = new Quaternionf(Vector3f.AxisY, fAngle*MathUtil.Rad2Degf);
        //    Quaternionf alignAxisRotation = Quaternionf.FromTo(Vector3f.AxisY, Frame3f.Identity.GetAxis(nRotationAxis));
        //    snap_indicator.SetLocalRotation(alignAxisRotation * planeRotation);

        //    snap_indicator.SafeUpdateLines((lines) => {
        //        lines.Color.a = (on_snap) ? 1.0f : 0.15f;
        //    });
        //}


    }



	//
	// this Widget implements translation constrained to an axis
	// 
	public class AxisTranslationWidget : IWidget
	{
		// scaling distance along axis multiplied by this value. 
		// By default click-point stays under cursor (ie direct manipulation), but 
		// you can slow it down by setting this to return a smaller value.
		// Also you can use this to compensate for global scene scaling (hence the function)
		public Func<float> TranslationScaleF = () => { return 1.0f; };


		public Func<Frame3f , int , float , float> DeltaDistanceConstraintF = null;


		int nTranslationAxis;

		public AxisTranslationWidget( int nFrameAxis )
		{
			nTranslationAxis = nFrameAxis;
		}

		// stored frames from target used during click-drag interaction
		Frame3f translateFrameL;        // local-spaace frame
		Frame3f translateFrameW;        // world-space frame
		Vector3 translateAxisW;     // world translation axis (redundant...)

		// computed values during interaction
		Frame3f raycastFrame;       // camera-facing plane containing translateAxisW
		float fTranslateStartT;   // start T-value along translateAxisW

		public static float ClosestPointOnLineT( Vector3 p0 , Vector3 dir , Vector3 pt )
		{
			float t = ( pt - p0 ).Dot( dir );
			return t;
		}
		public bool BeginCapture( MMDModel target , HitResult hit )
		{
			// save local and world frames
			translateFrameL = target.GetGameObjectFrame( CoordSpace.ObjectCoords );
			translateFrameW = target.GetGameObjectFrame( CoordSpace.WorldCoords );
			translateAxisW = translateFrameW.GetAxis( nTranslationAxis );

			// save t-value of closest point on translation axis, so we can find delta-t
			Vector3 vWorldHitPos = hit.HitPosition;
			fTranslateStartT = ClosestPointOnLineT(
				translateFrameW.Origin , translateAxisW , vWorldHitPos );

			// construct plane we will ray-intersect with in UpdateCapture()
			Vector3 makeUp = Vector3.Cross( Platform.Program.Camera.Forward , translateAxisW ).GetNormalized( );
			Vector3 vPlaneNormal = Vector3.Cross( makeUp , translateAxisW ).GetNormalized( );
			raycastFrame = new Frame3f( vWorldHitPos , vPlaneNormal );

			return true;
		}

		public bool UpdateCapture( MMDModel target , RayWrap worldRay )
		{
			int normalAxis = 2;
			if ( nTranslationAxis == 2 )
			{
				normalAxis = 1;
			}
			// ray-hit with plane that contains translation axis
			Vector3 planeHit = raycastFrame.RayPlaneIntersection( worldRay.From , worldRay.Dir , normalAxis ).Value;

			// figure out new T-value along axis, then our translation update is delta-t
			float fNewT = ClosestPointOnLineT( translateFrameW.Origin , translateAxisW , planeHit );
			float fDeltaT = ( fNewT - fTranslateStartT );
			fDeltaT *= TranslationScaleF( );
			if ( DeltaDistanceConstraintF != null )
				fDeltaT = DeltaDistanceConstraintF( translateFrameL , nTranslationAxis , fDeltaT );

			// construct new frame translated along axis (in local space)
			Frame3f newFrame = translateFrameL;
			newFrame.Origin += fDeltaT * translateFrameL.GetAxis( nTranslationAxis );

			// update target
			//target.SetLocalFrame (newFrame, CoordSpace.ObjectCoords);
			target.Position = newFrame.Origin;
			return true;
		}

		public bool EndCapture( MMDModel target )
		{
			return true;
		}


		//static Interval1d CosVisibilityRange = new Interval1d(
		//    -Math.Cos(45 * MathUtil.Deg2Rad), Math.Cos(15 * MathUtil.Deg2Rad));
		//public bool CheckVisibility(ref Frame3f curFrameW, ref Vector3d eyePosW) {
		//    Vector3d axis = curFrameW.GetAxis(nTranslationAxis);
		//    Vector3d eyevec = (eyePosW - curFrameW.Origin).Normalized;
		//    double dot = axis.Dot(eyevec);
		//    return CosVisibilityRange.Contains(dot);
		//}

	}
}

#endif
