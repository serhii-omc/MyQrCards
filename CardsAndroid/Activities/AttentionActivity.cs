using System;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
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
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class AttentionActivity : Activity
    {
        TextView _mainTextTv, _infoTv;
        Button _acceptBn, _cancelBn;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        CardsPCL.CommonMethods.Methods _methods = new CardsPCL.CommonMethods.Methods();
        AccountActions _accountActions = new AccountActions();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        ProgressBar _activityIndicator;
        public static bool CancelPurge;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Attention);
            InitElements();

            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            CardsCreatingProcessActivity.CameFrom = Constants.attention_purge;
        }

        async Task<bool> AcceptBn_TouchUpInside(object sender, EventArgs e)
        {
            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return true;
            }

            _activityIndicator.Visibility = ViewStates.Visible;
            _acceptBn.Visibility = ViewStates.Gone;
            _cancelBn.Visibility = ViewStates.Gone;

            var deviceId = NativeMethods.GetDeviceId();
            var clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            string res = null;
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
                    return false;
                }
            }
            Analytics.TrackEvent($"{"ActionJwt:"} {res}");
            _activityIndicator.Visibility = ViewStates.Gone;
            _acceptBn.Visibility = ViewStates.Visible;
            _cancelBn.Visibility = ViewStates.Visible;
            if (res.Contains("actionJwt"))
            {
                var deserializedValue = JsonConvert.DeserializeObject<AccountVerificationModel>(res);
                Analytics.TrackEvent($"{"ActionJwt:"} {deserializedValue.actionJwt}");
                _databaseMethods.InsertActionJwt(deserializedValue.actionJwt);
                EmailActivity.ActionToken = deserializedValue.actionToken;
                EmailActivity.RepeatAfter = deserializedValue.repeatAfter;
                EmailActivity.ValidTill = deserializedValue.validTill;
                _databaseMethods.InsertValidTillRepeatAfter(EmailActivity.ValidTill, EmailActivity.RepeatAfter, ConfirmEmailActivity.EmailValue);
                WaitingEmailConfirmActivity.CameFromPurge = true;
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

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _acceptBn = FindViewById<Button>(Resource.Id.acceptBn);
            _cancelBn = FindViewById<Button>(Resource.Id.cancelBn);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _activityIndicator.Visibility = ViewStates.Gone;
            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();
            _mainTextTv.Text = TranslationHelper.GetString("attention", _ci);
            _infoTv.Text = TranslationHelper.GetString("ifYouContinueANewAccountWillBeCreated", _ci);
            _acceptBn.Text = TranslationHelper.GetString("confirm", _ci);
            _cancelBn.Text = TranslationHelper.GetString("cancel", _ci);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _acceptBn.SetTypeface(tf, TypefaceStyle.Normal);
            _cancelBn.SetTypeface(tf, TypefaceStyle.Normal);
            _acceptBn.Click += async (s, e) => await AcceptBn_TouchUpInside(s, e);
            _cancelBn.Click += (s, e) =>
            {
                CancelPurge = true;
                OnBackPressed();
            };
        }
    }
}
