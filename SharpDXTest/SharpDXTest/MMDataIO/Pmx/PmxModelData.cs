﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpDX;

namespace MMDataIO.Pmx
{
    [Serializable]
    public class PmxModelData
    {
        public PmxHeaderData Header { get; set; } = new PmxHeaderData();
        public PmxVertexData[] VertexArray { get; set; } = { };
        public PmxMaterialData[] MaterialArray { get; set; } = { };
        public PmxBoneData[] BoneArray { get; set; } = { };
        public PmxMorphData[] MorphArray { get; set; } = { };
        public PmxSlotData[] SlotArray { get; set; } = { };
        public PmxRigidData[] RigidArray { get; set; } = { };
        public int[] VertexIndices { get; set; } = { };
        public string[] TextureFiles { get; set; } = { };

		public PmxModelData( )
		{
		}

		public PmxModelData( BinaryReader path )
		{
			Read( path );
		}

        public object Clone() => new PmxModelData()
        {
            Header = CloneUtil.Clone(Header),
            VertexArray = CloneUtil.CloneArray(VertexArray),
            MaterialArray = CloneUtil.CloneArray(MaterialArray),
            BoneArray = CloneUtil.CloneArray(BoneArray),
            MorphArray = CloneUtil.CloneArray(MorphArray),
            SlotArray = CloneUtil.CloneArray(SlotArray),
            VertexIndices = CloneUtil.CloneArray(VertexIndices),
            TextureFiles = CloneUtil.CloneArray(TextureFiles),
        };

		public void Write( string path )
		{
			using ( FileStream sr = new FileStream( path , FileMode.Create , FileAccess.Write ) )
			{
				using ( BinaryWriter bin = new BinaryWriter( sr ) )
				{
					Write( bin );
				}

			}
		}

        public void Write(BinaryWriter writer)
        {
            Header.VertexIndexSize = CalcIndexSize(VertexArray.Length);
            Header.TextureIndexSize = CalcIndexSize(TextureFiles.Length);
            Header.MaterialIndexSize = CalcIndexSize(MaterialArray.Length);
            Header.BoneIndexSize = CalcIndexSize(BoneArray.Length);
            Header.MorphIndexSize = CalcIndexSize(MorphArray.Length);
            Header.RigidIndexSize = CalcIndexSize(RigidArray.Length);

            WritePmxData(Header, writer, Header);
            WritePmxData(VertexArray, writer, Header);
            WriteData(VertexIndices, (i, ex) => ex.WritePmxId(Header.VertexIndexSize,i), writer);
            WriteData(TextureFiles, (s, ex) => ex.WriteText(Header.Encoding, s), writer);
            WritePmxData(MaterialArray, writer, Header);
            WritePmxData(BoneArray, writer, Header);
            WritePmxData(MorphArray, writer, Header);
            WritePmxData(SlotArray, writer, Header);
            WritePmxData(RigidArray, writer, Header);
            writer.Write(0);//Number of Joint
            writer.Write(0);//Number of SoftBody

            byte CalcIndexSize(int count)
            {
                if (count <= sbyte.MaxValue)
                    return 1;
                else if (count <= short.MaxValue)
                    return 2;
                else
                    return 4;
            };
        }

        public void Read(BinaryReader reader)
        {
            ReadPmxData(Header, reader, Header);
            VertexArray = ReadPmxData<PmxVertexData>(reader, Header);
            VertexIndices = ReadData<int>((p, i) => p.ReadPmxId(Header.VertexIndexSize), reader);
            TextureFiles = ReadData<string>((p, i) => p.ReadText(Header.Encoding), reader);
            MaterialArray = ReadPmxData<PmxMaterialData>(reader, Header);
            BoneArray = ReadPmxData<PmxBoneData>(reader, Header);
            MorphArray = ReadPmxData<PmxMorphData>(reader, Header);
            SlotArray = ReadPmxData<PmxSlotData>(reader, Header);
            RigidArray = ReadPmxData<PmxRigidData>(reader, Header);
        }

        public void ReadPmd(BinaryReader reader)
        {
            ReadPmxData(Header, reader, Header, true);
            VertexArray = ReadPmxData<PmxVertexData>(reader, Header, true);
            VertexIndices = ReadData<int>((p, i) => p.ReadUInt16(), reader);
            TextureFiles = ReadData<string>((p, i) => p.ReadText(Header.Encoding), reader);
            MaterialArray = ReadPmxData<PmxMaterialData>(reader, Header, true);
            BoneArray = ReadPmxData<PmxBoneData>(reader, Header, reader.ReadUInt16(), true);
            MorphArray = ReadPmxData<PmxMorphData>(reader, Header, true);
            SlotArray = ReadPmxData<PmxSlotData>(reader, Header, true);
        }

        private void WriteData<T>(T[] data, Action<T, BinaryWriter> action, BinaryWriter writer)
        {
            writer.Write(data.Length);
            Array.ForEach(data, d => action.Invoke(d, writer));
        }

        private void WritePmxData<T>(T data, BinaryWriter writer, PmxHeaderData header) where T : IPmxData
        {
            data.Write(writer, header);
        }

        private void WritePmxData<T>(T[] data, BinaryWriter writer, PmxHeaderData header) where T : IPmxData
        {
            writer.Write(data.Length);
            Array.ForEach(data, d => d.Write(writer, header));
        }

        private T[] ReadData<T>(Func<BinaryReader, T, T> valueFunc, BinaryReader reader)
        {
            int len = reader.ReadInt32();
            T[] array = new T[len];

            for (int i = 0; i < len; i++)
            {
                array[i] = valueFunc(reader, array[i]);
            }
            return array;
        }

        private void ReadPmxData<T>(T data, BinaryReader reader, PmxHeaderData header, bool pmd = false) where T : IPmxData
        {
            if (pmd)
            {
                data.ReadPmd(reader, header);
            }
            else
            {
                data.Read(reader, header);
            }
        }

        private T[] ReadPmxData<T>(BinaryReader reader, PmxHeaderData header, bool pmd = false) where T : IPmxData, new()
        {
            return ReadPmxData<T>(reader, header, reader.ReadInt32(), pmd);
        }

        private T[] ReadPmxData<T>(BinaryReader reader, PmxHeaderData header, int len, bool pmd = false) where T : IPmxData, new()
        {
            T[] array = new T[len];

            for (int i = 0; i < len; i++)
            {
                array[i] = new T();

                if (pmd)
                {
                    array[i].ReadPmd(reader, header);
                }
                else
                {
                    array[i].Read(reader, header);
                }
            }
            return array;
        }
    }

    public static class PmxBinaryIOExtensions
    {
        public static void WritePmxId(this BinaryWriter writer, int size, int id)
        {
            switch (size)
            {
                case 1:
                    writer.Write((byte)id);
                    break;

                case 2:
                    writer.Write((short)id);
                    break;

                case 4:
                    writer.Write(id);
                    break;

                default:
                    throw new ArgumentException("Unexpected size of byte was given");
            }
        }

        public static void WriteText(this BinaryWriter writer, Encoding encoding, string text)
        {
            if (text == null) text = "";

            byte[] bytes = encoding.GetBytes(text.ToCharArray());

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        public static int ReadPmxId(this BinaryReader reader, byte size)
        {
            int id = 0;

            switch (size)
            {
                case 1:
                    id = reader.ReadByte();
                    break;

                case 2:
                    id = reader.ReadUInt16();
                    break;

					// todo おそらく全部Uint,ただCloneなどでボクシングできないので一旦おき
                case 4:
                    id = reader.ReadInt32();
                    break;
            }
            return id;
        }

        public static string ReadText(this BinaryReader reader, Encoding encoding)
        {
            int len = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(len);

            string str = encoding.GetString(bytes);

            return str;
        }

        public static string ReadText(this BinaryReader reader, Encoding encoding, int length)
        {
            byte[] bytes = reader.ReadBytes(length);

            string str = encoding.GetString(bytes);

            return str.TrimEnd();
        }

        public static Vector2 ReadVector2(this BinaryReader r) => new Vector2(r.ReadSingle(), r.ReadSingle());

        public static Vector3 ReadVector3(this BinaryReader r) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

        public static Vector4 ReadVector4(this BinaryReader r) => new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

        public static Quaternion ReadQuaternion(this BinaryReader r) => new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    }

    public enum IndexType : byte
    {
        VERTEX = 2,
        TEXTURE = 3,
        MATERIAL = 4,
        BONE = 5,
        MORPH = 6,
        RIGID = 7,
    }
}
