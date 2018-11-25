using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpDXTest
{
	public static class ReactiveHelper
	{
		public static IObservable<Unit> CreateEve(Action<EventHandler> addHandler , Action<EventHandler> removeHandler )
		{
			var eve2 = Observable.FromEvent<EventHandler , EventArgs>(
				h => ( s , e ) => h( e ) ,
				addHandler ,
				removeHandler )
				.ToUnit( );
			return eve2;
		}
		public static IObservable<Unit> TextBoxChanged( TextBox textBox )
		{
			return Observable.FromEvent<EventHandler , EventArgs>(
				h => ( s , e ) => h( e ) ,
				h => textBox.TextChanged += h ,
				h => textBox.TextChanged -= h )
				.ToUnit( );
		}
		public static IObservable<Unit> BarChanged( TrackBar trackBar )
		{
			return Observable.FromEvent<EventHandler , EventArgs>(
				h => ( s , e ) => h( e ) ,
				h => trackBar.ValueChanged += h ,
				h => trackBar.ValueChanged -= h )
				.ToUnit( );
		}
	}
}
