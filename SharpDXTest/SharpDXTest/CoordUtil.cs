using SharpDX;
using SharpHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{

	public static class CordUtil
	{
		public static Vector3 InvX( this Vector3 v )
		{
			return new Vector3( -v.X , v.Y , v.Z );
		}

		public static string Csv( this Vector3 v )
		{
			return v.X + "," + v.Y + "," + v.Z;
		}

		public static Vector2 Normal( this Vector2 v , Vector2 v2 )
		{
			Vector2 temp = ( v - v2 );
			temp.Normalize( );
			return new Vector2( -temp.Y , temp.X );
		}

		public static float Dot( this Vector3 v , Vector3 v2 )
		{
			return Vector3.Dot( v , v2 );
		}

		public static Vector3 Cross( this Vector3 v , Vector3 v2 )
		{
			return Vector3.Cross( v , v2 );
		}

		public static Matrix TransMat( this Vector3 v )
		{
			return Matrix.Translation( v );
		}

		public static bool IsZero( this Vector3 v )
		{
			return
			  v.X.IsNearlyZero( ) &&
			  v.Y.IsNearlyZero( ) &&
			  v.Z.IsNearlyZero( );
		}

		public static Vector3 Copy( this Vector3 vector )
		{
			return new Vector3( vector.X , vector.Y , vector.Z );
		}

		public static Vector3 YZInverted( this Vector3 vector )
		{
			return new Vector3( vector.X , vector.Z , vector.Y );
		}

		public static Vector3 GetNormalized( this Vector3 v )
		{
			Vector3 temp = new Vector3( v.X , v.Y , v.Z );
			temp.Normalize( );
			return temp;
		}

		public static string DebugStr( this Vector3 v )
		{
			return VDBDebugger.vdb_point( v.X , v.Y , v.Z );
		}

		public static Quaternion ToQuaternion( this Vector3 v )
		{
			return ToQuaternion( v.Y , v.Z , v.X );
		}

		public static Quaternion QuatFromEuler( this Vector3 v )
		{
			return ToQuaternion( new Vector3( v.X.Rad( ) , v.Y.Rad( ) , v.Z.Rad( ) ) );
		}

		public static Vector3 ToV3( this Vector4 v )
		{
			return new Vector3( v.X , v.Y , v.Z );
		}

		public static Matrix RotMat( this Quaternion q )
		{
			return Matrix.RotationQuaternion( q );
		}

		public static Quaternion FromTo( this Vector3 vFrom , Vector3 vTo )
		{
			// [TODO] this page seems to have optimized version:
			//    http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors

			// [RMS] not ideal to explicitly normalize here, but if we don't,
			//   output quaternion is not normalized and this causes problems,
			//   eg like drift if we do repeated SetFromTo()
			Vector3 from = vFrom.GetNormalized( ), to = vTo.GetNormalized( );
			Vector3 bisector = ( from + to ).GetNormalized( );
			Quaternion q;
			q.W = from.Dot( bisector );
			if ( q.W != 0 )
			{
				Vector3 cross = from.Cross( bisector );
				q.X = cross.X;
				q.Y = cross.Y;
				q.Z = cross.Z;
			}
			else
			{
				float invLength;
				if ( Math.Abs( from.X ) >= Math.Abs( from.Y ) )
				{
					// V1.x or V1.z is the largest magnitude component.
					invLength = ( float )( 1.0 / Math.Sqrt( from.X * from.X + from.Z * from.Z ) );
					q.X = -from.Z * invLength;
					q.Y = 0;
					q.Z = +from.X * invLength;
				}
				else
				{
					// V1.y or V1.z is the largest magnitude component.
					invLength = ( float )( 1.0 / Math.Sqrt( from.Y * from.Y + from.Z * from.Z ) );
					q.X = 0;
					q.Y = +from.Z * invLength;
					q.Z = -from.Y * invLength;
				}
			}
			q.Normalize( );   // aaahhh just to be safe...
			return q;
		}

		static float Copysign( float a , float b )
		{
			if ( b < 0 )
			{
				return -a.Abs( );
			}
			return a.Abs( );
		}

		public static Vector3 EulerAngle( this Quaternion q )
		{
			float sin = 2.0f * ( q.W * q.X + q.Y * q.Z );
			float cos = 1.0f - 2.0f * ( q.X * q.X + q.Y * q.Y );
			float roll = ( float )Math.Atan2( sin , cos );
			float sinP = 2 * ( q.W * q.Y - q.Z * q.X );
			float pitch = 0;
			if ( sinP.Abs( ) >= 1 )
			{
				float p = ( float )( Math.PI / 2.0 );
				pitch = Copysign( p , sinP );
			}
			else
			{
				pitch = ( float )Math.Asin( sinP );
			}
			float sinY = 2.0f * ( q.W * q.Z + q.X * q.Y );
			float cosY = 1.0f - 2.0f * ( q.Y * q.Y + q.Z * q.Z );
			float yaw = ( float )Math.Atan2( sinY , cosY );
			return new Vector3( roll.Deg( ) , pitch.Deg( ) , yaw.Deg( ) );
		}

		public static Quaternion ToQuaternion( float pitch , float yaw , float roll )
		{
			//https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
			Quaternion q;
			// Abbreviations for the various angular functions
			float cy = ( float )Math.Cos( yaw * 0.5 );
			float sy = ( float )Math.Sin( yaw * 0.5 );
			float cr = ( float )Math.Cos( roll * 0.5 );
			float sr = ( float )Math.Sin( roll * 0.5 );
			float cp = ( float )Math.Cos( pitch * 0.5 );
			float sp = ( float )Math.Sin( pitch * 0.5 );

			q.W = cy * cr * cp + sy * sr * sp;
			q.X = cy * sr * cp - sy * cr * sp;
			q.Y = cy * cr * sp + sy * sr * cp;
			q.Z = sy * cr * cp - cy * sr * sp;
			return q;
		}

		public static Matrix CopyMat( this Matrix m )
		{
			Matrix temp = new Matrix( );
			temp.M11 = m.M11;
			temp.M12 = m.M12;
			temp.M13 = m.M13;
			temp.M14 = m.M14;
			temp.M21 = m.M21;
			temp.M22 = m.M22;
			temp.M23 = m.M23;
			temp.M24 = m.M24;
			temp.M31 = m.M31;
			temp.M32 = m.M32;
			temp.M33 = m.M33;
			temp.M34 = m.M34;
			temp.M41 = m.M41;
			temp.M42 = m.M42;
			temp.M43 = m.M43;
			temp.M44 = m.M44;
			return temp;
		}

		public static Vector4 TransByMat( this Matrix m , Vector3 v )
		{
			return Vector3.Transform( v , m );
		}

		public static Matrix Inverted( this Matrix m )
		{
			Matrix temp = m.CopyMat( );
			temp.Invert( );
			return temp;
		}

		public static Vector3 EulerAngle( this Matrix m )
		{
			m.Decompose( out Vector3 scale , out Quaternion rot , out Vector3 trans );
			return rot.EulerAngle( );
		}

		public static void SetPosition( this TexturedVertex vert , Vector3 pos )
		{
			vert.Position = pos;
		}
	}
}
