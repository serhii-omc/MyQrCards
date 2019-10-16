using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using CardsAndroid.Adapters;
using CardsAndroid.Models;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;
using CardsPCL.Models;
using Newtonsoft.Json;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using ZXing;
using ZXing.Mobile;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using Permission = Plugin.Permissions.Abstractions.Permission;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait/*, MainLauncher = true*/)]
    public class QrActivity : AppCompatActivity
    {
        System.Timers.Timer _connectionWaitingTimer, _checkHeaderValueTimer;
        public static int ClickedPosition, CurrentPosition, TintClickedCardId;
        private V7Toolbar _mToolbar;
        private Android.Support.V7.App.ActionBarDrawerToggle _drawerToggle;
        private DrawerLayout _drawerLayout;
        //UIImage logoImageDemonstr;
        //List<CardsIOS.Models.QR_Logo_ID_Model> qR_Logo_ID_List;
        Cards _cards = new Cards();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        Attachments _attachments = new Attachments();
        Companies _companies = new Companies();
        ImageView _arrowsIv, _enterIv;
        static ImageView _demoQrIv, _demoCompanyLogoIv;
        //UIButton QR_button = new UIButton();
        //static UIButton current_qrBn;
        List<QrListModel> _deserializedCardsList;
        List<QrListModel> _reverseList;
        int _globalTag, _cardsCount;
        static RelativeLayout _demonstrationRl, _closeRl;
        RelativeLayout _plusRl, _leftDrawer;
        static RelativeLayout _tintRl;
        Button _editBn, _removeBn, _showInWebBn, _linkQrBn;
        //public static List<string> card_names;
        Methods _methods = new Methods();
        NativeMethods _nativeMethods = new NativeMethods();
        QrAdapter _qRAdapter;
        static LinearLayout _mainLl, _loadingLl;
        Typeface _tf;
        //UIActionSheet option;
        CardsPCL.CommonMethods.Accounts _accounts = new CardsPCL.CommonMethods.Accounts();
        //public static string just_created_card_name { get; set; }
        //public static bool is_premium;
        //public static bool ShowWeHaveNotDownloadedCard;
        bool _showWeHaveNotDownloadedCardScrollDraggingInformer;
        public static string CurrentCardNameHeader { get; set; }
        static string _editDeleteIndicator = "";
        int _index;
        //public static CGPoint content_offset;
        //public SidebarController SidebarController { get; private set; }
        //public SidebarController SideBarController;
        //public UIViewController holderVC;
        IntPtr _handle;
        int _countCached = 0;
        //FileInfo[] cached_files_array;

        TextView _mainTextTv, _myCardTv, _orderStickerTv, _cloudSyncTv, _premiumTv, _aboutTv, _enterTv, _infoTv;
        static TextView _headerTv;
        ProgressBar _activityIndicator;
        static RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;

        public static bool IsPremium;
        public static int CardsRemaining;
        public static string JustCreatedCardName { get; set; }
        public static string TintClickedCardUrl { get; internal set; }

        public static int PersonalCardId;
        public static bool ShowWeHaveNotDownloadedCard;
        public static List<string> CardNames = new List<string>();
        static Activity _context;
        // Features.
        public static string ExtraEmploymentData, CompanyLogoInQr, ExtraPersonData;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        //ScrollRightDetector rightDetector = new ScrollRightDetector();
        //ScrollLeftDetector leftDetector = new ScrollLeftDetector();
        DragDetector _dragDetector = new DragDetector();
        //List<QRListModel> demo_list = new List<QRListModel>();
        double _fullRecyclerWidth;
        int _fullWidthInPx;
        // Width of QrRow.
        int _rowWidthPixels;
        // Margin right of QrRow.
        int _marginRightPixels;
        //private bool we_here;
        string clientName;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            //else
            //nativeMethods.checkStoragePermissions(_context);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            SetContentView(Resource.Layout.QR);

            _removeBn = FindViewById<Button>(Resource.Id.removeBn);
            _editBn = FindViewById<Button>(Resource.Id.editBn);
            _showInWebBn = FindViewById<Button>(Resource.Id.showInWebBn);
            _linkQrBn = FindViewById<Button>(Resource.Id.linkQrBn);
            _plusRl = FindViewById<RelativeLayout>(Resource.Id.plusRL);
            _tintRl = FindViewById<RelativeLayout>(Resource.Id.tintRL);

            _removeBn.Click += delegate
            {
                if (!_methods.IsConnected())
                {
                    CallSnackBar(TranslationHelper.GetString("connectionRequired", _ci));
                    return;
                }
                RemoveBn_TouchUpInside(null, null);
            };
            _editBn.Click += delegate
            {
                if (!_methods.IsConnected())
                {
                    CallSnackBar(TranslationHelper.GetString("connectionRequired", _ci));
                    return;
                }
                StartActivity(typeof(EditActivity));
            };
            _showInWebBn.Click += delegate
            {
                if (!_methods.IsConnected())
                {
                    CallSnackBar(TranslationHelper.GetString("connectionRequired", _ci));
                    return;
                }
                var uri = Android.Net.Uri.Parse(TintClickedCardUrl);
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            };
            _linkQrBn.Click += delegate
             {
                 LinkStickerActivity.CardId = TintClickedCardId;
                 StartActivity(typeof(LinkStickerActivity));
             };
            _plusRl.Click += PlusBn_TouchUpInside;
            _tintRl.Click += (s, e) => _tintRl.Visibility = ViewStates.Gone;
            InitLeftMenu();
        }

        protected override async void OnResume()
        {
            base.OnResume();
            _context = this;
            //we_here = true;
            // TODO this might help to prevent System.InvalidOperationException: Collection was modified
            QrAdapter.QrsList?.Clear();

            _tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            if (!await CheckStoragePermissions(this))
            {
                //Thread.Sleep(2000);
                return;
            }
            InitElements();
            _closeRl.Click += (s, e) => CloseDemoView();

            if (!await FirstCheck())
            {
                //StartNoConnection();
                //Finish();
                return;
            }

            if (!_methods.IsConnected())
            {
                await ShowCachedQRs();
                LaunchConnectionWaitingTimer();
                return;
            }
            //removeBn.Click += RemoveBn_TouchUpInside;
            //editBn.Click += (s, e) => Toast.MakeText(this, "Сделаю когда-нибудь потом", ToastLength.Short).Show();
            //plusRL.Click += PlusBn_TouchUpInside;
            //tintRL.Click += (s, e) => tintRL.Visibility = ViewStates.Gone;

            //await DoOnResumeStuff();
            //await InitElements();
        }

        public async Task<bool> CheckStoragePermissions(Activity context)
        {
            PermissionStatus permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

            if (permissionStatus != PermissionStatus.Granted)
            {
                try
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
                }
                catch (Exception ex)
                {
                    return false;
                }
                //permissionStatus = results[Permission.Storage];
                //RequestRuntimePermissions();
                RequestStoragePermissions();
                return false;
            }
            else
            {
                //Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", ci), ToastLength.Long).Show();
                return true;
            }
        }
        private const int REQUEST_PERMISSION_CODE = 1000;
        public async void RequestStoragePermissions()
        {
            await Task.Delay(1500);
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted
                          || CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                    {
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, REQUEST_PERMISSION_CODE);
                }
                else
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                    {
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, REQUEST_PERMISSION_CODE);
                }
        }

        //bool shown = false;
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case REQUEST_PERMISSION_CODE:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                        {
                            //StartActivity(typeof(MainActivity));
                        }
                        else
                        {
                            Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", _ci), ToastLength.Short).Show();
                        }
                        break;
                    }
            }
        }

        public static void CallSnackBar(string message)
        {
            var snackbar = Snackbar.Make(_headerTv, message, 1500);
            snackbar.View.SetBackgroundColor(Color.ParseColor(_context.GetString(Resource.Color.buttonOrangeColor)));
            snackbar.Show();
        }

        void StartNoConnection()
        {
            NoConnectionActivity.ActivityName = this;
            StartActivity(typeof(NoConnectionActivity));
        }

        private async Task<bool> FirstCheck()
        {
            var cardscountFromDB = _databaseMethods.GetCardNames();

            bool needRedownload = false;
            int countCachedQrs = _nativeMethods.GetCountCachedQrs();
            int countCachedLogos = _nativeMethods.GetCountCachedLogos();

            // Compare count of cached images
            if (!new[] { countCachedQrs, countCachedLogos }.All(x => x == cardscountFromDB.Count))
                needRedownload = true;

            if (cardscountFromDB.Count > 0 && !needRedownload)
            {
                await ShowCachedQRs();

                var taskA = new Task(async () =>
                {
                    await DoOnResumeStuffBackground();
                });
                taskA.Start();
            }
            else
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return false;
                }
                await DoOnResumeStuff();
            }
            return true;
        }

        private void PlusBn_TouchUpInside(object sender, EventArgs e)
        {
            if (!CheckStoragePermissions(this).Result)
                return;
            if (_methods.IsConnected())
            {
                if (!IsPremium && CardsRemaining == 0)
                    StartActivity(typeof(PremiumSplashActivity));
                else if (IsPremium && CardsRemaining == 0)
                    Toast.MakeText(this, TranslationHelper.GetString("cardsLimitHasBeenReached", _ci), ToastLength.Short).Show();
                else
                {
                    //clear_current_card_name_and_pos();
                    StartActivity(typeof(CreatingCardActivity));
                }
            }
            else
            {
                CallSnackBar(TranslationHelper.GetString("connectionRequired", _ci));
                //if (cards_remaining > 0)
                //{
                //    //clear_current_card_name_and_pos();
                //    StartActivity(typeof(CreatingCardActivity));
                //}
                //else
                //{
                //    Toast.MakeText(this, TranslationHelper.GetString("needTurnOnInternetToCheckCardsLimit", ci), ToastLength.Long).Show();

                //    NoConnectionActivity.activityName = this;
                //    StartActivity(typeof(NoConnectionActivity));
                //    Finish();
                //    return;
                //}
            }
        }

        void RemoveBn_TouchUpInside(object sender, EventArgs e)
        {
            //var TaskA = new Task(async () =>
            //{
            //    await nativeMethods.RemoveOfflineCache();
            //    //var QRs_cache_dir = Path.Combine(docs, Constants.QRs_cache_dir);
            //});
            //TaskA.Start();
            //databaseMethods.CleanCardNames();
            RemoveCardProcessActivity.CardId = TintClickedCardId;
            //string[] tableItems = new string[] { "Удалить " + reverseList[global_tag].name + "?" };
            //option = new UIActionSheet(null, null, "Не удалять", null, tableItems);

            //option.Clicked += (btn_sender, args) => //Console.WriteLine("{0} Clicked", args.ButtonIndex);
            //{
            //    if (args.ButtonIndex == 1)
            //    {
            //        removeBn.Hidden = true;
            //        editBn.Hidden = true;
            //        dividerView.Hidden = true;
            //        tintBn.Hidden = true;
            //        option = null;
            //    }
            //    if (args.ButtonIndex == 0)
            //    {

            //    }
            //};
            //if (remove_clicked)
            //{
            //remove_clicked = false;
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("remove", _ci) + " " + QrAdapter.QrsList[CurrentPosition].CardName + "?");
            builder.SetPositiveButton(TranslationHelper.GetString("remove", _ci), async (object sender1, DialogClickEventArgs e1) =>
            {
                //set index to 0 because we do not need to display wrong card name after deleting
                _index = 0;
                CurrentPosition = 0;
                _editDeleteIndicator = Constants.delete;
                clear_current_card_name_and_pos();
                StartActivity(typeof(RemoveCardProcessActivity));
            });
            builder.SetCancelable(true);
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
                _tintRl.Visibility = ViewStates.Gone;
            });
            Android.Support.V7.App.AlertDialog dialog = builder.Create();

            dialog.Show();
            //}
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
                    RunOnUiThread(() =>
                    {
                        CurrentPosition = ((LinearLayoutManager)_recyclerView.GetLayoutManager()).FindFirstCompletelyVisibleItemPosition();
                        Toast.MakeText(this, TranslationHelper.GetString("connectionRestored", _ci), ToastLength.Short).Show();
                        StartActivity(typeof(QrActivity));
                    });
                    _connectionWaitingTimer.Stop();
                    _connectionWaitingTimer.Dispose();
                }
            };
            _connectionWaitingTimer.Start();
        }

        private async Task<bool> ShowCachedQRs()
        {
            CardNames = _databaseMethods.GetCardNames();
            _cardsCount = CardNames.Count();
            if (_cardsCount == 0)
            {
                clear_current_card_name_and_pos();
                StartActivity(typeof(MyCardActivity));
                return true;
            }
            _headerTv.Text = CardNames[0];

            // Insert just created card name.
            if (ShowWeHaveNotDownloadedCard)
                if (!String.IsNullOrEmpty(JustCreatedCardName))
                {
                    _showWeHaveNotDownloadedCardScrollDraggingInformer = true;
                    CardNames.Insert(1, JustCreatedCardName);
                    _cardsCount++;
                }
            //plusRL.Visibility = ViewStates.Gone;
            //QRAdapter.offline = true;
            QrAdapter.QrsList = await _nativeMethods.GetCachedQrsAndLogos();
            SetMainView();

            // TODO this is here for scroll to card that has been edited previously
            if (CurrentPosition >= 0)
            {
                await Task.Delay(50);
                try { _recyclerView.SmoothScrollToPosition(CurrentPosition); } catch { }
            }

            // Need this for slow dragging sticking.
            CalculateDimensions();
            return true;
        }

        // TODO refactor this method
        async Task<bool> DoOnResumeStuffBackground()
        {
            IsPremium = false;
            CardsRemaining = 0;
            //card_names = null;
            _reverseList = null;
            _deserializedCardsList = null;
            _globalTag = 0;
            _cardsCount = 0;

            // TODO
            //await Clear();

            // TODO
            RunOnUiThread(() => _plusRl.Visibility = ViewStates.Gone);
            //await InitElements();

            // clear features
            ExtraEmploymentData = null;
            CompanyLogoInQr = null;
            ExtraPersonData = null;
            _showWeHaveNotDownloadedCardScrollDraggingInformer = false;
            //SetLoadingView();
            if (!_methods.IsConnected())
            {
                RunOnUiThread(() => _plusRl.Visibility = ViewStates.Visible);
                return false;
            }

            // TODO set this here
            var resSync = await SyncCachedCard(/*false*/);
            RunOnUiThread(() => _mainTextTv.Text = TranslationHelper.GetString("dataAcquisition", _ci));
            string cardsRemainingcontent = null;
            try
            {
                cardsRemainingcontent = await _accounts.AccountAuthorize(_databaseMethods.GetAccessJwt(), clientName);//"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NDk1NjEyOTcsIkFjY291bnRDbGllbnRUb2tlbiI6IjVQYUw2Sm9XcWdJIiwiQWNjb3VudElEIjoyMCwiQ2xpZW50SUQiOiJiNTA2OTJlNmNkYTdhYTlkIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.UBoRD61YNYjOu60Wu1bpotRAFvxbMaDzt6PCxQrMtKA");
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                    return false;
            }
            if (String.IsNullOrEmpty(cardsRemainingcontent))
            {
                if (!_methods.IsConnected())
                {
                    // TODO
                    //NoConnectionActivity.activityName = this;
                    //StartActivity(typeof(NoConnectionActivity));
                    //Finish();
                    return false;
                }
            }
            if (/*cardsRemainingcontent == Constants.StatusCode409 || */cardsRemainingcontent == Constants.status_code401)
            {
                RunOnUiThread(() => ShowSeveralDevicesRestriction());
                return false;
            }

            var desAuth = JsonConvert.DeserializeObject<AuthorizeRootObject>(cardsRemainingcontent);
            try
            {
                foreach (var subs in desAuth.subscriptions)
                {
                    if (subs.limitations != null)
                        if (subs.limitations.allowMultiClients)
                        {
                            IsPremium = true;
                            break;
                        }
                }
                //if (!is_premium)
                foreach (var subscription in desAuth.subscriptions)
                {
                    if (subscription.limitations?.cardsRemaining == null)
                    {
                        CardsRemaining = 10;
                        break;
                    }
                    else
                    {
                        if (subscription.limitations != null)
                            if (subscription.limitations.cardsRemaining > CardsRemaining)
                                CardsRemaining = subscription.limitations.cardsRemaining.Value;
                    }
                }
                try
                {
                    foreach (var subscription in desAuth.subscriptions)
                    {
                        if (subscription.features != null)
                        {
                            foreach (var feature in subscription.features)
                            {
                                if (String.IsNullOrEmpty(ExtraEmploymentData))
                                    if (feature == Constants.ExtraEmploymentData)
                                        ExtraEmploymentData = feature;
                                if (String.IsNullOrEmpty(CompanyLogoInQr))
                                    if (feature == Constants.CompanyLogoInQr)
                                        CompanyLogoInQr = feature;
                                if (String.IsNullOrEmpty(ExtraPersonData))
                                    if (feature == Constants.ExtraPersonData)
                                        ExtraPersonData = feature;
                            }
                        }
                    }
                }
                catch { }
            }
            catch { }

            RunOnUiThread(() => _plusRl.Visibility = ViewStates.Visible);

            string resCardsList = null;
            try
            {
                resCardsList = await _cards.CardsListGetEtag(_databaseMethods.GetAccessJwt(), clientName); //"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NDk1NjEyOTcsIkFjY291bnRDbGllbnRUb2tlbiI6IjVQYUw2Sm9XcWdJIiwiQWNjb3VudElEIjoyMCwiQ2xpZW50SUQiOiJiNTA2OTJlNmNkYTdhYTlkIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.UBoRD61YNYjOu60Wu1bpotRAFvxbMaDzt6PCxQrMtKA");
                if (resCardsList == Constants.status_code304)
                {
                    CardNames = _databaseMethods.GetCardNames();
                    return false;
                }
                else
                {
                    RunOnUiThread(async () => await DoOnResumeStuff());
                    return true;
                }
                //if (!we_here)
                //return false;
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
            if (String.IsNullOrEmpty(resCardsList))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return false;
                }
            }

            //ShowWeHaveNotDownloadedCard = false;
            //try
            //{
            //    deserialized_cards_list = JsonConvert.DeserializeObject<List<QRListModel>>(res_cards_list);
            //}
            //catch { }
            //try
            //{
            //    #region placing primary card to the end of the list (the list will be reversed)
            //    var primary_card_index = deserialized_cards_list.FindIndex(x => x.isPrimary);
            //    var primary_card_item = deserialized_cards_list[primary_card_index];
            //    deserialized_cards_list.RemoveAt(primary_card_index);
            //    deserialized_cards_list.Add(primary_card_item);
            //    #endregion placing primary card to the end of the list (the list will be reversed)
            //}
            //catch
            //{ }
            ////SetLoadingView();

            //// TODO crash here
            //reverseList = deserialized_cards_list.AsEnumerable().Reverse().ToList();
            //databaseMethods.InsertLastCloudSync(DateTime.Now);
            //if (reverseList.Count > 0)
            //{
            //    // TODO
            //    await Task.Run(() => InitializeCardsFromBackground());
            //    try
            //    {
            //        var TaskA = new Task(async () => await Cache_cards());
            //        TaskA.Start();
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
            //else
            //{
            //    clear_current_card_name_and_pos();
            //    StartActivity(typeof(MyCardActivity));
            //}
            //SetMainView();
            return true;
        }

        async Task<bool> DoOnResumeStuff()
        {
            //arrowsIV.Visibility = ViewStates.Gone;
            IsPremium = false;
            CardsRemaining = 0;
            CardNames = null;
            _reverseList = null;
            _deserializedCardsList = null;
            _globalTag = 0;
            _cardsCount = 0;

            //TODO
            //await Clear();

            //await InitElements();

            //clear features
            ExtraEmploymentData = null;
            CompanyLogoInQr = null;
            ExtraPersonData = null;
            _showWeHaveNotDownloadedCardScrollDraggingInformer = false;
            SetLoadingView(TranslationHelper.GetString("dataAcquisition", _ci));
            var resSync = await SyncCachedCard();
            //mainTextTV.Text = TranslationHelper.GetString("dataAcquisition", ci);
            string cardsRemainingcontent = null;
            try
            {
                cardsRemainingcontent = await _accounts.AccountAuthorize(_databaseMethods.GetAccessJwt(), clientName);//"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NDk1NjEyOTcsIkFjY291bnRDbGllbnRUb2tlbiI6IjVQYUw2Sm9XcWdJIiwiQWNjb3VudElEIjoyMCwiQ2xpZW50SUQiOiJiNTA2OTJlNmNkYTdhYTlkIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.UBoRD61YNYjOu60Wu1bpotRAFvxbMaDzt6PCxQrMtKA");
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
            if (String.IsNullOrEmpty(cardsRemainingcontent))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return false;
                }
            }
            if (/*res_card_data == Constants.status_code409 ||*/ cardsRemainingcontent == Constants.status_code401)
            {
                ShowSeveralDevicesRestriction();
                return false;
            }
            string resCardsList = null;
            try
            {
                resCardsList = await _cards.CardsListGet(_databaseMethods.GetAccessJwt(), clientName); //"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NDk1NjEyOTcsIkFjY291bnRDbGllbnRUb2tlbiI6IjVQYUw2Sm9XcWdJIiwiQWNjb3VudElEIjoyMCwiQ2xpZW50SUQiOiJiNTA2OTJlNmNkYTdhYTlkIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.UBoRD61YNYjOu60Wu1bpotRAFvxbMaDzt6PCxQrMtKA");
                if (/*res_card_data == Constants.status_code409 ||*/resCardsList == Constants.status_code401)
                {
                    ShowSeveralDevicesRestriction();
                    return false;
                }
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
            if (String.IsNullOrEmpty(resCardsList))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return false;
                }
            }

            ShowWeHaveNotDownloadedCard = false;
            try
            {
                _deserializedCardsList = JsonConvert.DeserializeObject<List<QrListModel>>(resCardsList);
            }
            catch { }
            try
            {
                #region placing primary card to the end of the list (the list will be reversed)
                var primaryCardIndex = _deserializedCardsList.FindIndex(x => x.IsPrimary);
                var primaryCardItem = _deserializedCardsList[primaryCardIndex];
                _deserializedCardsList.RemoveAt(primaryCardIndex);
                _deserializedCardsList.Add(primaryCardItem);
                #endregion placing primary card to the end of the list (the list will be reversed)
            }
            catch
            { }
            SetLoadingView(TranslationHelper.GetString("dataAcquisition", _ci));
            var desAuth = JsonConvert.DeserializeObject<AuthorizeRootObject>(cardsRemainingcontent);
            try
            {
                foreach (var subs in desAuth.subscriptions)
                {
                    if (subs.limitations != null)
                        if (subs.limitations.allowMultiClients)
                        {
                            IsPremium = true;
                            break;
                        }
                }
                //if (!is_premium)
                foreach (var subscription in desAuth.subscriptions)
                {
                    if (subscription.limitations?.cardsRemaining == null)
                    {
                        CardsRemaining = 10;
                        break;
                    }
                    else
                    {
                        if (subscription.limitations != null)
                            if (subscription.limitations.cardsRemaining > CardsRemaining)
                                CardsRemaining = subscription.limitations.cardsRemaining.Value;
                    }
                }
                try
                {
                    foreach (var subscription in desAuth.subscriptions)
                    {
                        if (subscription.features != null)
                        {
                            foreach (var feature in subscription.features)
                            {
                                if (String.IsNullOrEmpty(ExtraEmploymentData))
                                    if (feature == Constants.ExtraEmploymentData)
                                        ExtraEmploymentData = feature;
                                if (String.IsNullOrEmpty(CompanyLogoInQr))
                                    if (feature == Constants.CompanyLogoInQr)
                                        CompanyLogoInQr = feature;
                                if (String.IsNullOrEmpty(ExtraPersonData))
                                    if (feature == Constants.ExtraPersonData)
                                        ExtraPersonData = feature;
                            }
                        }
                    }
                }
                catch { }
            }
            catch { }

            _reverseList = _deserializedCardsList.AsEnumerable().Reverse().ToList();
            _databaseMethods.InsertLastCloudSync(DateTime.Now);
            if (_reverseList.Count > 0)
            {
                await Task.Run(() => InitializeCards());
                //RunOnUiThread(async()=> await Task.Run(() => InitializeCards()));
                try
                {
                    var taskA = new Task(async () => await Cache_cards());
                    taskA.Start();
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                clear_current_card_name_and_pos();
                StartActivity(typeof(MyCardActivity));
            }

            // Need this when we delete everything except onelast card
            try { _recyclerView.SetAdapter(_qRAdapter); } catch { }

            SetMainView();
            return true;
        }

        private void SetMainView()
        {
            _demonstrationRl.Visibility = ViewStates.Gone;
            _loadingLl.Visibility = ViewStates.Gone;
            _mainLl.Visibility = ViewStates.Visible;
        }

        private void SetLoadingView(string message)
        {
            _demonstrationRl.Visibility = ViewStates.Gone;
            _loadingLl.Visibility = ViewStates.Visible;
            _mainLl.Visibility = ViewStates.Gone;
            _mainTextTv.Text = message;
        }

        public static void OpenDemoView()
        {
            _demoCompanyLogoIv.Background = null;
            if (QrAdapter.QrsList[ClickedPosition].LogoImage == null)
                _demoCompanyLogoIv.SetBackgroundResource(Resource.Drawable.company_qr_template);
            _demoQrIv.SetImageBitmap(QrAdapter.QrsList[ClickedPosition].QrImage);
            _demoCompanyLogoIv.SetImageBitmap(QrAdapter.QrsList[ClickedPosition].LogoImage);
            _demonstrationRl.Visibility = ViewStates.Visible;
            _loadingLl.Visibility = ViewStates.Gone;
            _mainLl.Visibility = ViewStates.Gone;
        }

        public static void CloseDemoView()
        {
            _demonstrationRl.Visibility = ViewStates.Gone;
            _loadingLl.Visibility = ViewStates.Gone;
            _mainLl.Visibility = ViewStates.Visible;
        }

        private bool InitializeCards()
        {
            _cardsCount = _deserializedCardsList.Count;
            var lastCard = _deserializedCardsList[_deserializedCardsList.Count - 1];

            int i = 0;
            CardNames = new List<string>();

            _countCached = 0;
            _databaseMethods.CleanCardNames();
            NativeMethods.ResetQrsList();

            var options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 200,//(int)View.Frame.Width,
                Height = 200,//(int)View.Frame.Width,
                ErrorCorrection = ErrorCorrectionLevel.H
            };
            var writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;

            writer.Options = options;
            var qr = new ZXing.BarcodeWriterSvg();
            qr.Options = options;
            qr.Format = ZXing.BarcodeFormat.QR_CODE;

            foreach (var item in _reverseList)
            {
                CardNames = CardNames ?? new List<string>();
                var qrImg = writer.Write(_reverseList[i].Url);
                CardNames.Add(item.Name);
                CompanyLogoNotPublicLogo logo;
                if (_reverseList[i].Company != null)
                    logo = _reverseList[i].Company.logo;
                else
                    logo = null;
                Bitmap imageLogo = null;
                if (!String.IsNullOrEmpty(CompanyLogoInQr))
                {

                    if (logo != null)
                    {
                        try
                        {
                            imageLogo = NativeMethods.GetBitmapFromUrl(_reverseList[i].Company.logo.url);
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
                    }
                }
                QrAdapter.QrsList.Add(new QrListModel
                {
                    QrImage = qrImg,
                    LogoImage = imageLogo,
                    CardName = item.Name,
                    IsPrimary = item.IsPrimary,
                    Id = item.Id,
                    Url = item.Url,
                    Person = new Person
                    {
                        firstName = item.Person.firstName,
                        lastName = item.Person.lastName
                    }
                });
                _databaseMethods.InsertCardName(item.Name, item.Id, item.Url, item.Person.firstName, item.Person.lastName);
                CardNames?.Clear();
                i++;
            }
            if (CardNames == null)
                CardNames = new List<string>();
            foreach (var item in QrAdapter.QrsList)
                CardNames.Add(item.CardName);

            // Need this for slow dragging sticking.
            CalculateDimensions();

            RunOnUiThread(async () =>
            {
                _qRAdapter.NotifyDataSetChanged();
                SetMainView();
                _headerTv.Text = CardNames[0];
                if (_editDeleteIndicator != Constants.delete)
                    try { _headerTv.Text = CardNames[_index]; } catch { }
                //if (!String.IsNullOrEmpty(just_created_card_name))
                //display_just_created_card();
                if (CurrentPosition >= 0)
                {
                    await Task.Delay(50);
                    try { _recyclerView.SmoothScrollToPosition(CurrentPosition); } catch { }
                }
            });
            return true;
        }

        //private bool InitializeCardsFromBackground()
        //{
        //cards_count = deserialized_cards_list.Count;
        //var lastCard = deserialized_cards_list[deserialized_cards_list.Count - 1];

        //int i = 0;
        //card_names = new List<string>();

        //count_cached = 0;
        //try
        //{
        //    databaseMethods.CleanCardNames();
        //}
        //catch (Exception ex)
        //{

        //}

        //var options = new QrCodeEncodingOptions
        //{
        //    DisableECI = true,
        //    CharacterSet = "UTF-8",
        //    Width = 200,//(int)View.Frame.Width,
        //    Height = 200,//(int)View.Frame.Width,
        //    ErrorCorrection = ErrorCorrectionLevel.H
        //};
        //var writer = new BarcodeWriter();
        //writer.Format = BarcodeFormat.QR_CODE;

        ////NativeMethods.ResetQrsList();
        //var backgroundQrsList = new List<QRListModel>();

        //writer.Options = options;
        //var qr = new ZXing.BarcodeWriterSvg();
        //qr.Options = options;
        //qr.Format = ZXing.BarcodeFormat.QR_CODE;
        //foreach (var item in reverseList)
        //{
        //    Bitmap qrImg = null;
        //    try
        //    {
        //        qrImg = writer.Write(reverseList[i].url);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    // TODO
        //    card_names?.Add(item?.name);
        //    CompanyLogoNotPublicLogo logo;
        //    if (item?.company != null)
        //        logo = item?.company?.logo;
        //    else
        //        logo = null;
        //    Bitmap image_logo = null;
        //    if (!String.IsNullOrEmpty(CompanyLogoInQr))
        //    {

        //        if (logo != null)
        //        {
        //            try
        //            {
        //                image_logo = NativeMethods.GetBitmapFromUrl(reverseList[i].company.logo.url);
        //            }
        //            catch (Exception ex)
        //            {
        //                if (!methods.IsConnected())
        //                {
        //                    NoConnectionActivity.activityName = this;
        //                    StartActivity(typeof(NoConnectionActivity));
        //                    Finish();
        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //    //QRAdapter.qrsList.Add(new QRListModel { QRImage = qrImg, LogoImage = image_logo, CardName = item.name, isPrimary = item.isPrimary, id = item.id, url = item.url });
        //    backgroundQrsList.Add(new QRListModel { QRImage = qrImg, LogoImage = image_logo, CardName = item.name, isPrimary = item.isPrimary, id = item.id, url = item.url });
        //    // TODO CRASHES HERE
        //    databaseMethods.InsertCardName(item.name, item.id, item.url);
        //    //card_names?.Clear();
        //    i++;
        //}

        //    //backgroundQrsList

        //    // TODO
        //    //card_names = /*card_names ??*/ new List<string>();
        //    var background_card_names = new List<string>();

        //    foreach (var item in backgroundQrsList)
        //        background_card_names.Add(item.CardName);

        //    card_names = background_card_names;

        //    // Need this for slow dragging sticking.
        //    CalculateDimensions();

        //    RunOnUiThread(async () =>
        //    {
        //        try
        //        {
        //            //qRAdapter.NotifyDataSetChanged();
        //            //recyclerView.SetAdapter(qRAdapter);
        //            //recyclerView.GetAdapter().;
        //            QRAdapter.qrsList = backgroundQrsList;
        //            qRAdapter = new QRAdapter(/*demo_list, */this, tf, ci);
        //            recyclerView.SwapAdapter(qRAdapter, true);
        //            //qRAdapter.NotifyDataSetChanged();
        //            //recyclerView = null;
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        SetMainView();
        //        headerTV.Text = card_names?[0];
        //        if (edit_delete_indicator != Constants.delete)
        //            try
        //            {
        //                headerTV.Text = card_names[index];
        //            }
        //            catch (Exception ex)
        //            {
        //                // TODO
        //                //
        //                try
        //                {
        //                    index = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindFirstCompletelyVisibleItemPosition();
        //                    headerTV.Text = card_names[index];
        //                }
        //                catch (Exception ex1)
        //                {
        //                    //headerTV.Text = "приехали....";
        //                }
        //            }
        //        //if (!String.IsNullOrEmpty(just_created_card_name))
        //        //display_just_created_card();
        //        if (currentPosition >= 0)
        //        {
        //            await Task.Delay(50);
        //            try
        //            {
        //                recyclerView.SmoothScrollToPosition(currentPosition);
        //            }
        //            catch (Exception ex)
        //            {
        //            }
        //        }
        //    });
        //    CallSnackBar(TranslationHelper.GetString("dataUpdated", ci));
        //    return true;
        //}


        public static void ShowTint()
        {
            _tintRl.Visibility = ViewStates.Visible;
        }

        private void CalculateDimensions()
        {
            // Margin right of QrRow.
            _marginRightPixels = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Resources.DisplayMetrics);
            // Width of QrRow.
            _rowWidthPixels = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 300, Resources.DisplayMetrics);
            _fullWidthInPx = QrAdapter.QrsList.Count * _rowWidthPixels + (_marginRightPixels * (QrAdapter.QrsList.Count - 1));
            _fullRecyclerWidth = _fullWidthInPx;
        }

        async Task<bool> Cache_cards()
        {
            await _nativeMethods.RemoveOfflineCache();
            int index = 0;
            try
            {
                if (QrAdapter.QrsList?.Count > 0)
                    RunOnUiThread(() => SetLoadingView(TranslationHelper.GetString("cachingData", _ci)));
                foreach (var item in QrAdapter.QrsList)
                {
                    try
                    {
                        if (QrAdapter.QrsList[index].LogoImage != null)
                            await _nativeMethods.CacheQrOffline(QrAdapter.QrsList[index].QrImage, QrAdapter.QrsList[index].LogoImage, index);
                        else
                        {
                            try
                            {
                                var companyLodoDefaultDrawable = GetDrawable(Resource.Drawable.company_qr_template);
                                await _nativeMethods.CacheQrOffline(QrAdapter.QrsList[index].QrImage, ((BitmapDrawable)companyLodoDefaultDrawable).Bitmap, index);
                            }
                            catch
                            {
                                RunOnUiThread(() => SetMainView());
                            }
                        }
                        index++;
                    }
                    catch (Exception ex)
                    {
                        RunOnUiThread(() => SetMainView());
                        return false;
                    }
                }

                RunOnUiThread(() => SetMainView());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            _databaseMethods.InsertEtag(Cards.ETagNew);
            return true;
        }

        public static void clear_current_card_name_and_pos()
        {
            CurrentCardNameHeader = String.Empty;
            CurrentPosition = 0;
            ClickedPosition = 0;
            try
            {
                _recyclerView.ScrollToPosition(0);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<bool> SyncCachedCard(/*bool showLoadingView = true*/)
        {
            //if (showLoadingView)
            //SetLoadingView(TranslationHelper.GetString("cardIsSynchronizing", ci));

            #region uploading photos
            bool photosExist = true;
            var personalImages = await _nativeMethods.GetPersonalImages();
            if (personalImages == null)
                photosExist = false;
            else
                photosExist = true;
            var documentsLogo = await _nativeMethods.GetDocumentsLogo();
            if (documentsLogo != null)
                photosExist = true;
            int? logoId = null;
            List<int> attachmentsIdsList = new List<int>();
            if (photosExist)
            {
                _mainTextTv.Text = TranslationHelper.GetString("photosAreBeingUploaded", _ci);
                AttachmentsUploadModel resPhotos = null;
                try
                {
                    resPhotos = await _attachments.UploadAndroid(_databaseMethods.GetAccessJwt(), clientName, personalImages, documentsLogo);
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
                if (resPhotos == null)
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        return false;
                    }
                }
                if (resPhotos != null)
                {
                    logoId = resPhotos.logo_id;
                    attachmentsIdsList = resPhotos.attachments_ids;
                    if (logoId == Constants.image_upload_status_code401)
                    {
                        ShowSeveralDevicesRestriction();
                        return false;
                    }
                }
            }
            #endregion uploading photos
            RunOnUiThread(() => _mainTextTv.Text = TranslationHelper.GetString("dataAcquisition", _ci));
            string companyCardRes = "";
            if (!CompanyDataActivity.CompanyNull)
            {
                try
                {
                    // TODO potentially dangerouse sqlite call
                    if (logoId != null)
                        companyCardRes = await _companies.CreateCompanyCard(_databaseMethods.GetAccessJwt(), clientName, _databaseMethods.GetDataFromCompanyCard(), logoId);
                    else
                        companyCardRes = await _companies.CreateCompanyCard(_databaseMethods.GetAccessJwt(), clientName, _databaseMethods.GetDataFromCompanyCard());
                    if (companyCardRes == Constants.image_upload_status_code401.ToString())
                    {
                        ShowSeveralDevicesRestriction();
                        return false;
                    }
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
                if (String.IsNullOrEmpty(companyCardRes))
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        return false;
                    }
                }
            }
            try
            {
                string userCardRes = null;
                try
                {
                    // TODO potencially dangerous sqlite
                    if (!CompanyDataActivity.CompanyNull)
                    {
                        var deserialized = JsonConvert.DeserializeObject<CompanyCardResponse>(companyCardRes);
                        userCardRes = await _cards.CreatePersonalCard(_databaseMethods.GetAccessJwt(),
                                                                       _databaseMethods.GetDataFromUsersCard(deserialized.id,
                                                                                                            _databaseMethods.GetLastSubscription(),
                                                                                                            EditCompanyDataActivity.Position,
                                                                                                            EditCompanyDataActivity.CorporativePhone),
                                                                       attachmentsIdsList,
                                                                       clientName);


                    }
                    else
                        userCardRes = await _cards.CreatePersonalCard(_databaseMethods.GetAccessJwt(),
                                                                       _databaseMethods.GetDataFromUsersCard(null,
                                                                                                            _databaseMethods.GetLastSubscription(),
                                                                                                            EditCompanyDataActivity.Position,
                                                                                                            EditCompanyDataActivity.CorporativePhone),
                                                                       attachmentsIdsList,
                                                                       clientName);
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
                    try
                    {
                        // TODO Commented below during Quick Cache implementation
                        //nativeMethods.ClearAll();
                    }
                    catch
                    { }
                }
                if (String.IsNullOrEmpty(userCardRes))
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        return false;
                    }
                }
                if (userCardRes == Constants.image_upload_status_code401.ToString())
                {
                    ShowSeveralDevicesRestriction();
                    return false;
                }
                try
                {
                    var usersCardDes = JsonConvert.DeserializeObject<CompanyCardResponse>(userCardRes);
                    PersonalCardId = usersCardDes.id;
                    QrActivity.JustCreatedCardName = _databaseMethods.get_card_name();
                    _nativeMethods.ClearAll();

                    CardDoneActivity.CardId = PersonalCardId;
                    //CardDoneViewController.variant_displaying = 1;

                    _databaseMethods.InsertLastCloudSync(DateTime.Now);
                    //StartActivity(typeof(CardDoneActivity));
                    var taskA = new Task(async () =>
                    {
                        //await nativeMethods.RemovePersonalImages();
                        //await nativeMethods.RemoveLogo();
                        //await nativeMethods.RemoveOfflineCache();
                        //var QRs_cache_dir = Path.Combine(docs, Constants.QRs_cache_dir);
                    });
                }
                catch
                {
                    //var deserialized_error = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(user_card_res);
                    ////if (deserialized_error[0].message == Constants.alreadyDone)
                    //if (deserialized_error[0].code == Constants.alreadyDone)
                    //    Toast.MakeText(this, TranslationHelper.GetString("cardWithThisNameExists", ci), ToastLength.Long).Show();
                    //pop();
                }
            }
            catch
            {
                //var deserialized_error = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(company_card_res);
                //if (deserialized_error != null)
                //{
                //    Toast.MakeText(this, deserialized_error[0].message, ToastLength.Long).Show();
                //    pop();
                //}
                //else
                //StartActivity(typeof(CardDoneActivity));
            }
            return true;
        }



        private async Task<bool> Clear()
        {
            await _nativeMethods.RemoveOfflineCache();
            #region clearing
            NativeMethods.ResetSocialNetworkList();
            //CompanyDataActivity.currentImage = null;
            CompanyDataActivity.CroppedResult = null;
            NativeMethods.ResetCompanyAddress();
            CompanyDataActivity.CompanyName = null;
            CompanyDataActivity.LinesOfBusiness = null;
            CompanyDataActivity.Position = null;
            CompanyDataActivity.FoundationYear = null;
            CompanyDataActivity.Clients = null;
            CompanyDataActivity.CompanyPhone = null;
            CompanyDataActivity.CorporativePhone = null;
            CompanyDataActivity.Fax = null;
            CompanyDataActivity.CompanyEmail = null;
            CompanyDataActivity.CorporativeSite = null;
            EditPersonalDataActivity.MySurname = null;
            EditPersonalDataActivity.MyName = null;
            EditPersonalDataActivity.MyMiddlename = null;
            EditPersonalDataActivity.MyPhone = null;
            EditPersonalDataActivity.MyEmail = null;
            EditPersonalDataActivity.MyHomePhone = null;
            EditPersonalDataActivity.MySite = null;
            EditPersonalDataActivity.MyDegree = null;
            EditPersonalDataActivity.MyCardName = null;
            try { PersonalImageAdapter.Photos.Clear(); } catch { }
            EditPersonalDataActivity.MyBirthDate = null;
            NativeMethods.ResetHomeAddress();
            EditPersonalProcessActivity.CompanyId = null;
            EditCompanyDataActivity.Position = null;
            EditCompanyDataActivity.LogoId = null;
            #endregion clearing
            return true;
        }

        private void InitElements()
        {
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _arrowsIv = FindViewById<ImageView>(Resource.Id.arrowsIV);
            _layoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _mainLl = FindViewById<LinearLayout>(Resource.Id.mainLL);
            _loadingLl = FindViewById<LinearLayout>(Resource.Id.loadingLL);
            _closeRl = FindViewById<RelativeLayout>(Resource.Id.closeRL);
            _demonstrationRl = FindViewById<RelativeLayout>(Resource.Id.demonstrationRL);
            _demoQrIv = FindViewById<ImageView>(Resource.Id.demoQrIV);
            _demoCompanyLogoIv = FindViewById<ImageView>(Resource.Id.demoCompanyLogoIV);
            _plusRl.Visibility = ViewStates.Visible;
            _recyclerView?.AddOnScrollListener(_dragDetector);
            QrAdapter.Offline = false;
            // TODO
            _qRAdapter = new QrAdapter(this, _tf, _ci);
            _tintRl.Visibility = ViewStates.Gone;
            _removeBn.Text = TranslationHelper.GetString("removeBn", _ci);
            _editBn.Text = TranslationHelper.GetString("editBn", _ci);
            _showInWebBn.Text = TranslationHelper.GetString("watchInWeb", _ci);
            _linkQrBn.Text = TranslationHelper.GetString("linkQrSticker", _ci);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.SetAdapter(_qRAdapter);
            CheckHeaderValue();
            try
            {
                new LinearSnapHelper().AttachToRecyclerView(_recyclerView);
            }
            catch (Exception ex)
            {

            }
            _headerTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _mainTextTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _removeBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _editBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _showInWebBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _linkQrBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _arrowsIv.Visibility = ViewStates.Gone;
            //dragging_subs();
        }

        private void InitLeftMenu()
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
            _myCardTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _orderStickerTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _cloudSyncTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _premiumTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _aboutTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _enterTv.SetTypeface(_tf, TypefaceStyle.Normal);
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
            FindViewById<RelativeLayout>(Resource.Id.myCardRL).Click += delegate
            {
                _drawerLayout.CloseDrawer(_leftDrawer);
            };
            FindViewById<RelativeLayout>(Resource.Id.myCardRL).Click += MyCardClick;
            FindViewById<RelativeLayout>(Resource.Id.orderStickerRL).Click += OrderClick;
            FindViewById<RelativeLayout>(Resource.Id.cloudSyncRL).Click += CloudSyncClick;
            FindViewById<RelativeLayout>(Resource.Id.premiumRL).Click += PremiumClick;
            FindViewById<RelativeLayout>(Resource.Id.aboutRL).Click += AboutClick;
            //FindViewById<RelativeLayout>(Resource.Id.enterRL).Click += LoginRLClick;
            //button to open Left Drawer
            FindViewById<RelativeLayout>(Resource.Id.leftRL).Click += LeftRlClick;
            _drawerToggle = new NativeClasses.ActionBarDrawerToggle(this, _drawerLayout, Resource.String.openDrawer, Resource.String.closeDrawer);
            _drawerLayout.DrawerSlide += (s, e) =>
              {
                  if (!CheckStoragePermissions(this).Result)
                  {
                      _drawerLayout.CloseDrawer(_leftDrawer);
                      return;
                  }
                  _tintRl.Visibility = ViewStates.Gone;
              };
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
                CurrentPosition = 0;
                try { _recyclerView.SmoothScrollToPosition(0); } catch { }
                //StartActivity(typeof(QrActivity));
            }
            else
                _drawerLayout.CloseDrawer(_leftDrawer);
        }

        private void LeftRlClick(object sender, EventArgs e)
        {
            if (!CheckStoragePermissions(this).Result)
                return;
            if (_drawerLayout.IsDrawerOpen(_leftDrawer))
            {
                _drawerLayout.CloseDrawer(_leftDrawer);
            }
            else
            {
                _drawerLayout.OpenDrawer(_leftDrawer);
                _tintRl.Visibility = ViewStates.Gone;
            }
        }

        private void LoginRlClick(object sender, EventArgs e)
        {
            if (!_databaseMethods.UserExists())
            {
                _drawerLayout.CloseDrawer(_leftDrawer);
                _databaseMethods.InsertLoginedFrom(Constants.from_slide_menu);
                StartActivity(typeof(EmailActivity));
            }
            else
                ShowLogOutAlert();
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (_checkHeaderValueTimer != null)
            {
                _checkHeaderValueTimer.Stop();
                _checkHeaderValueTimer.Dispose();
            }
            try
            {
                _connectionWaitingTimer.Stop();
                _connectionWaitingTimer.Dispose();
            }
            catch { }
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
                Intent intent = new Intent(this, typeof(MyCardActivity));
                intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
                StartActivity(intent);
                Finish();
                //StartActivity(typeof(MyCardActivity));
            });
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        private void CheckHeaderValue()
        {
            _checkHeaderValueTimer = new System.Timers.Timer();
            int interval = 300;
            _checkHeaderValueTimer.Interval = interval;
            _checkHeaderValueTimer.Elapsed += delegate
            {
                _checkHeaderValueTimer.Interval = interval;

                RunOnUiThread(() =>
                {
                    try
                    {
                        _index = ((LinearLayoutManager)_recyclerView.GetLayoutManager()).FindFirstCompletelyVisibleItemPosition();
                        if (_index >= 0)
                        {
                            if (CardNames != null)
                                CurrentCardNameHeader = CardNames[_index];
                            //else
                            //current_card_name_header = "Снова приехали";
                            _headerTv.Text = CurrentCardNameHeader;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                });
            };
            _checkHeaderValueTimer.Start();
        }
        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out(this);
            MyCardActivity.DeviceRestricted = true;
            Intent intent = new Intent(this, typeof(MyCardActivity));
            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            try
            {
                if (_demonstrationRl.Visibility == ViewStates.Visible)
                {
                    CloseDemoView();
                    return;
                }

                if (_tintRl.Visibility == ViewStates.Visible)
                {
                    _tintRl.Visibility = ViewStates.Gone;
                    return;
                }
            }
            catch
            {

            }
            //base.OnBackPressed();
            FinishAffinity();
        }
    }
}
