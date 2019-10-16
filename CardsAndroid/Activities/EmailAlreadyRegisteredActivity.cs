using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EmailAlreadyRegisteredActivity : Activity
    {
        TextView _mainTextTv, _infoTv;
        Button _nextBn, _premiumBn;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AttentionActivity.CancelPurge = false;

            SetContentView(Resource.Layout.EmailAlreadyRegistered);
            InitElements();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (AttentionActivity.CancelPurge)
                OnBackPressed();
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _nextBn = FindViewById<Button>(Resource.Id.nextBn);
            _premiumBn = FindViewById<Button>(Resource.Id.premiumBn);
            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();
            _mainTextTv.Text = TranslationHelper.GetString("thisEmailIsAlreadyRegisteredInTheSystem", _ci);
            _infoTv.Text = TranslationHelper.GetString("usingTheAppOnMultDevicesIsAvailByPremSubs", _ci);
            _nextBn.Text = TranslationHelper.GetString("next", _ci);
            _premiumBn.Text = TranslationHelper.GetString("moreAboutPremium", _ci);
            _nextBn.Click += (s, e) => StartActivity(typeof(AttentionActivity));
            _premiumBn.Click += (s, e) => StartActivity(typeof(PremiumActivity));
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _nextBn.SetTypeface(tf, TypefaceStyle.Normal);
            _premiumBn.SetTypeface(tf, TypefaceStyle.Normal);
        }
    }
}
