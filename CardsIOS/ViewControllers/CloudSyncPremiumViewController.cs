using CardsPCL.Database;
using CoreGraphics;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace CardsIOS
{
    public partial class CloudSyncPremiumViewController : UIViewController
    {
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        public CloudSyncPremiumViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;
            new AppDelegate().disableAllOrientation = true;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                headerLabel.Frame = new Rectangle(0/*Convert.ToInt32(View.Frame.Width) / 5*/, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 5, Convert.ToInt32(View.Frame.Width) / 18);
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                headerLabel.Frame = new Rectangle(0,/*Convert.ToInt32(View.Frame.Width) / 5,*/ Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 5, Convert.ToInt32(View.Frame.Width) / 18);
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            }

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            bgIV.Frame = new CGRect(0, -50, View.Frame.Width, View.Frame.Height + 50);
            headerLabel.Text = "Облачная синхронизация";
            lastSyncLabel.Frame = new CGRect(0, headerView.Frame.Height * 1.2, Convert.ToInt32(View.Frame.Width), View.Frame.Height / 7);
            lastSyncLabel.Text = "Последняя" + "\r\n" + "синхронизация";
            var last_sync_value = databaseMethods.GetLastCloudSync().ToString();
            if (!String.IsNullOrEmpty(last_sync_value))
                lastSyncValueLabel.Text = last_sync_value.Replace('/', '.');
            else
                lastSyncValueLabel.Text = "Не выполнена";
            lastSyncValueLabel.SizeToFit();
            var width = lastSyncValueLabel.Frame.Width;
            lastSyncValueLabel.Frame = new CGRect((View.Frame.Width - width) / 2, lastSyncLabel.Frame.Y + lastSyncLabel.Frame.Height, width, View.Frame.Width / 8);
            timerSyncBgIV.Frame = new CGRect((View.Frame.Width - width) / 2 - width / 6, lastSyncLabel.Frame.Y + lastSyncLabel.Frame.Height, width + width / 3, View.Frame.Width / 8);
        }
    }
}