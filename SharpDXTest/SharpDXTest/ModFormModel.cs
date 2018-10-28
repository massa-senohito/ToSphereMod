using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpDXTest
{
	public class ModFormModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string _MorphName;

		public string MorphName
		{
			get
			{
				return _MorphName;
			}
			set
			{
				_MorphName = value;
				PropertyChanged( _MorphName , new PropertyChangedEventArgs( nameof( _MorphName ) ) );
			}
		}
	}
}
