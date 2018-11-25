using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpDXTest
{
	public partial class VertMorphViewer : Form
	{
		List<BarControl> BarControls = new List<BarControl>( );

		public List<ReactiveProperty<int>> BarValues
		{
			get
			{
				return BarControls.Select( b => b.Coef ).ToList( );
			}
		}

		public VertMorphViewer( List<VertexMorph> morphs)
		{
			InitializeComponent( );
			Point point = new Point( 10 , 40 );
			foreach ( var morph in morphs )
			{
				BarControl item = new BarControl( morph.MorphName );
				item.Location = point;
				point.Y += item.Height + 3;
				BarControls.Add( item );
				Controls.Add( item );
			}
		}
	}
}
