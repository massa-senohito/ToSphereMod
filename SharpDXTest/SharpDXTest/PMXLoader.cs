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
    public class VertexMorph
    {
        public string MorphName
        {
            get;
            private set;
        }

        public string MorphNameE
        {
            get;
            private set;
        }

        public MorphSlotType MorphSlot
        {
            get;
            private set;
        }

        public List<PmxMorphVertexData> VertexMorphs
        {
            get;
            private set;
        }

        public VertexMorph( PmxMorphData morph )
        {
            MorphName = morph.MorphName;
            MorphNameE = morph.MorphNameE;
            MorphSlot = morph.SlotType;
            VertexMorphs = morph.MorphArray.Cast<PmxMorphVertexData>( ).ToList();
        }

        public static List<VertexMorph> LoadVertMorphs( PmxModelData modelData )
        {
            return modelData.MorphArray.Where( m => m.MorphType == MorphType.VERTEX )
                .Select( m => new VertexMorph( m ) ).ToList();
        }

        public override string ToString()
        {
            return MorphName;
        }
    }

    public class PMXLoader
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
            return new TexturedVertex( vert.Pos , vert.Normal , vert.Uv );
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
            string sphereName = string.Empty;

            // テクスチャがない場合255が使われる
            if ( data.TextureId != 255 )
            {
                texName = model.TextureFiles[ data.TextureId ];
            }
            if ( data.SphereId != 255 )
            {
                sphereName = model.TextureFiles[ data.SphereId ];
            }
            Material temp = 
                new Material( data.MaterialName , texName , sphereName , flattenFace , faceIndex ,data.Mode);
            return temp;
        }

        static IEnumerable<Material> GetMat( PmxModelData model )
        {
            for ( int i = 0 ; i < model.MaterialArray.Length ; i++ )
            {
                yield return GetMat( model , i );
            }
        }

        public PmxModelData PmxModelData;
        public MMDModel MMDModel;
        public string PmxModelPath;

        public string FolderPath
        {
            get
            {
                return Directory.GetParent( PmxModelPath ).FullName;
            }
        }

        public PMXLoader( string path )
        {
            using ( FileStream sr = new FileStream( path , FileMode.Open , FileAccess.Read ) )
            {
                using ( BinaryReader bin = new BinaryReader( sr ) )
                {
                    PmxModelData = new PmxModelData( bin );
                    PmxModelPath = path;
                    //PmxModelData.Write( path + "ex.pmx" );
                    var vert = PmxModelData.VertexArray.Select( GetVert ).ToArray();
                    IEnumerable<Material> mats = GetMat( PmxModelData );
                    string parent = Directory.GetParent( path ).FullName;
                    MMDModel = new MMDModel( vert , mats , parent );
                    MMDModel.SetMorph( VertexMorph.LoadVertMorphs( PmxModelData ) );
                }
            }
        }

        public bool HasSameNameMorph( string name )
        {
            return PmxModelData.HasSameNameMorph( name );
        }

        public void WriteUpdated()
        {
            PmxModelData.Write( PmxModelPath + "ex.pmx" );
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

