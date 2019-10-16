
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
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class PremiumSplashActivity : Activity
    {
        ImageView _cardsLogoIv;
        TextView _mainTextTv, _infoTv;
        Button _detailsBn, _thanksBn;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.EmailAlreadyRegistered);
            InitElements();
            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();
            _detailsBn.Click += (s, e) => StartActivity(typeof(PremiumActivity));
            _thanksBn.Click += (s, e) => OnBackPressed();
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _cardsLogoIv = FindViewById<ImageView>(Resource.Id.cardsLogoIV);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _thanksBn = FindViewById<Button>(Resource.Id.nextBn);
            _detailsBn = FindViewById<Button>(Resource.Id.premiumBn);
            _cardsLogoIv.SetImageResource(Resource.Drawable.premium_logo);
            _mainTextTv.Text = TranslationHelper.GetString("availableForPremium", _ci) + "!";
            _infoTv.Text = TranslationHelper.GetString("toCreateSecondAndSubSequentCards", _ci);
            _thanksBn.Text = TranslationHelper.GetString("thanks", _ci);
            _detailsBn.Text = TranslationHelper.GetString("moreAboutPremium", _ci);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _thanksBn.SetTypeface(tf, TypefaceStyle.Normal);
            _detailsBn.SetTypeface(tf, TypefaceStyle.Normal);
            _thanksBn.SetTypeface(tf, TypefaceStyle.Normal);
        }
    }
}
