//#define CPU8
using SharpDX;
using SharpDXTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using V3 = SharpDX.Vector3;

namespace BlenderModifier
{
  public class SphereCast
  {
    public float fac = 0.3f;
    public float radius = 2f;
    float len;
    public float size;
    public Matrix Pivot;
    public V3 Offset;
    Matrix TempMatrix;
    Matrix InvercePivot;
    int endt;
    V3[] SphereVertice;
    int length = 8;

    public void StartDeform(V3[] OriginalVertice)
    {
      Matrix ObjectMatrix = Matrix.Identity;
      Matrix InverceObject = Matrix.Identity;
      var offsetPivot = Matrix.Translation(Offset) + Pivot;
      //if (flag & MOD_CAST_USE_OB_TRANSFORM) {
      {
        InvercePivot = offsetPivot.Inverted();

        TempMatrix = InvercePivot * ObjectMatrix;

        InvercePivot = TempMatrix.Inverted();
      }
      InverceObject = ObjectMatrix.Inverted();
      V3 center = InverceObject.TransByMat(offsetPivot.TranslationVector).ToV3();

      var pivot = offsetPivot.TranslationVector;
      var ovs = OriginalVertice;
      len = size < float.Epsilon ? radius : size;
      // todo
      if (len <= 0)
      {
        len += ovs.Aggregate(0.0f, (acc, v) => acc += Vector3.Distance(v, center)) / ovs.Length;
      }
      //PmdWeightAuto.PmxUtil.BoneAttitude() は外から
      //modifier_get_vgroup(ob, dm, cmd->defgrp_name, &dvert, &defgrp_index);
#if CPU8
      for (int i = 0; i < length; i++)
      {
        ThreadPool.QueueUserWorkItem(id => cpuVShader(OriginalVertice, (int)id, length), i);
      }
#else
      NocpuVShader(OriginalVertice);

#endif
    }
    public SphereCast(Matrix pivot)
    {
      Pivot = pivot;
    }
    public V3[] GetSpereUntilEnd(V3[] ovs)
    {
      SphereVertice = ovs;
      StartDeform(ovs);
#if CPU8
      while (true)
      {
        if (endt%length==0 && endt!=0) return SphereVertice;
        Thread.Sleep(1);
      }
#else
      return SphereVertice;
#endif

    }

    void cpuVShader(V3[] vs, int id, int count)
    {

      var lenc = vs.Length / count;
      // 負荷分散
      var start = id * lenc;

      for (int i = start; i < start + lenc; i++)
      {

        var tmpCo = vs[i];
        {
          //\blenderSource\blender\source\blender\blenlib\BLI_math_matrix.h
          tmpCo = TempMatrix.TransByMat(tmpCo).ToV3();
        }
        if (i >= vs.Length) break;
        if (tmpCo.Length() > radius) continue;
        var vec = tmpCo;
        var facm = 1.0f - fac;

        var nv = vec.GetNormalized();
        tmpCo = nv * len * fac + tmpCo * facm;
        {
          tmpCo = InvercePivot.TransByMat(tmpCo).ToV3();
        }
        SphereVertice[i] = tmpCo;
      }
      endt++;
    }

    void NocpuVShader(V3[] vs)//, int id, int count)
    {

      var lenc = vs.Length;
      var start = 0;

      for (int i = start; i < lenc; i++)
      {

        var tmpCo = vs[i];
        {
          //\blenderSource\blender\source\blender\blenlib\BLI_math_matrix.h
          tmpCo = TempMatrix.TransByMat(tmpCo).ToV3();
          var t = TempMatrix.TranslationVector;
        }
        if (i >= vs.Length) break;
        if (tmpCo.Length() > radius) continue;
        var debtmpCo = tmpCo;
        var vec = new V3(tmpCo.X, tmpCo.Y, tmpCo.Z);
        var facm = 1.0f - fac;

        var nv = vec.GetNormalized();
        tmpCo = nv * len * fac + tmpCo * facm;
        {
          tmpCo = InvercePivot.TransByMat(tmpCo).ToV3();
        }
        SphereVertice[i] = tmpCo;
      }
      endt++;
    }
  }


}
