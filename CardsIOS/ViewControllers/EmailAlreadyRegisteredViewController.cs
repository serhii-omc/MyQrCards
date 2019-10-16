using CardsPCL;
using CardsPCL.CommonMethods;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace CardsIOS
{
    public partial class EmailAlreadyRegisteredViewController : UIViewController
    {
        Methods methods = new Methods();
        UIStoryboard storyboard = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        public EmailAlreadyRegisteredViewController(IntPtr handle) : base(handle)
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


            next_Bn.TouchUpInside += (s, e) =>
              {
                  AttentionViewController.alreadyRegisteredViewController = this.NavigationController;
                  var vc = storyboard.InstantiateViewController(nameof(AttentionViewController));
                  this.NavigationController.PushViewController(vc, true);
              };
            premiumBn.TouchUpInside += (s, e) =>
              {
                  var vc = storyboard.InstantiateViewController(nameof(PremiumViewController));
                  this.NavigationController.PushViewController(vc, true);
              };
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
            next_Bn.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
            next_Bn.Layer.BorderWidth = 1f;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            if (deviceModel.Contains("X"))
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            else
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);

            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            cardsLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(cardsLogo.Frame.X) + Convert.ToInt32(View.Frame.Width) / 3) + 35, Convert.ToInt32(View.Frame.Width), 60);
            //var d = cardsLogo.Frame.X;
            mainTextTV.Text = "Этот email уже" + "\r\n" + "зарегистрирован в системе";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            mainTextTV.Lines = 2;

            infoLabel.Text = "Использование приложения на" + "\r\n" + "нескольких устройствах доступно" + "\r\n" + "по Premium подписке.";
            infoLabel.Lines = 4;
            infoLabel.Frame = new Rectangle(0, Convert.ToInt32(mainTextTV.Frame.Y + mainTextTV.Frame.Height), Convert.ToInt32(View.Frame.Width), 100);
            premiumBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 8,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            premiumBn.SetTitle("ПОДРОБНЕЕ О PREMIUM", UIControlState.Normal);
            premiumBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            next_Bn.SetTitle("ДАЛЕЕ", UIControlState.Normal);
            next_Bn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                          (int)(premiumBn.Frame.Y + premiumBn.Frame.Height + 5),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            next_Bn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            premiumBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
        }
        public void GoToNetworkLostScreen()
        {
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
        }
    }
}