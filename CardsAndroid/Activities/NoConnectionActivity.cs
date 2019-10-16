using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, NoHistory = true)]
    public class NoConnectionActivity : Activity
    {
        System.Timers.Timer _connectionWaitingTimer;
        Methods _methods = new Methods();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        TextView _infoTv;
        ImageButton _reconnectBn;
        Button _exitBn;
        public static Activity ActivityName;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.NoConnection);

            InitElements();

            _exitBn.Click += (s, e) =>
              {
                  CallExitMenu();
              };
        }

        private void CallExitMenu()
        {
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("exitWithoutSavingData", _ci));
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("confirm", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
                Intent intent;
                if (_databaseMethods.UserExists() && _databaseMethods.GetCardNames()?.Count > 0)
                    intent = new Intent(this, typeof(QrActivity));
                else
                    intent = new Intent(this, typeof(MyCardActivity));
                intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                StartActivity(intent);
            });
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        protected override void OnResume()
        {
            base.OnResume();
            LaunchConnectionWaitingTimer();
        }

        protected override void OnPause()
        {
            base.OnPause();
            try
            {
                _connectionWaitingTimer.Stop();
                _connectionWaitingTimer.Dispose();
            }
            catch { }
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _reconnectBn = FindViewById<ImageButton>(Resource.Id.reconnectBn);
            _exitBn = FindViewById<Button>(Resource.Id.exitBn);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _infoTv.Text = TranslationHelper.GetString("connectionRequired", _ci)
                        + "\r\n" + TranslationHelper.GetString("turnInternetOn", _ci);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _exitBn.Text = TranslationHelper.GetString("mainScreen", _ci);
            _exitBn.SetTypeface(tf, TypefaceStyle.Normal);
            _reconnectBn.Click += (s, e) => StartActivity(new Intent(this, ActivityName.Class));
        }

        private void LaunchConnectionWaitingTimer()
        {
            _connectionWaitingTimer = new System.Timers.Timer();
            _connectionWaitingTimer.Interval = 1000;

            _connectionWaitingTimer.Elapsed += delegate
            {
                _connectionWaitingTimer.Interval = 1000;
                if (_methods.IsConnected())
                {
                    StartActivity(new Intent(this, ActivityName.Class));
                    _connectionWaitingTimer.Stop();
                    _connectionWaitingTimer.Dispose();
                }
            };
            _connectionWaitingTimer.Start();
        }

        public override void OnBackPressed()
        {
            CallExitMenu();
        }
    }
}
