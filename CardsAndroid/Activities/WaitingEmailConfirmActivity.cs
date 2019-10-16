using System;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;
using CardsPCL.Models;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait/*, NoHistory = true*/)]
    public class WaitingEmailConfirmActivity : Activity
    {
        public static bool CameFromPurge { get; set; }
        System.Timers.Timer _timer;
        int _minutes = 1;
        int _seconds = 60;
        int _days = 1;
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        AccountActions _accountActions = new AccountActions();
        CardsPCL.CommonMethods.Methods _methods = new CardsPCL.CommonMethods.Methods();
        Accounts _accounts = new Accounts();
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        TextView _mainTextTv, _infoTv, _timerMainTv, _timerValueTv, _emailTv, _email2Tv;
        ProgressBar _activityIndicator;
        Button _resendBn;
        string clientName;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            SetContentView(Resource.Layout.WaitingEmailConfirm);

            InitElements();

            LaunchTimer();

            //resendBn.TouchUpInside += ResendBn_TouchUpInside;
        }

        private void LaunchTimer()
        {
            _resendBn.Enabled = false;
            _resendBn.SetTextColor(this.Resources.GetColor(Resource.Color.editTextLineColor));
            var repeatAfter = _databaseMethods.GetRepeatAfter();
            //var now_string = string.Format("{0}/{1}/{2}", DateTime.UtcNow.Date.Month, DateTime.UtcNow.Date.Day, DateTime.UtcNow.Date.Year);
            //var now = Convert.ToDateTime(now_string, CultureInfo.InvariantCulture);
            var time = repeatAfter.TimeOfDay;
            var date = string.Format("{0}/{1}/{2}", repeatAfter.Date.Month, repeatAfter.Date.Day, repeatAfter.Date.Year);
            var repAfter = date + " " + time;
            var repAfterNew = Convert.ToDateTime(repAfter, CultureInfo.InvariantCulture);
            var resTime = repAfterNew - DateTime.UtcNow;//now;
            _seconds = resTime.Seconds;
            _minutes = resTime.Minutes;
            //hours = res_time.Hours;
            _days = resTime.Days;
            //_hours = _days * 24 + resTime.Hours;

            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            var repeatAfter_ = _databaseMethods.GetRepeatAfter();

            //Toast.MakeText(this, $"repeat after: {repeatAfter_}\nutc now: {DateTime.UtcNow}", ToastLength.Long).Show();
            //Toast.MakeText(this, $"minutes {_minutes} seconds {_seconds}", ToastLength.Long).Show();

            _timer.Elapsed += delegate
            {
                //var diff = DateTime.Compare(repeatAfter_, DateTime.UtcNow);
                //var diff1 = DateTime.Compare(DateTime.UtcNow, repeatAfter_);

                //Toast.MakeText(this, $"diff {diff.ToString()}\ndiff1 {diff1.ToString()}", ToastLength.Long).Show();

                //if (diff1 == -1 || diff == 0)
                //{
                //    RunOnUiThread(() =>
                //    {
                //        //resendBn.SetTitleColor(UIColor.FromRGB(255, 99, 62), UIControlState.Normal);
                //    });
                //}
                Console.WriteLine("MAKAKA inside timer");
                if (_seconds <= 0)
                    _seconds = 60;
                _seconds--;
                if (_seconds == 0 && _minutes >= 0)
                {
                    _minutes--;
                    _seconds = 60;
                }
                if (_minutes == 60)
                {
                    RunOnUiThread(() => { _timerValueTv.Text = $"00:00"; });
                }
                //this construction used here to display timer value correctly
                if (_seconds == 60)
                {
                    if (_minutes < 10)
                        RunOnUiThread(() => { _timerValueTv.Text = $"0{_minutes}:00"; });
                    else
                        RunOnUiThread(() => { _timerValueTv.Text = $"{_minutes}:00"; });
                }
                else if (_seconds < 10)
                {
                    if (_minutes < 10)
                        RunOnUiThread(() => { _timerValueTv.Text = $"0{_minutes}:0{_seconds}"; });
                    else
                        RunOnUiThread(() => { _timerValueTv.Text = $"{_minutes}:0{_seconds}"; });
                }
                else
                {
                    if (_minutes < 10)
                        RunOnUiThread(() => { _timerValueTv.Text = $"0{_minutes}:{_seconds}"; });
                    else
                        RunOnUiThread(() => { _timerValueTv.Text = $"{_minutes}:{_seconds}"; });
                }
                if ((_minutes <= 0 && _seconds <= 0) || _minutes < 0 || _seconds < 0 /*; || _hours < 0 */)
                {
                    Console.WriteLine($"MAKAKA minutes {_minutes}");
                    Console.WriteLine($"MAKAKA seconds {_seconds}");
                    //if (_minutes <= 0 && _seconds <= 0)
                    {
                        //_seconds = 00;
                        _timer.Stop();
                        Console.WriteLine("MAKAKA timer stopped");
                        RunOnUiThread(() =>
                        {
                            //Thread.Sleep(1000);
                            _timerValueTv.Text = "00:00";
                            _resendBn.Enabled = true;
                            _resendBn.SetTextColor(this.Resources.GetColor(Resource.Color.buttonOrangeColor));
                            //resendBn.SetTitleColor(UIColor.FromRGB(255, 99, 62), UIControlState.Normal);
                        });
                    }
                    //_minutes = 60;
                }
            };
            _timer.Start();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            CardsCreatingProcessActivity.CameFrom = Constants.email_confirmation_waiting;
            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                Analytics.TrackEvent("147 finish waiting email");
                return;
            }

            AccountActions.cycledRequestCancelled = false;
            string res = null;
            try
            {
                res = await _accountActions.AccountActionsGet(_databaseMethods.GetActionJwt(), clientName);
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    Analytics.TrackEvent("164 finish waiting email");
                    return;
                }
            }
            if (String.IsNullOrEmpty(res))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    Analytics.TrackEvent("175 finish waiting email");
                    return;
                }
            }
            if (res.Contains("processed"))
            {
                var deserializedGet = JsonConvert.DeserializeObject<AccountActionsGetModel>(res);
                string resAuth = null;
                try
                {
                    resAuth = await _accounts.AccountAuthorize(deserializedGet.accountClientJwt, clientName);
                }
                catch (Exception ex)
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        Analytics.TrackEvent("194 finish waiting email");
                        return;
                    }
                }
                if (String.IsNullOrEmpty(resAuth))
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        Analytics.TrackEvent("205 finish waiting email");
                        return;
                    }
                }
                var deserialized = JsonConvert.DeserializeObject<AuthorizeRootObject>(resAuth);
                int lastSubscription = deserialized.subscriptions[deserialized.subscriptions.Count - 1].id;
                _databaseMethods.InsertLoginAfter(deserialized.accessJwt, deserialized.accountUrl, lastSubscription);
                try
                {
                    foreach (var subs in deserialized.subscriptions)
                    {
                        if (subs.limitations != null)
                            if (subs.limitations.allowMultiClients)
                            {
                                QrActivity.IsPremium = true;
                                break;
                            }
                    }
                    //if (!is_premium)
                    foreach (var subscription in deserialized.subscriptions)
                    {
                        if (subscription.limitations != null)
                            if (subscription.limitations?.cardsRemaining == null)
                            {
                                QrActivity.CardsRemaining = 10;
                                break;
                            }
                            else
                            if (subscription.limitations.cardsRemaining > QrActivity.CardsRemaining)
                                QrActivity.CardsRemaining = subscription.limitations.cardsRemaining.Value;
                    }
                    foreach (var subscription in deserialized.subscriptions)
                    {
                        if (subscription.features != null)
                        {
                            foreach (var feature in subscription.features)
                            {
                                if (feature == Constants.ExtraEmploymentData)
                                    QrActivity.ExtraEmploymentData = feature;
                                if (feature == Constants.CompanyLogoInQr)
                                    QrActivity.CompanyLogoInQr = feature;
                                if (feature == Constants.ExtraPersonData)
                                    QrActivity.ExtraPersonData = feature;
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
                if (!resAuth.Contains("userTermsAccepted"))
                {
                    var deviceId = NativeMethods.GetDeviceId();
                    string resApplyTerms = null;
                    try
                    {
                        resApplyTerms = await _accounts.ApplyUserTerms(deserialized.accessJwt, deserializedGet.processed, deviceId);
                    }
                    catch (Exception ex)
                    {
                        if (!_methods.IsConnected())
                        {
                            NoConnectionActivity.ActivityName = this;
                            StartActivity(typeof(NoConnectionActivity));
                            Finish();
                            Analytics.TrackEvent("274 finish waiting email");
                            return;
                        }
                    }
                    if (String.IsNullOrEmpty(resApplyTerms))
                    {
                        if (!_methods.IsConnected())
                        {
                            NoConnectionActivity.ActivityName = this;
                            StartActivity(typeof(NoConnectionActivity));
                            Finish();
                            Analytics.TrackEvent("285 finish waiting email");
                            return;
                        }
                    }
                    if (resApplyTerms.ToLower().Contains(": 20") || resApplyTerms.ToLower().Contains("accepted"))
                    {
                        if (_databaseMethods.GetLoginedFrom() == Constants.from_card_creating)
                        {
                            Intent intent = new Intent(this, typeof(CardsCreatingProcessActivity));
                            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                            StartActivity(intent);
                            //StartActivity(typeof(CardsCreatingProcessActivity));
                            //Finish();
                        }
                        else if (_databaseMethods.GetLoginedFrom() == Constants.from_slide_menu)
                        {
                            Intent intent = new Intent(this, typeof(MyCardActivity));
                            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                            StartActivity(intent);
                            //StartActivity(typeof(MyCardActivity));
                            //Finish();
                        }
                        else if (_databaseMethods.GetLoginedFrom() == Constants.from_card_creating_premium)
                        {
                            Intent intent = new Intent(this, typeof(PersonalDataActivity));
                            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                            StartActivity(intent);
                            //StartActivity(typeof(PersonalDataActivity));
                            //Finish();
                        }
                        else
                        {
                            if (!QrActivity.IsPremium)
                            {
                                Intent intent = new Intent(this, typeof(MyCardActivity));
                                intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                                StartActivity(intent);
                                //StartActivity(typeof(MyCardActivity));
                            }
                            else
                            {
                                Intent intent = new Intent(this, typeof(QrActivity));
                                intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                                StartActivity(intent);
                                //StartActivity(typeof(QRActivity));
                            }
                            Finish();
                        }
                    }
                }
                else
                {
                    if (_databaseMethods.GetLoginedFrom() == Constants.from_card_creating)
                    {
                        Intent intent = new Intent(this, typeof(CardsCreatingProcessActivity));
                        intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                        StartActivity(intent);
                        //StartActivity(typeof(CardsCreatingProcessActivity)); 
                        //Finish();
                    }
                    else if (_databaseMethods.GetLoginedFrom() == Constants.from_slide_menu)
                    {
                        if (!QrActivity.IsPremium)
                        {
                            Intent intent = new Intent(this, typeof(MyCardActivity));
                            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                            StartActivity(intent);
                            //StartActivity(typeof(MyCardActivity));
                        }
                        else
                        {
                            Intent intent = new Intent(this, typeof(QrActivity));
                            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                            StartActivity(intent);
                            //StartActivity(typeof(QRActivity));
                        }
                        Finish();
                    }
                    else if (_databaseMethods.GetLoginedFrom() == Constants.from_card_creating_premium)
                    {
                        Intent intent = new Intent(this, typeof(PersonalDataActivity));
                        intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                        StartActivity(intent);
                        //StartActivity(typeof(PersonalDataActivity));
                        //Finish();
                    }
                    else
                    {
                        if (!QrActivity.IsPremium)
                        {
                            Intent intent = new Intent(this, typeof(MyCardActivity));
                            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                            StartActivity(intent);
                            //StartActivity(typeof(MyCardActivity));
                        }
                        else
                        {
                            Intent intent = new Intent(this, typeof(QrActivity));
                            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                            StartActivity(intent);
                            //StartActivity(typeof(QRActivity));
                        }
                        Finish();
                    }
                }
            }
        }
        async Task<bool> ResendBn_TouchUpInside(object sender, EventArgs e)
        {
            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                Analytics.TrackEvent("346 finish waiting email");
                return true;
            }

            if (!CameFromPurge)
            {
                await ResendPremium();
                return true;
            }

            _activityIndicator.Visibility = ViewStates.Visible;
            _resendBn.Visibility = ViewStates.Gone;

            var deviceId = NativeMethods.GetDeviceId();
            var clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            string res = "";
            try
            {
                res = await _accountActions.AccountPurge(clientName, ConfirmEmailActivity.EmailValue, deviceId);
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    Analytics.TrackEvent("373 finish waiting email");
                    return false;
                }
            }
            _resendBn.Visibility = ViewStates.Visible;
            _activityIndicator.Visibility = ViewStates.Gone;

            if (String.IsNullOrEmpty(res))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    Analytics.TrackEvent("387 finish waiting email");
                    return false;
                }
            }
            Analytics.TrackEvent($"{"ActionJwt:"} {res}");
            _activityIndicator.Visibility = ViewStates.Gone;
            _resendBn.Visibility = ViewStates.Visible;
            if (res.Contains("actionJwt"))
            {
                var deserializedValue = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                Analytics.TrackEvent($"{"ActionJwt:"} {deserializedValue.actionJwt}");
                _databaseMethods.InsertActionJwt(deserializedValue.actionJwt);
                EmailActivity.ActionToken = deserializedValue.actionToken;
                EmailActivity.RepeatAfter = deserializedValue.repeatAfter;
                EmailActivity.ValidTill = deserializedValue.validTill;
                _databaseMethods.InsertValidTillRepeatAfter(EmailActivity.ValidTill, EmailActivity.RepeatAfter, ConfirmEmailActivity.EmailValue);
                StartActivity(typeof(WaitingEmailConfirmActivity));
            }
            else
            {
                if (res.Contains(Constants.alreadyDone))
                {
                    var possibleRepeat = TimeZone.CurrentTimeZone.ToLocalTime(_databaseMethods.GetRepeatAfter());
                    var hour = possibleRepeat.Hour.ToString();
                    var minute = possibleRepeat.Minute.ToString();
                    var second = possibleRepeat.Second.ToString();
                    if (hour.Length < 2)
                        hour = "0" + hour;
                    if (minute.Length < 2)
                        minute = "0" + minute;
                    if (second.Length < 2)
                        second = "0" + second;
                    Toast.MakeText(this, TranslationHelper.GetString("requestAlreadyDone", _ci)
                    + hour + ":" + minute + ":" + second, ToastLength.Long).Show();
                    return true;
                }
                Toast.MakeText(this, TranslationHelper.GetString("smthngWentWrong", _ci), ToastLength.Short).Show();
            }

            return true;
        }
        async Task<bool> ResendPremium()
        {
            AccountActions.cycledRequestCancelled = true;
            _activityIndicator.Visibility = ViewStates.Visible;
            _resendBn.Visibility = ViewStates.Gone;
            var clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            string res = null;
            try
            {
                res = await _accountActions.AccountVerification(clientName, _databaseMethods.GetEmailFromValidTill_RepeatAfter(), Guid.NewGuid().ToString()/*NativeMethods.GetDeviceId()*/);
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    Analytics.TrackEvent("446 finish waiting email");
                    return false;
                }
            }
            if (String.IsNullOrEmpty(res))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    Analytics.TrackEvent("457 finish waiting email");
                    return false;
                }
            }
            _activityIndicator.Visibility = ViewStates.Gone;
            _resendBn.Visibility = ViewStates.Visible;
            if (res.Contains("actionJwt"))
            {
                var deserializedValue = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                _databaseMethods.InsertActionJwt(deserializedValue.actionJwt);
                EmailActivity.ActionToken = deserializedValue.actionToken;
                EmailActivity.RepeatAfter = deserializedValue.repeatAfter;
                EmailActivity.ValidTill = deserializedValue.validTill;
                _databaseMethods.InsertValidTillRepeatAfter(EmailActivity.ValidTill, EmailActivity.RepeatAfter, ConfirmEmailActivity.EmailValue);
                //var intent = new Intent(this, typeof(WaitingEmailConfirmActivity));
                //intent.SetFlags(ActivityFlags.NoHistory);
                //intent.SetFlags(ActivityFlags.NewTask);
                StartActivity(typeof(WaitingEmailConfirmActivity));
                return true;
                //Finish();
                //ViewDidAppear(false);
            }
            //if (res.ToLower().Contains("ыполнено ранее"))
            //{
            Toast.MakeText(this, TranslationHelper.GetString("RequestReExecutionTimeHasNotYetCome", _ci), ToastLength.Short).Show();
            //}
            return true;
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _timerMainTv = FindViewById<TextView>(Resource.Id.timerMainTV);
            _timerValueTv = FindViewById<TextView>(Resource.Id.timerValueTV);
            _emailTv = FindViewById<TextView>(Resource.Id.emailTV);
            _email2Tv = FindViewById<TextView>(Resource.Id.email2TV);
            _resendBn = FindViewById<Button>(Resource.Id.resendBn);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _activityIndicator.Visibility = ViewStates.Gone;
            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();
            _mainTextTv.Text = TranslationHelper.GetString("waitingConfirm", _ci);
            _infoTv.Text = TranslationHelper.GetString("onYourEmailAddress", _ci);
            _emailTv.Text = ConfirmEmailActivity.EmailValue;
            _email2Tv.Text = TranslationHelper.GetString("waitingEmailTextInfo", _ci);
            _timerMainTv.Text = TranslationHelper.GetString("requestAfter", _ci);
            _resendBn.Text = TranslationHelper.GetString("sendLinkRepeatedly", _ci);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _emailTv.SetTypeface(tf, TypefaceStyle.Normal);
            _email2Tv.SetTypeface(tf, TypefaceStyle.Normal);
            _timerMainTv.SetTypeface(tf, TypefaceStyle.Normal);
            _resendBn.SetTypeface(tf, TypefaceStyle.Normal);
            _resendBn.Click += async (s, e) =>
            {
                var res = await ResendBn_TouchUpInside(s, e);
            };
        }
    }
}
