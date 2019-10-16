using System;
using System.Drawing;
using CardsIOS.NativeClasses;
using CardsPCL;
using CardsPCL.Database;
using CoreGraphics;
using Foundation;
using UIKit;

namespace CardsIOS
{
    public partial class SideMenuViewController : UIViewController
    {
        static DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        static UIAlertController option;
        static UIStoryboard sb;
        public SideMenuViewController(IntPtr handle/*, UINavigationController navigationController*/) : base(handle)
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

            sb = UIStoryboard.FromName("Main", null);
            var vc = sb.InstantiateViewController(nameof(RootQRViewController));
            if (!databaseMethods.userExists())
                vc = sb.InstantiateViewController(nameof(CreatingCardViewController));
            var email_vc = sb.InstantiateViewController(nameof(EmailViewControllerNew));
            var about_app_vc = sb.InstantiateViewController(nameof(AboutAppViewController));
            var premium_vc = sb.InstantiateViewController(nameof(PremiumViewController));

            //reloadOption();

            myCardIV.TouchUpInside += (s, e) =>
            {
                if (databaseMethods.userExists())
                {
                    close_menu();
                    ViewController.navigationController.PushViewController(vc, true);
                }
                else
                {
                    close_menu();
                    ViewController.navigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
                }
            };
            myCardBn.TouchUpInside += (s, e) =>
            {
                if (databaseMethods.userExists())
                {
                    close_menu();
                    ViewController.navigationController.PushViewController(vc, true);
                }
                else
                {
                    close_menu();
                    ViewController.navigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
                }
            };
            orderIV.TouchUpInside += (s, e) => OpenOrderLink();
            orderBn.TouchUpInside += (s, e) => OpenOrderLink();
            cloudIV.TouchUpInside += (s, e) =>
            {
                close_menu();
                if (String.IsNullOrEmpty(QRViewController.ExtraPersonData))
                    ViewController.navigationController.PushViewController(sb.InstantiateViewController(nameof(CloudSyncViewController)), true);
                else
                    ViewController.navigationController.PushViewController(sb.InstantiateViewController(nameof(CloudSyncPremiumViewController)), true);
            };
            cloudBn.TouchUpInside += (s, e) =>
            {
                close_menu();
                if (String.IsNullOrEmpty(QRViewController.ExtraPersonData))
                    ViewController.navigationController.PushViewController(sb.InstantiateViewController(nameof(CloudSyncViewController)), true);
                else
                    ViewController.navigationController.PushViewController(sb.InstantiateViewController(nameof(CloudSyncPremiumViewController)), true);
            };
            premiumIV.TouchUpInside += (s, e) => { close_menu(); ViewController.navigationController.PushViewController(premium_vc, true); };
            premiumBn.TouchUpInside += (s, e) => { close_menu(); ViewController.navigationController.PushViewController(premium_vc, true); };
            aboutIV.TouchUpInside += (s, e) => { close_menu(); ViewController.navigationController.PushViewController(about_app_vc, true); };
            aboutBn.TouchUpInside += (s, e) => { close_menu(); ViewController.navigationController.PushViewController(about_app_vc, true); };
            enterIV.TouchUpInside += (s, e) =>
            {
                if (!databaseMethods.userExists())
                {
                    close_menu();
                    databaseMethods.InsertLoginedFrom(Constants.from_slide_menu);
                    ViewController.navigationController.PushViewController(email_vc, true);
                }
                else
                    this.PresentViewController(option, true, null);
            };
            enterBn.TouchUpInside += (s, e) =>
            {
                if (!databaseMethods.userExists())
                {
                    close_menu();
                    databaseMethods.InsertLoginedFrom(Constants.from_slide_menu);
                    ViewController.navigationController.PushViewController(email_vc, true);
                }
                else
                {
                    reloadOption();
                    this.PresentViewController(option, true, null);
                }
            };
            if (databaseMethods.userExists())
            {
                enterIV.Transform = CGAffineTransform.MakeRotation((nfloat)Math.PI);
                enterBn.SetTitle("Выйти", UIControlState.Normal);
            }
            aboutBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            cloudBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            enterBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            orderBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            myCardBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            premiumBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
        }

        private void InitElements()
        {
            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            new AppDelegate().disableAllOrientation = true;
            splashIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 8,
                                         (Convert.ToInt32(View.Frame.Height) / 10),
                                          (Convert.ToInt32(View.Frame.Height) / 6) + 15,
                                         Convert.ToInt32(View.Frame.Height) / 6);
            myCardIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 20),
                                           Convert.ToInt32(splashIV.Frame.Y) + Convert.ToInt32(splashIV.Frame.Height) + 50,
                                           Convert.ToInt32(View.Frame.Height) / 28,
                                           Convert.ToInt32(View.Frame.Height) / 28);
            orderIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 20),
                                          Convert.ToInt32(myCardIV.Frame.Y) + Convert.ToInt32(myCardIV.Frame.Height) + Convert.ToInt32(myCardIV.Frame.Height * 1.2),
                                          Convert.ToInt32(View.Frame.Height) / 28,
                                          Convert.ToInt32(View.Frame.Height) / 28);
            cloudIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 20),
                                          Convert.ToInt32(orderIV.Frame.Y) + Convert.ToInt32(myCardIV.Frame.Height) + Convert.ToInt32(myCardIV.Frame.Height * 1.2),
                                          Convert.ToInt32(View.Frame.Height) / 28,
                                          Convert.ToInt32(View.Frame.Height) / 28);
            premiumIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 20),
                                            Convert.ToInt32(cloudIV.Frame.Y) + Convert.ToInt32(myCardIV.Frame.Height) + Convert.ToInt32(myCardIV.Frame.Height * 1.2),
                                            Convert.ToInt32(View.Frame.Height) / 28,
                                            Convert.ToInt32(View.Frame.Height) / 28);
            aboutIV.Frame = new Rectangle(Convert.ToInt32((View.Frame.Width / 20) + 6),
                                          Convert.ToInt32(premiumIV.Frame.Y) + Convert.ToInt32(myCardIV.Frame.Height) + Convert.ToInt32(myCardIV.Frame.Height * 1.2),
                                          Convert.ToInt32(View.Frame.Height) / 56,
                                          Convert.ToInt32(View.Frame.Height) / 28);
            enterIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 20),
                                          Convert.ToInt32(View.Frame.Height) - Convert.ToInt32(aboutIV.Frame.Height * 2),
                                          Convert.ToInt32(View.Frame.Height) / 31,
                                          Convert.ToInt32(View.Frame.Height) / 28);
            myCardBn.Frame = new Rectangle(Convert.ToInt32(enterIV.Frame.X + enterIV.Frame.Width * 1.5),
                                           Convert.ToInt32(splashIV.Frame.Y) + Convert.ToInt32(splashIV.Frame.Height) + 50,
                                           Convert.ToInt32(View.Frame.Width),
                                           Convert.ToInt32(View.Frame.Height) / 28);
            orderBn.Frame = new Rectangle(Convert.ToInt32(enterIV.Frame.X + enterIV.Frame.Width * 1.5),
                                          Convert.ToInt32(myCardIV.Frame.Y) + Convert.ToInt32(myCardIV.Frame.Height) + Convert.ToInt32(myCardIV.Frame.Height * 1.2),
                                          Convert.ToInt32(View.Frame.Width),
                                          Convert.ToInt32(View.Frame.Height) / 28);
            cloudBn.Frame = new Rectangle(Convert.ToInt32(enterIV.Frame.X + enterIV.Frame.Width * 1.5),
                                          Convert.ToInt32(orderIV.Frame.Y) + Convert.ToInt32(myCardIV.Frame.Height) + Convert.ToInt32(myCardIV.Frame.Height * 1.2),
                                          Convert.ToInt32(View.Frame.Width),
                                          Convert.ToInt32(View.Frame.Height) / 28);
            premiumBn.Frame = new Rectangle(Convert.ToInt32(enterIV.Frame.X + enterIV.Frame.Width * 1.5),
                                            Convert.ToInt32(cloudIV.Frame.Y) + Convert.ToInt32(myCardIV.Frame.Height) + Convert.ToInt32(myCardIV.Frame.Height * 1.2),
                                            Convert.ToInt32(View.Frame.Width),
                                            Convert.ToInt32(View.Frame.Height) / 28);
            aboutBn.Frame = new Rectangle(Convert.ToInt32(enterIV.Frame.X + enterIV.Frame.Width * 1.5),
                                          Convert.ToInt32(premiumIV.Frame.Y) + Convert.ToInt32(myCardIV.Frame.Height) + Convert.ToInt32(myCardIV.Frame.Height * 1.2),
                                          Convert.ToInt32(View.Frame.Width),
                                          Convert.ToInt32(View.Frame.Height) / 28);
            enterBn.Frame = new Rectangle(Convert.ToInt32(enterIV.Frame.X + enterIV.Frame.Width * 1.5),
                                          Convert.ToInt32(View.Frame.Height) - Convert.ToInt32(aboutIV.Frame.Height * 2),
                                          Convert.ToInt32(View.Frame.Width),
                                          Convert.ToInt32(View.Frame.Height) / 28);
            myCardBn.SetTitle("Моя визитка", UIControlState.Normal);
            orderBn.SetTitle("Заказать наклейку c QR", UIControlState.Normal);
            cloudBn.SetTitle("Облачная синхронизация", UIControlState.Normal);
            premiumBn.SetTitle("Premium", UIControlState.Normal);
            aboutBn.SetTitle("О приложении", UIControlState.Normal);
            enterBn.SetTitle("Войти", UIControlState.Normal);
        }

        public static void close_menu()
        {
            try { QRViewController.clear_current_card_name_and_pos(); } catch { }
            try { RootQRViewController.SidebarController.ToggleMenu(); } catch { }
            try { RootMyCardViewController.SidebarController.ToggleMenu(); } catch { }
        }
        public static void log_out()
        {
            close_menu();
            LogOutClass.log_out();

            var vc = sb.InstantiateViewController(nameof(RootMyCardViewController));
            try
            {
                ViewController.navigationController.PushViewController(vc, true);
            }
            catch (Exception ex)
            {

            }
        }
        public static void reloadOption()
        {
            option = null;
            option = UIAlertController.Create("",
                                              null,
                                              UIAlertControllerStyle.ActionSheet);//(null, null, "Отменить", null, "Выйти");
            var titleAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.Red,
            };
            var premium_vc = sb.InstantiateViewController(nameof(PremiumViewController));
            // Add Actions
            if (!QRViewController.is_premium)
            {
                option.AddAction(UIAlertAction.Create("Подробнее о Premium", UIAlertActionStyle.Default, (action) =>
                {
                    close_menu();
                    ViewController.navigationController.PushViewController(premium_vc, true);
                }));
                option.SetValueForKey(new NSAttributedString("Вы собираетесь выйти из учетной записи. Облачное хранение данных предусмотрено только для Premium-подписки, поэтому при следующем входе Вам придется создать свою визитку заново",
                                                         titleAttributes),
                                  new NSString("attributedTitle"));
            }
            else
            {
                option.SetValueForKey(new NSAttributedString("Выход", titleAttributes), new NSString("attributedTitle"));
            }
            option.AddAction(UIAlertAction.Create("Выйти из профиля", UIAlertActionStyle.Default, (action) => log_out()));
            option.AddAction(UIAlertAction.Create("Отменить", UIAlertActionStyle.Cancel, null/*, (action) => Console.WriteLine("Cancel button pressed.")*/));
        }

        private void OpenOrderLink()
        {
            NSString urlString = new NSString("https://myqrcards.com/sticker");
            NSUrl myFileUrl = new NSUrl(urlString);
            UIApplication.SharedApplication.OpenUrl(myFileUrl);
        }
    }
}