using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Interpolation
{
	//無限に補完する、小さいところ0-1ではゆったりしているが
	public static float Fade( float a )
	{
		return a * a * a * ( a * ( a * 6 - 15 ) + 10 );
	}

	public static float Circle( float a )
	{
		if ( a <= 0.5f )
		{
			a *= 2;
			return ( float )( 1 - Math.Sqrt( 1 - a * a ) ) / 2.0f;
		}
		a--;
		a *= 2;
		return ( float )( Math.Sqrt( 1 - a * a ) + 1 ) / 2.0f;
	}
	public class Elastic
	{
		float value, power, scale, bounces;
		public Elastic( float value , float power , int bounces , float scale )
		{
			this.value = value;
			this.power = power;
			this.scale = scale;
			this.bounces = bounces * ( float )Math.PI * ( bounces % 2 == 0 ? 1 : -1 );
		}
		public void SetValue( float v , float p , int b , float s )
		{
			value = v;
			power = p;
			scale = s;
			bounces = b;
		}
		public float Apply( float a )
		{
			if ( a <= 0.5f )
			{
				a *= 2;
				var temp = Math.Pow( value , power * ( a - 1 ) ) * Math.Sin( a * bounces ) * scale / 2;
				return ( float )temp;
			}
			a = 1 - a;
			a *= 2;
			var temp2 = 1 - Math.Pow( value , power * ( a - 1 ) ) * Math.Sin( a * bounces ) * scale / 2;
			return ( float )temp2;
		}
		public float InApply( float a )
		{
			if ( a >= 0.99 )
				return 1;
			var temp = Math.Pow( value , power * ( a - 1 ) ) * Math.Sin( a * bounces ) * scale;
			return ( float )temp;
		}
		public float OutApply( float a )
		{
			a = 1 - a;
			var temp = ( 1 - Math.Pow( value , power * ( a - 1 ) ) * Math.Sin( a * bounces ) * scale );
			return ( float )temp;

		}
	}
}
