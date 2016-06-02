using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Opengl;
using Android.Graphics;

namespace XFracts
{
	// the ConfigurationChanges flags set here keep the EGL context
	// from being destroyed whenever the device is rotated or the
	// keyboard is shown (highly recommended for all GL apps)
	[Activity (Label = "XFracts",
				ConfigurationChanges=ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
				MainLauncher = true)]
	public class XFractsActivity : Activity
	{
		FractalGLSurfaceView mGLView;
		FractalCPUView mCPUView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			RequestWindowFeature (WindowFeatures.NoTitle);
			Window.SetFlags (WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			// Create a GLSurfaceView instance and set it
			// as the ContentView for this Activity
			mGLView = new FractalGLSurfaceView (this);
			mCPUView = new FractalCPUView (this);
			SetContentView (mGLView);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			menu.Add("Algorithm");
			menu.Add("Palette");
			menu.Add("Set as Wallpaper");
//			menu.Add("Switch View");
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.TitleFormatted.ToString()) { 
			case "Algorithm":
				StartActivity (typeof (AlgorithmSwitchActivity));
				break;
			case "Palette":
				StartActivity (typeof (PaletteSwitchActivity));
				break;
			case "Set as Wallpaper":
				mGLView.TakeScreenshot ();
				break;
			case "Switch View":
				SetContentView (mCPUView);
				break;
			}
			return base.OnOptionsItemSelected(item);
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			// The following call pauses the rendering thread.
			// If your OpenGL application is memory intensive,
			// you should consider de-allocating objects that
			// consume significant memory here.
			mGLView.OnPause ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			// The following call resumes a paused rendering thread.
			// If you de-allocated graphic objects for onPause()
			// this is a good place to re-allocate them.
			mGLView.OnResume ();
		}
	}
}


