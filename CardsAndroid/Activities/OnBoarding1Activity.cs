using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class OnBoarding1Activity : Activity
    {
        TextView _mainTextTv, _infoTv;
        Button _nextBn, _skipBn, _enterBn;
        ImageView _cardsLogoIv;
        RelativeLayout _loginRL;
        CultureInfo _ci = NativeClasses.GetCurrentCulture.GetCurrentCultureInfo();
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.OnBoarding1);
            InitElements();
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _loginRL = FindViewById<RelativeLayout>(Resource.Id.loginRL);
            _nextBn = FindViewById<Button>(Resource.Id.nextBn);
            _skipBn = FindViewById<Button>(Resource.Id.skipBn);
            _enterBn = FindViewById<Button>(Resource.Id.enterBn);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _cardsLogoIv = FindViewById<ImageView>(Resource.Id.cardsLogoIV);
            _mainTextTv.Text = TranslationHelper.GetString("createCards", _ci);
            _infoTv.Text = TranslationHelper.GetString("fillPersonal", _ci)
                        + "\r\n" + TranslationHelper.GetString("andCorporativeData", _ci)
                        + "\r\n" + TranslationHelper.GetString("addCompanyLogo", _ci);
            _nextBn.Text = TranslationHelper.GetString("next", _ci);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _nextBn.SetTypeface(tf, TypefaceStyle.Normal);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _skipBn.SetTypeface(tf, TypefaceStyle.Normal);

            _nextBn.Click += NextBn_Click;
            _skipBn.Click += (s, e) => StartActivity(typeof(MyCardActivity));
            _enterBn.Click += (s, e) => StartActivity(typeof(EmailActivity));

            _skipBn.Visibility = ViewStates.Gone;
        }

        void NextBn_Click(object sender, EventArgs e)
        {
            if (_mainTextTv.Text == TranslationHelper.GetString("createCards", _ci))
            {
                _mainTextTv.Text = TranslationHelper.GetString("shareWithPartners", _ci);
                _infoTv.Text = TranslationHelper.GetString("proposeYourPartner", _ci)
                    + "\r\n" + TranslationHelper.GetString("scanQR", _ci)
                    + "\r\n" + TranslationHelper.GetString("andSaveContactInfo", _ci);
                _cardsLogoIv.SetBackgroundResource(Resource.Drawable.onBoard2Logo);
                _skipBn.Visibility = ViewStates.Visible;
                _loginRL.Visibility = ViewStates.Gone;
            }
            else if (_mainTextTv.Text == TranslationHelper.GetString("shareWithPartners", _ci))
            {
                _mainTextTv.Text = TranslationHelper.GetString("orderStickers", _ci);
                _infoTv.Text = TranslationHelper.GetString("shareQR", _ci)
                    + "\r\n" + TranslationHelper.GetString("asFromApp", _ci)
                    + "\r\n" + TranslationHelper.GetString("fromSpecialQRSticker", _ci);
                _cardsLogoIv.SetBackgroundResource(Resource.Drawable.onBoard3Logo);
                _skipBn.Visibility = ViewStates.Gone;
            }
            else if (_mainTextTv.Text == TranslationHelper.GetString("orderStickers", _ci))
            {
                Intent intent = new Intent(this, typeof(MyCardActivity));
                //intent.AddFlags(ActivityFlags.ClearTask);
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
                Finish();
                //StartActivity(typeof(MyCardActivity));
            }
        }
    }
}
