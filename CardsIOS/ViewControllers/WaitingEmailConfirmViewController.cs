using System;
using System.Drawing;
using System.Threading;
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
    public partial class WaitingEmailConfirmViewController : UIViewController
    {
        System.Timers.Timer timer;
        int minutes = 1;
        int seconds = 60;
        int hours = 1;
        int days = 1;
        public static bool cameFromPurge { get; set; }
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        AccountActions accountActions = new AccountActions();
        Methods methods = new Methods();
        CardsPCL.CommonMethods.Accounts accounts = new CardsPCL.CommonMethods.Accounts();
        static UINavigationController thisNavController;
        string UDID;

        public WaitingEmailConfirmViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();

            thisNavController = this.NavigationController;

            InitElements();

            LaunchTimer();

            resendBn.TouchUpInside += async (s, e) => await ResendBn_TouchUpInside(s, e);
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            CardsCreatingProcessViewController.came_from = Constants.email_confirmation_waiting;
            if (!methods.IsConnected())
            {
                PushNoConnection();
                return;
            }
            InvokeInBackground(async () =>
            {
                AccountActions.cycledRequestCancelled = false;
                string res = "";
                try
                {
                    var actionJwt = databaseMethods.GetActionJwt();
                    if (String.IsNullOrEmpty(actionJwt))
                        InvokeOnMainThread(() =>
                        {
                            PopThis();
                            return;
                        });
                    else
                        res = await accountActions.AccountActionsGet(actionJwt, UDID);
                }
                catch
                {
                    if (!methods.IsConnected())
                        InvokeOnMainThread(() =>
                        {
                            PushNoConnection();
                            return;
                        });
                    return;
                }
                InvokeOnMainThread(() =>
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(res))
                            if (res.Contains("processed"))
                            {
                                var deserialized_get = JsonConvert.DeserializeObject<AccountActionsGetModel>(res);
                                InvokeInBackground(async () =>
                                {
                                    string res_auth = null;
                                    try
                                    {
                                        res_auth = await accounts.AccountAuthorize(deserialized_get.accountClientJwt, UDID);
                                    }
                                    catch
                                    {
                                        if (!methods.IsConnected())
                                            InvokeOnMainThread(() =>
                                            {
                                                PushNoConnection();
                                                return;
                                            });
                                        return;
                                    }
                                    AuthorizeRootObject deserialized = null;
                                    if (res_auth != null)
                                        deserialized = JsonConvert.DeserializeObject<AuthorizeRootObject>(res_auth);
                                    else
                                    {
                                        InvokeOnMainThread(() => { PopThis(); return; });
                                        return;
                                    }
                                    if (deserialized == null)
                                    {
                                        InvokeOnMainThread(() => { PopThis(); return; });
                                        return;
                                    }
                                    if (deserialized.subscriptions == null)
                                    {
                                        InvokeOnMainThread(() => { PopThis(); return; });
                                        return;
                                    }

                                    int lastSubscription = deserialized.subscriptions[deserialized.subscriptions.Count - 1].id;
                                    databaseMethods.InsertLoginAfter(deserialized.accessJwt, deserialized.accountUrl, lastSubscription);
                                    try
                                    {
                                        foreach (var subs in deserialized.subscriptions)
                                        {
                                            if (subs.limitations != null)
                                                if (subs.limitations.allowMultiClients)
                                                {
                                                    QRViewController.is_premium = true;
                                                    break;
                                                }
                                        }
                                        //if (!is_premium)
                                        foreach (var subscription in deserialized.subscriptions)
                                        {
                                            if (subscription.limitations != null)
                                                if (subscription.limitations?.cardsRemaining == null)
                                                {
                                                    QRViewController.cards_remaining = 10;
                                                    break;
                                                }
                                                else
                                                {
                                                    if (subscription.limitations != null)
                                                        if (subscription.limitations?.cardsRemaining != null)
                                                            if (subscription.limitations?.cardsRemaining > QRViewController.cards_remaining)
                                                                QRViewController.cards_remaining = subscription.limitations.cardsRemaining.Value;
                                                }
                                        }
                                        foreach (var subscription in deserialized.subscriptions)
                                        {
                                            NativeMethods.ClearFeatures();
                                            if (subscription.features != null)
                                            {
                                                foreach (var feature in subscription.features)
                                                {
                                                    if (feature == Constants.ExtraEmploymentData)
                                                        QRViewController.ExtraEmploymentData = feature;
                                                    if (feature == Constants.CompanyLogoInQr)
                                                        QRViewController.CompanyLogoInQr = feature;
                                                    if (feature == Constants.ExtraPersonData)
                                                        QRViewController.ExtraPersonData = feature;
                                                }
                                            }
                                            else
                                            {

                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    if (!String.IsNullOrEmpty(res_auth))
                                        if (!res_auth.Contains("userTermsAccepted"))
                                        {
                                            var UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
                                            //var UDID = UIDevice.CurrentDevice.Name;
                                            string res_apply_terms = null;
                                            try
                                            {
                                                res_apply_terms = await accounts.ApplyUserTerms(deserialized.accessJwt, deserialized_get.processed, UDID);
                                            }
                                            catch
                                            {
                                                if (!methods.IsConnected())
                                                    InvokeOnMainThread(() =>
                                                    {
                                                        PushNoConnection();
                                                        return;
                                                    });
                                                return;
                                            }
                                            if (String.IsNullOrEmpty(res_apply_terms))
                                            {
                                                InvokeOnMainThread(() => { thisNavController?.PopViewController(true); return; });
                                                return;
                                            }
                                            if (res_apply_terms.ToLower().Contains(": 20") || res_apply_terms.ToLower().Contains("accepted"))
                                            {
                                                if (databaseMethods.GetLoginedFrom() == Constants.from_card_creating)
                                                {
                                                    InvokeOnMainThread(() =>
                                                    {
                                                        try
                                                        {
                                                            thisNavController.PushViewController(sb.InstantiateViewController(nameof(CardsCreatingProcessViewController)), true);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Analytics.TrackEvent($"{"218 push nav controller exception"}{ex}");
                                                        }
                                                    });
                                                }
                                                else if (databaseMethods.GetLoginedFrom() == Constants.from_slide_menu)
                                                {
                                                    InvokeOnMainThread(() =>
                                                    {
                                                        try
                                                        {
                                                            thisNavController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Analytics.TrackEvent($"{"232 push nav controller exception"}{ex}");
                                                        }
                                                    });
                                                }
                                                else if (databaseMethods.GetLoginedFrom() == Constants.from_card_creating_premium)
                                                {
                                                    InvokeOnMainThread(() => { thisNavController.PushViewController(sb.InstantiateViewController(nameof(PersonalDataViewControllerNew)), true); });
                                                }
                                                else
                                                {
                                                    InvokeOnMainThread(() =>
                                                    {
                                                        try
                                                        {
                                                            if (!QRViewController.is_premium)
                                                                thisNavController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
                                                            else
                                                                thisNavController.PushViewController(sb.InstantiateViewController(nameof(RootQRViewController)), true);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Analytics.TrackEvent($"{"251 or 251 push nav controller exception"}{ex}");
                                                        }
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (databaseMethods.GetLoginedFrom() == Constants.from_card_creating)
                                            {
                                                InvokeOnMainThread(() =>
                                                {
                                                    try
                                                    {
                                                        thisNavController.PushViewController(sb.InstantiateViewController(nameof(CardsCreatingProcessViewController)), true);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Analytics.TrackEvent($"{"271 push nav controller exception"}{ex}");
                                                    }
                                                });
                                            }
                                            else if (databaseMethods.GetLoginedFrom() == Constants.from_slide_menu)
                                            {
                                                InvokeOnMainThread(() =>
                                                    {
                                                        try
                                                        {
                                                            if (!QRViewController.is_premium)
                                                                thisNavController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
                                                            else
                                                                thisNavController.PushViewController(sb.InstantiateViewController(nameof(RootQRViewController)), true);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Analytics.TrackEvent($"{"286 or 288 push nav controller exception"}{ex}");
                                                        }
                                                    });
                                            }
                                            else if (databaseMethods.GetLoginedFrom() == Constants.from_card_creating_premium)
                                            {
                                                InvokeOnMainThread(() => { thisNavController.PushViewController(sb.InstantiateViewController(nameof(PersonalDataViewControllerNew)), true); });
                                            }
                                            else
                                            {
                                                InvokeOnMainThread(() =>
                                                {
                                                    try
                                                    {
                                                        if (!QRViewController.is_premium)
                                                            thisNavController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
                                                        else
                                                            thisNavController.PushViewController(sb.InstantiateViewController(nameof(RootQRViewController)), true);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Analytics.TrackEvent($"{"307 or 309 push nav controller exception"}{ex}");
                                                    }
                                                });
                                            }
                                        }
                                });
                            }
                    }
                    catch (Exception ex)
                    {
                        Analytics.TrackEvent($"{"ViewWillAppear:"}{ex}");
                    }
                });
            });
        }

        private void PopThis()
        {
            thisNavController.PopViewController(true);
        }

        private void PushNoConnection()
        {
            NoConnectionViewController.view_controller_name = GetType().Name;
            thisNavController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
        }

        async Task<bool> ResendBn_TouchUpInside(object sender, EventArgs e)
        {
            if (!methods.IsConnected())
            {
                PushNoConnection();
                return false;
            }

            if (!cameFromPurge)
            {
                await ResendPremium();
                return true;
            }

            activityIndicator.Hidden = false;
            resendBn.Hidden = true;
            var emailText = emailLabel.Text;
            InvokeInBackground(async () =>
            {
                var deviceName = UIDevice.CurrentDevice.Name;//IdentifierForVendor.ToString();
                string res = "";
                try
                {
                    res = await accountActions.AccountPurge(deviceName, emailText, UDID);
                }
                catch (Exception ex)
                {
                    if (!methods.IsConnected())
                        InvokeOnMainThread(() =>
                        {
                            PushNoConnection();
                            return;
                        });
                    InvokeOnMainThread(() =>
                    {
                        resendBn.Hidden = false;
                        activityIndicator.Hidden = true;
                    });
                }
                Analytics.TrackEvent($"{deviceName} {res}");
                InvokeOnMainThread(() =>
                {
                    activityIndicator.Hidden = true;
                    resendBn.Hidden = false;
                    if (res.Contains("actionJwt"))
                    {
                        var deserialized_value = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                        databaseMethods.InsertActionJwt(deserialized_value.actionJwt);
                        Analytics.TrackEvent($"{"actionJwt:"} {deserialized_value.actionJwt}");
                        EmailViewControllerNew.actionToken = deserialized_value.actionToken;
                        EmailViewControllerNew.repeatAfter = deserialized_value.repeatAfter.AddSeconds(30);
                        EmailViewControllerNew.validTill = deserialized_value.validTill;
                        databaseMethods.InsertValidTillRepeatAfter(EmailViewControllerNew.validTill, EmailViewControllerNew.repeatAfter, ConfirmEmailViewControllerNew.email_value);
                        var vc = sb.InstantiateViewController(nameof(WaitingEmailConfirmViewController));
                        thisNavController.PushViewController(vc, true);
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
            return true;
        }
        async Task<bool> ResendPremium()
        {
            AccountActions.cycledRequestCancelled = true;
            activityIndicator.Hidden = false;
            resendBn.Hidden = true;
            var deviceName = UIDevice.CurrentDevice.Name;//IdentifierForVendor.ToString();
            var UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
            if (!methods.IsConnected())
            {
                activityIndicator.Hidden = true;
                resendBn.Hidden = false;
                return false;
            }
            InvokeInBackground(async () =>
            {
                string res = "";
                try
                {
                    res = await accountActions.AccountVerification(deviceName, databaseMethods.GetEmailFromValidTill_RepeatAfter(), UDID);
                }
                catch
                {
                    if (!methods.IsConnected())
                        InvokeOnMainThread(() =>
                        {
                            PushNoConnection();
                            return;
                        });
                    return;
                }
                InvokeOnMainThread(() =>
                {
                    activityIndicator.Hidden = true;
                    resendBn.Hidden = false;
                    if (res.Contains("actionJwt"))
                    {
                        var deserialized_value = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                        databaseMethods.InsertActionJwt(deserialized_value.actionJwt);
                        EmailViewControllerNew.actionToken = deserialized_value.actionToken;
                        EmailViewControllerNew.repeatAfter = deserialized_value.repeatAfter;
                        EmailViewControllerNew.validTill = deserialized_value.validTill;
                        databaseMethods.InsertValidTillRepeatAfter(EmailViewControllerNew.validTill, EmailViewControllerNew.repeatAfter, ConfirmEmailViewControllerNew.email_value);
                        //ViewDidLoad();
                        //ViewDidAppear(false);
                        var vc = sb.InstantiateViewController(nameof(WaitingEmailConfirmViewController));
                        thisNavController.PushViewController(vc, true);
                    }
                    if (res.ToLower().Contains("ыполнено ранее"))
                    {
                        UIAlertView alert = new UIAlertView()
                        {
                            Title = "Ошибка",
                            Message = "Время повторного выполнения запроса ещё не наступило. Попробуйте повторить позже."
                        };
                        alert.AddButton("OK");
                        alert.Show();
                    }
                });
            });
            return true;
        }

        private void LaunchTimer()
        {
            var repeatAfter = databaseMethods.GetRepeatAfter();
            var now = DateTime.UtcNow;
            var time = repeatAfter.TimeOfDay;
            var date = repeatAfter.Date.ToString().Split(' ')[0];//.Substring(0, 10);
            var repeat_after = date + " " + time;
            var repeat_after_new = Convert.ToDateTime(repeat_after/*, CultureInfo.InvariantCulture*/);
            var res_time = repeat_after_new - now;
            seconds = res_time.Seconds;
            minutes = res_time.Minutes;
            //hours = res_time.Hours;
            days = res_time.Days;
            hours = days * 24 + res_time.Hours;

            timer = new System.Timers.Timer();
            timer.Interval = 1000;

            timer.Elapsed += delegate
            {
                var diff = DateTime.Compare(repeat_after_new, DateTime.UtcNow);
                var diff1 = DateTime.Compare(DateTime.UtcNow, repeat_after_new);
                if (diff1 == -1 || diff == 0)
                {
                    InvokeOnMainThread(() =>
                    {
                        //resendBn.Enabled = true;
                        //resendBn.SetTitleColor(UIColor.FromRGB(255, 99, 62), UIControlState.Normal);
                    });
                }
                seconds--;
                if (seconds == 0 && minutes >= 0)
                {
                    minutes--;
                    seconds = 60;
                }
                if (minutes == 60)
                {
                    InvokeOnMainThread(() => { timer_valueLabel.Text = $"{hours}:00:00"; });
                }
                //this construction used here to display timer value correctly
                if (seconds == 60)
                {
                    if (minutes < 10)
                        InvokeOnMainThread(() => { timer_valueLabel.Text = $"{hours}:0{minutes}:00"; });
                    else
                        InvokeOnMainThread(() => { timer_valueLabel.Text = $"{hours}:{minutes}:00"; });
                }
                else if (seconds < 10)
                {
                    if (minutes < 10)
                        InvokeOnMainThread(() => { timer_valueLabel.Text = $"{hours}:0{minutes}:0{seconds}"; });
                    else
                        InvokeOnMainThread(() => { timer_valueLabel.Text = $"{hours}:{minutes}:0{seconds}"; });
                }
                else
                {
                    if (minutes < 10)
                        InvokeOnMainThread(() => { timer_valueLabel.Text = $"{hours}:0{minutes}:{seconds}"; });
                    else
                        InvokeOnMainThread(() => { timer_valueLabel.Text = $"{hours}:{minutes}:{seconds}"; });
                }
                if (minutes == 0 && seconds == 1)
                {
                    hours--;
                    seconds = 60;
                    minutes = 60;
                    if (hours < 0)
                    {
                        timer.Stop();
                        InvokeOnMainThread(() =>
                        {
                            Thread.Sleep(1000);
                            timer_valueLabel.Text = "00:00";
                            resendBn.Enabled = true;
                            resendBn.SetTitleColor(UIColor.FromRGB(255, 99, 62), UIControlState.Normal);
                        });
                    }
                }
            };
            InvokeInBackground(() => timer.Start());
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(resendBn.Frame.Y + resendBn.Frame.Height / 2 - View.Frame.Width / 20), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            activityIndicator.Hidden = true;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            //image_bgIV.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));
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
            backBn.TouchUpInside += (s, e) =>
            {
                PopThis();
            };
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            emailLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 5,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.X) + Convert.ToInt32(emailLogo.Frame.Height)), Convert.ToInt32(View.Frame.Width), 26);
            //var d = cardsLogo.Frame.X;
            emailLabel.TextColor = UIColor.FromRGB(255, 99, 62);
            mainTextTV.Text = "Ожидание подтверждения";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            infoLabel1.Text = "На ваш электронный адрес";
            infoLabel1.Frame = new Rectangle(0, Convert.ToInt32(mainTextTV.Frame.Y + mainTextTV.Frame.Height), Convert.ToInt32(View.Frame.Width), 29);
            emailLabel.Text = ConfirmEmailViewControllerNew.email_value;

            if (!deviceModel.Contains("e 5") && !deviceModel.Contains("e 4") && !deviceModel.ToLower().Contains("e se"))
            {
                emailLabel.Frame = new Rectangle(0, Convert.ToInt32(infoLabel1.Frame.Y + infoLabel1.Frame.Height), Convert.ToInt32(View.Frame.Width), 29);

                infoLabel2.Frame = new Rectangle(0, Convert.ToInt32(emailLabel.Frame.Y), Convert.ToInt32(View.Frame.Width), 130);
                infoLabel2.Text = "отправлено письмо. Перейдите по ссылке" +
                "\r\n" + "из полученного письма. Если письмо не" +
                    "\r\n" + "пришло, проверьте папку \u00ABСпам\u00BB";
                infoLabel2.Lines = 5;
                timer_main_label.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Height) / 10 * 7), Convert.ToInt32(View.Frame.Width), 29);
            }
            else
            {
                emailLabel.Frame = new Rectangle(0, Convert.ToInt32(infoLabel1.Frame.Y + 20), Convert.ToInt32(View.Frame.Width), 29);
                infoLabel2.Frame = new Rectangle(0, Convert.ToInt32(emailLabel.Frame.Y + 10), Convert.ToInt32(View.Frame.Width), 130);
                infoLabel2.Text =
                    "отправлено письмо." + "\r\n" +
                    "Перейдите по ссылке" + "\r\n" +
                    "из полученного письма." + "\r\n" +
                    "Если письмо не пришло," + "\r\n" +
                    "проверьте папку \u00ABСпам\u00BB";
                infoLabel2.Lines = 5;
                //timer_main_label.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Height) / 10 * 8), Convert.ToInt32(View.Frame.Width), 29);
            }

            resendBn.Font = UIFont.FromName(Constants.fira_sans, 13f);
            resendBn.SetTitle("ОТПРАВИТЬ ССЫЛКУ ПОВТОРНО", UIControlState.Normal);
            //resendBn.SetTitleColor(UIColor.FromRGB(255,99,62), UIControlState.Normal);

            timer_main_label.TextAlignment = UITextAlignment.Center;
            timer_main_label.Text = "Повторно отправить запрос\nможно будет через:";
            timer_main_label.Lines = 2;
            //timer_main_label.SizeToFit();
            timer_main_label.Frame = new CoreGraphics.CGRect(0/* - timer_main_label.Frame.Width)/2*/, (Convert.ToInt32(View.Frame.Height) / 10 * 7.5), View.Frame.Width, /*timer_main_label.Frame.Height*/60);
            timerBgIV.Frame = new Rectangle((int)(View.Frame.Width / 7 * 3), (int)(timer_main_label.Frame.Y + timer_main_label.Frame.Height), (int)(View.Frame.Width / 5), (int)(View.Frame.Width / 14));
            timer_valueLabel.Frame = new Rectangle((int)(timerBgIV.Frame.X), (int)(timerBgIV.Frame.Y), (int)(timerBgIV.Frame.Width), (int)(timerBgIV.Frame.Height));
            timer_valueLabel.Font = UIFont.FromName(Constants.fira_sans, 13f);

            resendBn.Enabled = false;
            resendBn.Frame = new Rectangle(0,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 9,
                                         Convert.ToInt32(View.Frame.Width),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(resendBn.Frame.Y + resendBn.Frame.Height / 2 - View.Frame.Width / 20), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            activityIndicator.Hidden = true;
        }
    }
}