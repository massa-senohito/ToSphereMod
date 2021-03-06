﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SharpDX;

namespace MMDataIO.Pmx
{
    public interface IPmxMorphTypeData : IPmxData
    {
        int Index { get; set; }
    }

    [Serializable]
    public struct PmxMorphBoneData : IPmxMorphTypeData
    {
        public int Index { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public object Clone() => new PmxMorphBoneData()
        {
            Index = Index,
            Position = Position,
            Rotation = Rotation,
        };

        public void Write(BinaryWriter writer, PmxHeaderData header)
        {
            writer.WritePmxId(header.BoneIndexSize, Index);

            writer.WriteV(Position);
            writer.WriteQ(Rotation);
        }

        public void Read(BinaryReader reader, PmxHeaderData header)
        {
            Index = reader.ReadPmxId(header.BoneIndexSize);

            Position = reader.ReadVector3();
            Rotation = reader.ReadQuaternion();
        }

        public void ReadPmd(BinaryReader reader, PmxHeaderData header)
        {
        }
    }

    [Serializable]
    public struct PmxMorphGroupData : IPmxMorphTypeData
    {
        public int Index { get; set; }
        public float Weight { get; set; }

        public object Clone() => new PmxMorphGroupData()
        {
            Index = Index,
            Weight = Weight,
        };

        public void Write(BinaryWriter writer, PmxHeaderData header)
        {
            writer.WritePmxId(header.MorphIndexSize, Index);
            writer.Write(Weight);
        }

        public void Read(BinaryReader reader, PmxHeaderData header)
        {
            Index = reader.ReadPmxId(header.MorphIndexSize);
            Weight = reader.ReadSingle();
        }

        public void ReadPmd(BinaryReader reader, PmxHeaderData header)
        {
        }
    }

    [Serializable]
    public struct PmxMorphMaterialData : IPmxMorphTypeData
    {
        public int Index { get; set; }

        public byte CalcType { get; set; }
        public Vector4 Diffuse { get; set; }
        public Vector3 Specular { get; set; }
        public float Shininess { get; set; }
        public Vector3 Ambient { get; set; }
        public Vector4 Edge { get; set; }
        public float EdgeThick { get; set; }
        public Vector4 Texture { get; set; }
        public Vector4 SphereTexture { get; set; }
        public Vector4 ToonTexture { get; set; }

        public object Clone() => new PmxMorphMaterialData()
        {
            Index = Index,
            CalcType = CalcType,
            Diffuse = Diffuse,
            Specular = Specular,
            Shininess = Shininess,
            Ambient = Ambient,
            Edge = Edge,
            EdgeThick = EdgeThick,
            Texture = Texture,
            SphereTexture = SphereTexture,
            ToonTexture = ToonTexture,
        };

        public void Write(BinaryWriter writer, PmxHeaderData header)
        {
            writer.WritePmxId(header.MaterialIndexSize, Index);

            writer.Write(CalcType);
            writer.WriteV(Diffuse);
            writer.WriteV(Specular);
            writer.Write(Shininess);
            writer.WriteV(Ambient);
            writer.WriteV(Edge);
            writer.Write(EdgeThick);
            writer.WriteV(Texture);
            writer.WriteV(SphereTexture);
            writer.WriteV(ToonTexture);
        }

        public void Read(BinaryReader reader, PmxHeaderData header)
        {
            Index = reader.ReadPmxId(header.MaterialIndexSize);

            CalcType = reader.ReadByte();
            Diffuse = reader.ReadVector4();
            Specular = reader.ReadVector3();
            Shininess = reader.ReadSingle();
            Ambient = reader.ReadVector3();
            Edge = reader.ReadVector4();
            EdgeThick = reader.ReadSingle();
            Texture = reader.ReadVector4();
            SphereTexture = reader.ReadVector4();
            ToonTexture = reader.ReadVector4();
        }

        public void ReadPmd(BinaryReader reader, PmxHeaderData header)
        {
        }
    }

    [Serializable]
    public struct PmxMorphUVData : IPmxMorphTypeData
    {
        public int Index { get; set; }
        public Vector4 Uv { get; set; }

        public object Clone() => new PmxMorphUVData()
        {
            Index = Index,
            Uv = Uv,
        };

        public void Write(BinaryWriter writer, PmxHeaderData header)
        {
            writer.WritePmxId(header.VertexIndexSize, Index);
            writer.WriteV(Uv);
        }

        public void Read(BinaryReader reader, PmxHeaderData header)
        {
            Index = reader.ReadPmxId(header.VertexIndexSize);
            Uv = reader.ReadVector4();
        }

        public void ReadPmd(BinaryReader reader, PmxHeaderData header)
        {
        }
    }

    [Serializable]
    public struct PmxMorphVertexData : IPmxMorphTypeData
    {
        public int Index { get; set; }
        public Vector3 Position { get; set; }

        public object Clone() => new PmxMorphVertexData()
        {
            Index = Index,
            Position = Position,
        };

        public void Write(BinaryWriter writer, PmxHeaderData header)
        {
            writer.WritePmxId(header.VertexIndexSize, Index);
            writer.WriteV(Position);
        }

        public void Read(BinaryReader reader, PmxHeaderData header)
        {
            Index = reader.ReadPmxId(header.VertexIndexSize);
            Position = reader.ReadVector3();
        }

        public void ReadPmd(BinaryReader reader, PmxHeaderData header)
        {
        }

		public override string ToString()
		{
			return "VertexMorph,\"testc\"," +Index.ToString( ) + "," + Position.X +"," + Position.Y + "," + Position.Z;
		}
	}

    [Serializable]
    public struct PmxMorphImpulseData : IPmxMorphTypeData
    {
        public int Index { get; set; }
        public bool IsLocal { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Torque { get; set; }

        public object Clone() => new PmxMorphImpulseData
        {
            Index = Index,
            IsLocal = IsLocal,
            Velocity = Velocity,
            Torque = Torque
        };

        public void Read(BinaryReader reader, PmxHeaderData header)
        {
            Index = reader.ReadPmxId(header.RigidIndexSize);
            IsLocal = reader.ReadBoolean();
            Velocity = reader.ReadVector3();
            Torque = reader.ReadVector3();
        }

        public void ReadPmd(BinaryReader reader, PmxHeaderData header)
        {
            throw new NotImplementedException();
        }

        public void Write(BinaryWriter writer, PmxHeaderData header)
        {
            writer.WritePmxId(header.RigidIndexSize, Index);
            writer.Write(IsLocal);
            writer.WriteV(Velocity);
            writer.WriteV(Torque);
        }
    }
}
