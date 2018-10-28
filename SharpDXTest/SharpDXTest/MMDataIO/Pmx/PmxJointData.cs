using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace MMDataIO.Pmx
{
	[Serializable]
	public class PmxJointData : IPmxData
	{
		public string RigidName = "";
		public string RigidNameE = "";

		public byte JointType;
		public int BodyA;
		public int BodyB;
		/*
12 : float3	| 位置(x,y,z)
12 : float3	| 回転(x,y,z) -> ラジアン角

12 : float3	| 移動制限-下限(x,y,z)
12 : float3	| 移動制限-上限(x,y,z)
12 : float3	| 回転制限-下限(x,y,z) -> ラジアン角
12 : float3	| 回転制限-上限(x,y,z) -> ラジアン角

12 : float3	| バネ定数-移動(x,y,z)
12 : float3	| バネ定数-回転(x,y,z)
		 */
		public Vector3 Position;
		public Vector3 Rot;
		public Vector3 MoveLower;
		public Vector3 MoveUpper;
		public Vector3 RotLower;
		public Vector3 RotUpper;
		public Vector3 SpringMove;
		public Vector3 SpringRot;

		public object Clone()
		{
			throw new NotImplementedException( );
		}

		public void Read( BinaryReader reader , PmxHeaderData header )
		{
			RigidName = reader.ReadText( header.Encoding );
			RigidNameE = reader.ReadText( header.Encoding );
			JointType = reader.ReadByte( );

			BodyA = reader.ReadPmxId( header.RigidIndexSize );
			BodyB = reader.ReadPmxId( header.RigidIndexSize );

			Position = reader.ReadVector3( );
			Rot = reader.ReadVector3( );
			MoveLower = reader.ReadVector3( );
			MoveUpper = reader.ReadVector3( );
			RotLower = reader.ReadVector3( );
			RotUpper = reader.ReadVector3( );
			SpringMove = reader.ReadVector3( );
			SpringRot = reader.ReadVector3( );
		}

		public void ReadPmd( BinaryReader reader , PmxHeaderData header )
		{
			throw new NotImplementedException( );
		}

		public void Write( BinaryWriter writer , PmxHeaderData header )
		{
			writer.WriteText( header.Encoding ,RigidName);
			writer.WriteText( header.Encoding ,RigidNameE);
			writer.Write(JointType );

			writer.WritePmxId( header.RigidIndexSize ,BodyA);
			writer.WritePmxId( header.RigidIndexSize ,BodyB);

			writer.WriteV( Position);
			writer.WriteV(Rot );
			writer.WriteV(MoveLower );
			writer.WriteV(MoveUpper );
			writer.WriteV(RotLower );
			writer.WriteV(RotUpper );
			writer.WriteV(SpringMove );
			writer.WriteV(SpringRot );
		}
	}
}
