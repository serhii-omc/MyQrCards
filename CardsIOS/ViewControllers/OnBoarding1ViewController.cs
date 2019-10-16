using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CoreGraphics;
using CoreImage;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace CardsIOS
{
    public partial class OnBoarding1ViewController : UIViewController
    {
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Attachments attachments = new Attachments();
        UIStoryboard sb = UIStoryboard.FromName("Main", null);

        public OnBoarding1ViewController(IntPtr handle) : base(handle)
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

            nextBn.TouchUpInside += (s, e) =>
              {
                  if (mainTextTV.Text == "Создавайте визитки")
                  {
                      mainTextTV.Text = "Делитесь с партнерами";
                      infoLabel.Text = "Предложите вашему партнеру"
                          + "\r\n" + "отсканировать QR-код с визитки"
                          + "\r\n" + "и сохранить контактную информацию";
                      cardsLogo.Image = UIImage.FromBundle("onBoard2Logo");
                      accountView.Hidden = true;
                      skipBn.Hidden = false;
                  }
                  else if (mainTextTV.Text == "Делитесь с партнерами")
                  {
                      mainTextTV.Text = "Заказывайте наклейки";
                      infoLabel.Text = "Делитесь QR-кодом"
                          + "\r\n" + "как из приложения, так"
                          + "\r\n" + "и со специальной QR наклейки";
                      cardsLogo.Image = UIImage.FromBundle("onBoard3Logo");
                      skipBn.Hidden = true;
                  }
                  else if (mainTextTV.Text == "Заказывайте наклейки")
                  {
                      GoToMyCard();
                  }
              };
            skipBn.TouchUpInside += (s, e) =>
              {
                  GoToMyCard();
              };
            enterBn.TouchUpInside += (s, e) =>
              {
                  var vc = sb.InstantiateViewController(nameof(EmailViewControllerNew));
                  this.NavigationController.PushViewController(vc, true);
              };
        }

        private void GoToMyCard()
        {
            var vc = sb.InstantiateViewController(nameof(RootMyCardViewController));
            this.NavigationController.PushViewController(vc, true);
        }

        private void InitElements()
        {
            new AppDelegate().disableAllOrientation = true;
            backgroundIV.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);

            skipBn.Hidden = true;

            cardsLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(cardsLogo.Frame.X) + Convert.ToInt32(View.Frame.Width) / 3) + 35, Convert.ToInt32(View.Frame.Width), 26);
            mainTextTV.Text = "Создавайте визитки";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            //var singlelineHeight = infoLabel.Frame.Height;
            infoLabel.Lines = 3;
            infoLabel.Text = "Заполняйте личные" + "\r\n" + "и корпоративные данные," + "\r\n" + "добавляйте лого компании";
            //infoLabel.BackgroundColor = UIColor.Brown;
            infoLabel.Frame = new Rectangle(0, Convert.ToInt32(mainTextTV.Frame.Y) + 29, Convert.ToInt32(View.Frame.Width), /*(int)singlelineHeight*3*/100);
            //infoLabel.SizeToFit();
            //infoLabel.Frame = new Rectangle(0, (int)infoLabel.Frame.Y, (int)View.Frame.Width, (int)infoLabel.Frame.Height);
            nextBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 9,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);

            skipBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                          (Convert.ToInt32(View.Frame.Height) / 10) * 8,
                                          Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                          Convert.ToInt32(View.Frame.Height) / 12);

            nextBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            nextBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);

            var distanceBeweenInfoAndButton = nextBn.Frame.Y - infoLabel.Frame.Y - infoLabel.Frame.Height;
            accountView.Frame = new CGRect(0, infoLabel.Frame.Y + infoLabel.Frame.Height + (distanceBeweenInfoAndButton - 60) / 2 - 10, View.Frame.Width, 60);
            accountExistsLabel.Frame = new CGRect(0, 0, View.Frame.Width, 30);
            accountExistsLabel.Text = "У вас уже есть аккаунт?";
            enterBn.Frame = new CGRect(0, 30, View.Frame.Width, 30);
            enterBn.SetTitleColor(UIColor.FromRGB(255, 99, 62), UIControlState.Normal);
            enterBn.SetTitle("Войти >", UIControlState.Normal);

            skipBn.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
            skipBn.Layer.BorderWidth = 1f;
            skipBn.SetTitle("ПРОПУСТИТЬ", UIControlState.Normal);
            skipBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
        }
    }
}