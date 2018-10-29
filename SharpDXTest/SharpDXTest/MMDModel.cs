﻿using BlenderModifier;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpHelper;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vert = SharpHelper.TexturedVertex;

namespace SharpDXTest
{
	public class Material
	{
		//;Material,材質名,材質名(英),拡散色_R,拡散色_G,拡散色_B,拡散色_A(非透過度),反射色_R,反射色_G,反射色_B,反射強度,環境色_R,環境色_G,環境色_B,両面描画(0/1),地面影(0/1),セルフ影マップ(0/1),セルフ影(0/1),頂点色(0/1),描画(0:Tri/1:Point/2:Line),エッジ(0/1),エッジサイズ,エッジ色_R,エッジ色_G,エッジ色_B,エッジ色_A,テクスチャパス,スフィアテクスチャパス,スフィアモード(0:無効/1:乗算/2:加算/3:サブテクスチャ),Toonテクスチャパス,メモ
		//Material,"スカート腕ヘドフォン","en",1,1,1,1,0,0,0,50,0.4,0.4,0.4,1,1,1,1,0,0,1,0.6,0.3,0.2,0.4,0.6,"k_huku1.png","body00_s.bmp",2,"toon_defo.bmp",""
		//;Face,親材質名,面Index,頂点Index1,頂点Index2,頂点Index3
		//Face,"スカート腕ヘドフォン",0,858,840,855
		public string Name;

		public string TexName;
		public string SphereName;

		public IEnumerable<Vert> Vertice;

		public IEnumerable<int[]> Faces;

		public int[] FlattenFace;

		public Material( string line )
		{
			string[] csv = line.Split( ',' );
			Name = csv[ 1 ];
			TexName = csv[ 26 ];
			TexName = TexName.Replace( "\"" , "" );
		}
		//      Pmx.Material[0].Faces[0].Vertex1.Position

		// PMXEditorから持って来る用
		public Material( string name , string texName , string sphereName , IEnumerable<Vert> vertice , IEnumerable<int[]> faces )
		{
			Name = name;
			TexName = texName;
			SphereName = sphereName;
			Vertice = vertice;
			Faces = faces;
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

	[StructLayout( LayoutKind.Explicit )]
	public struct GPUData
	{
		[FieldOffset( 0 )]
		public Matrix World;

		[FieldOffset( 64 )]
		public Matrix View;

		[FieldOffset( 128 )]
		public Matrix WorldViewProjection;

		[FieldOffset( 128 + 64 )]
		public float Alpha;
		
		// 2の乗数（128bit）でないと例外が起きるので詰め物をする
		[FieldOffset( 512 - 4 )]
		public float NULL;
	}

	public class MMDModel : System.IDisposable
	{
		public Vert[] Vertice;

		public Vert[] OrigVertice
		{
			get;
			private set;
		}

		public int[] Index;

		public Face[] Faces;

		public IEnumerable<Material> Materials;

		SphereCast Cast;

		public string DirPath
		{
			get;
		}

		Option<ShaderResourceView> TryGetResource(SharpDevice device , string path )
		{

			string filename = DirPath + Path.DirectorySeparatorChar + path;
			if ( File.Exists( filename ) )
			{
				return Option.Return(device.LoadTextureFromFile(filename));
			}
			else
			{
				return Option.Return<ShaderResourceView>( );
			}
		}

		SharpMesh GetSharpMesh( SharpDevice device )
		{
			SharpMesh mesh = new SharpMesh( device );
			List<int> indices = new List<int>( );
			List<Face> faces = new List<Face>( );
			List<string> texList = new List<string>( );
			List<string> notFoundTexList = new List<string>( );
			int icount = 0;
			foreach ( Material item in Materials )
			{
				indices.AddRange( item.FlattenFace );
				int faceCount = item.FlattenFace.Count( );

				SharpSubSet subSet = new SharpSubSet( )
				{
					IndexCount = faceCount ,
					StartIndex = icount ,
				};

				if ( item.TexName != string.Empty )
				{
					var filename = TryGetResource(device,item.TexName);
					// 存在しないパスなら読まない
					filename.Match( () => notFoundTexList.Add( item.TexName ) , r => subSet.DiffuseMap = r );
				}

				if ( item.SphereName != string.Empty )
				{
					var filename = TryGetResource(device,item.SphereName);
					filename.Match( () => notFoundTexList.Add( item.SphereName) , r => subSet.SphereMap = r );
				}

				mesh.SubSets.Add( subSet );

				for ( int i = icount ; i < icount + faceCount ; i += 3 )
				{
					int ind1 = indices[ i ];
					int ind2 = indices[ i + 1 ];
					int ind3 = indices[ i + 2 ];
					Face item1 = new Face( Vertice[ ind1 ].Position , Vertice[ ind2 ].Position , Vertice[ ind3 ].Position , item.Name );
					//Debug.WriteLine("first " + item1.TriString);
					faces.Add( item1 );

				}
				icount += faceCount;
			}
			Index = indices.ToArray( );
			Faces = faces.ToArray( );
			var inds = Index.Select( x => x.ToString( ) );
			var facesS = Faces.Select( x => x.ToString( ) );
			//inds.Concat( facesS ).Concat( texList ).WriteFile( DirPath + "loadedFile.txt" );
			notFoundTexList.WriteFile( DirPath + "notFoundTextures.txt" );

			ModelStr = Faces.Select( f => f.TriString ).ConcatStr( );
			mesh.SetOnly( Vertice , Index );
			return mesh;
		}

		public MMDModel( string path )
		{
			DirPath = Directory.GetParent( path ).FullName;
			string[] lines = File.ReadAllLines( path );
			//;Face,親材質名,面Index,頂点Index1,頂点Index2,頂点Index3
			//Face,"スカート腕ヘドフォン",0,858,840,855
			// のように最初の文字列がVertexだと頂点 Faceだと面になる ;が最初に来るものは説明用のものなので弾く
			var gr = lines.GroupBy( l => l.Split( ',' )[ 0 ] );
			var gs = gr.Where( g => !g.Key.Contains( ";" ) ).ToDictionary( s => s.Key , g => g.ToList( ) );

			Vertice = ParseCSV( gs[ "Vertex" ] ).ToArray( );
			OrigVertice = new Vert[ Vertice.Length ];
			Vertice.ArrayFullCopy( OrigVertice );
			// 次の文字列は材質名になるので、材質名のグループを作る
			var faceGr = gs[ "Face" ].GroupBy( s => s.Split( ',' )[ 1 ] ).ToDictionary( s => s.Key , g => g.ToList( ) );
			Materials = Material.MakeFromCSV( gs[ "Material" ] , faceGr , Vertice );

			GpuData = new GPUData( );
			GpuData.Alpha = 1;
			Cast = new SphereCast( Matrix.Zero );
		}

		public MMDModel( Vert[] vertex , IEnumerable<Material> material , string path )
		{
			DirPath = path;
			Vertice = vertex;
			OrigVertice = new Vert[ Vertice.Length ];
			Vertice.ArrayFullCopy( OrigVertice );
			Materials = material;
			GpuData = new GPUData( );
			GpuData.Alpha = 1;
			Cast = new SphereCast( Matrix.Zero );
		}

		protected SharpMesh Mesh;
		Buffer GPUDataBuffer;
		GPUData GpuData;
		SharpShader Shader;
		VertexShaderStage VertexShader;
		PixelShaderStage PixelShader;

		public void LoadTexture( SharpDevice device )
		{

			Mesh = GetSharpMesh( device );

			//init shader
			Shader = new SharpShader( device , "../../HLSLModel.txt" ,
				new SharpShaderDescription( ) { VertexShaderFunction = "VS" , PixelShaderFunction = "PS" } ,
				new InputElement[] {
						new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
						new InputElement("NORMAL"  , 0, Format.R32G32B32_Float, 12, 0),
						new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
				} );
			GPUDataBuffer = Shader.CreateBuffer<GPUData>( );
			VertexShader = device.DeviceContext.VertexShader;
			PixelShader = device.DeviceContext.PixelShader;

		}

		public Matrix World = Matrix.Identity;

		public Vector3 Position
		{
			get
			{
				return World.TranslationVector;
			}
			set
			{
				World.TranslationVector = value;

				//Util.DebugWrite(World.TransByMat(Vector3.UnitX).ToString());
				foreach ( Face i in Faces )
				{

					i.Update( World );
					//Util.DebugWrite(i.TriString);
					//return;
				}
				// todo 今の所頂点座標を変更する必要がないのでいじってはいない
				//for (int i = 0; i < Vertice.Length; i++)
				//{
				//  var item = Vertice[i];

				//  item.Position = World.TransByMat(item.Position).ToV3();
				//}

			}
		}

		public Quaternion Rotation
		{
			get
			{
				World.Decompose( out Vector3 scale , out Quaternion rot , out Vector3 trans );
				return rot;
			}
			set
			{
				// rotationをなくさないと相対回転になる
				World = World * Matrix.RotationQuaternion( value );
			}
		}

		public float Alpha
		{
			get
			{
				return GpuData.Alpha;
			}
			set
			{
				GpuData.Alpha = value;
			}
		}

		public void Update( SharpDevice device , Matrix View , Matrix Projection )
		{
			//apply shader
			Shader.Apply( );

			//apply constant buffer to shader
			VertexShader.SetConstantBuffer( 0 , GPUDataBuffer );
			PixelShader.SetConstantBuffer( 0 , GPUDataBuffer );

			GpuData.View = View;
			GpuData.WorldViewProjection = World * View * Projection;
			GpuData.World = World;
			device.UpdateData( GPUDataBuffer , GpuData );
			Mesh.Begin( );
			for ( int i = 0 ; i < Mesh.SubSets.Count ; i++ )
			{
				SharpSubSet sharpSubSet = Mesh.SubSets[ i ];
				device.DeviceContext.PixelShader.SetShaderResource( 0 , sharpSubSet.DiffuseMap );
				device.DeviceContext.PixelShader.SetShaderResource( 1 , sharpSubSet.SphereMap );
				//set texture
				Mesh.Draw( i );
			}
			// https://gamedev.stackexchange.com/questions/49779/different-shaders-for-different-objects-directx-11
		}

		public void OnFactorChanged( float factor )
		{
			Cast.Fac = factor;
		}

		public void OnRadiusChanged( float radius )
		{
			Cast.Radius = radius;
		}

		public string ModelStr
		{
			get; private set;

		}
		public static IEnumerable<TexturedVertex> ParseCSV( IEnumerable<string> lines )
		{
			//; Vertex,頂点Index,位置_x,位置_y,位置_z,法線_x,法線_y,法線_z,エッジ倍率,UV_u,UV_v,追加UV1_x,追加UV1_y,追加UV1_z,追加UV1_w,追加UV2_x,追加UV2_y,追加UV2_z,追加UV2_w,追加UV3_x,追加UV3_y,追加UV3_z,追加UV3_w,追加UV4_x,追加UV4_y,追加UV4_z,追加UV4_w,ウェイト変形タイプ(0:BDEF1 / 1:BDEF2 / 2:BDEF4 / 3:SDEF / 4:QDEF),ウェイト1_ボーン名,ウェイト1_ウェイト値,ウェイト2_ボーン名,ウェイト2_ウェイト値,ウェイト3_ボーン名,ウェイト3_ウェイト値,ウェイト4_ボーン名,ウェイト4_ウェイト値,C_x,C_y,C_z,R0_x,R0_y,R0_z,R1_x,R1_y,R1_z
			//   Vertex,0,0.3916405,16.48059,-0.7562667,0.383015,0.4676141,-0.7966408,1,0.8393391,0.7603291,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"上半身2",1,"",0,"",0,"",0,0,0,0,0,0,0,0,0,0

			foreach ( string item in lines )
			{
				var csv = item.Split( ',' );
				yield return new Vert(
					new Vector3( csv[ 2 ].Float( ) , csv[ 3 ].Float( ) , csv[ 4 ].Float( ) ) ,
					new Vector3( csv[ 5 ].Float( ) , csv[ 6 ].Float( ) , csv[ 7 ].Float( ) ) ,
					new Vector2( csv[ 9 ].Float( ) , csv[ 10 ].Float( ) ) );
			}
		}

		public void ToSphere( Vector3 pos , bool add = false )
		{
			Cast.Offset = pos;

			// ミラーの場合、元のモデルに対して変更を行うと上書きになってしまう
			// 順序が生まれてしまうが、変更後に対して作用させる
			var originalVertPos = add ?
				Vertice    .Select( v => v.Position ).ToArray( ) :
				OrigVertice.Select( v => v.Position ).ToArray( );

			Vector3[] castedVertice = Cast.GetSpereUntilEnd( originalVertPos );
			for ( int i = 0 ; i < castedVertice.Length ; i++ )
			{
				Vertice[ i ].Position = castedVertice[ i ];
			}
			Mesh.SetOnly( Vertice , Index.ToArray( ) );
		}

		public IEnumerable<MMDataIO.Pmx.PmxMorphVertexData> DifferVert()
		{
			for ( int i = 0 ; i < Vertice.Length ; i++ )
			{
				int ind = i;
				Vector3 origPos = OrigVertice[ ind ].Position;
				Vector3  curPos = Vertice[ ind ].Position;
				if ( !origPos.Equals( curPos ) )
				{
					yield return new MMDataIO.Pmx.PmxMorphVertexData( )
						{ Index = ind , Position = curPos - origPos };
				}
			}
		}

		public IEnumerable<HitResult> HitPos( RayWrap ray )
		{
			foreach ( Face item in Faces )
			{
				HitResult res = ray.IntersectFace( item );
				if ( res.IsHit )
				{
					yield return res;
				}
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		protected virtual void Dispose( bool disposing )
		{
			if ( !disposedValue )
			{
				if ( disposing )
				{
					Mesh.Dispose( );
					VertexShader.Dispose( );
					PixelShader.Dispose( );
					GPUDataBuffer.Dispose( );
					Shader.Dispose( );
				}

				// TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
				// TODO: 大きなフィールドを null に設定します。

				disposedValue = true;
			}
		}

		// TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
		// ~MMDModel() {
		//   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
		//   Dispose(false);
		// }

		// このコードは、破棄可能なパターンを正しく実装できるように追加されました。
		public void Dispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose( true );
			// TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
			// GC.SuppressFinalize(this);
		}
		#endregion
	}

}
