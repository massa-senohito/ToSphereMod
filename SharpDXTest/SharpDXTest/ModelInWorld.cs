using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpHelper;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{
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

	public class ModelInWorld
	{
		public Matrix World = Matrix.Identity;
		public Face[] Faces;

		public SharpMesh Mesh
		{
			get;
			protected set;
		}

		protected SharpShader Shader;
		protected VertexShaderStage VertexShader;
		protected PixelShaderStage PixelShader;

		protected Buffer GPUDataBuffer;
		protected GPUData GpuData;

		public static ModelInWorld Create( SharpMesh mesh , Matrix world , string shaderPath)
		{
			var temp = new ModelInWorld( )
			{
				Mesh = mesh ,
				World = world ,
			};
			temp.PrepareShader( mesh.Device , shaderPath );
			return temp;
		}

		public void SetFaceFromSharpMesh( string matName )
		{
			List<Face> faces = new List<Face>( );
			for ( int i = 0 ; i < Mesh.Indices.Length ; i += 3 )
			{
				int ind1 = Mesh.Indices[ i ];
				int ind2 = Mesh.Indices[ i + 1 ];
				int ind3 = Mesh.Indices[ i + 2 ];
				Face item1 = new Face( Mesh.Vertice[ ind1 ] , Mesh.Vertice[ ind2 ] , Mesh.Vertice[ ind3 ] , matName );
				//Debug.WriteLine("first " + item1.TriString);
				faces.Add( item1 );

			}
			Faces = faces.ToArray( );
			UpdateFace( );
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

		public IEnumerable<string> AllTriString
		{
			get
			{
				foreach ( var item in Faces )
				{
					yield return item.TriString;
				}
			}
		}

		protected void PrepareShader( SharpDevice device , string shaderPath)
		{
			//init shader
			Shader = new SharpShader( device , shaderPath ,
				new SharpShaderDescription( ) { VertexShaderFunction = "VS" , PixelShaderFunction = "PS" } ,
				new InputElement[] {
						new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
						new InputElement("NORMAL"  , 0, Format.R32G32B32_Float, 12, 0),
						new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
				} );
			VertexShader = device.DeviceContext.VertexShader;
			PixelShader = device.DeviceContext.PixelShader;

			GPUDataBuffer = Shader.CreateBuffer<GPUData>( );
		}

		protected void SendGPUData( SharpDevice device , Matrix View , Matrix Projection )
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
		}

		public void DrawAllSubSet( Matrix View , Matrix Projection )
		{
			Mesh.Begin( );
			SendGPUData( Mesh.Device , View , Projection );
			Mesh.Draw( );
		}

		protected bool IsDirty = false;

		void OnDirty()
		{
			UpdateFace( );
			// todo 今の所頂点座標を変更する必要がないのでいじってはいない
			//for (int i = 0; i < Vertice.Length; i++)
			//{
			//  var item = Vertice[i];

			//  item.Position = World.TransByMat(item.Position).ToV3();
			//}
		}

		internal void UpdateFace()
		{
			foreach ( Face i in Faces )
			{

				i.Update( World );
				//Util.DebugWrite(i.TriString);
			}
		}

		public void TransUpdate()
		{
			if ( IsDirty )
			{
				OnDirty( );
			}
			IsDirty = false;
		}

		public Vector3 Position
		{
			get
			{
				return World.TranslationVector;
			}
			set
			{
				World.TranslationVector = value;
				IsDirty = true;
				//Util.DebugWrite(World.TransByMat(Vector3.UnitX).ToString());


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
				var pos = Position;
				World = Matrix.RotationQuaternion( value );
				Position = pos;
				IsDirty = true;
			}
		}

		public Vector3 Scale
		{
			get
			{
				return World.ScaleVector;
			}
			set
			{
				World.ScaleVector = value;
				IsDirty = true;
			}
		}

	}
}
