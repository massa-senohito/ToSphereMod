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

    public ClickedAxis Axis;
    public Vector3 ClickedPos;
    public ClickState(Vector3 vector,ClickedAxis axis)
    {
      Axis = axis;
      ClickedPos = vector;
    }
    public static ClickState Null = new ClickState(Vector3.Zero,ClickedAxis.None);
    public void Update(Vector3 vector,ClickedAxis axis)
    {
      ClickedPos = vector;
      Axis = axis;
    }
     
  }
  internal class DraggableAxis : MMDModel
  {

    ClickState state = ClickState.Null;

    public DraggableAxis(string path):base(path)
    {

    }

    // hitpos からrayがくる
    void OnClicked(RayWrap ray)
    {
      foreach (var item in Faces)
      {
        var res = ray.IntersectFace(item);
        if (res.IsHit)
        {
          if(res.Info == "red")
          {
            state.Update(res.HitPosition, ClickedAxis.X);
          }
          if(res.Info == "green")
          {
            state.Update(res.HitPosition, ClickedAxis.Y);
          }
          if(res.Info == "blue")
          {
            state.Update(res.HitPosition, ClickedAxis.Z);
          }
        }
        // 別のとこクリックするとドラッグ外す
        else
        {
          state = ClickState.Null;
        }
      }

    }

    void Dragging()
    {

    }
  }
}


/*
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


        public Func<Frame3f, int, float, float> DeltaDistanceConstraintF = null;


        int nTranslationAxis;

		public AxisTranslationWidget(int nFrameAxis)
		{
			nTranslationAxis = nFrameAxis;
		}

		// stored frames from target used during click-drag interaction
		Frame3f translateFrameL;		// local-spaace frame
		Frame3f translateFrameW;		// world-space frame
		Vector3 translateAxisW;		// world translation axis (redundant...)

		// computed values during interaction
		Frame3f raycastFrame;		// camera-facing plane containing translateAxisW
		float fTranslateStartT;		// start T-value along translateAxisW

		public bool BeginCapture(ITransformable target, Ray3f worldRay, UIRayHit hit)
		{
			// save local and world frames
			translateFrameL = target.GetLocalFrame (CoordSpace.ObjectCoords);
			translateFrameW = target.GetLocalFrame (CoordSpace.WorldCoords);
			translateAxisW = translateFrameW.GetAxis (nTranslationAxis);

			// save t-value of closest point on translation axis, so we can find delta-t
			Vector3 vWorldHitPos = hit.hitPos;
			fTranslateStartT = Distance.ClosestPointOnLineT(
				translateFrameW.Origin, translateAxisW, vWorldHitPos);

            // construct plane we will ray-intersect with in UpdateCapture()
            Vector3 makeUp = Vector3.Cross(FPlatform.MainCamera.Forward(), translateAxisW).Normalized;
            Vector3 vPlaneNormal = Vector3.Cross(makeUp, translateAxisW).GetNormalized();
            raycastFrame = new Frame3f(vWorldHitPos, vPlaneNormal);

            return true;
		}

		public override bool UpdateCapture(ITransformable target, Ray3f worldRay)
		{
			// ray-hit with plane that contains translation axis
			Vector3 planeHit = raycastFrame.RayPlaneIntersection(worldRay.Origin, worldRay.Direction, 2);

			// figure out new T-value along axis, then our translation update is delta-t
			float fNewT = Distance.ClosestPointOnLineT (translateFrameW.Origin, translateAxisW, planeHit);
			float fDeltaT = (fNewT - fTranslateStartT);
            fDeltaT *= TranslationScaleF();
            if (DeltaDistanceConstraintF != null)
                fDeltaT = DeltaDistanceConstraintF(translateFrameL, nTranslationAxis, fDeltaT);

            // construct new frame translated along axis (in local space)
            Frame3f newFrame = translateFrameL;
			newFrame.Origin += fDeltaT * translateFrameL.GetAxis(nTranslationAxis);

			// update target
			target.SetLocalFrame (newFrame, CoordSpace.ObjectCoords);

			return true;
		}

        public override bool EndCapture(ITransformable target)
        {
            return true;
        }

        public override void Disconnect()
        {
            RootGameObject.Destroy();
        }


        static Interval1d CosVisibilityRange = new Interval1d(
            -Math.Cos(45 * MathUtil.Deg2Rad), Math.Cos(15 * MathUtil.Deg2Rad));
        public override bool CheckVisibility(ref Frame3f curFrameW, ref Vector3d eyePosW) {
            Vector3d axis = curFrameW.GetAxis(nTranslationAxis);
            Vector3d eyevec = (eyePosW - curFrameW.Origin).Normalized;
            double dot = axis.Dot(eyevec);
            return CosVisibilityRange.Contains(dot);
        }

    }
}


   
   
   */