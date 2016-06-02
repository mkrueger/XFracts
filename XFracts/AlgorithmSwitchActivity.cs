
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Opengl;

namespace XFracts
{
	[Activity (Label = "AlgorithmSwitchActivity")]			
	class AlgorithmSwitchActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.AlgorithmSwitcher);
			
			FindViewById<Button> (Resource.Id.algorithm1).Click += (sender, args) => {
				Options.Fractal = new Mandelbrot ();
				Finish ();
			};
			
			FindViewById<Button> (Resource.Id.algorithm2).Click += (sender, args) => {
				Options.Fractal = new JuliaSet (0f, 0.8f);
				Finish ();
			};

			FindViewById<Button> (Resource.Id.algorithm3).Click += (sender, args) => {
				Options.Fractal = new JuliaSet (-0.6f, 0.6f);
				Finish ();
			};

			FindViewById<Button> (Resource.Id.algorithm4).Click += (sender, args) => {
				Options.Fractal = new JuliaSet (0.39f, 0.6f);
				Finish ();
			};

			FindViewById<Button> (Resource.Id.algorithm5).Click += (sender, args) => {
				Options.Fractal = new JuliaSet (-0.8f, 0.2f);
				Finish ();
			};

			FindViewById<Button> (Resource.Id.algorithm6).Click += (sender, args) => {
				Options.Fractal = new JuliaSet (-1f, 0f);
				Finish ();
			};

		}
	}
}

