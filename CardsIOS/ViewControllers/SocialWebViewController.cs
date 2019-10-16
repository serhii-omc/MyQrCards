using CardsIOS.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using Foundation;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Linq;
using UIKit;
using WebKit;

namespace CardsIOS
{
    public partial class SocialWebViewController : UIViewController
    {
        WKWebView webView;
        NSString urlString;
        Cards cards = new Cards();
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Methods methods = new Methods();
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        public static int card_id;
        string UDID;
        public SocialWebViewController(IntPtr handle) : base(handle)
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
            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }
            InvokeInBackground(async () =>
               {
                   string res_card_data = null;
                   try
                   {
                       res_card_data = await cards.CardDataGet(databaseMethods.GetAccessJwt(), card_id, UDID);
                   }
                   catch
                   {
                       if (!methods.IsConnected())
                           InvokeOnMainThread(() =>
                           {
                               NoConnectionViewController.view_controller_name = GetType().Name;
                               this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                               return;
                           });
                       return;
                   }
                   if (/*res_card_data == Constants.status_code409 ||*/ res_card_data == Constants.status_code401)
                   {
                       InvokeOnMainThread(() =>
                       {
                           ShowSeveralDevicesRestriction();
                           return;
                       });
                       return;
                   }
                   var des_card_data = JsonConvert.DeserializeObject<CardsDataModel>(res_card_data);
                   InvokeOnMainThread(() =>
                   {
                       urlString = new NSString(des_card_data.url);
                       NSUrl url = new NSUrl(urlString);
                       webView = new WKWebView(View.Frame, new WKWebViewConfiguration());
                       View.AddSubview(webView);
                       var request = new NSMutableUrlRequest(url);
                       //request.HttpMethod = "Post";
                       webView.LoadRequest(request);
                       webView.Frame = new Rectangle(0, Convert.ToInt32(headerView.Frame.X + headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height - headerView.Frame.Height));
                   });
               });

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
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Web";
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height / 2 - View.Frame.Width / 20), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
        }

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}