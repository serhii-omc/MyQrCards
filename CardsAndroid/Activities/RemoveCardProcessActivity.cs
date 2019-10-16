
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class RemoveCardProcessActivity : Activity
    {
        public static int? CardId;
        TextView _mainTextTv;
        ProgressBar _activityIndicator;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        Methods _methods = new Methods();
        Cards _cards = new Cards();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        string clientName;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            SetContentView(Resource.Layout.LoadingLayout);

            InitElements();

            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }
            HttpResponseMessage res = null;
            try
            {
                res = await _cards.CardDelete(_databaseMethods.GetAccessJwt(), Convert.ToInt32(CardId), clientName);
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            if (res == null)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            NativeMethods.ResetSocialNetworkList();


            if (res.StatusCode.ToString().Contains("401") || res.StatusCode.ToString().ToLower().Contains(Constants.status_code401))
            {
                RunOnUiThread(() =>
                {
                    ShowSeveralDevicesRestriction();
                    return;
                });
                return;
            }
            CardId = null;
            RunOnUiThread(() =>
            {
                _databaseMethods.InsertLastCloudSync(DateTime.Now);
                OnBackPressed();
            });

            //backBn.TouchUpInside += (s, e) =>
            //{
            //    this.NavigationController.PopViewController(true);
            //};
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _mainTextTv.Text = TranslationHelper.GetString("cardIsDeleting", _ci);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
        }

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out(this);
            MyCardActivity.DeviceRestricted = true;
            Intent intent = new Intent(this, typeof(MyCardActivity));
            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
            StartActivity(intent);
        }
    }
}
