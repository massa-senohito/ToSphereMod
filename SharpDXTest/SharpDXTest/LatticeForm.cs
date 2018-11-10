using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reactive;
using Reactive.Bindings;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using SharpDX;
using SharpHelper;
using SharpDXTest;

namespace SharpDXTest
{
	public partial class LatticeForm : Form
	{
		List<ModelInWorld> LatticePoint = new List<ModelInWorld>();

		public LatticeForm(MMDModel model, SharpDevice device)
		{
			InitializeComponent( );

			Lattice = new LatticeDef( model );

			foreach ( var item in Lattice.LatticeData )
			{
				var pos = item.Value.Position;
				LatticePoint.Add( ModelInWorld.Create( SharpMesh.CreateQuad( device , 0.1f , true) , pos.TransMat( ) , "../../HLSL.txt") );
			}

			var lats = new[]
			{
				Lat0,
				Lat1,
				Lat2,
				Lat3,
				Lat4,
				Lat5,
				Lat6,
				Lat7,
				Lat8,
			};

			for ( int i = 0 ; i < lats.Length ; i++ )
			{
#if true
				Lattice.LatticeString[ i ].BindTo( lats[ i ] , x=>x.Text , BindingMode.TwoWay ,
					targetUpdateTrigger: ReactiveHelper.TextBoxChanged( lats[ i ] ) );
#endif
			}
			//Lat.BindTo( Lat0 , x => x.Text , BindingMode.TwoWay ,
			//	targetUpdateTrigger: ReactiveHelper.CreateEve( h=>Lat0.TextChanged += h , h=>Lat0.TextChanged -= h) );
			//Lat.BindTo( Lattice , x=>x.LatticeData[0] , BindingMode.TwoWay , targetUpdateTrigger:)
		}

		public void FixedUpdate( Matrix View , Matrix Projection )
		{
			Lattice.FixedUpdate( );

			//foreach ( var point in LatticePoint )
			for ( int i = 0 ; i < LatticePoint.Count ; i++ )
			{
				var point = LatticePoint[ i ];

				point.Position = Lattice.LatticeData[ i ].Value.Position;
				point.DrawAllSubSet( View , Projection );
			}
			//Lattice.LatticeData[ 1 ].Value = new TexturedVertex( Vector3.One , Vector3.One , Vector2.One );
		}

		LatticeDef Lattice;
	}
}
