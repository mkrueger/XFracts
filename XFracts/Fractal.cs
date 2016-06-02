using System;

namespace XFracts
{
	public abstract class Fractal
	{
		public abstract float InitialScale {
			get;
		}
		
		public abstract float InitialX {
			get;
		}
		
		public abstract float InitialY {
			get;
		}
		
		public abstract string FragmentShader {
			get;
		}

		public abstract void Calculate (int[] colors, int width, int height);
	}

	public class Mandelbrot : Fractal
	{
		public override float InitialScale {
			get {
				return 2.2f;
			}
		}

		public override float InitialX {
			get {
				return 0.5f;
			}
		}
		
		public override float InitialY {
			get {
				return 0f;
			}
		}

		public override string FragmentShader {
			get {
				return @"
void main() {
	int n;
	float r0 = ratio * (v_texcoord.x - 0.5) * scale - center.x;
	float i0 = (v_texcoord.y - 0.5) * scale - center.y;

	float i = i0;
	float r = r0;

	for(n=0; n < 32; n++) {
		float nr = r * r - i * i + r0;
		float ni = 2.0 * r * i   + i0;

		if (nr * nr + ni * ni > 4.0)
			break;
		i = ni;
		r = nr;
	}
	gl_FragColor = texture2D(palette, vec2(float(n) / 20.0, 1.0));
}";
			}
		}

		double factor = 2.0;

		double GetDistance (int x, int y)
		{
			double gint_x = Math.Round (x * factor) / factor;
			double gint_y = Math.Round (y * factor) / factor;
			return Math.Sqrt ((x - gint_x) * (x - gint_x) + (y - gint_y) * (y - gint_y));
		}

		public override void Calculate (int[] colors, int width, int height)
		{
			double ratio = width / (double)height;
			0f
			int[] palette = new int[Options.PaletteSize + 1];
			var bytePalette = Options.GeneratePalette ();
			for (int i = 0; i < Options.PaletteSize; i++) {
				palette [i] = bytePalette [i * 4] | bytePalette [i * 4 + 1] << 8 | bytePalette [i * 4 + 2] << 16 | bytePalette [i * 4 + 3] << 24;
			}

			double d, d1;

			d = 0;
			d1 = 0;

			int o = 0;
			for (int y = 0; y < height; y++) {
				double i0 = (y / (double)height - 0.5) * Options.Scale - Options.Y;
				
				for (int x = 0; x < width; x++) {
					double r0 = ratio * (x / (double)width - 0.5) * Options.Scale - Options.X;
					
					double i = i0;
					double r = r0;
					double m = 0;

					int n;
					for (n = 0; n < 40; n++) {
						var xsq = r * r;
						var ysq = i * i;
						var nr = xsq - ysq + r0;
						var ni = 2 * r * i + i0;
						if (nr * nr + ni * ni > 4.0) {

							m = n + 2 - (Math.Log (Math.Log (Math.Sqrt (xsq + ysq)) / Math.Log (2)) / Math.Log (4));
							break;
						}

						d1 = d;
						d = Math.Min (d, GetDistance (x, y));

						r = nr;
						i = ni;
					}

					int color;

					color = (int)((m * d + (1 - m) * d1) * 10200);
					;

					colors [o++] = palette [color % Options.PaletteSize];
				}
			}
		}
	}
	
	public class JuliaSet : Fractal
	{
		public float RConstant { get; set; }

		public float IConstant { get; set; }
		
		public override float InitialScale {
			get {
				return 2f;
			}
		}

		public override float InitialX {
			get {
				return 0.0f;
			}
		}
		
		public override float InitialY {
			get {
				return 0f;
			}
		}

		public JuliaSet (float rConstant, float iConstant)
		{
			this.RConstant = rConstant;
			this.IConstant = iConstant;
		}

		static string AddConstant (float f)
		{
			if (f == 0.0)
				return "";
			string result;
			if (f > 0) {
				result = "+" + f.ToString (System.Globalization.CultureInfo.InvariantCulture);
			} else {
				result = f.ToString (System.Globalization.CultureInfo.InvariantCulture);
			}

			if (result.IndexOf ('.') < 0)
				return result + ".0";
			return result;
		}

		public override string FragmentShader {
			get {
				return @"
void main() {
	int n;
	float r0 = ratio * (v_texcoord.x - 0.5) * scale - center.x;
	float i0 = (v_texcoord.y - 0.5) * scale - center.y;

	float i = i0;
	float r = r0;

	for(n=0; n < 32; n++) {
		float nr = r * r - i * i" + AddConstant (RConstant) + @";
		float ni = 2.0 * r * i" + AddConstant (IConstant) + @";

		if (nr * nr + ni * ni > 4.0)
			break;
		i = ni;
		r = nr;
	}
	gl_FragColor = texture2D(palette, vec2(float(n) / 20.0, 1.0));
}";
			}
		}

		public override void Calculate (int[] colors, int width, int height)
		{
			double ratio = width / (double)height;
			
			var palette = new int[Options.PaletteSize];
			var bytePalette = Options.GeneratePalette ();
			for (int i = 0; i < Options.PaletteSize; i++) {
				palette [i] = bytePalette [i * 4] | bytePalette [i * 4 + 1] << 8 | bytePalette [i * 4 + 2] << 16 | bytePalette [i * 4 + 3] << 24;
			}
			
			
			int o = 0;
			for (int y = 0; y < height; y++) {
				double i0 = (y / (double)height - 0.5) * Options.Scale - Options.Y;
				
				for (int x = 0; x < width; x++) {
					double r0 = ratio * (x / (double)width - 0.5) * Options.Scale - Options.X;
					
					double i = i0;
					double r = r0;
					
					int n;
					for (n = 0; n < Options.PaletteSize; n++) {
						var nr = r * r - i * i + RConstant;
						var ni = 2 * r * i + IConstant;
						if (nr * nr + ni * ni > 4.0)
							break;
						r = nr;
						i = ni;
					}
					colors [o++] = palette [n];
				}
			}
		}

	}
}

