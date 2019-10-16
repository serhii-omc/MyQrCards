using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.Database;
using CardsPCL.Localization;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MyCardActivity : AppCompatActivity
    {
        private V7Toolbar _mToolbar;
        private Android.Support.V7.App.ActionBarDrawerToggle _drawerToggle;
        private DrawerLayout _drawerLayout;
        private RelativeLayout _leftDrawer, _plusRl;
        public static bool DeviceRestricted;
        private ArrayAdapter _mLeftAdapter;
        TextView _headerTv, _myCardTv, _orderStickerTv, _cloudSyncTv, _premiumTv, _aboutTv, _enterTv, _mainTextTv, _infoTv;
        Button _nextBn, _enterBn;
        ImageView _enterIv;
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        NativeMethods _nativeMethods = new NativeMethods();
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        //protected override void OnResume()
        //{
        //    SetContentView(Resource.Layout.MyCard);
        //    InitElements();
        //    if (DeviceRestricted)
        //    {
        //        call_premium_option_menu(true);
        //        DeviceRestricted = false;
        //    }
        //}

        protected override void OnResume()
        {
            base.OnResume();
            SetContentView(Resource.Layout.MyCard);
            InitElements();
            if (DeviceRestricted)
            {
                call_premium_option_menu(true);
                DeviceRestricted = false;
            }
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);

            _enterBn = FindViewById<Button>(Resource.Id.enterBn);
            _nextBn = FindViewById<Button>(Resource.Id.nextBn);
            _plusRl = FindViewById<RelativeLayout>(Resource.Id.plusRL);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTv = FindViewById<TextView>(Resource.Id.infoTV);
            _mainTextTv.Text = TranslationHelper.GetString("create_", _ci)
                        + "\r\n" + TranslationHelper.GetString("firstCard", _ci);
            _infoTv.Text = TranslationHelper.GetString("fillOnlyThat", _ci)
                        + "\r\n" + TranslationHelper.GetString("thatYouWantToShow", _ci)
                        + "\r\n" + TranslationHelper.GetString("yourPartners", _ci);
            _nextBn.Text = TranslationHelper.GetString("create", _ci);
            _nextBn.Click += NextBn_Click;
            _enterBn.Click += (s, e) => StartActivity(typeof(EmailActivity));
            _plusRl.Click += PlusBn_TouchUpInside;

            _headerTv.Text = TranslationHelper.GetString("myCard", _ci);

            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _nextBn.SetTypeface(tf, TypefaceStyle.Normal);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            _infoTv.SetTypeface(tf, TypefaceStyle.Normal);

            InitLeftMenu(tf);

            if (_databaseMethods.UserExists())
                _enterBn.Visibility = ViewStates.Gone;
        }

        private void InitLeftMenu(Typeface tf)
        {
            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            _leftDrawer = FindViewById<RelativeLayout>(Resource.Id.left_drawer);
            _myCardTv = FindViewById<TextView>(Resource.Id.myCardTV);
            _orderStickerTv = FindViewById<TextView>(Resource.Id.orderStickerTV);
            _cloudSyncTv = FindViewById<TextView>(Resource.Id.cloudSyncTV);
            _premiumTv = FindViewById<TextView>(Resource.Id.premiumTV);
            _aboutTv = FindViewById<TextView>(Resource.Id.aboutTV);
            _enterTv = FindViewById<TextView>(Resource.Id.enterTV);
            _enterIv = FindViewById<ImageView>(Resource.Id.enterIV);
            _myCardTv.Text = TranslationHelper.GetString("myCard", _ci);
            _orderStickerTv.Text = TranslationHelper.GetString("orderStickerQR", _ci);
            _cloudSyncTv.Text = TranslationHelper.GetString("cloudSync", _ci);
            _premiumTv.Text = TranslationHelper.GetString("premium", _ci);
            _aboutTv.Text = TranslationHelper.GetString("aboutApp", _ci);
            _enterTv.Text = TranslationHelper.GetString("enter", _ci);
            _myCardTv.SetTypeface(tf, TypefaceStyle.Normal);
            _orderStickerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _cloudSyncTv.SetTypeface(tf, TypefaceStyle.Normal);
            _premiumTv.SetTypeface(tf, TypefaceStyle.Normal);
            _aboutTv.SetTypeface(tf, TypefaceStyle.Normal);
            _enterTv.SetTypeface(tf, TypefaceStyle.Normal);
            // Prevent clicking items laying under it.
            _leftDrawer.Click += (s, e) => { };
            if (_databaseMethods.UserExists())
            {
                _enterIv.RotationY = (float)180;
                _enterTv.Text = TranslationHelper.GetString("logout", _ci);
            }
            else
                _enterIv.RotationY = (float)0;
            FindViewById<RelativeLayout>(Resource.Id.enterRL).Click += LoginRlClick;
            FindViewById<RelativeLayout>(Resource.Id.myCardRL).Click += MyCardClick;
            FindViewById<RelativeLayout>(Resource.Id.orderStickerRL).Click += OrderClick;
            FindViewById<RelativeLayout>(Resource.Id.cloudSyncRL).Click += CloudSyncClick;
            FindViewById<RelativeLayout>(Resource.Id.premiumRL).Click += PremiumClick;
            FindViewById<RelativeLayout>(Resource.Id.aboutRL).Click += AboutClick;
            //button to open Left Drawer
            FindViewById<RelativeLayout>(Resource.Id.leftRL).Click += LeftRlClick;
            _drawerToggle = new NativeClasses.ActionBarDrawerToggle(this, _drawerLayout, Resource.String.openDrawer, Resource.String.closeDrawer);
        }

        private void OrderClick(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("https://myqrcards.com/sticker");
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }

        private void AboutClick(object sender, EventArgs e)
        {
            _drawerLayout.CloseDrawer(_leftDrawer);
            StartActivity(typeof(AboutActivity));
        }

        private void PremiumClick(object sender, EventArgs e)
        {
            _drawerLayout.CloseDrawer(_leftDrawer);
            StartActivity(typeof(PremiumActivity));
        }

        private void CloudSyncClick(object sender, EventArgs e)
        {
            _drawerLayout.CloseDrawer(_leftDrawer);
            if (String.IsNullOrEmpty(QrActivity.ExtraPersonData))
                StartActivity(typeof(CloudSyncActivity));
            else
                StartActivity(typeof(CloudSyncPremiumActivity));
        }

        private void MyCardClick(object sender, EventArgs e)
        {
            if (_databaseMethods.UserExists())
            {
                _drawerLayout.CloseDrawer(_leftDrawer);
                StartActivity(typeof(QrActivity));
            }
            else
                _drawerLayout.CloseDrawer(_leftDrawer);
        }

        void PlusBn_TouchUpInside(object sender, EventArgs e)
        {
            if (_databaseMethods.UserExists())
            {
                if (!QrActivity.IsPremium && QrActivity.CardsRemaining == 0)
                    call_premium_option_menu();
                else if (QrActivity.IsPremium && QrActivity.CardsRemaining == 0)
                    Toast.MakeText(this, TranslationHelper.GetString("cardsLimitHasBeenReached", _ci), ToastLength.Short).Show();
                if (QrActivity.CardsRemaining > 0)
                {
                    StartActivity(typeof(CreatingCardActivity));
                }
            }
            else
            {
                StartActivity(typeof(PersonalDataActivity));
            }
        }

        private void LeftRlClick(object sender, EventArgs e)
        {
            if (_drawerLayout.IsDrawerOpen(_leftDrawer))
            {
                _drawerLayout.CloseDrawer(_leftDrawer);
            }
            else
            {
                _drawerLayout.OpenDrawer(_leftDrawer);
            }
        }

        private void LoginRlClick(object sender, EventArgs e)
        {
            if (!_databaseMethods.UserExists())
            {
                _drawerLayout.CloseDrawer(_leftDrawer);
                _databaseMethods.InsertLoginedFrom(Constants.from_slide_menu);
                StartActivity(typeof(EmailActivity));
                //Intent intent = new Intent(this, typeof(EmailActivity));
                //intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                //StartActivity(intent);
                //Finish();
                //Intent intent = new Intent(this, typeof(EmailActivity));
                //intent.AddFlags(ActivityFlags.NewTask);
                //StartActivity(intent);
                //Finish();
            }
            else
                ShowLogOutAlert();
        }

        void call_premium_option_menu(bool showRestricion = false)
        {
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
            //builder.SetTitle(TranslationHelper.GetString("moreAboutPremium", ci));
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            if (!_databaseMethods.UserExists())
            {
                //constraintItems = new string[] { "Подробнее о Premium", "Войти в учетную запись" };
                builder.SetPositiveButton(TranslationHelper.GetString("login", _ci), (object sender1, DialogClickEventArgs e1) =>
                {
                    StartActivity(typeof(EmailActivity));
                });
                builder.SetNeutralButton(TranslationHelper.GetString("premium", _ci), (object sender1, DialogClickEventArgs e1) =>
                {
                    StartActivity(typeof(PremiumActivity));
                });
            }

            if (showRestricion)
                builder.SetMessage(TranslationHelper.GetString("workOnSeveralDevicesRestricted", _ci));
            else
                builder.SetMessage(TranslationHelper.GetString("cardsLimitHasBeenReached", _ci));
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        private void ShowLogOutAlert()
        {
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("logoutRequest", _ci));
            if (!QrActivity.IsPremium)
            {
                builder.SetMessage(TranslationHelper.GetString("logoutNotPremium", _ci));
                builder.SetNeutralButton(TranslationHelper.GetString("premium", _ci), (object sender1, DialogClickEventArgs e1) =>
                {
                    StartActivity(typeof(PremiumActivity));
                });
            }
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("logout", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
                LogOutClass.log_out(this);
                try { _drawerLayout.CloseDrawer(_leftDrawer); } catch { }
                InitElements();
            });
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        private void NextBn_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(PersonalDataActivity));
        }
    }
}