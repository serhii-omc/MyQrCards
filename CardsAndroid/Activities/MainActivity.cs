using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using CardsPCL;
using CardsPCL.Database;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using VKontakte.Utils;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true, MainLauncher = true)]
    public class MainActivity : Activity
    {
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        System.Timers.Timer _timer;

        protected override async void OnResume()
        {
            base.OnResume();

            SetContentView(Resource.Layout.Main);

            //InputStream input = Assets.Open("my_asset.txt");

            if (!IsTaskRoot)
            { // Don't start the app again from icon on launcher.
                Intent intent = Intent;
                String intentAction = intent.Action;
                if (intent.HasCategory(Intent.CategoryLauncher) && intentAction != null && intentAction.Equals(Intent.ActionMain))
                {
                    Finish();
                    return;
                }
            }

            AppCenter.Start(Constants.appCenterSecretDroid, typeof(Analytics), typeof(Crashes));
            AppCenter.Start(Constants.appCenterSecretDroid, typeof(Distribute));
            await Distribute.SetEnabledAsync(true);
            bool enabled = await Distribute.IsEnabledAsync();

            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new System.Timers.Timer();
            _timer.Interval = 150;
            _timer.Elapsed += delegate
            {
                _timer.Stop();
                _timer.Dispose();
                RunOnUiThread(() =>
                {
                    if (!_databaseMethods.UserExists())
                        StartActivity(typeof(OnBoarding1Activity));
                    else
                        StartActivity(typeof(QrActivity));
                });
                _timer.Stop();
                _timer.Dispose();
            };
            _timer.Start();
        }
    }
}

