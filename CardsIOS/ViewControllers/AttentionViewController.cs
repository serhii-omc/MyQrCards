using System;
using System.Drawing;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using Foundation;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using UIKit;

namespace CardsIOS
{
    public partial class AttentionViewController : UIViewController
    {
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        AccountActions accountActions = new AccountActions();
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        Methods methods = new Methods();
        public static UINavigationController alreadyRegisteredViewController;
        public AttentionViewController(IntPtr handle) : base(handle)
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

            if (methods.IsConnected())
            {
                acceptBn.TouchUpInside += AcceptBn_TouchUpInside;
                cancelBn.TouchUpInside += (s, e) =>
                  {
                      NavigationController.PopViewController(false);
                      alreadyRegisteredViewController.PopViewController(true);
                  };
            }
            else
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            CardsCreatingProcessViewController.came_from = Constants.attention_purge;
        }

        void AcceptBn_TouchUpInside(object sender, EventArgs e)
        {
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }

            activityIndicator.Hidden = false;
            acceptBn.Hidden = true;
            cancelBn.Hidden = true;
            InvokeInBackground(async () =>
            {
                var deviceName = UIDevice.CurrentDevice.Name;//IdentifierForVendor.ToString();
                var UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
                string res = null;
                try
                {
                    res = await accountActions.AccountPurge(deviceName, ConfirmEmailViewControllerNew.email_value, UDID);
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
                Analytics.TrackEvent($"{deviceName} {res}");
                InvokeOnMainThread(() =>
                {
                    activityIndicator.Hidden = true;
                    acceptBn.Hidden = false;
                    cancelBn.Hidden = false;
                    if (res.Contains("actionJwt"))
                    {
                        var deserialized_value = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                        databaseMethods.InsertActionJwt(deserialized_value.actionJwt);
                        Analytics.TrackEvent($"{"actionJwt:"} {deserialized_value.actionJwt}");
                        EmailViewControllerNew.actionToken = deserialized_value.actionToken;
                        EmailViewControllerNew.repeatAfter = deserialized_value.repeatAfter;
                        EmailViewControllerNew.validTill = deserialized_value.validTill;
                        databaseMethods.InsertValidTillRepeatAfter(EmailViewControllerNew.validTill, EmailViewControllerNew.repeatAfter, ConfirmEmailViewControllerNew.email_value);
                        WaitingEmailConfirmViewController.cameFromPurge = true;
                        var vc = sb.InstantiateViewController(nameof(WaitingEmailConfirmViewController));
                        this.NavigationController.PushViewController(vc, true);
                    }
                    else
                    {
                        UIAlertView alert = new UIAlertView()
                        {
                            Title = "Ошибка",
                            Message = "Что-то пошло не так."
                        };
                        if (res.Contains(Constants.alreadyDone))
                        {
                            var possibleRepeat = TimeZone.CurrentTimeZone.ToLocalTime(databaseMethods.GetRepeatAfter());
                            var hour = possibleRepeat.Hour.ToString();
                            var minute = possibleRepeat.Minute.ToString();
                            var second = possibleRepeat.Second.ToString();
                            if (hour.Length < 2)
                                hour = "0" + hour;
                            if (minute.Length < 2)
                                minute = "0" + minute;
                            if (second.Length < 2)
                                second = "0" + second;
                            alert.Message = "Запрос был выполнен ранее. Следующий можно будет выполнить после "
                            + hour + ":" + minute + ":" + second;
                            alert.AddButton("OK");
                            alert.Show();
                            return;
                        }
                        alert.AddButton("OK");
                        alert.Show();
                    }
                });
            });
        }

        private void InitElements()
        {

            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            cancelBn.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
            cancelBn.Layer.BorderWidth = 1f;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            if (deviceModel.Contains("X"))
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            else
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            cardsLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(cardsLogo.Frame.X) + Convert.ToInt32(View.Frame.Width) / 3) + 35, Convert.ToInt32(View.Frame.Width), 26);
            mainTextTV.Text = "Внимание!";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);

            infoLabel.Text = "Если вы продолжите, будет создана новая" + "\r\n" + "учетная запись и предыдущие визитные" + "\r\n" + "карточки заменятся новой";
            infoLabel.Lines = 4;
            if (deviceModel.ToLower().Contains("e 5") || deviceModel.ToLower().Contains("e 4") || deviceModel.ToLower().Contains("se"))
                infoLabel.Font = UIFont.FromName(Constants.fira_sans, 14f);
            else
                infoLabel.Font = UIFont.FromName(Constants.fira_sans, 17f);
            infoLabel.Frame = new Rectangle(0, Convert.ToInt32(mainTextTV.Frame.Y + mainTextTV.Frame.Height), Convert.ToInt32(View.Frame.Width), 100);
            acceptBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 8,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            acceptBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            acceptBn.SetTitle("ПОДТВЕРДИТЬ", UIControlState.Normal);
            cancelBn.SetTitle("ОТМЕНА", UIControlState.Normal);
            cancelBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                           (int)(acceptBn.Frame.Y + acceptBn.Frame.Height + 5),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            cancelBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            acceptBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            activityIndicator.Hidden = true;
        }
    }
}