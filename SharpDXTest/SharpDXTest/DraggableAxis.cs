using g3;
using SharpDX;
using SharpDXTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{
	public enum ClickedAxis
	{
		X,
		Y,
		Z,
		None,
	}

	class ClickState
	{
		f3.AxisTranslationWidget XAxis = new f3.AxisTranslationWidget( 0 );
		f3.AxisTranslationWidget YAxis = new f3.AxisTranslationWidget( 1 );
		f3.AxisTranslationWidget ZAxis = new f3.AxisTranslationWidget( 2 );
		public ClickedAxis Axis;
		public bool IsDragging;
		public MMDModel Parent;

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
					XAxis.BeginCapture( Parent , res );
					break;
				case ClickedAxis.Y:
					YAxis.BeginCapture( Parent , res );
					break;
				case ClickedAxis.Z:
					ZAxis.BeginCapture( Parent , res );
					break;
				case ClickedAxis.None:
					break;
				default:
					break;
			}
		}
		public void Update( RayWrap ray )
		{
			switch ( Axis )
			{
				case ClickedAxis.X:
					XAxis.UpdateCapture( Parent , ray );
					break;
				case ClickedAxis.Y:
					YAxis.UpdateCapture( Parent , ray );
					break;
				case ClickedAxis.Z:
					ZAxis.UpdateCapture( Parent , ray );
					break;
				case ClickedAxis.None:
					break;
				default:
					break;
			}

		}


	}
	internal class DraggableAxis : MMDModel
	{

		ClickState state;

		public DraggableAxis( string path ) : base( path )
		{
			state = new ClickState( this , ClickedAxis.None );

		}

		// hitpos からrayがくる
		public void OnClicked( Mouse mouse , RayWrap ray )
		{
			if ( !mouse.Clicked )
			{
				state.EndClick( );
				return;
			}
			Util.DebugWrite( $"DraggingState {state.IsDragging}" );
			if ( state.IsDragging )
			{
				Util.DebugWrite( $"Dragging {state.Axis}" );
				Dragging( ray );
				return;
			}

			foreach ( Face item in Faces )
			{
				HitResult res = ray.IntersectFace( item );
				if ( res.IsHit )
				{
					if ( res.Info.Contains( "red" ) )
					{
						state.Update( res , ClickedAxis.X );
					}
					if ( res.Info.Contains( "green" ) )
					{
						state.Update( res , ClickedAxis.Y );
					}
					if ( res.Info.Contains( "blue" ) )
					{
						state.Update( res , ClickedAxis.Z );
					}
					Util.DebugWrite( $"start {res.Info}" );
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
	//
	// this Widget implements translation constrained to an axis
	// 
	public class AxisTranslationWidget //: Standard3DTransformWidget
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
		public enum CoordSpace
		{
			ObjectCoords,
			WorldCoords,
		}
		// stored frames from target used during click-drag interaction
		Frame3f translateFrameL;        // local-spaace frame
		Frame3f translateFrameW;        // world-space frame
		Vector3 translateAxisW;     // world translation axis (redundant...)

		// computed values during interaction
		Frame3f raycastFrame;       // camera-facing plane containing translateAxisW
		float fTranslateStartT;   // start T-value along translateAxisW
		public static Frame3f GetGameObjectFrame( MMDModel go , CoordSpace eSpace )
		{
			if ( eSpace == CoordSpace.WorldCoords )
				return new Frame3f( go.Position , go.Rotation );
			else if ( eSpace == CoordSpace.ObjectCoords )
				//return new Frame3f(go.transform.localPosition, go.transform.localRotation);
				return new Frame3f( go.Position , go.Rotation );
			else
				throw new ArgumentException( "not possible without refernce to scene!" );
		}
		public static float ClosestPointOnLineT( Vector3 p0 , Vector3 dir , Vector3 pt )
		{
			float t = ( pt - p0 ).Dot( dir );
			return t;
		}
		public bool BeginCapture( MMDModel target , HitResult hit )
		{
			// save local and world frames
			translateFrameL = GetGameObjectFrame( target , CoordSpace.ObjectCoords );
			translateFrameW = GetGameObjectFrame( target , CoordSpace.WorldCoords );
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