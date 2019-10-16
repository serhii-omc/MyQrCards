using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
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
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class EmailActivity : Activity
    {
        EditText _emailEt;
        Button _nextBn;
        TextView _mainTextTv, _infoTv;
        AccountActions _accountActions = new AccountActions();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        Methods _methods = new Methods();
        public static string ActionToken { get; set; }
        public static DateTime RepeatAfter { get; set; }
        public static DateTime ValidTill { get; set; }
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        ProgressBar _activityIndicator;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Analytics.TrackEvent("Garbadge collection");
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Email);

            InitElements();

            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }
            _nextBn.Click += async (s, e) => await NextBnTouch();
            //{
            //    var res = await accountActions.AccountVerification(NativeMethods.GetDeviceId(), emailET.Text.ToLower());
            //};
        }

        private async Task<bool> NextBnTouch()
        {
            if (String.IsNullOrEmpty(_emailEt.Text))
                Toast.MakeText(this, TranslationHelper.GetString("enterEmail", _ci), ToastLength.Short).Show();
            else
            {
                try
                {
                    ConfirmEmailActivity.EmailValue = _methods.EmailValidation(_emailEt.Text);
                    _activityIndicator.Visibility = ViewStates.Visible;
                    _nextBn.Visibility = ViewStates.Gone;
                    var clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
                    string res = null;
                    try
                    {
                        res = await _accountActions.AccountVerification(clientName, _emailEt.Text.ToLower(), Guid.NewGuid().ToString()/*NativeMethods.GetDeviceId()*/);
                    }
                    catch (Exception ex)
                    {
                        if (!_methods.IsConnected())
                        {
                            NoConnectionActivity.ActivityName = this;
                            StartActivity(typeof(NoConnectionActivity));
                            Finish();
                            return false;
                        }
                    }
                    Analytics.TrackEvent($"{clientName} {res}");
                    if (String.IsNullOrEmpty(res))
                    {
                        if (!_methods.IsConnected())
                        {
                            NoConnectionActivity.ActivityName = this;
                            StartActivity(typeof(NoConnectionActivity));
                            Finish();
                            return false;
                        }
                    }
                    _activityIndicator.Visibility = ViewStates.Gone;
                    _nextBn.Visibility = ViewStates.Visible;
                    string errorMessage = "";
                    if (res.Contains(Constants.alreadyDone))
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
                            return false;
                        }
                    }
                    if (res.Contains(Constants.SubscriptionConstraint) || String.IsNullOrEmpty(res))
                    {
                        errorMessage = "_";
                        StartActivity(typeof(EmailAlreadyRegisteredActivity));
                    }
                    else if (res.Contains(Constants.emailFieldNotValid))
                        errorMessage = TranslationHelper.GetString("incorrectEmailFormat", _ci);
                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        if (!errorMessage.Contains("_"))
                            Toast.MakeText(this, errorMessage, ToastLength.Short).Show();
                    }
                    if (res.Contains(Constants.actionJwt))
                    {
                        var deserializedValue = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                        _databaseMethods.InsertActionJwt(deserializedValue.actionJwt);
                        Analytics.TrackEvent($"{"ActionJwt: "} {deserializedValue.actionJwt}");
                        ActionToken = deserializedValue.actionToken;
                        RepeatAfter = deserializedValue.repeatAfter;
                        ValidTill = deserializedValue.validTill;
                        _databaseMethods.InsertValidTillRepeatAfter(ValidTill, RepeatAfter, ConfirmEmailActivity.EmailValue);
                        StartActivity(typeof(WaitingEmailConfirmActivity));
                    }
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, TranslationHelper.GetString("incorrectEmail", _ci), ToastLength.Short).Show();
                    _activityIndicator.Visibility = ViewStates.Gone;
                    _nextBn.Visibility = ViewStates.Visible;
                }
            }
            return true;
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _nextBn = FindViewById<Button>(Resource.Id.nextBn);
            _emailEt = FindViewById<EditText>(Resource.Id.emailET);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _mainTextTv.Text = TranslationHelper.GetString("specifyEmail", _ci);
            _infoTv.Text = TranslationHelper.GetString("weSendYouLink", _ci)
                        + "\r\n" + TranslationHelper.GetString("forAppEntrance", _ci);
            _nextBn.Text = TranslationHelper.GetString("enter", _ci);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _nextBn.SetTypeface(tf, TypefaceStyle.Normal);
            _emailEt.SetTypeface(tf, TypefaceStyle.Normal);
        }
    }
}
