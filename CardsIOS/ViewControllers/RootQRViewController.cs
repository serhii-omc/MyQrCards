using Foundation;
using SidebarNavigation;
using System;
using UIKit;

namespace CardsIOS
{
    public partial class RootQRViewController : UIViewController
    {
        public static SidebarController SidebarController;
        public RootQRViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;
            SidebarController = ((AppDelegate)UIApplication.SharedApplication.Delegate).SideBarController;
            UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
            SideMenuViewController menuVC = (SideMenuViewController)sb.InstantiateViewController(nameof(SideMenuViewController));
            QRViewController contentVC = (QRViewController)sb.InstantiateViewController(nameof(QRViewController));
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