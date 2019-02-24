using SharpDXTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using V3 = SharpDX.Vector3;

namespace BlenderModifier
{
	public partial class SphereModForm : Form
	{
		public EventHandler OnUpdate;
		public ModFormModel Model = new ModFormModel();
		public bool HasError
		{
			get;
			private set;
		}

        PMXLoader Loader;

		public SphereModForm( PMXLoader loader)
		{
			InitializeComponent( );
			FactorBox.Text = "0.3";
			RadiusBox.Text = "2";
			FactorBar.ValueChanged += FactorBar_ValueChanged;
			FactorBar.Value = 30;
			RadiusBar.ValueChanged += RadiusBar_ValueChanged;
			RadiusBar.Value = 200;
			UIAlphaBar.Value = 100;
			const string PropertyName = nameof( MorphNameBox.Text );
			Binding binding = new Binding( PropertyName , Model , "MorphName" );
			MorphNameBox.DataBindings.Add(binding);
            Loader = loader;
			label4.Text = "";
		}

		private void RadiusBar_ValueChanged( object sender , EventArgs e )
		{
			Radius = RadiusBar.Value * 0.01f;
			;
		}

		private void FactorBar_ValueChanged( object sender , EventArgs e )
		{
			Factor = FactorBar.Value * 0.01f;
		}

		public void SetFactorBoxChanged( EventHandler f )
		{
			FactorBox.TextChanged += f;
		}

		public void SetRadiusBoxChanged( EventHandler f )
		{
			RadiusBox.TextChanged += f;
		}

		public void SetOffsetBoxChanged( EventHandler f )
		{
			OffsetBox.TextChanged += f;
		}

		public void SetAlphaBarChanged( EventHandler f )
		{
			UIAlphaBar.ValueChanged += f;
		}

		public void SetScaleChanged( EventHandler f )
		{
			XScale.TextChanged += f;
			YScale.TextChanged += f;
			ZScale.TextChanged += f;
		}

		public void SetRotChanged( EventHandler f )
		{
			XRot.TextChanged += f;
			YRot.TextChanged += f;
			ZRot.TextChanged += f;
		}

		// バインディング追加したけど思ったタイミングで発火しないのでイベント実装
		public void SetMorphNameChanged( EventHandler f )
		{
			MorphNameBox.TextChanged += f;
		}

		public void SetError( string message = "" )
		{
			label4.Text = message;
			if ( message == string.Empty )
			{
				HasError = false;
			}
			else
			{
				HasError = true;
			}
		}

		public string MorphName
		{
			get
			{
				return MorphNameBox.Text;
			}
            private set
            {
                MorphNameBox.Text = value;
            }
		}

		public void SetOffset( V3 v3 )
		{
			// モデル頂点位置が変更されるのでフォーカスを失って正常に文字入力できなくなる
			if ( !OffsetBox.Focused )
			{
				OffsetBox.Text = v3.Csv( );
			}
		}

		public Option< V3 > GetOffset()
		{
			string tmp = OffsetBox.Text;
			string[] arr = tmp.Split( ',' );
			try
			{

				var flArr = arr.Select( float.Parse ).ToArray( );
				return Option.Return( new V3( flArr[ 0 ] , flArr[ 1 ] , flArr[ 2 ] ) );
			}
			catch ( Exception e )
			{
				return Option.Return<V3>( );
			}

		}

		public V3 ToSphereScale
		{
			get
			{
				return new V3( XScale.Text.Float( ) , YScale.Text.Float( ) , ZScale.Text.Float( ) ); 
			}
			set
			{
				var x = value.X;
				var y = value.Y;
				var z = value.Z;
				XScale.Text = x.ToString();
				YScale.Text = y.ToString();
				ZScale.Text = z.ToString();
			}
		}

		public V3 EulerRotate
		{
			get
			{
				return new V3( XRot.Text.Float( ) , YRot.Text.Float( ) , ZRot.Text.Float( ) );
			}
			set
			{

				if ( OffsetBox.Focused )
				{
					return;
				}
				var x = value.X;
				var y = value.Y;
				var z = value.Z;
				XRot.Text = x.ToString( );
				YRot.Text = y.ToString( );
				ZRot.Text = z.ToString( );
			}
		}

		public int BoneID
		{
			get
			{
				bool success = int.TryParse( BoneBox.Text , out int result );
				if ( success )
				{
					return result;
				}
				else
				{
					return 0;
				}
			}
		}

		public float Factor
		{
			get
			{
				bool success = float.TryParse( FactorBox.Text , out float result );
				return result;
			}
			private set
			{
				FactorBox.Text = value.ToString( );
			}
		}

		public float Radius
		{
			get
			{
				bool success = float.TryParse( RadiusBox.Text , out float result );
				return result;
			}
			private set
			{
				RadiusBox.Text = value.ToString( );
			}
		}

		public float Alpha
		{
			get
			{
				int v = UIAlphaBar.Value;
				return v * 0.01f;
			}
		}

		private void updateToolStripMenuItem_Click( object sender , EventArgs e )
		{
			OnUpdate?.Invoke( sender , e );
		}

        public void Save()
        {
            //fact
            // rad
            // name
            // pos
            // rot
            // scale
            var contain = new List<string>( )
            {
                FactorBox.Text,
                RadiusBox.Text,
                MorphName,
                OffsetBox.Text,
                EulerRotate.Csv(),
                ToSphereScale.Csv(),
            };
            File.WriteAllLines( Path.Combine( Loader.FolderPath , MorphName + ".txt" ) , contain.ToArray( ) );
        }

        private void saveToolStripMenuItem_Click( object sender , EventArgs e )
        {
            Save( );
        }

        private void loadToolStripMenuItem_Click( object sender , EventArgs e )
        {
            OpenFileDialog dialog = new OpenFileDialog( );
            dialog.Filter = "txt|*.txt";
            if ( dialog.ShowDialog( ) == DialogResult.OK )
            {
                var lines = File.ReadAllLines( dialog.FileName );
                Factor = lines[ 0 ].Float();
                Radius = lines[ 1 ].Float( );
                MorphName = lines[ 2 ];
                SetOffset( lines[ 3 ].V3( ) );
                EulerRotate = lines[ 4 ].V3( );
                ToSphereScale = lines[ 5 ].V3( );
            }

        }
    }
}

