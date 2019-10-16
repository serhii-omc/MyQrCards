using Foundation;
using SidebarNavigation;
using UIKit;
using Google.Maps;
using CardsIOS.NativeClasses;
using VKontakte;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace CardsIOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        public SidebarController SideBarController { get; set; }
        public bool disableAllOrientation = false;
        public override UIWindow Window
        {
            get;
            set;
        }
        public static AppDelegate Instance()
        {
            return (AppDelegate)UIApplication.SharedApplication.Delegate;
        }
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            try
            {
                return VKSdk.ProcessOpenUrl(url, sourceApplication)
                || Facebook.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation)
                || base.OpenUrl(application, url, sourceApplication, annotation);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override void OnActivated(UIApplication application)
        {
            Facebook.CoreKit.AppEvents.ActivateApp();
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            MapServices.ProvideAPIKey(CardsPCL.Constants.mapsApiKey);
            AppCenter.Start(CardsPCL.Constants.appCenterSecretIosRelease, typeof(Analytics), typeof(Crashes));

            // Open desktop site version in WebView
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(new NSDictionary("UserAgent",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.3 Safari/7046A194A"));

            //window = new UIWindow(UIScreen.MainScreen.Bounds);

            // set our root view controller with the sidebar menu as the apps root view controller
            //window.RootViewController = new RootViewController();


            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method
            Window.RootViewController = new UINavigationController(Window.RootViewController);
            //Window.MakeKeyAndVisible();
            //var fontName = "FiraSans-Regular.ttf";
            //var fontPath = NSBundle.MainBundle.PathForResource(
            //    System.IO.Path.GetFileNameWithoutExtension(fontName),
            //    System.IO.Path.GetExtension(fontName));

            //// overriding default font with custom font that supports Japanese symbols
            //var font = SkiaSharp.SKTypeface.FromFile(fontPath);
            //Infragistics.Core.Controls.TypefaceManager.Instance.OverrideDefaultTypeface(font);
            //Window.RootViewController = new UINavigationController(Window.RootViewController);
            UILabel.Appearance.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 17);
            Facebook.CoreKit.Profile.EnableUpdatesOnAccessTokenChange(true);
            Facebook.CoreKit.ApplicationDelegate.SharedInstance.FinishedLaunching(application, launchOptions);
            Facebook.CoreKit.Settings.AutoLogAppEventsEnabled = true;
            Facebook.CoreKit.Settings.AppId = "398626670741726";
            VKSdk.Initialize("6789848");
            SetAutoKeyboard();
            return true;
        }

        private void SetAutoKeyboard()
        {
            Xamarin.IQKeyboardManager.SharedManager.EnableAutoToolbar = true;
            Xamarin.IQKeyboardManager.SharedManager.ShouldResignOnTouchOutside = true;
            Xamarin.IQKeyboardManager.SharedManager.ToolbarDoneBarButtonItemText = "Скрыть клавиатуру";
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
        {
            if (disableAllOrientation == true)
            {            
                return UIInterfaceOrientationMask.All;
            }
			return UIInterfaceOrientationMask.Portrait;
        }
       

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }



        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}

