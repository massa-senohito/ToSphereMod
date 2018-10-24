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
	//\blender\source\blender\modifiers\intern\MOD_cast.c
	public class SphereCast
	{
		public float Fac = 0.3f;
		public float Radius = 2f;
		float Len;
		public float Size;
		public Matrix Pivot;
		public V3 Offset;
		Matrix TempMatrix;
		Matrix InvercePivot;
		V3[] SphereVertice;

		public void StartDeform( V3[] OriginalVertice )
		{
			Matrix objectMatrix = Matrix.Identity;
			Matrix InverceObject = Matrix.Identity;
			Matrix offsetPivot = Matrix.Translation( Offset ) + Pivot;

			// 軸のオブジェクトを中心として使用するため必ず通す
			//if (flag & MOD_CAST_USE_OB_TRANSFORM)
			{
				InvercePivot = offsetPivot.Inverted( );

				TempMatrix = InvercePivot * objectMatrix;

				InvercePivot = TempMatrix.Inverted( );
			}
			InverceObject = objectMatrix.Inverted( );
			V3 center = InverceObject.TransByMat( offsetPivot.TranslationVector ).ToV3( );

			V3 pivot = offsetPivot.TranslationVector;
			V3[] ovs = OriginalVertice;
			Len = Size < float.Epsilon ? Radius : Size;
			// blenderではあまり良い結果にならなかったためsizeは対応しない
			if ( Len <= 0 )
			{
				Len += ovs.Aggregate( 0.0f , ( acc , v ) => acc += Vector3.Distance( v , center ) ) / ovs.Length;
			}
			//modifier_get_vgroup(ob, dm, cmd->defgrp_name, &dvert, &defgrp_index);

			VerticeProc( OriginalVertice );

		}

		public SphereCast( Matrix pivot )
		{
			Pivot = pivot;
		}

		public V3[] GetSpereUntilEnd( V3[] ovs )
		{
			SphereVertice = ovs;
			StartDeform( ovs );

			return SphereVertice;
		}


		void VerticeProc( V3[] vs )
		{

			int lenc = vs.Length;
			int start = 0;

			for ( int i = start ; i < lenc ; i++ )
			{

				V3 tmpCo = vs[ i ];
				{
					//\blenderSource\blender\source\blender\blenlib\BLI_math_matrix.h
					// 軸オブジェクト中心の座標系に頂点を持っていく
					tmpCo = TempMatrix.TransByMat( tmpCo ).ToV3( );
					V3 t = TempMatrix.TranslationVector;
				}
				if ( i >= vs.Length )
				{
					break;
				}
				if ( tmpCo.Length( ) > Radius )
				{
					continue;
				}
				V3 vec = new V3( tmpCo.X , tmpCo.Y , tmpCo.Z );
				float facm = 1.0f - Fac;

				V3 nv = vec.GetNormalized( );
				tmpCo = nv * Len * Fac + tmpCo * facm;
				{
					// 軸中心の座標系頂点を元の位置に戻す
					tmpCo = InvercePivot.TransByMat( tmpCo ).ToV3( );
				}
				SphereVertice[ i ] = tmpCo;
			}
		}
	}

}
