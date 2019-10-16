using Foundation;
using System;
using System.Drawing;
using UIKit;
using SidebarNavigation;
using CardsPCL.Database;
using CardsPCL;

namespace CardsIOS
{
    public partial class MyCardViewController : UIViewController
    {
        public SidebarController SidebarController { get; private set; }
        public SidebarController SideBarController;
        public UIViewController holderVC;
        public static bool device_restricted;
        IntPtr handle;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        public MyCardViewController(IntPtr handle) : base(handle)
        {
            this.handle = handle;
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();
            createBn.TouchUpInside += (s, e) =>
              {
                  var vc = sb.InstantiateViewController(nameof(PersonalDataViewControllerNew));
                  this.NavigationController.PushViewController(vc, true);
              };
            leftMenuBn.TouchUpInside += (s, e) =>
            {
                RootMyCardViewController.SidebarController.ToggleMenu();
            };
            plusBn.TouchUpInside += PlusBn_TouchUpInside;
            if (device_restricted)
            {
                call_premium_option_menu(true);
                device_restricted = false;
            }
            enterBn.TouchUpInside += (s, e) =>
            {
                var vc = sb.InstantiateViewController(nameof(EmailViewControllerNew));
                this.NavigationController.PushViewController(vc, true);
            };
        }

        void PlusBn_TouchUpInside(object sender, EventArgs e)
        {
            UIViewController vc = new UIViewController();
            if (databaseMethods.userExists())
            {
                if (!QRViewController.is_premium && QRViewController.cards_remaining == 0)
                    call_premium_option_menu();
                else if (QRViewController.is_premium && QRViewController.cards_remaining == 0)
                {
                    UIAlertView alert = new UIAlertView()
                    {
                        Title = "Ошибка",
                        Message = "Достигнут лимит визиток для текущей подписки"
                    };
                    alert.AddButton("OK");
                    alert.Show();
                }
                if (QRViewController.cards_remaining > 0)
                {
                    vc = sb.InstantiateViewController(nameof(CreatingCardViewController));
                    this.NavigationController.PushViewController(vc, true);
                }
            }
            else
            {
                vc = sb.InstantiateViewController(nameof(PersonalDataViewControllerNew));
                this.NavigationController.PushViewController(vc, true);
            }
        }


        private void InitElements()
        {
            new AppDelegate().disableAllOrientation = true;

            SidebarController = ((AppDelegate)UIApplication.SharedApplication.Delegate).SideBarController;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;

            backgroundIV.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));

            cardsLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextLabel.Frame = new Rectangle(0, (Convert.ToInt32(cardsLogo.Frame.X) + Convert.ToInt32(View.Frame.Width) / 3) + 35, Convert.ToInt32(View.Frame.Width), 70);
            //var d = cardsLogo.Frame.X;
            mainTextLabel.Text = "Создайте \r\n первую визитку";
            mainTextLabel.Font = mainTextLabel.Font.WithSize(22f);
            infoLabel.Lines = 2;
            infoLabel.Text = "Заполните только ту информацию," + "\r\n" + "которую хотите показать" + "\r\n" + "своим партнерам";

            infoLabel.Lines = 3;
            infoLabel.Frame = new Rectangle(0, Convert.ToInt32(mainTextLabel.Frame.Y) + 60, Convert.ToInt32(View.Frame.Width), 100);
            createBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 9,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            createBn.Font = UIFont.FromName(Constants.fira_sans, 17f);

            enterBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                       (Convert.ToInt32(View.Frame.Height) / 10) * 8,
                                       Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                       Convert.ToInt32(View.Frame.Height) / 12);
            enterBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            enterBn.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
            enterBn.Layer.BorderWidth = 1f;
            enterBn.SetTitle("Войти >", UIControlState.Normal);
            enterBn.SetTitleColor(UIColor.FromRGB(255, 99, 62), UIControlState.Normal);
            enterBn.Font = UIFont.FromName(Constants.fira_sans, 15f);

            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                leftMenuBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 40,
                                                 Convert.ToInt32(View.Frame.Width) / 22 + 20,
                                                 Convert.ToInt32(View.Frame.Width) / 8,
                                                 Convert.ToInt32(View.Frame.Width) / 8);
                plusBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) - (Convert.ToInt32(leftMenuBn.Frame.Width) + (Convert.ToInt32(View.Frame.Width) / 40)),
                                             Convert.ToInt32(View.Frame.Width) / 22 + 20,
                                             Convert.ToInt32(View.Frame.Width) / 8,
                                             Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                leftMenuBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 40,
                                                 Convert.ToInt32(View.Frame.Width) / 22,
                                                 Convert.ToInt32(View.Frame.Width) / 8,
                                                 Convert.ToInt32(View.Frame.Width) / 8);
                plusBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) - (Convert.ToInt32(leftMenuBn.Frame.Width) + (Convert.ToInt32(View.Frame.Width) / 40)),
                                             Convert.ToInt32(View.Frame.Width) / 22,
                                             Convert.ToInt32(View.Frame.Width) / 8,
                                             Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            plusBn.ImageEdgeInsets = new UIEdgeInsets(plusBn.Frame.Width / 4, plusBn.Frame.Width / 4, plusBn.Frame.Width / 4, plusBn.Frame.Width / 4);
            leftMenuBn.ImageEdgeInsets = new UIEdgeInsets(plusBn.Frame.Width / 3.3F, plusBn.Frame.Width / 4, plusBn.Frame.Width / 3.3F, plusBn.Frame.Width / 4);
            headerLabel.Text = "Моя визитка";
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            createBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);

            if (databaseMethods.userExists())
                enterBn.Hidden = true;
        }

        void call_premium_option_menu(bool show_restricion = false)
        {
            string[] constraintItems = new string[] { "Подробнее о Premium" };

            if (!databaseMethods.userExists())
                constraintItems = new string[] { "Подробнее о Premium", "Войти в учетную запись" };
            var option_const = new UIActionSheet(null, null, "Отменить", null, constraintItems);

            if (show_restricion)
                option_const.Title = "Запрещена работа на нескольких устройствах";
            else
                option_const.Title = "Достигнут лимит визиток для текущей подписки";
            option_const.Clicked += (btn_sender, args) =>
            {
                if (args.ButtonIndex == 0)
                {
                    NavigationController.PushViewController(sb.InstantiateViewController(nameof(PremiumViewController)), true);
                }
                if (!databaseMethods.userExists())
                    if (args.ButtonIndex == 1)
                    {
                        NavigationController.PushViewController(sb.InstantiateViewController(nameof(EmailViewControllerNew)), true);
                    }
            };
            option_const.ShowInView(View);
        }
    }
}