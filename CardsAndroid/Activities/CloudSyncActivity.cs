using System.Globalization;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class CloudSyncActivity : Activity
    {
        TextView _headerTv, _mainTextTv, _infoTv;
        Button _detailsBn;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.cloud_sync);
            InitElements();

            _detailsBn.Click += (s, e) => StartActivity(typeof(PremiumActivity));
            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => base.OnBackPressed();
        }
        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _detailsBn = FindViewById<Button>(Resource.Id.detailsBn);
            _mainTextTv.Text = TranslationHelper.GetString("availableForPremium", _ci);
            _headerTv.Text = TranslationHelper.GetString("cloudSync", _ci);
            _detailsBn.Text = TranslationHelper.GetString("moreAboutPremium", _ci);
            _infoTv.Text = TranslationHelper.GetString("forStoringDataInTheCloud", _ci);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _detailsBn.SetTypeface(tf, TypefaceStyle.Normal);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
        }
    }
}
