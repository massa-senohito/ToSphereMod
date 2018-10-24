using SharpDX;
using SharpHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{
	class DebugLine : MMDModel
	{
		List<int> Depth = new List<int>( );
		List<int> Near = new List<int>( );
		List<int> Upper = new List<int>( );
		List<int> Lower = new List<int>( );
		List<int> Left = new List<int>( );
		List<int> Right = new List<int>( );
		int NearUpper;
		int DepthUpper;

		int NearLower;
		int DepthLower;

		int NearLeft;
		int DepthLeft;

		int NearRight;
		int DepthRight;
		//0 , 0.8838835,-25,0,0.8164951,-0.5773492,1
		//0 , 0.8838835,0,0,0.8164951,0.5773492,1,0,
		//-0.8838835,0,0,-0.8164951,0,0.5773492,1,
		//0.8838835,0,0,0.8164951,0,0.5773492,1,0,
		//0 , -0.8838835,0,0,-0.8164951,0.5773492,1,
		//0.8838835,0,-25,0.8164951,0,-0.5773492,1
		//0 , -0.8838835,-25,0,-0.8164951,-0.5773492
		//-0.8838835,0,-25,-0.8164951,0,-0.5773492

		public DebugLine( string path ) : base( path )
		{
		}

		void Ind()
		{
			for ( int i = 0 ; i < Vertice.Length ; i++ )
			{
				bool isDepth = false;
				if ( Vertice[ i ].Position.Z < -20 )
				{
					Depth.Add( i );
					isDepth = true;
				}
				else
				{
					Near.Add( i );
				}
				if ( 0.5f < Vertice[ i ].Position.X && Vertice[ i ].Position.X < 1.0f )
				{
					Right.Add( i );
					if ( isDepth )
					{
						DepthRight = i;
					}
					else
					{
						NearRight = i;
					}
				}
				if ( -1.0f < Vertice[ i ].Position.X && Vertice[ i ].Position.X < -0.5f )
				{
					Left.Add( i );
					if ( isDepth )
					{
						DepthLeft = i;
					}
					else
					{
						NearLeft = i;
					}
				}
				if ( 0.5f < Vertice[ i ].Position.Y && Vertice[ i ].Position.Y < 1.0f )
				{
					Upper.Add( i );
					if ( isDepth )
					{
						DepthUpper = i;
					}
					else
					{
						NearUpper = i;
					}
				}
				if ( -1.0f < Vertice[ i ].Position.Y && Vertice[ i ].Position.Y < -0.5f )
				{
					Lower.Add( i );
					if ( isDepth )
					{
						DepthLower = i;
					}
					else
					{
						NearLower = i;
					}
				}
			}


		}

		public void AfterLoaded()
		{
			Ind( );
		}

		public void SetLine( Vector3 from , Vector3 to )
		{
			float leng = 1.0f;
			Vertice[ DepthUpper ].Position = to + Vector3.UnitY * leng;
			Vertice[ DepthLower ].Position = to - Vector3.UnitY * leng;
			Vertice[ DepthRight ].Position = to + Vector3.UnitX * leng;
			Vertice[ DepthLeft ].Position = to - Vector3.UnitX * leng;

			Vertice[ NearUpper ].Position = from + Vector3.UnitY * leng;
			Vertice[ NearLower ].Position = from - Vector3.UnitY * leng;
			Vertice[ NearRight ].Position = from + Vector3.UnitX * leng;
			Vertice[ NearLeft ].Position = from - Vector3.UnitX * leng;
			Mesh.SetOnly( Vertice , Index.ToArray( ) );
		}

		public void OnClicked( Mouse mouse , RayWrap ray )
		{
			for ( int i = 0 ; i < Depth.Count ; i++ )
			{
				Vertice[ Depth[ i ] ].Position = ray.To;
			}
			foreach ( var i in Near )
			{
				//i.SetPosition(ray.From);
			}

			Mesh.SetOnly( Vertice , Index.ToArray( ) );
		}
	}
}
