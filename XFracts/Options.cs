using System;
using Java.Nio;
using Android.Graphics;

namespace XFracts
{
	public static class Options
	{
		public static float X {
			get;
			set;
		}
		
		public static float Y {
			get;
			set;
		}
		
		public static float Scale {
			get;
			set;
		}

		static int palette;
		public static int Palette {
			get {
				return palette;
			}
			set {
				palette = value;
				OnPaletteChanged (EventArgs.Empty);
			}
		}
		public static event EventHandler PaletteChanged;

		static void OnPaletteChanged (EventArgs e)
		{
			var handler = PaletteChanged;
			if (handler != null)
				handler (null, e);
		}
		


		static Fractal fractal;
		public static Fractal Fractal {
			get {
				return fractal;
			}
			set {
				fractal = value;
				Scale = value.InitialScale;
				X = value.InitialX;
				Y = value.InitialY;
				OnFractalChanged (EventArgs.Empty);
			}
		}
		public static event EventHandler FractalChanged;

		static void OnFractalChanged (EventArgs e)
		{
			var handler = FractalChanged;
			if (handler != null)
				handler (null, e);
		}

		public static byte[] GeneratePalette ()
		{
			var pixels = new byte [PaletteSize * 4];

			int o = 0;
			switch (palette) {
			case 0:
				for (int x = 0; x < PaletteSize; x++) {
					int value = (x % 1020) / 2;
					int color =  value <= 255 ? value : 255 - (value - 255);
					if (x >= PaletteSize - 10) {
						pixels[o++] = 0;
						pixels[o++] = 0;
						pixels[o++] = 0;
					} else {
						pixels[o++] = (byte)(255 - color) ;
						pixels[o++] = (byte)(255 - color);
						pixels[o++] = (byte)(255 - color / 3) ;
					}
					pixels[o++] = 255;
				}
				break;
			case 1:
				for (int x = 0; x < PaletteSize; x++) {
					int value = (x % 1020) / 2;
					int color =  value <= 255 ? value : 255 - (value - 255);
					if (x >= PaletteSize - 10) {
						pixels[o++] = 0;
						pixels[o++] = 0;
						pixels[o++] = 0;
					} else {
						pixels[o++] = (byte)(255 - color / 3);
						pixels[o++] = (byte)(255 - color);
						pixels[o++] = (byte)(128 - color / 2);
					}
					pixels[o++] = 255;
				}
				break;
			case 2:
				for (int x = 0; x < PaletteSize; x++) {
					int value = (x % 1020) / 2;
					int color =  value <= 255 ? value : 255 - (value - 255);
					if (x >= PaletteSize - 10) {
						pixels[o++] = 0;
						pixels[o++] = 0;
						pixels[o++] = 0;
					} else {
						pixels[o++] = (byte)(color / 2);
						pixels[o++] = (byte)(color);
						pixels[o++] = (byte)(127 + color / 2);
					}
					pixels[o++] = 255;
				}
				break;
			case 3:
				for (int x = 0; x < PaletteSize; x++) {
					int value = (x % 1020) / 2;
					int color =  value <= 255 ? value : 255 - (value - 255);
					if (x >= PaletteSize - 10) {
						pixels[o++] = 0;
						pixels[o++] = 0;
						pixels[o++] = 0;
					} else {
						pixels[o++] = (byte)(color);
						pixels[o++] = (byte)(color / 2);
						pixels[o++] = (byte)0;
					}
					pixels[o++] = 255;
				}
				break;
			}
			return pixels;
		}

		static Options ()
		{
			Fractal = new Mandelbrot ();
		}

		public const int PaletteSize = 1021;
	}
}

