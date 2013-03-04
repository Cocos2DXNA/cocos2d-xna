using System;

#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Microsoft.Xna.Framework;
#endif
#if IPHONE || IOS
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace tests
{
#if IPHONE || IOS
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		private Game1 game;

		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			game = new Game1();
			game.Run();
		}
		
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
#endif
#if WINDOWS || XBOX || PSM
#if !NETFX_CORE
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif
#endif

#if ANDROID
    [Activity(
        Label = "Tests",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)
    ]
    public class Activity1 : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Game1.Activity = this;
            var game = new Game1();

            var frameLayout = new FrameLayout(this);
            frameLayout.AddView(game.Window);
            this.SetContentView(frameLayout);

            //SetContentView(game.Window);
            game.Run(GameRunBehavior.Asynchronous);
        }
    }
#endif
#if NETFX_CORE 
    public static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
            var factory = new MonoGame.Framework.GameFrameworkViewSource<Game1>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
#endif
}

