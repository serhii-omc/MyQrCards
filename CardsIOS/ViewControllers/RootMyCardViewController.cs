using CardsPCL.Database;
using Foundation;
using SidebarNavigation;
using System;
using UIKit;

namespace CardsIOS
{
    public partial class RootMyCardViewController : UIViewController
    {
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        public static SidebarController SidebarController;
        public RootMyCardViewController(IntPtr handle) : base(handle)
        {

        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Enable back navigation using swipe.
            if (databaseMethods.userExists())
                NavigationController.InteractivePopGestureRecognizer.Delegate = null;
            else
                NavigationController.InteractivePopGestureRecognizer.Enabled = false;
            new AppDelegate().disableAllOrientation = true;
            SidebarController = ((AppDelegate)UIApplication.SharedApplication.Delegate).SideBarController;
            UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
            SideMenuViewController menuVC = (SideMenuViewController)sb.InstantiateViewController(nameof(SideMenuViewController));
            MyCardViewController contentVC = (MyCardViewController)sb.InstantiateViewController(nameof(MyCardViewController));
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            SidebarController = new SidebarController(this, contentVC, menuVC);
            SidebarController.MenuLocation = MenuLocations.Left;
            SidebarController.MenuWidth = Convert.ToInt32(View.Frame.Width - Convert.ToInt32(View.Frame.Width) / 6);
            SidebarController.Sidebar.GestureActiveArea = SidebarController.MenuWidth;
            contentVC.SideBarController = SidebarController;
            contentVC.holderVC = this;
        }
    }
}