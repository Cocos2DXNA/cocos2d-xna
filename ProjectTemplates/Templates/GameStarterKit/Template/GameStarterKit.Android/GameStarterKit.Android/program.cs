using System;
using System.Diagnostics;

using Android.Content.PM;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Uri = Android.Net.Uri;
using Microsoft.Xna.Framework;

namespace GameStarterKit
{

/*
Note: https://monogame.codeplex.com/discussions/361463
If your game is landscape only and you plan to support a portrait-only lock screen
then you need to add the configuration changes option that Aranda mentions:

  , ConfigurationChanges = ConfigChanges.Orientation 
                           | ConfigChanges.Keyboard 
                           | ConfigChanges.KeyboardHidden 
                           | (ConfigChanges)1024 /* ConfigChanges.ScreenSize */

*/
    [Activity(
        Label = "$safeprojectname$",
               AlwaysRetainTaskState = true,
               Icon = "@drawable/Icon",
               Theme = "@style/Theme.NoTitleBar",
		// This is where you set the orientations supported by your game.
               ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,
               LaunchMode = Android.Content.PM.LaunchMode.SingleInstance,
        MainLauncher = true,
		// If your game has orientation changes, then you need to enable them here
        ConfigurationChanges =  ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)
    ]
    public class Activity1 : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var game = new Game1();

			var frameLayout = new FrameLayout(this);
            frameLayout.AddView((View)game.Services.GetService(typeof(View)));
            this.SetContentView(frameLayout);

            game.Run(GameRunBehavior.Asynchronous);
        }
		/*
		Implement other lifecycle methods for your Activity. Your game will launch through this
		activity class.
		*/
    }
}

