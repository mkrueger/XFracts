
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;
using System.Threading.Tasks;

namespace XFracts
{
	public class FractalCPUView : View
	{
		public FractalCPUView (Context context) :
			base (context)
		{
		}

		BitmapDrawable CalculateBitmap () 
		{
			int[] colors = new int [Width * Height];
			Options.Fractal.Calculate (colors, Width, Height);
			var bitmap = Bitmap.CreateBitmap (colors, Width, Height, Bitmap.Config.Argb8888);

			return new BitmapDrawable (bitmap);
		}

		protected override void OnSizeChanged (int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged (w, h, oldw, oldh);
			draw = CalculateBitmap ();
		}

		BitmapDrawable draw;

		public override void Draw (Android.Graphics.Canvas canvas)
		{
			base.Draw (canvas);

			if (draw != null)
				draw.Draw (canvas);
		}
	}
}

