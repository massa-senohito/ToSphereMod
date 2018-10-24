using MMDataIO.Pmx;
using SharpHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{
	class PMXLoader
	{
		static IEnumerable<int> IndexRange( PmxModelData model )
		{
			List<int> faces = model.MaterialArray.Select( x => x.FaceCount ).ToList( );
			int sum = 0;
			for ( int i = 0 ; i < faces.Count( ) ; i++ )
			{
				sum += faces[ i ];
				yield return sum;
			}
		}

		static IEnumerable<string> IndiceString( PmxModelData model )
		{
			int FaceCount = 0;
			int matIndex = 0;
			List<int> ranges = IndexRange( model ).ToList( );
			for ( int i = 0 ; i < model.VertexIndices.Length ; i += 3 )
			{
				string inds = $"Face,";

				// countは0含まないので
				if ( i + 3 > ranges[ matIndex ] )
				{
					matIndex++;
					FaceCount = 0;
				}
				inds += "\"" + model.MaterialArray[ matIndex ].MaterialName + "\"," + FaceCount + ",";
				inds += model.VertexIndices[ i ] + ",";
				inds += model.VertexIndices[ i + 1 ] + ",";
				inds += model.VertexIndices[ i + 2 ];
				yield return inds;

				FaceCount++;
			}
		}

		static IEnumerable<int[]> IndiceByMaterial( PmxModelData model , int ind)
		{
			int FaceCount = 0;
			int matIndex = 0;
			List<int> ranges = IndexRange( model ).ToList( );
			int start = 0;

			if ( ind > 0 )
			{
				start = ranges[ ind - 1 ];
			}

			for ( int i = start ; i < ranges[ind] ; i += 3 )
			{

				if ( i + 3 > ranges[ matIndex ] )
				{
					matIndex++;
					FaceCount = 0;
				}

				yield return
					new int[] {
					model.VertexIndices[ i ] ,
					model.VertexIndices[ i + 1 ] ,
					model.VertexIndices[ i + 2 ] };

				FaceCount++;
			}
		}

		static TexturedVertex GetVert( PmxVertexData vert )
		{
			return new TexturedVertex( vert.Pos , vert.Uv );
		}

		static TexturedVertex GetVert( PmxModelData model , int vertId )
		{
			return GetVert(model.VertexArray[ vertId ]);
		}

		static Material GetMat(  PmxModelData model , int matInd)
		{
			PmxMaterialData data = model.MaterialArray[matInd];
			var faceIndex = IndiceByMaterial( model , matInd);
			var flattenFace = faceIndex.SelectMany( x => x ).Select( x => GetVert( model , x ) );
			string texName = string.Empty;

			// テクスチャがない場合255が使われる
			if ( data.TextureId != 255 )
			{
				texName = model.TextureFiles[ data.TextureId ];
			}
			Material temp = new Material( data.MaterialName , texName , flattenFace , faceIndex );
			return temp;
		}

		static IEnumerable<Material> GetMat( PmxModelData model )
		{
			for ( int i = 0 ; i < model.MaterialArray.Length ; i++ )
			{
				yield return GetMat( model , i );
			}
		}

		public static MMDModel Load( string path )
		{
			using ( FileStream sr = new FileStream( path , FileMode.Open , FileAccess.Read ) )
			{
				using ( BinaryReader bin = new BinaryReader( sr ) )
				{
					PmxModelData model = new PmxModelData( bin );
					var vert = model.VertexArray.Select( GetVert ).ToArray();
					IEnumerable<Material> mats = GetMat( model );
					string parent = Directory.GetParent( path ).FullName;
					return new MMDModel( vert , mats , parent );
				}
			}
		}

		public static void WriteTestCSV( string arg )
		{
			// todo softbodyなどが無視されている
			using ( FileStream sr = new FileStream( arg , FileMode.Open , FileAccess.Read ) )
			{
				using ( BinaryReader bin = new BinaryReader( sr ) )
				{
					PmxModelData model = new PmxModelData( bin );
					var vert = model.VertexArray.Select( x => x.Pos.ToString( ) );
					var mats = model.MaterialArray.Select( x => "mat" + x.FaceCount.ToString( ) );

					var cont = vert.Concat( mats ).Concat( IndiceString( model ) );
					File.WriteAllLines( "MMDModelTest.txt" , cont );
				}
			}
		}
	}
}

