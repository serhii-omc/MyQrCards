
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
using CardsPCL.Database;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class CloudSyncPremiumActivity : Activity
    {
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        TextView _headerTv, _lastSyncTv, _lastSyncValueTv;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.cloud_sync_premium);

            InitElements();
            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => base.OnBackPressed();
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _lastSyncTv = FindViewById<TextView>(Resource.Id.lastSyncTV);
            _lastSyncValueTv = FindViewById<TextView>(Resource.Id.lastSyncValueTV);

            _headerTv.Text = TranslationHelper.GetString("cloudSync", _ci);
            _lastSyncTv.Text = TranslationHelper.GetString("lastSync", _ci);
            var lastSyncValue = _databaseMethods.GetLastCloudSync().ToString();
            if (!String.IsNullOrEmpty(lastSyncValue))
                _lastSyncValueTv.Text = lastSyncValue.Replace('/', '.');
            else
                _lastSyncValueTv.Text = TranslationHelper.GetString("notExecuted", _ci);

            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _lastSyncTv.SetTypeface(tf, TypefaceStyle.Normal);
            _lastSyncValueTv.SetTypeface(tf, TypefaceStyle.Normal);
        }
    }
}
