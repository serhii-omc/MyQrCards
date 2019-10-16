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

namespace CardsIOS
{
    public partial class CardDoneViewController : UIViewController
    {
        public static int card_id;
        Methods methods = new Methods();
        Cards cards = new Cards();
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        UIStoryboard storyboard = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        string UDID;
        public CardDoneViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
            InitElements();

            readyBn.TouchUpInside += (s, e) =>
              {
                  if(methods.IsConnected())
                   this.NavigationController.PushViewController(storyboard.InstantiateViewController(nameof(RootQRViewController)), true);
                  else
                  {
                      QRViewController.ShowWeHaveNotDownloadedCard=true;
                      this.NavigationController.PushViewController(storyboard.InstantiateViewController(nameof(RootQRViewController)), true);
                  }
              };
            watch_in_webBn.TouchUpInside += new EventHandler(this.ShowInWebClick);
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            var vc_list = this.NavigationController.ViewControllers.ToList();
            vc_list.RemoveAt(vc_list.Count - 2);

            this.NavigationController.ViewControllers = vc_list.ToArray();

            new AppDelegate().disableAllOrientation = true;
            watch_in_webBn.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
            watch_in_webBn.Layer.BorderWidth = 1f;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;

            cardsLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(cardsLogo.Frame.X) + Convert.ToInt32(View.Frame.Width) / 3) + 35, Convert.ToInt32(View.Frame.Width), 26);
            //var d = cardsLogo.Frame.X;
            mainTextTV.Text = "Визитка готова!";
            mainTextTV.Font = mainTextTV.Font.WithSize(22f);
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);

            infoLabel.Lines = 3;
            infoLabel.Frame = new Rectangle(0, Convert.ToInt32(mainTextTV.Frame.Y) + 29, Convert.ToInt32(View.Frame.Width), 100);
            readyBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 8,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            readyBn.SetTitle("ГОТОВО", UIControlState.Normal);
            watch_in_webBn.SetTitle("ПОСМОТРЕТЬ В WEB", UIControlState.Normal);
            watch_in_webBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                                 (int)(readyBn.Frame.Y + readyBn.Frame.Height + 5),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            watch_in_webBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            readyBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            readyBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            //if (variant_displaying == 1)
            //{
                infoLabel.Text = "Предложите вашему партнеру" + "\r\n" + "отсканировать с нее QR-код" + "\r\n" + "и сохранить контактную информацию";
                watch_in_webBn.Hidden = false;
                readyBn.SetTitle("ПРОДОЛЖИТЬ", UIControlState.Normal);
            //}
            //else if (variant_displaying == 2)
            //{
            //    infoLabel.Text = "При первом подключении" + "\r\n" + "данные визитки" + "\r\n" + "синронизируются с сервером";
            //    watch_in_webBn.Hidden = true;
            //}
        }

        void ShowInWebClick(object sender, EventArgs e)
        {
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(storyboard.InstantiateViewController(nameof(NoConnectionViewController)), false);
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
                            this.NavigationController.PushViewController(storyboard.InstantiateViewController(nameof(NoConnectionViewController)), false);
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
                    NSString urlString = new NSString(des_card_data.url);
                    NSUrl myFileUrl = new NSUrl(urlString);
                    UIApplication.SharedApplication.OpenUrl(myFileUrl);
                });
            });
        }
        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(storyboard.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}