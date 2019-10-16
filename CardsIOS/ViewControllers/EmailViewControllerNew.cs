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
    public partial class EmailViewControllerNew : UIViewController
    {
        AccountActions accountActions = new AccountActions();
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        Methods methods = new Methods();
        //public static string actionJwt { get; set; }
        public static string actionToken { get; set; }
        public static DateTime repeatAfter { get; set; }
        public static DateTime validTill { get; set; }

        public EmailViewControllerNew(IntPtr handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Analytics.TrackEvent("Dispose in action");
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();

            nextBn.TouchUpInside += async (s, e) => await nextBnTouch();
        }

        private async Task<bool> nextBnTouch()
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
                    var UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
                    var deviceName = UIDevice.CurrentDevice.Name;
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
                                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                                return;
                            });
                        return false;
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
                    if (res.Contains(Constants.alreadyDone))
                    {
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
                            return false;
                        }
                    }
                    if (res.Contains(Constants.SubscriptionConstraint) || String.IsNullOrEmpty(res))
                    {
                        error_message = "_";
                        var vc = sb.InstantiateViewController(nameof(EmailAlreadyRegisteredViewController));
                        this.NavigationController.PushViewController(vc, true);
                    }
                    else if (res.Contains(Constants.emailFieldNotValid))
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
                    if (res.Contains(Constants.actionJwt))
                    {
                        var deserialized_value = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                        databaseMethods.InsertActionJwt(deserialized_value.actionJwt);
                        Analytics.TrackEvent($"{"actionJwt:"} {deserialized_value.actionJwt}");
                        actionToken = deserialized_value.actionToken;
                        repeatAfter = deserialized_value.repeatAfter.AddSeconds(30);
                        validTill = deserialized_value.validTill;
                        databaseMethods.InsertValidTillRepeatAfter(validTill, repeatAfter, ConfirmEmailViewControllerNew.email_value);
                        var vc = sb.InstantiateViewController(nameof(WaitingEmailConfirmViewController));
                        this.NavigationController.PushViewController(vc, true);
                    }
                }
                catch (Exception ex)
                {
                    UIAlertView alert_empty = new UIAlertView()
                    {
                        Title = "Email некорректен"
                    };
                    alert_empty.AddButton("OK");
                    alert_empty.Show();
                }

            }
            return true;
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            backBn.TouchUpInside += (s, e) => this.NavigationController.PopViewController(true);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3F, backBn.Frame.Width / 2.5F, backBn.Frame.Height / 3F, backBn.Frame.Width / 2.5F);

            mainTextTV.Text = "Укажите email";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            infoLabel.Text = "Мы отправим вам ссылку" + "\r\n" + "для входа в приложение";

            EmailTextField.Placeholder = "Email";
            EmailTextField.TextColor = UIColor.White;
            EmailTextField.ReturnKeyType = UIReturnKeyType.Done;
            EmailTextField.ShouldReturn = _ => EmailTextField.EndEditing(true);

            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);

            nextBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            nextBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            nextBn.SetTitle("Продолжить", UIControlState.Normal);

            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Hidden = true;
        }
    }
}