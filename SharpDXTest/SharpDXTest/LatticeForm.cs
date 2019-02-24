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
using Platform;

namespace SharpDXTest
{
	public partial class LatticeForm : Form
	{
		List<ModelInWorld> LatticePoint = new List<ModelInWorld>();
		DraggableAxis LatticePointControl;
		Option<int> HandlingIndex = Option.Return<int>();
		SharpDevice Device;

		public LatticeForm(MMDModel model, SharpDevice device )
		{
			InitializeComponent( );

			Lattice = new LatticeDef( model , new int[] { 4,4,4} );

			//foreach ( var item in Lattice.LatticeData )
			for ( int i = 0 ; i < Lattice.LatticeData.Length ; i++ )
			{
				var item = Lattice.LatticeData[ i ];
				var pos = item.Value.Position;
				SharpMesh mesh = SharpMesh.CreateQuad( device , 0.1f , true );
				ModelInWorld rectModel = ModelInWorld.Create( mesh , pos.TransMat( ) , "../../HLSL.txt" );
				LatticePoint.Add( rectModel );
				rectModel.SetFaceFromSharpMesh( "lattice" + i );
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

			LatticePointControl = new DraggableAxis( "axis/axisold.csv" );
		}

		internal void LoadTexture( SharpDevice device )
		{
			Device = device;
			LatticePointControl.LoadTexture( device );
			LatticePointControl.Scale = new Vector3( 0.2f );
		}


		public void FixedUpdate( Matrix View , Matrix Projection )
		{
			Lattice.FixedUpdate( );
			LatticePointControl.Update( Device , View , Projection );
			for ( int i = 0 ; i < LatticePoint.Count ; i++ )
			{
				var point = LatticePoint[ i ];
				point.Position = Lattice.LatticeData[ i ].Value.Position;
				point.DrawAllSubSet( View , Projection );
			}
		}

		public IEnumerable<string> AllLatticeString
		{
			get
			{
				foreach ( var item in LatticePoint)
				{
					foreach ( var str in item.AllTriString)
					{
						yield return str;
					}
				}
			}
		}

		public void OnClicked( Mouse mouse , RayWrap ray )
		{
			var camera = Program.Camera;
			if ( mouse.RightClicked )
			{
				var hitPoss = HitPos( ray );
				var nearestHitted = hitPoss.MinValue( pos =>( pos.HitPosition - camera.Position).Length() );
				//hitted.ToString( ).DebugWrite( );
				// ドラッグしてるとき別のにフォーカス取られるのを避ける
				if ( nearestHitted.HasValue && !LatticePointControl.IsDragging)
				{
					HitResult value = nearestHitted.Value;
					LatticePointControl.Position = value.HitPosition;
					// Lattice の文字分消す
					var index = value.Info.Remove( 0 , 7 ).Int();
					HandlingIndex = new Some<int>(index);
					//Lattice.LatticeData[index].Value
				}
			}

			if ( HandlingIndex.HasValue )
			{
				LatticePointControl.OnClicked( mouse , ray );
				Lattice.LatticeData[ HandlingIndex.Value ].Value = new TexturedVertex( LatticePointControl.Position);
			}
		}

		IEnumerable<HitResult> HitPos( RayWrap ray )
		{
			foreach ( var point in LatticePoint )
			{
				foreach ( var hitResult in point.HitPos( ray ) )
				{
					yield return hitResult;
				}
			}
		}

		LatticeDef Lattice;

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		protected virtual void ResourceDispose( bool disposing )
		{
			if ( !disposedValue )
			{
				if ( disposing )
				{
					foreach ( var item in LatticePoint )
					{
						item.Dispose( );
					}
					LatticePointControl.Dispose( );
				}

				// TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
				// TODO: 大きなフィールドを null に設定します。

				disposedValue = true;
			}
		}

		// TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
		// ~MMDModel() {
		//   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
		//   Dispose(false);
		// }

		// このコードは、破棄可能なパターンを正しく実装できるように追加されました。
		public void ResourceDispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			ResourceDispose( true );
			// TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
