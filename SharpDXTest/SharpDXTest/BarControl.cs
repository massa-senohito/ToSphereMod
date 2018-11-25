using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reactive.Bindings;
using Reactive.Bindings.Binding;

namespace SharpDXTest
{
	public partial class BarControl : UserControl
	{
		string MorphName;
		public ReactiveProperty< int > Coef = new ReactiveProperty< int >();

		public BarControl( string morphName)
		{
			InitializeComponent( );
			//Lattice.LatticeString[ i ].BindTo( lats[ i ] , x=>x.Text , BindingMode.TwoWay ,
			//	targetUpdateTrigger: ReactiveHelper.TextBoxChanged( lats[ i ] ) );

			Coef.BindTo( trackBar , x => x.Value , BindingMode.TwoWay , targetUpdateTrigger: ReactiveHelper.BarChanged( trackBar ) );
			MorphName = morphName;
			label.Text = MorphName;
		}

		public float CoefValue
		{
			get
			{
				return Coef.Value / 100.0f;
			}
		}

	}
}
