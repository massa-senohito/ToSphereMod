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
		public LatticeForm(MMDModel model)
		{
			InitializeComponent( );
			Lattice = new LatticeDef( model );
			Lat.BindTo( Lat0 , x => x.Text , BindingMode.TwoWay ,
				targetUpdateTrigger: ReactiveHelper.CreateEve( h=>Lat0.TextChanged += h , h=>Lat0.TextChanged -= h) );
			//Lat.BindTo( Lattice , x=>x.LatticeData[0] , BindingMode.TwoWay , targetUpdateTrigger:)
		}
		public void Update()
		{
			Lattice.FixedUpdate( );
		}
		public ReactiveProperty<TexturedVertex> Lat { get; } = new ReactiveProperty<TexturedVertex>();
		LatticeDef Lattice;
	}
}
