using BlenderModifier;
using Reactive.Bindings;
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

    public class MMDModel : ModelInWorld 
    {
        public Vert[] Vertice;

        public Vert[] OrigVertice
        {
            get;
            private set;
        }

        public int[] Index;

        public List<Material> Materials;

        ModelMorph MMDModelMorph = new ModelMorph();
        public void BindMorphProp( List<ReactiveProperty<int>> reactives )
        {
            MMDModelMorph.Bind( reactives );
        }

        public List<VertexMorph> Morphs
        {
            get
            {
                return MMDModelMorph.Morphs;
            }
        }

        internal void SetMorph( List<VertexMorph> list )
        {
            MMDModelMorph.SetMorph( list );
        }
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
            Materials = Material.MakeFromCSV( gs[ "Material" ] , faceGr , Vertice ).ToList();

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
            Materials = material.ToList();
            GpuData = new GPUData( );
            GpuData.Alpha = 1;
            Cast = new SphereCast( Matrix.Zero );
        }

        Buffer MaterialDataBuffer;


        public void LoadTexture( SharpDevice device )
        {

            Mesh = GetSharpMesh( device );

            PrepareShader( device , "../../HLSLModel.txt" );

            MaterialDataBuffer = Shader.CreateBuffer<MaterialGPUData>( );
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
            TransUpdate( );

            SendGPUData( device , View , Projection );

            MMDModelMorph.UpdateMorph( OrigVertice , Vertice );

            UpdateMesh( );

            Mesh.Begin( );
            for ( int i = 0 ; i < Mesh.SubSets.Count ; i++ )
            {
                SharpSubSet sharpSubSet = Mesh.SubSets[ i ];
                PixelShaderStage pixelShader = device.DeviceContext.PixelShader;
                MaterialGPUData materialData = Materials[ i ].MaterialData;
                device.DeviceContext.PixelShader.SetShaderResource( 0 , sharpSubSet.DiffuseMap );
                device.DeviceContext.PixelShader.SetShaderResource( 1 , sharpSubSet.SphereMap );
                pixelShader.SetConstantBuffer( 1 , MaterialDataBuffer );
                device.UpdateData( MaterialDataBuffer , materialData );
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

        public void CastScale( Vector3 scale )
        {
            Cast.Scale = scale;
        }

        public void ToSphere( Matrix pivot , bool add = false )
        {
            pivot.Decompose( out Vector3 scale , out Quaternion rot , out Vector3 trans );
            Cast.Offset = trans;
            Cast.Rot = rot.RotMat();
            // ミラーの場合、元のモデルに対して変更を行うと上書きになってしまう
            // 順序が生まれてしまうが、変更後に対して作用させる
            // 指定材質以外影響を受けないようにしたい
            List<SharpSubSet> subSets = Mesh.SubSets;
#if false
            for ( int subInd = 0 ; subInd < subSets.Count; subInd++ )
            {
                //int subInd = 14;
                int startInd = subSets[ subInd ].StartIndex / 3;
                int count = subSets[ subInd ].IndexCount;

                // それぞれ面のインデックス
                var indice = Index.Range( startInd , count ).Distinct( );

                var originalVertPos = add ?
                    Vertice    .TakeIndice(indice).Select( v => v.Position ).ToArray( ) :
                    OrigVertice.TakeIndice(indice).Select( v => v.Position ).ToArray( );

                Vector3[] castedVertice = Cast.GetSphereUntilEnd( originalVertPos );
                var indiceA = indice.ToArray( );
                for ( int i = 0 ; i < indiceA.Length ; i++ )
                //foreach ( var i in indice )
                {

                    Vertice[ indiceA[i] ].Position = castedVertice[ i ];
                }
            }
#else
            var originalVertPos = add ?
                Vertice    .Select( v => v.Position ).ToArray( ) :
                OrigVertice.Select( v => v.Position ).ToArray( );

            Vector3[] castedVertice = Cast.GetSphereUntilEnd( originalVertPos );
            for ( int i = 0 ; i < castedVertice.Length ; i++ )
            {
                Vertice[ i ].Position = castedVertice[ i ];
            }
#endif
            UpdateMesh( );
        }

        public void UpdateMesh()
        {
            Mesh?.SetOnly( Vertice , Index.ToArray( ) );
        }

        public void ToSphere( Vector3 pos , bool add = false )
        {
            ToSphere( Matrix.Translation( pos ) , add);
        }

        // 差分頂点を抜き出し頂点モーフにする
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

    }

}
