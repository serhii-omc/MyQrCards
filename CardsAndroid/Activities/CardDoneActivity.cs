
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
using CardsPCL.Models;
using Newtonsoft.Json;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait/*, MainLauncher = true*/)]
    public class CardDoneActivity : Activity
    {
        public static int CardId;
        TextView _mainTextTv, _infoTv;
        Button _readyBn, _watchInWebBn;
        RelativeLayout _headerRl;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        Methods _methods = new Methods();
        Cards _cards = new Cards();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        string clientName;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.EmailAlreadyRegistered);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            InitElements();

            _readyBn.Click += (s, e) =>
            {
                if (_methods.IsConnected())
                {
                    Intent intent = new Intent(this, typeof(QrActivity));
                    intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                    StartActivity(intent);
                    //StartActivity(typeof(QRActivity));
                    QrActivity.CurrentPosition = 1;
                }
                else
                {
                    QrActivity.ShowWeHaveNotDownloadedCard = true;
                    Intent intent = new Intent(this, typeof(QrActivity));
                    intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                    StartActivity(intent);
                    //StartActivity(typeof(QRActivity));
                }
            };
            _watchInWebBn.Click += async (s, e) => await ShowInWebClick(s, e);
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _readyBn = FindViewById<Button>(Resource.Id.premiumBn);
            _watchInWebBn = FindViewById<Button>(Resource.Id.nextBn);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _headerRl = FindViewById<RelativeLayout>(Resource.Id.headerRL);
            _headerRl.Visibility = ViewStates.Gone;
            _mainTextTv.Text = TranslationHelper.GetString("cardReady", _ci);
            _infoTv.Text = TranslationHelper.GetString("offerYourPartnerScanQR", _ci);
            _readyBn.Text = TranslationHelper.GetString("ready", _ci);
            _watchInWebBn.Text = TranslationHelper.GetString("watchInWeb", _ci);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _readyBn.SetTypeface(tf, TypefaceStyle.Normal);
            _watchInWebBn.SetTypeface(tf, TypefaceStyle.Normal);
        }

        async Task<bool> ShowInWebClick(object sender, EventArgs e)
        {
            if (CardId == 0)
                return false;
            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return false;
            }
            string resCardData = null;
            try
            {
                resCardData = await _cards.CardDataGet(_databaseMethods.GetAccessJwt(), CardId, clientName);
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
            if (String.IsNullOrEmpty(resCardData))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return false;
                }
            }
            if (/*res_card_data == Constants.status_code409 || */ resCardData == Constants.status_code401)
            {
                ShowSeveralDevicesRestriction();
                return false;
            }
            var desCardData = JsonConvert.DeserializeObject<CardsDataModel>(resCardData);
            var uri = Android.Net.Uri.Parse(desCardData.url);
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
            return true;
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
