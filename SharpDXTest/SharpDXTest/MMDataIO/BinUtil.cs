using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDataIO
{
	public static class BinUtil
	{
		public static void WriteV( this BinaryWriter writer ,Vector2 v)
		{
			writer.Write( v.X );
			writer.Write( v.Y );
		}

		public static void WriteV( this BinaryWriter writer ,Vector3 v)
		{
			writer.Write( v.X );
			writer.Write( v.Y );
			writer.Write( v.Z );
		}

		public static void WriteV( this BinaryWriter writer ,Vector4 v)
		{
			writer.Write( v.X );
			writer.Write( v.Y );
			writer.Write( v.Z );
			writer.Write( v.W );
		}

		public static void WriteQ( this BinaryWriter writer ,Quaternion v)
		{
			writer.Write( v.X );
			writer.Write( v.Y );
			writer.Write( v.Z );
			writer.Write( v.W );
		}
	}
}
