using MMDataIO.Pmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vert = SharpHelper.TexturedVertex;

namespace SharpDXTest
{
	[StructLayout( LayoutKind.Explicit )]
	public struct MaterialGPUData 
	{
		[FieldOffset( 0 )]
		public bool UseSphere;

		[FieldOffset( 1 )]
		public bool IsAdd;

		[FieldOffset( 2 )]
		public float Alpha;
		
		// 2の乗数（128bit）でないと例外が起きるので詰め物をする
		[FieldOffset( 16 - 4 )]
		public float NULL;
	}

	public class Material
	{
		//;Material,材質名,材質名(英),拡散色_R,拡散色_G,拡散色_B,拡散色_A(非透過度),反射色_R,反射色_G,反射色_B,反射強度,環境色_R,環境色_G,環境色_B,両面描画(0/1),地面影(0/1),セルフ影マップ(0/1),セルフ影(0/1),頂点色(0/1),描画(0:Tri/1:Point/2:Line),エッジ(0/1),エッジサイズ,エッジ色_R,エッジ色_G,エッジ色_B,エッジ色_A,テクスチャパス,スフィアテクスチャパス,スフィアモード(0:無効/1:乗算/2:加算/3:サブテクスチャ),Toonテクスチャパス,メモ
		//Material,"スカート腕ヘドフォン","en",1,1,1,1,0,0,0,50,0.4,0.4,0.4,1,1,1,1,0,0,1,0.6,0.3,0.2,0.4,0.6,"k_huku1.png","body00_s.bmp",2,"toon_defo.bmp",""
		//;Face,親材質名,面Index,頂点Index1,頂点Index2,頂点Index3
		//Face,"スカート腕ヘドフォン",0,858,840,855
		public string Name;

		public string TexName;
		public string SphereName;
		public SphereMode Sphere;

		public IEnumerable<Vert> Vertice;

		public IEnumerable<int[]> Faces;

		public int[] FlattenFace;
		public MaterialGPUData MaterialData = new MaterialGPUData();

		public Material( string line )
		{
			string[] csv = line.Split( ',' );
			Name = csv[ 1 ];
			TexName = csv[ 26 ];
			TexName = TexName.Replace( "\"" , "" );
		}
		//      Pmx.Material[0].Faces[0].Vertex1.Position

		// PMXEditorから持って来る用
		public Material( string name , string texName , string sphereName , IEnumerable<Vert> vertice , IEnumerable<int[]> faces , SphereMode sphere = SphereMode.DISBLE)
		{
			Name = name;
			TexName = texName;
			SphereName = sphereName;
			Vertice = vertice;
			Faces = faces;
			Sphere = sphere;
			MaterialData.Alpha = 1;
			var sphereMode = Sphere;
			switch ( sphereMode )
			{
				case SphereMode.DISBLE:
					break;
				case SphereMode.MULT:
					MaterialData.UseSphere = true;
					break;
				case SphereMode.ADD:
					MaterialData.UseSphere = true;
					MaterialData.IsAdd = true;
					break;
				case SphereMode.SUB_TEXTURE:
					break;
				default:
					break;
			}

			FlattenFace = Faces.SelectMany( x => x ).ToArray( );
		}

		private IEnumerable<Vert> GetVertex( Vert[] vert )
		{
			foreach ( int[] i in Faces )
			{
				yield return vert[ i[ 0 ] ];
				yield return vert[ i[ 1 ] ];
				yield return vert[ i[ 2 ] ];
			}
		}

		public static IEnumerable<Material> MakeFromCSV( IEnumerable<string> lines , Dictionary<string , List<string>> dictionary , Vert[] verts )
		{
			foreach ( string item in lines )
			{
				Material mat = new Material( item );
				List<string> facecsv = dictionary[ mat.Name ];
				mat.Faces = Util.ParseFaceCSVAll( facecsv );
				mat.Vertice = mat.GetVertex( verts );
				mat.FlattenFace = mat.Faces.SelectMany( x => x ).ToArray( );
				yield return mat;
			}
		}

		public override string ToString()
		{
			return Name + " : " + TexName;
		}

	}
}
