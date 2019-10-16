using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CardsAndroid.Adapters;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;
using Com.Yalantis.Ucrop;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Permission = Plugin.Permissions.Abstractions.Permission;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class EditPersonalDataActivity : AppCompatActivity
    {
        EditText _surnameEt, _nameMiddlenameEt, _mobilePhoneEt, _emailEt, _homePhoneEt, _siteEt, _degreeEt, _cardNameEt;
        TextInputLayout _surnameTil, _nameMiddlenameTil, _cardNameTil;
        public static bool ChangedSomething;
        static int _pressedPhotoPosition;
        DatePickerDialog _ddtime;
        RecyclerView _imageRecyclerView;
        RecyclerView.LayoutManager _imageLayoutManager;
        TextView _headerTv, _birthdateMainTv, _birthdateValueTv, _homeAddressTv, _socialNetworkTv;
        Button _resetBn, _continueBn;
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        Attachments _attachments = new Attachments();
        ImageView _siteLockIv;
        RelativeLayout _backRl, _birthdateRl, _homeAddressRl, _socialNetworkRl;
        ProgressBar _activityIndicator;
        ScrollView _scrollView;
        /*public static */
        EditPersonalImageAdapter _editPersonalImageAdapter;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        NativeMethods _nativeMethods = new NativeMethods();
        Methods _methods = new Methods();
        public event EventHandler PhotosPermissionAllowed;
        static bool _initFinished = false;
        public static bool IsPrimary;
        public static string MySurname, MyName, MyMiddlename, MyPhone, MyEmail, MyHomePhone, MySite, MyDegree, MyCardName, MyBirthDate, MyCardNameOriginal;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PersonalData);
            ChangedSomething = false;
            _initFinished = false;
            _nativeMethods.CheckStoragePermissions(this);
            InitElements();
            _continueBn.Click += async (s, e) =>
            {
                if (!await _nativeMethods.CheckStoragePermissions(this))
                {
                    Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", _ci), ToastLength.Long).Show();
                    //Thread.Sleep(2000);
                    return;
                }

                string errorMessage = "";
                if (String.IsNullOrEmpty(MySurname))
                    errorMessage += TranslationHelper.GetString("enterSurname", _ci);
                if (String.IsNullOrEmpty(_nameMiddlenameEt.Text))
                    errorMessage += TranslationHelper.GetString("enterName", _ci);
                if (String.IsNullOrEmpty(MyCardName))
                    errorMessage += TranslationHelper.GetString("enterCardName", _ci);
                else
                {
                    if (QrAdapter.QrsList != null)
                    {
                        if (QrAdapter.QrsList.Count > 0)
                        {
                            bool containsName = QrActivity.CardNames.Any(item => item == MyCardName);
                            //bool isCurrentName = QRActivity.card_names.Any(item => item == QRActivity.current_card_name_header);
                            if (MyCardName == QrActivity.CurrentCardNameHeader)
                                containsName = false;
                            if (containsName)
                                errorMessage += TranslationHelper.GetString("cardWithThisNameExists", _ci);
                        }
                    }
                }
                try
                {
                    if (!String.IsNullOrEmpty(MyPhone))
                    {
                        if (MyPhone.Length > 16)
                            errorMessage += TranslationHelper.GetString("phoneIncorrect", _ci);
                    }
                }
                catch
                {
                    errorMessage += TranslationHelper.GetString("phoneIncorrect", _ci);
                }
                if (!String.IsNullOrEmpty(MyEmail))
                {
                    try
                    {
                        _emailEt.Text = _methods.EmailValidation(MyEmail);
                    }
                    catch
                    {
                        errorMessage += TranslationHelper.GetString("emailIncorrect", _ci);
                    }
                }

                if (String.IsNullOrEmpty(errorMessage))
                {
                    QrActivity.CurrentCardNameHeader = MyCardName;
                    //we need this to substitute email value for login
                    var dateTimeStub = new DateTime();
                    _databaseMethods.InsertValidTillRepeatAfter(dateTimeStub, dateTimeStub, _emailEt.Text);

                    _activityIndicator.Visibility = ViewStates.Visible;
                    _continueBn.Visibility = ViewStates.Gone;
                    // Caching card to db.
                    if (!await Task.Run(async () => await CacheData()))
                        return;
                    _activityIndicator.Visibility = ViewStates.Gone;
                    _continueBn.Visibility = ViewStates.Visible;
                    //TaskA.Start();
                    StartActivity(typeof(EditPersonalProcessActivity));
                }
                else
                {
                    //nameMiddlenameET.SetHighlightColor(Color.Red);
                    //ColorStateList colorStateList = ColorStateList.ValueOf(Color.Red);
                    //nameMiddlenameET.BackgroundTintList = colorStateList;
                    //FindViewById<TextInputLayout>(Resource.Id.surnameTIL).ErrorEnabled = true;
                    //FindViewById<TextInputLayout>(Resource.Id.surnameTIL).Error = " ";
                    //FindViewById<TextInputLayout>(Resource.Id.surnameTIL).

                    //nameMiddlenameET.SetHintTextColor(colorStateList);
                    _surnameTil.ErrorEnabled = false;
                    _nameMiddlenameTil.ErrorEnabled = false;
                    _cardNameTil.ErrorEnabled = false;

                    if (String.IsNullOrEmpty(_surnameEt.Text))
                    {
                        _surnameTil.ErrorEnabled = true;
                        _surnameTil.Error = TranslationHelper.GetString("requiredData", _ci);
                    }
                    if (String.IsNullOrEmpty(_nameMiddlenameEt.Text))
                    {
                        _nameMiddlenameTil.ErrorEnabled = true;
                        _nameMiddlenameTil.Error = TranslationHelper.GetString("requiredData", _ci);
                    }
                    if (String.IsNullOrEmpty(_cardNameEt.Text))
                    {
                        _cardNameTil.ErrorEnabled = true;
                        _cardNameTil.Error = TranslationHelper.GetString("requiredData", _ci);
                    }

                    _scrollView.ScrollTo(0, 0);

                    Toast.MakeText(this, errorMessage, ToastLength.Long).Show();
                }
            };
            _surnameEt.TextChanged += (s, e) =>
            {
                MySurname = _surnameEt.Text;
                if (_initFinished)
                    ChangedSomething = true;
            };
            _nameMiddlenameEt.TextChanged += (s, e) =>
            {
                if (_nameMiddlenameEt.Text.Contains(" ") && _nameMiddlenameEt.Text.IndexOf(" ", 1) > 1)
                {
                    String[] nameMiddle = _nameMiddlenameEt.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    MyName = nameMiddle[0];
                    try
                    {
                        MyMiddlename = nameMiddle[1];
                    }
                    catch { MyMiddlename = string.Empty; }
                }
                else
                {
                    MyMiddlename = string.Empty;
                    MyName = _nameMiddlenameEt.Text;
                }
                if (_initFinished)
                    ChangedSomething = true;
            };
            _mobilePhoneEt.TextChanged += (s, e) =>
            {
                if (!String.IsNullOrEmpty(_mobilePhoneEt.Text))
                {
                    try
                    {
                        char l = _mobilePhoneEt.Text[_mobilePhoneEt.Text.Length - 1];
                        if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                            MyPhone = _mobilePhoneEt.Text;
                        else
                            _mobilePhoneEt.Text = _mobilePhoneEt.Text.Remove(_mobilePhoneEt.Text.Length - 1);
                        _mobilePhoneEt.SetSelection(_mobilePhoneEt.Text.Length);
                    }
                    catch { }
                }
                else
                    MyPhone = _mobilePhoneEt.Text;
                if (_initFinished)
                    ChangedSomething = true;
            };
            _emailEt.TextChanged += (s, e) =>
            {
                MyEmail = _emailEt.Text;
                if (_initFinished)
                    ChangedSomething = true;
            };
            _homePhoneEt.TextChanged += (s, e) =>
            {
                if (!String.IsNullOrEmpty(_homePhoneEt.Text))
                {
                    try
                    {
                        char l = _homePhoneEt.Text[_homePhoneEt.Text.Length - 1];
                        if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                            MyHomePhone = _homePhoneEt.Text;
                        else
                            _homePhoneEt.Text = _homePhoneEt.Text.Remove(_homePhoneEt.Text.Length - 1);
                        _homePhoneEt.SetSelection(_homePhoneEt.Text.Length);
                    }
                    catch { }
                }
                else
                    MyHomePhone = _homePhoneEt.Text;
                if (_initFinished)
                    ChangedSomething = true;
            };
            if (string.IsNullOrEmpty(QrActivity.ExtraPersonData))
                _siteEt.Touch += (s, e) =>
                {
                    if (e.Event.Action == MotionEventActions.Up)
                    {
                        if (QrActivity.ExtraPersonData != null)
                            CallKeyboard(_siteEt);
                        else
                            _nativeMethods.CallPremiumOptionMenu(this).Show();
                    }
                };

            _siteEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraPersonData != null)
                    MySite = _siteEt.Text;
                else
                {
                    if (!String.IsNullOrEmpty(_siteEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _siteEt.Text = null;
                    }
                    //siteET.EndEditing(true);
                }
                if (_initFinished)
                    ChangedSomething = true;
            };
            _degreeEt.TextChanged += (s, e) =>
            {
                MyDegree = _degreeEt.Text;
                if (_initFinished)
                    ChangedSomething = true;
            };
            _cardNameEt.TextChanged += (s, e) =>
            {
                MyCardName = _cardNameEt.Text;
                if (_initFinished)
                    ChangedSomething = true;
            };
        }

        private void CallKeyboard(EditText editText)
        {
            InputMethodManager inputMethodManager = this.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.ShowSoftInput(editText, ShowFlags.Forced);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
            editText.RequestFocus();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _initFinished = false;
            FillFields();
        }

        protected override void OnPause()
        {
            base.OnPause();
            //scrollView.Hidden = true;
            //activityIndicator.Hidden = false;
        }

        private async Task<bool> CacheData()
        {
            // Clean folder first of all.
            await _nativeMethods.RemovePersonalImages();

            // -1 because we always have null at index 0 of this list.
            int count = -1;
            try
            {
                foreach (var bitmap in EditPersonalImageAdapter.Photos)
                {
                    if (bitmap != null)
                    {
                        var res = await _nativeMethods.CachePersonalImage(bitmap, count);
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            if (MyBirthDate != null)
            {
                try
                {
                    var split = MyBirthDate.Split('.');
                    MyBirthDate = split[1] + "-" + split[0] + "-" + split[2];
                }
                catch (Exception ex) { }
            }
            RunOnUiThread(() =>
            _databaseMethods.InsertUsersCard(
            MyName,
            MySurname,
            MyMiddlename,
            MyPhone,
            MyEmail,
            MyHomePhone,
            MySite,
            MyDegree,
            MyCardName,
            MyBirthDate,
            HomeAddressActivity.MyCountry,
            HomeAddressActivity.MyRegion,
            HomeAddressActivity.MyCity,
            HomeAddressActivity.FullAddressStatic,
            HomeAddressActivity.MyIndex,
            HomeAddressActivity.MyNotation,
            NewCardAddressMapActivity.Lat,
            NewCardAddressMapActivity.Lng
            ));
            return true;
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _scrollView = FindViewById<ScrollView>(Resource.Id.scrollView1);
            _imageRecyclerView = FindViewById<RecyclerView>(Resource.Id.imageRecyclerView);
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _birthdateMainTv = FindViewById<TextView>(Resource.Id.birthdateMainTV);
            _birthdateValueTv = FindViewById<TextView>(Resource.Id.birthdateValueTV);
            _homeAddressTv = FindViewById<TextView>(Resource.Id.homeAddressTV);
            _socialNetworkTv = FindViewById<TextView>(Resource.Id.socialNetworkTV);
            _resetBn = FindViewById<Button>(Resource.Id.resetBn);
            _continueBn = FindViewById<Button>(Resource.Id.continueBn);
            _siteLockIv = FindViewById<ImageView>(Resource.Id.site_lockIV);
            _backRl = FindViewById<RelativeLayout>(Resource.Id.backRL);
            _resetBn.Text = TranslationHelper.GetString("reset", _ci);
            _continueBn.Text = TranslationHelper.GetString("save", _ci);
            _headerTv.Text = TranslationHelper.GetString("personalData", _ci);
            _birthdateMainTv.Text = TranslationHelper.GetString("birthday", _ci);
            _homeAddressTv.Text = TranslationHelper.GetString("homeAddress", _ci);
            _socialNetworkTv.Text = TranslationHelper.GetString("socialNetworks", _ci);
            _surnameEt = FindViewById<EditText>(Resource.Id.surnameET);
            _nameMiddlenameEt = FindViewById<EditText>(Resource.Id.nameMiddlenameET);
            _nameMiddlenameEt.InputType = Android.Text.InputTypes.TextFlagCapWords;
            _mobilePhoneEt = FindViewById<EditText>(Resource.Id.mobilePhoneET);
            _emailEt = FindViewById<EditText>(Resource.Id.emailET);
            _homePhoneEt = FindViewById<EditText>(Resource.Id.homePhoneET);
            _siteEt = FindViewById<EditText>(Resource.Id.siteET);
            _degreeEt = FindViewById<EditText>(Resource.Id.degreeET);
            _cardNameEt = FindViewById<EditText>(Resource.Id.cardNameET);
            _birthdateRl = FindViewById<RelativeLayout>(Resource.Id.birthdateRL);
            _homeAddressRl = FindViewById<RelativeLayout>(Resource.Id.homeAddressRL);
            _socialNetworkRl = FindViewById<RelativeLayout>(Resource.Id.socialNetworkRL);
            _imageLayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);

            _surnameTil = FindViewById<TextInputLayout>(Resource.Id.surnameTIL);
            _nameMiddlenameTil = FindViewById<TextInputLayout>(Resource.Id.nameMiddlenameTIL);
            _cardNameTil = FindViewById<TextInputLayout>(Resource.Id.cardNameTIL);

            _imageRecyclerView.SetLayoutManager(_imageLayoutManager);

            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _birthdateMainTv.SetTypeface(tf, TypefaceStyle.Normal);
            _birthdateValueTv.SetTypeface(tf, TypefaceStyle.Normal);
            _homeAddressTv.SetTypeface(tf, TypefaceStyle.Normal);
            _socialNetworkTv.SetTypeface(tf, TypefaceStyle.Normal);
            _resetBn.SetTypeface(tf, TypefaceStyle.Normal);
            _continueBn.SetTypeface(tf, TypefaceStyle.Normal);
            _surnameEt.SetTypeface(tf, TypefaceStyle.Normal);
            _nameMiddlenameEt.SetTypeface(tf, TypefaceStyle.Normal);
            _mobilePhoneEt.SetTypeface(tf, TypefaceStyle.Normal);
            _emailEt.SetTypeface(tf, TypefaceStyle.Normal);
            _homePhoneEt.SetTypeface(tf, TypefaceStyle.Normal);
            _siteEt.SetTypeface(tf, TypefaceStyle.Normal);
            _degreeEt.SetTypeface(tf, TypefaceStyle.Normal);
            _cardNameEt.SetTypeface(tf, TypefaceStyle.Normal);

            var imageList = new List<Bitmap>();
            imageList.Add(null);

            // Case if we copy card from existing.
            if (EditActivity.ImagesFromServerList != null)
                foreach (var item in EditActivity.ImagesFromServerList)
                {
                    // Null is the first item.
                    if (item != null)
                        imageList.Add(item);
                }

            if (EditPersonalImageAdapter.Photos == null)
                _editPersonalImageAdapter = new EditPersonalImageAdapter(imageList, this, _ci);
            else
            {
                if (PersonalImageAdapter.Photos.Count <= 1)
                    _editPersonalImageAdapter = new EditPersonalImageAdapter(imageList, this, _ci);
                else
                    _editPersonalImageAdapter = new EditPersonalImageAdapter(//PersonalImageAdapter._photos
                    imageList, this, _ci);
            }
            _imageRecyclerView.SetAdapter(_editPersonalImageAdapter);
            _resetBn.Click += (s, e) => ShowResetAlert();
            _backRl.Click += (s, e) => BackClicked();
            _birthdateRl.Click += BirthdayClicked;//DateSelect_OnClick;
            _homeAddressRl.Click += (s, e) => StartActivity(typeof(HomeAddressActivity));
            _socialNetworkRl.Click += (s, e) => StartActivity(typeof(SocialNetworksActivity));
            _birthdateValueTv.Text = null;
            //GET BITMAP FROM URL

            //var bmp = NativeMethods.GetBitmapFromUrl("http://juliasfairies.com/wp-content/uploads/JF-Logo-Final-05.png");
            ////using (var bmp = BitmapFactory.DecodeStream(url.OpenConnection().InputStream))
            //FindViewById<ImageView>(Resource.Id.imageView1).SetImageBitmap(bmp);
            if (String.IsNullOrEmpty(QrActivity.ExtraPersonData))
                _siteLockIv.Visibility = ViewStates.Visible;
            else
                _siteLockIv.Visibility = ViewStates.Gone;
            _mobilePhoneEt.FocusChange += (s, e) =>
            {
                if (!_mobilePhoneEt.IsFocused)
                    if (_mobilePhoneEt.Text.Length <= 2)
                    {
                        MyPhone = null;
                        _mobilePhoneEt.Text = null;
                    }
                if (_mobilePhoneEt.IsFocused)
                    if (String.IsNullOrEmpty(MyPhone))
                        _mobilePhoneEt.Text = "+7";
            };
            _homePhoneEt.FocusChange += (s, e) =>
            {
                if (!_homePhoneEt.IsFocused)
                    if (_homePhoneEt.Text.Length <= 2)
                    {
                        MyHomePhone = null;
                        _homePhoneEt.Text = null;
                    }
                if (_homePhoneEt.IsFocused)
                    if (String.IsNullOrEmpty(MyHomePhone))
                        _homePhoneEt.Text = "+7";
            };
        }

        void DateSelect_OnClick(object sender, EventArgs eventArgs)
        {
            //var fkfkf = new DateTime(2012, 11, 13);
            _ddtime = new DatePickerDialog(this, OnDateSet, DateTime.Today.Year, DateTime.Today.Month - 1,
                                                     DateTime.Today.Day
                                                             );

            _ddtime.DatePicker.DateTime = _selectedDate;
            _ddtime.SetTitle("");
            _ddtime.Show();
            //ddtime.DatePicker.DateTime = selected_Date;
            //ddtime.SetTitle("");
            //ddtime.Show();
            //frag.
            //frag.Show(FragmentManager, DatePickerFragment.TAG);
        }
        DateTime _selectedDate = new DateTime(1990, 10, 12);
        async void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            _selectedDate = e.Date;
            string dayOfMonth = e.DayOfMonth.ToString();
            int mth = e.Month;
            mth++;
            string month = mth.ToString();
            if (dayOfMonth.Length == 1)
                dayOfMonth = "0" + dayOfMonth;
            if (month.Length == 1)
                month = "0" + month;
            _birthdateValueTv.Text = dayOfMonth + "." + month + "." + e.Year;
            _birthdateMainTv.SetTextColor(Color.White);
            //myBirthDate = e.Year + "-" + month + "-" + dayOfMonth;
            MyBirthDate = dayOfMonth + "." + month + "." + e.Year;
            //var  default_date = e.Date.Year + "/" + e.Date.Month + "/" + e.Date.Day;
            //txt_selectDate.Text = e.Date.Day + "/" + e.Date.Month + "/" + e.Date.Year;
            //selected_Date = e.Date;
            ChangedSomething = true;
        }
        async Task<bool> ResetData()
        {
            MySurname = null;
            MyName = null;
            MyMiddlename = null;
            MyPhone = null;
            MyEmail = null;
            MyHomePhone = null;
            MySite = null;
            MyDegree = null;
            MyCardName = null;
            MyBirthDate = null;
            EditPersonalImageAdapter.Photos.Clear();
            EditPersonalImageAdapter.Photos.Add(null);
            _editPersonalImageAdapter.NotifyDataSetChanged();
            NativeMethods.ResetHomeAddress();
            NativeMethods.ResetSocialNetworkList();

            //EditCompanyDataActivity.Position = null;
            //if (Directory.Exists(cards_cache_dir))
            //    Directory.Delete(cards_cache_dir, true);
            _databaseMethods.CleanPersonalNetworksTable();
            _databaseMethods.ClearUsersCardTable();
            await _nativeMethods.RemovePersonalImages();
            _surnameEt.Text = null;
            _nameMiddlenameEt.Text = null;
            _mobilePhoneEt.Text = null;
            _emailEt.Text = null;
            _homePhoneEt.Text = null;
            _siteEt.Text = null;
            _degreeEt.Text = null;
            _cardNameEt.Text = null;
            FillFields();
            return true;
        }

        void BirthdayClicked(object s, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_birthdateValueTv.Text))
            {
                var builder = new Android.App.AlertDialog.Builder(this);
                builder.SetNegativeButton(TranslationHelper.GetString("remove", _ci), (object sender1, DialogClickEventArgs e1) =>
                {
                    MyBirthDate = null;
                    _birthdateValueTv.Text = null;
                    CheckBirthdate();
                });
                builder.SetCancelable(true);
                builder.SetPositiveButton(TranslationHelper.GetString("change", _ci), (object sender1, DialogClickEventArgs e1) =>
                {
                    DateSelect_OnClick(null, null);
                });
                Android.App.AlertDialog dialog = builder.Create();
                dialog.Show();
            }
            else
                DateSelect_OnClick(null, null);
        }

        private void BackClicked()
        {
            //if (view_for_picker.Hidden)
            //{
            //if (EditPersonalImageAdapter._photos != null)
            //    if (EditPersonalImageAdapter._photos.Count > 1)
            //    {
            //        ShowBackAlert();
            //        return;
            //    }
            //if (!String.IsNullOrEmpty(surnameET.Text))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(nameMiddlenameET.Text))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(mobilePhoneET.Text))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(emailET.Text))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(homePhoneET.Text))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(siteET.Text))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(degreeET.Text))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(cardNameET.Text))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(HomeAddressActivity.FullAddressStatic))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(HomeAddressActivity.myCountry))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(HomeAddressActivity.myRegion))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(HomeAddressActivity.myCity))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(HomeAddressActivity.myIndex))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(HomeAddressActivity.myNotation))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(NewCardAddressMapActivity.lat))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(NewCardAddressMapActivity.lng))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            //if (!String.IsNullOrEmpty(myBirthDate))
            //{
            //    ShowBackAlert();
            //    return;
            //}
            if (ChangedSomething)
            {
                ShowBackAlert();
                return;
            }
            //if (SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl != null)
            //if (SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Count > 0)
            //{
            //    this.PresentViewController(option_back, true, null);
            //    return;
            //}
            base.OnBackPressed();
            //}
            //}else
            //}
            //}    resetBn.Visibility = ViewStates.Visible;
            //}    //view_for_picker.Hidden = true;
            //}    //scrollView.Hidden = false;
        }


        private void ShowBackAlert()
        {
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("goBackWithoutSavingData", _ci));
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("confirm", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
                EditPersonalImageAdapter.Photos?.Clear();
                base.OnBackPressed();
            });
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        private void ShowResetAlert()
        {
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("reset", _ci) + "?");
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("confirm", _ci), (object sender1, DialogClickEventArgs e1) => ResetData());
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        private void FillFields()
        {
            _databaseMethods.InsertLoginedFrom(Constants.from_card_creating_premium);
            //var timer = new System.Timers.Timer();
            //timer.Interval = 50;
            try
            {
                //if (SocialNetworkTableViewSource<int, int>._checkedRows != null)
                //{
                //    if (SocialNetworkTableViewSource<int, int>.selectedIndexes.Count != 0)
                //    {
                //        social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //        social_netw_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                //    }
                //    else
                //    {
                //        social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //        //social_netw_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                //        social_netw_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
                //    }
                //}
                //else
                //{
                //    social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //    //social_netw_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                //    social_netw_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155)/*FromRGB(75, 75, 75)*/, UIControlState.Normal);
                //}
            }
            catch { }
            if (
                String.IsNullOrEmpty(HomeAddressActivity.FullAddressStatic) &&
                String.IsNullOrEmpty(HomeAddressActivity.MyCountry) &&
                String.IsNullOrEmpty(HomeAddressActivity.MyRegion) &&
                String.IsNullOrEmpty(HomeAddressActivity.MyCity) &&
                String.IsNullOrEmpty(HomeAddressActivity.MyIndex) &&
                String.IsNullOrEmpty(HomeAddressActivity.MyNotation) &&
                String.IsNullOrEmpty(NewCardAddressMapActivity.Lat) &&
                String.IsNullOrEmpty(NewCardAddressMapActivity.Lng)
            )
            {
                //address_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //address_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                _homeAddressTv.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColor));//UIColor.FromRGB(146, 150, 155)/*FromRGB(75, 75, 75)*/, UIControlState.Normal);
            }
            else
            {
                _homeAddressTv.SetTextColor(Color.White);
            }
            CheckBirthdate();
            _socialNetworkTv.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColor));
            foreach (var item in SocialNetworkAdapter.SocialNetworks)
            {
                if (!String.IsNullOrEmpty(item.UsersUrl))
                {
                    _socialNetworkTv.SetTextColor(Resources.GetColor(Resource.Color.vk_white));
                    break;
                }
            }
            ////timer.Elapsed += delegate
            ////{
            ////timer.Stop();
            ////timer.Dispose();
            ////InvokeOnMainThread(async () =>
            ////{
            if (!String.IsNullOrEmpty(MySurname))
                _surnameEt.Text = MySurname;
            if (!String.IsNullOrEmpty(MyName))
                _nameMiddlenameEt.Text = MyName + " " + MyMiddlename;
            if (!String.IsNullOrEmpty(MyPhone))
                _mobilePhoneEt.Text = MyPhone;
            if (!String.IsNullOrEmpty(MyEmail))
                _emailEt.Text = MyEmail;
            if (!String.IsNullOrEmpty(MyHomePhone))
                _homePhoneEt.Text = MyHomePhone;
            if (!String.IsNullOrEmpty(MySite))
            {
                if (MySite.ToLower().Contains("https://"))
                    MySite = MySite.Remove(0, "https://".Length);
                _siteEt.Text = MySite;
            }
            if (!String.IsNullOrEmpty(MyDegree))
                _degreeEt.Text = MyDegree;
            if (!String.IsNullOrEmpty(MyCardName))
                _cardNameEt.Text = MyCardName;
            //UIApplication.SharedApplication.KeyWindow.EndEditing(true);
            ////await Task.Delay(300);
            //scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height / 2));
            //if (deviceModel.Contains("e 5") || deviceModel.Contains("e 4") || deviceModel.ToLower().Contains("se"))
            //    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 1.8));
            //else
            //    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 2 - 350));
            //scrollView.ContentOffset = new CGPoint(0, 0);
            //scrollView.Hidden = false;
            //activityIndicator.Hidden = true;
            //    });
            //};
            //timer.Start();

            _initFinished = true;
        }
        private void CheckBirthdate()
        {
            if (!String.IsNullOrEmpty(MyBirthDate))
            {
                //birthday_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                _birthdateMainTv.SetTextColor(Color.White);
                _birthdateValueTv.Text = MyBirthDate;
                SetBirthdateTitle();
            }
            else
            {
                _birthdateMainTv.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColor));
                SetBirthdateTitle();
            }
        }

        void SetBirthdateTitle()
        {
            string[] birthArray = null;
            try { birthArray = MyBirthDate.Split('-'); } catch { return; }
            try { _birthdateValueTv.Text = birthArray[2] + "." + birthArray[1] + "." + birthArray[0]; }
            catch { _birthdateValueTv.Text = MyBirthDate; }
            //var ddtime = new DatePickerDialog(this, OnDateSet, Convert.ToInt32(birth_array[2]), Convert.ToInt32(birth_array[1]),
            //Convert.ToInt32(birth_array[0])
            //);

            if (_ddtime == null)
                _ddtime = new DatePickerDialog(this, OnDateSet, DateTime.Today.Year, DateTime.Today.Month - 1,
                                                     DateTime.Today.Day
                                                             );
            try
            {
                _selectedDate = new DateTime(year: Convert.ToInt32(birthArray[0]), month: Convert.ToInt32(birthArray[1]), day: Convert.ToInt32(birthArray[2]));
                //DatePickerDialog(this, )
                _ddtime.DatePicker.DateTime = _selectedDate;
            }
            catch
            {

            }
        }
        //void OnOldImageClick(object sender, EventArgs e)
        //{
        //    int index = 0;
        //    //current_imgBn = sender as UIButton;
        //    foreach (var img in EditViewController.images_from_server_list)
        //    {
        //        if (current_imgBn.CurrentImage == img)
        //        {
        //            CropOldPersonalImageViewController.global_index = index;
        //            CropOldPersonalImageViewController.came_from_adding = true;
        //            CropOldPersonalImageViewController.currentImage = current_imgBn.CurrentImage;
        //            var vc = sb.InstantiateViewController(nameof(CropOldPersonalImageViewController));
        //            this.NavigationController.PushViewController(vc, true);
        //            //string[] tableItems = new string[] { "Удалить фото" };

        //            //var id = EditViewController.ids_of_attachments[index];
        //            //normal = new UIActionSheet("Simple ActionSheet", null, "Cancel", "Delete", null);
        //            //option_delete = new UIActionSheet(null, null, "Не удалять", null, tableItems);
        //            //option_delete.Clicked += (btn_sender, args) => //Console.WriteLine("{0} Clicked", args.ButtonIndex);
        //            //{
        //            //    if (args.ButtonIndex == 0)
        //            //    {
        //            //        EditViewController.ids_of_attachments.RemoveAt(index);
        //            //        EditViewController.images_from_server_list.RemoveAt(index);
        //            //        var vc = NavigationController;
        //            //        NavigationController.PopViewController(false);
        //            //        vc.PushViewController(sb.InstantiateViewController(nameof(EditPersonalDataViewController)), false);
        //            //    }
        //            //};
        //            //option_delete.ShowInView(View);
        //            break;
        //        }
        //        //PersonalDataViewController.current_img_id = index;
        //        index++;
        //    }

        //    //int hashBn=sender.GetHashCode();
        //    //CropGalleryViewController.currentImage = current_imgBn.CurrentImage;
        //    //var vc = sb.InstantiateViewController("CropGalleryViewController");
        //    //this.NavigationController.PushViewController(vc, true);
        //}

        //void OnImageClick(object sender, EventArgs e)
        //{
        //    int index = 0;
        //    current_imgBn = sender as UIButton;
        //    foreach (var img in PersonalDataViewController.images_list)
        //    {
        //        if (current_imgBn.CurrentImage == img)
        //            PersonalDataViewController.current_img_id = index;
        //        index++;
        //    }

        //    CropGalleryViewController.came_from_adding = false;
        //    CropGalleryViewController.currentImage = current_imgBn.CurrentImage;
        //    var vc = sb.InstantiateViewController(nameof(CropGalleryViewController));
        //    this.NavigationController.PushViewController(vc, true);
        //}

        // Optional parameters for calling this method from viewHolder
        public void StartCropActivity(Android.Net.Uri uri, Activity context = null, int pressedPhotoPos = 0)
        {
            UCrop uCrop;
            _pressedPhotoPosition = pressedPhotoPos;
            if (context != null)
                uCrop = UCrop.Of(uri, Android.Net.Uri.FromFile(new Java.IO.File(context.CacheDir, Constants.destinationFileName)));
            else
                uCrop = UCrop.Of(uri, Android.Net.Uri.FromFile(new Java.IO.File(CacheDir, Constants.destinationFileName)));
            uCrop.WithAspectRatio(1, 1);

            uCrop.WithOptions(_nativeMethods.UCropOptions());

            if (context != null)
                uCrop.Start(context);
            else
                uCrop.Start(this);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == UCrop.RequestCrop)
            {
                if (data != null)
                {
                    ChangedSomething = true;
                    Android.Net.Uri resultUri = UCrop.GetOutput(data);
                    if (resultUri != null)
                    {
                        try
                        {
                            if (_pressedPhotoPosition == 0)
                                EditPersonalImageAdapter.Photos.Add(NativeMethods.GetBitmapFromUrl(resultUri.Path));
                            else
                            {
                                EditPersonalImageAdapter.Photos.RemoveAt(_pressedPhotoPosition);
                                EditPersonalImageAdapter.Photos.Insert(_pressedPhotoPosition, NativeMethods.GetBitmapFromUrl(resultUri.Path));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!_methods.IsConnected())
                            {
                                NoConnectionActivity.ActivityName = this;
                                StartActivity(typeof(NoConnectionActivity));
                                Finish();
                                return;
                            }
                        }
                    }
                }
                _editPersonalImageAdapter.NotifyDataSetChanged();
                return;
            }
            if (PictureMethods.CameraOrGalleryIndicator == Constants.gallery)
            {
                if (resultCode == Result.Ok)
                {
                    //FindViewById<ImageView>(Resource.Id.imageView1).SetImageURI(data.Data);
                    var res = _nativeMethods.CheckStoragePermissions(this);
                    if (!res.Result)
                        return;
                    try
                    {
                        StartCropActivity(data.Data);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else if (PictureMethods.CameraOrGalleryIndicator == Constants.camera)
            {
                int height = Resources.DisplayMetrics.HeightPixels;
                int width = Resources.DisplayMetrics.WidthPixels;
                var path = App.File.Path;

                Android.Net.Uri uri = Android.Net.Uri.FromFile(new Java.IO.File(path));
                Stream input = null;
                try
                {
                    input = this.ContentResolver.OpenInputStream(uri);
                }
                catch (Exception ex)
                {
                    var res = _nativeMethods.CheckStoragePermissions(this);
                    if (!res.Result)
                        return;
                }
                ////Use bitarray to use less memory                    
                //byte[] buffer = new byte[16 * 1024];
                //byte[] pictByteArray;

                if (input == null)
                {
                    //Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", ci), ToastLength.Long).Show();
                    //Task.Delay(1000);
                    return;
                }

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    int read;
                //    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                //    {
                //        ms.Write(buffer, 0, read);
                //    }
                //    pictByteArray = ms.ToArray();
                //}

                input.Close();
                // Dispose of the Java side bitmap.
                GC.Collect();
                try
                {
                    StartCropActivity(uri);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public override void OnBackPressed()
        {
            //base.OnBackPressed();
            BackClicked();
        }

        public bool AreStorageAndCamPermissionsGranted(Activity context)
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (
                             context.CheckSelfPermission(Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted
                          || context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted
                          || context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    // ActivityCompat.RequestPermissions(context, new String[]
                    //{
                    //             Manifest.Permission.Camera,
                    //             Manifest.Permission.ReadExternalStorage,
                    //             Manifest.Permission.WriteExternalStorage,
                    //}, REQUEST_PERMISSION_CODE);
                    return false;
                }
            return true;
        }

        public async Task<bool> CheckStoragePermissions()
        {
            var permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

            if (permissionStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
                permissionStatus = results[Permission.Location];
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

        public async Task<bool> CheckStorageAndCameraPermissions()
        {
            PermissionStatus permissionStorageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            PermissionStatus permissionCameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);

            if (permissionStorageStatus != PermissionStatus.Granted || permissionCameraStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
                permissionStorageStatus = results[Permission.Location];
                //RequestRuntimePermissions();
                RequestCameraAndStoragePermissions();
                return false;
            }
            else
            {
                //Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", ci), ToastLength.Long).Show();
                return true;
            }
        }

        private const int requestPermissionCode = 1000;
        public void RequestCameraAndStoragePermissions()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (
                             CheckSelfPermission(Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted
                          || CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted
                          || CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                    {
                                Manifest.Permission.Camera,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, requestPermissionCode);
                }
                else
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                    {
                                Manifest.Permission.Camera,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, requestPermissionCode);
                }
        }
        public void RequestStoragePermissions()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted
                          || CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                    {
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, requestPermissionCode);
                }
                else
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                    {
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                    }, requestPermissionCode);
                }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case requestPermissionCode:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                        {
                            PhotosPermissionAllowed?.Invoke(null, null);
                            //if (_continueBnPressed)
                            //{
                            //    ContinueBn_Click(null, null);
                            //}
                            //StartActivity(typeof(MainActivity));
                        }
                        else
                        {
                            //if (!shown)
                            //    Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", ci), ToastLength.Long).Show();
                            //shown = true;
                            //request_runtime_permissions();
                        }
                        break;
                    }
            }
        }
    }
}

