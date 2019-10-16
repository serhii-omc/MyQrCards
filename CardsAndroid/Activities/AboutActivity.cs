using System.Globalization;

using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text.Method;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class AboutActivity : Activity
    {
        TextView _headerTv, _licenseTv, _versionNumberTv;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.about);

            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _licenseTv = FindViewById<TextView>(Resource.Id.licenseTV);
            _versionNumberTv = FindViewById<TextView>(Resource.Id.versionNumberTV);
            _headerTv.Text = TranslationHelper.GetString("aboutApp", _ci);
            _licenseTv.Text = TranslationHelper.GetString("aboutAppText", _ci);
            _licenseTv.MovementMethod = new ScrollingMovementMethod();
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _licenseTv.SetTypeface(tf, TypefaceStyle.Normal);
            _versionNumberTv.SetTypeface(tf, TypefaceStyle.Normal);
            _versionNumberTv.Text = "Версия " + Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName;
        }
    }
}
