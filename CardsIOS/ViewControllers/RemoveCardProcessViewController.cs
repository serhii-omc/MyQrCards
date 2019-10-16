using System;
using System.Drawing;
using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using Foundation;
using UIKit;

namespace CardsIOS
{
    public partial class RemoveCardProcessViewController : UIViewController
    {
        public static int? card_id;
        Cards cards = new Cards();
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        Methods methods = new Methods();
        string UDID;
        public RemoveCardProcessViewController(IntPtr handle) : base(handle)
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
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }
            InvokeInBackground(async () =>
            {
                System.Net.Http.HttpResponseMessage res = null;
                try
                {
                    res = await cards.CardDelete(databaseMethods.GetAccessJwt(), Convert.ToInt32(card_id), UDID);
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
                ClearSocialNetworks();
                if (res.StatusCode.ToString().Contains("401") || res.StatusCode.ToString().ToLower().Contains(Constants.status_code401))
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSeveralDevicesRestriction();
                        return;
                    });
                    return;
                }
                card_id = null;
                InvokeOnMainThread(() =>
                {
                    databaseMethods.InsertLastCloudSync(DateTime.Now);
                    NavigationController.PopViewController(false);
                });
            });

            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
        }

        private void ClearSocialNetworks()
        {
            try
            {
                SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
                SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
            }
            catch { }
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;
            backBn.Hidden = true;
            var deviceModel = Xamarin.iOS.DeviceHardware.Model;

            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            }

            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            emailLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), 26);
            mainTextTV.Text = "Визитка удаляется";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
        }

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}