using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ConditionsActivity : Activity
    {
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Conditions);

            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            var headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            var conditionsTv = FindViewById<TextView>(Resource.Id.conditionsTV);
            headerTv.Text = TranslationHelper.GetString("conditionsHeader", _ci);

            headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            conditionsTv.SetTypeface(tf, TypefaceStyle.Normal);

            SpannableString content = new SpannableString(Constants.licenseUrl);
            content.SetSpan(new UnderlineSpan(), 0, content.Length(), 0);
            conditionsTv.SetText(content, TextView.BufferType.Spannable);

            //conditionsTV.Text = "здесь будут условия соглашения здесь будут условия соглашения здесь будут условия соглашения здесь будут условия соглашения здесь будут условия соглашения здесь будут условия соглашения ";

            conditionsTv.Click += (s, e) =>
              {
                  var uri = Android.Net.Uri.Parse(Constants.licenseUrl);
                  var intent = new Intent(Intent.ActionView, uri);
                  StartActivity(intent);
              };

            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();
        }
    }
}
