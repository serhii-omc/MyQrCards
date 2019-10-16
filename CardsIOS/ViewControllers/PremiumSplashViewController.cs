using CardsPCL;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace CardsIOS
{
    public partial class PremiumSplashViewController : UIViewController
    {
        public PremiumSplashViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        UIStoryboard storyboard = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            detailsBn.TouchUpInside+=(s,e)=> this.NavigationController.PushViewController(storyboard.InstantiateViewController(nameof(PremiumViewController)), true);
            thanksBn.TouchUpInside += (s, e) => this.NavigationController.PopViewController(true);
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            new AppDelegate().disableAllOrientation = true;
            thanksBn.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
            thanksBn.Layer.BorderWidth = 1f;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            if (deviceModel.Contains("X"))
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            else
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            cardsLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(cardsLogo.Frame.X) + Convert.ToInt32(View.Frame.Width) / 3) + 35, Convert.ToInt32(View.Frame.Width), 26);
            mainTextTV.Text = "Доступно для Premium!";
            mainTextTV.Font = mainTextTV.Font.WithSize(22f);

            detailsBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            infoLabel.Lines = 3;
            infoLabel.Frame = new Rectangle(0, Convert.ToInt32(mainTextTV.Frame.Y) + 29, Convert.ToInt32(View.Frame.Width), 100);
            detailsBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 8,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            detailsBn.SetTitle("ДОСТУПНО ДЛЯ PREMIUM", UIControlState.Normal);
            thanksBn.SetTitle("СПАСИБО", UIControlState.Normal);
            thanksBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                           (int)(detailsBn.Frame.Y + detailsBn.Frame.Height + 5),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            infoLabel.Text = "Для создания второй" + "\r\n" + "и последующих визиток," + "\r\n"+ "перейдите на Premium версию";
            thanksBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            detailsBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
        }
    }
}