using System;
using System.Drawing;
using System.Threading.Tasks;
using CardsIOS.NativeClasses;
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
    public partial class ConfirmEmailViewControllerNew : UIViewController
    {
        public static string email_value;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        AccountActions accountActions = new AccountActions();
        UIStoryboard storyboard = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        Methods methods = new Methods();

        public ConfirmEmailViewControllerNew(IntPtr handle) : base(handle)
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
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(storyboard.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }
            nextBn.TouchUpInside += async (s, e) =>
            {
                if (String.IsNullOrEmpty(EmailTextField.Text))
                {
                    UIAlertView alert_empty = new UIAlertView()
                    {
                        Title = "Введите email"
                    };
                    alert_empty.AddButton("OK");
                    alert_empty.Show();
                }
                else
                {
                    try
                    {
                        ConfirmEmailViewControllerNew.email_value = methods.EmailValidation(EmailTextField.Text);

                        activityIndicator.Hidden = false;
                        nextBn.Hidden = true;
                        var deviceName = UIDevice.CurrentDevice.Name;//IdentifierForVendor.ToString();
                        var UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
                        string res = null;
                        try
                        {
                            res = await accountActions.AccountVerification(deviceName, EmailTextField.Text.ToLower(), UDID);
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
                        Analytics.TrackEvent($"{deviceName} {res}");

                        activityIndicator.Hidden = true;
                        nextBn.Hidden = false;
                        string error_message = "";
                        UIAlertView alert = new UIAlertView()
                        {
                            Title = "Ошибка",
                            Message = "Что-то пошло не так."
                        };
                        if (res.ToLower().Contains(Constants.alreadyDone.ToLower()))
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
                        if (res.Contains("SubscriptionConstraint") || String.IsNullOrEmpty(res))
                        {
                            error_message = "_";
                            var vc = storyboard.InstantiateViewController(nameof(EmailAlreadyRegisteredViewController));
                            this.NavigationController.PushViewController(vc, true);
                        }
                        else if (res.Contains("The Email field is not a valid e-mail address"))
                            error_message = "Неверный формат почты";
                        if (!String.IsNullOrEmpty(error_message))
                        {
                            if (!error_message.Contains("_"))
                            {
                                alert = new UIAlertView()
                                {
                                    Title = "Ошибка",
                                    Message = error_message
                                };
                                alert.AddButton("OK");
                                alert.Show();
                            }
                        }
                        if (res.Contains("actionJwt"))
                        {
                            var deserialized_value = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                            databaseMethods.InsertActionJwt(deserialized_value.actionJwt);
                            Analytics.TrackEvent($"{"actionJwt:"} {deserialized_value.actionJwt}");
                            EmailViewControllerNew.actionToken = deserialized_value.actionToken;
                            EmailViewControllerNew.repeatAfter = deserialized_value.repeatAfter;
                            EmailViewControllerNew.validTill = deserialized_value.validTill;
                            databaseMethods.InsertValidTillRepeatAfter(EmailViewControllerNew.validTill, EmailViewControllerNew.repeatAfter, ConfirmEmailViewControllerNew.email_value);
                            var vc = storyboard.InstantiateViewController(nameof(WaitingEmailConfirmViewController));
                            this.NavigationController.PushViewController(vc, true);
                        }
                    }
                    catch
                    {
                        UIAlertView alert_empty = new UIAlertView()
                        {
                            Title = "Email некорректен"
                        };
                        alert_empty.AddButton("OK");
                        alert_empty.Show();
                    }
                }
            };
            EmailTextField.EditingChanged += (s, e) =>
            {
                email_value = EmailTextField.Text;
            };

            email_value = databaseMethods.GetEmailFromValidTill_RepeatAfter();
            var timer = new System.Timers.Timer();
            timer.Interval = 400;
            timer.Elapsed += delegate
            {
                timer.Stop();
                timer.Dispose();
                InvokeOnMainThread(() =>
                {
                    if (!String.IsNullOrEmpty(email_value))
                    {
                        EmailTextField.BecomeFirstResponder();
                        EmailTextField.Text = email_value;
                    }
                });
            };
            timer.Start();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            var timer = new System.Timers.Timer();
            timer.Interval = 50;

            timer.Elapsed += delegate
            {
                timer.Stop();
                timer.Dispose();
                InvokeOnMainThread(() =>
                {
                    if (!String.IsNullOrEmpty(email_value))
                    {
                        EmailTextField.BecomeFirstResponder();
                        EmailTextField.Text = email_value;
                    }
                });
            };
            timer.Start();
        }
        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;

            backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);

            nextBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);

            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            //var d = cardsLogo.Frame.X;
            mainTextTV.Text = "Подтверждение email";
            //mainTextTV.Font = mainTextTV.Font.WithSize(22f);
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            infoLabel.Text = "Подтвердите email для продолжения." +
                "\r\n" + "Он может потребоваться для входа" +
                "\r\n" + "в приложение, в случае его переустановки" +
                "\r\n" + "или замены устройства";
            
            //infoLabel.Lines = 5;

            nextBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            nextBn.SetTitle("Продолжить", UIControlState.Normal);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            //activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(nextBn.Frame.Y + nextBn.Frame.Height / 2 - View.Frame.Width / 20), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            activityIndicator.Hidden = true;

            EmailTextField.TranslatesAutoresizingMaskIntoConstraints = false;
            EmailTextField.Placeholder = "Email";
            EmailTextField.TextColor = UIColor.White;
            EmailTextField.ReturnKeyType = UIReturnKeyType.Done;
            EmailTextField.ShouldReturn = _ => View.EndEditing(true);
        }
    }
}