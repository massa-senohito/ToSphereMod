using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SharpDX;
using SharpHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{
	public class ModelMorph
	{
		public List<VertexMorph> Morphs
		{
			get;
			private set;
		}

		List< ReactiveProperty<float> > MorphCoef
		{
			get;
			set;
		}

		public void SetMorph( List<VertexMorph> morphs )
		{
			Morphs = morphs;
			//MorphCoef = Enumerable.Range( 0 , Morphs.Count ).Select( x => 0.0f ).ToList( );
		}

		public void Bind( List<ReactiveProperty<int>> reactives )
		{
			MorphCoef = reactives.Select( r => r.ToReactivePropertyAsSynchronized( v => v.Value , convert: i => i / 100.0f ,
				 convertBack: f => ( int )( f * 100.0f ) ).ToReactiveProperty( ) ).ToList( );
		}

		public void UpdateMorph( TexturedVertex[] orig , TexturedVertex[] verts)
		{
			//Vector3[] morphs = new Vector3[ verts.Length ];
			for ( int i = 0 ; i < Morphs?.Count ; i++ )
			{
				var currentMorph = Morphs[ i ];
				foreach ( var item in currentMorph.VertexMorphs )
				{
					//morphs
                    // todo ToSphereしないモデルはnullになる、ここでさせるより、モーフを呼ばない設定させるほうが良さそう
                    if(MorphCoef != null)
					verts[ item.Index ].Position += item.Position * MorphCoef[i].Value;
					//verts[ item.Index ].Position = orig[item.Index].Position + item.Position * MorphCoef[i].Value;
				}
			}
		}
	}
}
