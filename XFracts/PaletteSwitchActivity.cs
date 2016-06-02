
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
	[Activity (Label = "PaletteSwitchActivity")]			
	class PaletteSwitchActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.PaletteSwitcher);
			
			FindViewById<Button> (Resource.Id.button1).Click += (sender, args) => {
				Options.Palette = 0;
				Finish ();
			};
			
			FindViewById<Button> (Resource.Id.button2).Click += (sender, args) => {
				Options.Palette = 1;
				Finish ();
			};
			
			FindViewById<Button> (Resource.Id.button3).Click += (sender, args) => {
				Options.Palette = 2;
				Finish ();
			};

			FindViewById<Button> (Resource.Id.button4).Click += (sender, args) => {
				Options.Palette = 3;
				Finish ();
			};

			// Create your application here
		}
	}
}

