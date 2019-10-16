using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;
using Com.Yalantis.Ucrop;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class EditCompanyDataActivity : Activity
    {
        EditText _companyNameEt, _linesOfBusinessEt, _positionEt, _foundationYearEt, _clientsEt, _companyPhoneEt, _companyAdditionalPhoneEt, _corporativePhoneEt, _corporativePhoneAdditionalEt, _faxEt, _faxAdditionalEt, _companyEmailEt, _corporativeSiteEt;
        TextInputLayout _companyNameTil;
        TextView _headerTv, _logoTv, _companyAddressTv, _conditionsTv;
        RelativeLayout _backRl, _logoRl, _companyAddressRl;
        ImageView _logoWithImageIv, _companyLogoimageIv, _addressLockIv, _linesOfBusinessLockIv, _yearLockIv, _clientsLockIv, _companyPhoneLockIv, _companyAdditPhoneLockIv, _corporativePhoneLockIv, _corporativeAdditPhoneLockIv, _faxLockIv, _faxAdditLockIv, _emailLockIv, _siteLockIv;
        ProgressBar _activityLogoIndicator;
        Button _resetBn, _createCardBn;
        ProgressBar _activityIndicator;
        public static string CompanyName, LinesOfBusiness, Position, FoundationYear, Clients, CompanyPhone, CorporativePhone, Fax, CompanyEmail, CorporativeSite;
        public static int? LogoId;
        public static bool CompanyNull;
        public static bool ChangedSomething, ChangedCompanyData;
        public static Bitmap CroppedResult;
        ScrollView _scrollView;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        PictureMethods _pictureMethods = new PictureMethods();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        NativeMethods _nativeMethods = new NativeMethods();
        Methods _methods = new Methods();
        static bool _initFinished = false;

        protected override void OnResume()
        {
            base.OnResume();
            _initFinished = false;
            FillFields();

            if (EditActivity.IsCompanyReadOnly)
                DisableFields();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (!EditActivity.IsCompanyReadOnly)
                SetContentView(Resource.Layout.CompanyData);
            else
                SetContentView(Resource.Layout.CompanyDataDisabled);
            ChangedSomething = false;
            _initFinished = false;
            InitElements();
            _logoRl.Click += async (s, e) =>
            {
                if (_logoWithImageIv.Visibility == ViewStates.Gone)
                {
                    if (!AreStorageAndCamPermissionsGranted())
                        return;
                    ShowPhotoOrGalleryPopup();
                }
                else
                    ShowAlternativePopup();
            };
            _logoRl.LongClick += (s, e) =>
            {
                if (_logoWithImageIv.Visibility == ViewStates.Visible)
                    ShowAlternativePopup();
            };

            _backRl.Click += (s, e) => BackClicked();
            _resetBn.Click += (s, e) => ShowResetAlert();

            if (String.IsNullOrEmpty(QrActivity.ExtraEmploymentData) && !EditActivity.IsCompanyReadOnly)
                hang_locks();

            _companyAddressRl.Click += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                    StartActivity(typeof(CompanyAddressActivity));
                else
                    _nativeMethods.CallPremiumOptionMenu(this).Show();
            };

            _companyNameEt.TextChanged += (s, e) =>
            {
                CompanyName = _companyNameEt.Text;
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
            _linesOfBusinessEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                    LinesOfBusiness = _linesOfBusinessEt.Text;
                else
                {
                    if (!String.IsNullOrEmpty(_linesOfBusinessEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _linesOfBusinessEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedCompanyData = true;
                    ChangedSomething = true;
                }
            };
            _positionEt.TextChanged += (s, e) =>
            {
                Position = _positionEt.Text;
                if (_initFinished)
                    ChangedSomething = true;
            };
            _foundationYearEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                    FoundationYear = _foundationYearEt.Text;
                else
                {
                    if (!String.IsNullOrEmpty(_foundationYearEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _foundationYearEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedCompanyData = true;
                    ChangedSomething = true;
                }
            };
            _clientsEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                    Clients = _clientsEt.Text;
                else
                {
                    if (!String.IsNullOrEmpty(_clientsEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _clientsEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedCompanyData = true;
                    ChangedSomething = true;
                }
            };

            _createCardBn.Click += async (s, e) =>
            {
                string errorMessage = "";
                if (String.IsNullOrEmpty(CompanyName))
                    errorMessage += TranslationHelper.GetString("enterCompanyName", _ci);
                try
                {
                    if (!String.IsNullOrEmpty(FoundationYear))
                    {
                        int yearTemp = Convert.ToInt32(FoundationYear);
                        if (yearTemp < 1700 || yearTemp > 2999)
                            errorMessage += TranslationHelper.GetString("incorrectFoundationYear", _ci);
                    }
                }
                catch
                {
                    errorMessage += TranslationHelper.GetString("incorrectFoundationYear", _ci);
                }
                try
                {
                    if (!String.IsNullOrEmpty(CompanyPhone))
                    {
                        if (CompanyPhone.Length > 31)
                            errorMessage += TranslationHelper.GetString("companyPhoneIncorrect", _ci);
                    }
                }
                catch
                {
                    errorMessage += TranslationHelper.GetString("companyPhoneIncorrect", _ci);
                }
                try
                {
                    if (!String.IsNullOrEmpty(Fax))
                    {
                        if (Fax.Length > 31)
                            errorMessage += TranslationHelper.GetString("faxIncorrect", _ci);
                    }
                }
                catch
                {
                    errorMessage += TranslationHelper.GetString("faxIncorrect", _ci);
                }
                if (!String.IsNullOrEmpty(CompanyEmail))
                {
                    try
                    {
                        /*_companyEmailEt.Text =*/
                        _methods.EmailValidation(CompanyEmail);
                    }
                    catch
                    {
                        errorMessage += TranslationHelper.GetString("emailIncorrect", _ci);
                    }
                }
                /*try
                {
                  if (corporativeSite.ToLower().Contains("http://") || corporativeSite.ToLower().Contains("https://"))
                    {

                    }
                    else
                        error_message += "Сайт должен начинаться с http://, https:// ";
                }
                catch
                {
                    error_message += "Сайт должен начинаться с http://, https:// ";
                }*/
                CompanyNull = false;
                if (
                  String.IsNullOrEmpty(CompanyName) &&
                  String.IsNullOrEmpty(LinesOfBusiness) &&
                  String.IsNullOrEmpty(Position) &&
                  String.IsNullOrEmpty(FoundationYear) &&
                  String.IsNullOrEmpty(Clients) &&
                  String.IsNullOrEmpty(CompanyPhone) &&
                  String.IsNullOrEmpty(CorporativePhone) &&
                  String.IsNullOrEmpty(Fax) &&
                  String.IsNullOrEmpty(CompanyEmail) &&
                  String.IsNullOrEmpty(CorporativeSite) &&
                  String.IsNullOrEmpty(CompanyAddressActivity.Country) &&
                  String.IsNullOrEmpty(CompanyAddressActivity.Region) &&
                  String.IsNullOrEmpty(CompanyAddressActivity.City) &&
                  String.IsNullOrEmpty(CompanyAddressActivity.Index) &&
                  String.IsNullOrEmpty(CompanyAddressActivity.Notation) &&
                  //String.IsNullOrEmpty(CompanyAddressActivity.lat) &&
                  //String.IsNullOrEmpty(CompanyAddressActivity.lng) &&
                  String.IsNullOrEmpty(CompanyAddressMapActivity.CompanyLat) &&
                  String.IsNullOrEmpty(CompanyAddressMapActivity.CompanyLng) &&
                    _logoWithImageIv.Visibility == ViewStates.Gone)
                {
                    StartActivity(typeof(RemoveCompanyProcessActivity));
                    return;
                }
                if (String.IsNullOrEmpty(errorMessage))
                {
                    // Caching card to db.
                    //var TaskA = new Task(async () => await CacheLogo());

                    //TaskA.Start();
                    _activityIndicator.Visibility = ViewStates.Visible;
                    _createCardBn.Visibility = ViewStates.Gone;
                    if (!await Task.Run(async () => await CacheLogo()))
                        return;
                    _activityIndicator.Visibility = ViewStates.Gone;
                    _createCardBn.Visibility = ViewStates.Visible;
                    CachOtherData();
                    if (_databaseMethods.UserExists())
                        StartActivity(typeof(EditCompanyProcessActivity));
                    else
                    {
                        _databaseMethods.InsertLoginedFrom(Constants.from_card_creating);
                        StartActivity(typeof(ConfirmEmailActivity));
                    }
                }
                else
                {
                    _companyNameTil.ErrorEnabled = false;

                    if (String.IsNullOrEmpty(_companyNameEt.Text))
                    {
                        _companyNameTil.ErrorEnabled = true;
                        _companyNameTil.Error = TranslationHelper.GetString("requiredData", _ci);
                        _scrollView.ScrollTo(0, 0);
                    }
                    Toast.MakeText(this, errorMessage, ToastLength.Long).Show();
                }
            };
            if (string.IsNullOrEmpty(QrActivity.ExtraPersonData))
                SubscribeTouches();

            _companyPhoneEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(_companyPhoneEt.Text))
                    {
                        try
                        {
                            char l = _companyPhoneEt.Text[_companyPhoneEt.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                CompanyPhone = _companyPhoneEt.Text;
                            else
                                _companyPhoneEt.Text = _companyPhoneEt.Text.Remove(_companyPhoneEt.Text.Length - 1);
                            _companyPhoneEt.SetSelection(_companyPhoneEt.Text.Length);
                        }
                        catch { }
                    }
                    else
                        CompanyPhone = _companyPhoneEt.Text;
                }
                else
                {
                    if (!String.IsNullOrEmpty(_companyPhoneEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _companyPhoneEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
            _companyAdditionalPhoneEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(_companyAdditionalPhoneEt.Text))
                    {
                        try
                        {
                            char l = _companyAdditionalPhoneEt.Text[_companyAdditionalPhoneEt.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                return;
                            else
                                _companyAdditionalPhoneEt.Text = _companyAdditionalPhoneEt.Text.Remove(_companyAdditionalPhoneEt.Text.Length - 1);
                            _companyAdditionalPhoneEt.SetSelection(_companyAdditionalPhoneEt.Text.Length);
                        }
                        catch { }
                    }

                }
                else
                {
                    if (!String.IsNullOrEmpty(_companyAdditionalPhoneEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _companyAdditionalPhoneEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
            _faxAdditionalEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(_faxAdditionalEt.Text))
                    {
                        try
                        {
                            char l = _faxAdditionalEt.Text[_faxAdditionalEt.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                return;
                            else
                                _faxAdditionalEt.Text = _faxAdditionalEt.Text.Remove(_faxAdditionalEt.Text.Length - 1);
                            _faxAdditionalEt.SetSelection(_faxAdditionalEt.Text.Length);
                        }
                        catch { }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(_faxAdditionalEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _faxAdditionalEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
            _corporativePhoneEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(_corporativePhoneEt.Text))
                    {
                        try
                        {
                            char l = _corporativePhoneEt.Text[_corporativePhoneEt.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                CorporativePhone = _corporativePhoneEt.Text;
                            else
                                _corporativePhoneEt.Text = _corporativePhoneEt.Text.Remove(_corporativePhoneEt.Text.Length - 1);
                            _corporativePhoneEt.SetSelection(_corporativePhoneEt.Text.Length);
                        }
                        catch { }
                    }
                    else
                        CorporativePhone = _corporativePhoneEt.Text;
                }
                else
                {
                    if (!String.IsNullOrEmpty(_corporativePhoneEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _corporativePhoneEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
            _corporativePhoneAdditionalEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(_corporativePhoneAdditionalEt.Text))
                    {
                        try
                        {
                            char l = _corporativePhoneAdditionalEt.Text[_corporativePhoneAdditionalEt.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                return;
                            else
                                _corporativePhoneAdditionalEt.Text = _corporativePhoneAdditionalEt.Text.Remove(_corporativePhoneAdditionalEt.Text.Length - 1);
                            _corporativePhoneAdditionalEt.SetSelection(_corporativePhoneAdditionalEt.Text.Length);
                        }
                        catch { }
                    }

                }
                else
                {
                    if (!String.IsNullOrEmpty(_corporativePhoneAdditionalEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _corporativePhoneAdditionalEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
            _faxEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(_faxEt.Text))
                    {
                        try
                        {
                            char l = _faxEt.Text[_faxEt.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                Fax = _faxEt.Text;
                            else
                                _faxEt.Text = _faxEt.Text.Remove(_faxEt.Text.Length - 1);
                            _faxEt.SetSelection(_faxEt.Text.Length);
                        }
                        catch { }
                    }
                    else
                        Fax = _faxEt.Text;
                }
                else
                {
                    if (!String.IsNullOrEmpty(_faxEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _faxEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
            _companyEmailEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                    CompanyEmail = _companyEmailEt.Text;
                else
                {
                    if (!String.IsNullOrEmpty(_companyEmailEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _companyEmailEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
            _corporativeSiteEt.TextChanged += (s, e) =>
            {
                if (QrActivity.ExtraEmploymentData != null)
                    CorporativeSite = _corporativeSiteEt.Text;
                else
                {
                    if (!String.IsNullOrEmpty(_corporativeSiteEt.Text))
                    {
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                        _corporativeSiteEt.Text = null;
                    }
                }
                if (_initFinished)
                {
                    ChangedSomething = true;
                    ChangedCompanyData = true;
                }
            };
        }

        private void SubscribeTouches()
        {
            _linesOfBusinessEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_linesOfBusinessEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _foundationYearEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_foundationYearEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _clientsEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_clientsEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _companyPhoneEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_companyPhoneEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _companyAdditionalPhoneEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_companyAdditionalPhoneEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _corporativePhoneEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_corporativePhoneEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _corporativePhoneAdditionalEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_corporativePhoneAdditionalEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _faxEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_faxEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _faxAdditionalEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_faxAdditionalEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _companyEmailEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_companyEmailEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
            _corporativeSiteEt.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (QrActivity.ExtraPersonData != null)
                        CallKeyboard(_corporativeSiteEt);
                    else
                        _nativeMethods.CallPremiumOptionMenu(this).Show();
                }
            };
        }

        private void DisableFields()
        {
            _resetBn.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColorDisabled));
            _resetBn.Enabled = false;
            _nativeMethods.DisableEditText(this, _companyNameEt);
            _nativeMethods.DisableEditText(this, _linesOfBusinessEt);
            //_nativeMethods.DisableEditText(this, _positionEt);
            _nativeMethods.DisableEditText(this, _foundationYearEt);
            _nativeMethods.DisableEditText(this, _clientsEt);
            _nativeMethods.DisableEditText(this, _companyPhoneEt);
            _nativeMethods.DisableEditText(this, _companyAdditionalPhoneEt);
            _nativeMethods.DisableEditText(this, _corporativePhoneEt);
            _nativeMethods.DisableEditText(this, _corporativePhoneAdditionalEt);
            _nativeMethods.DisableEditText(this, _faxEt);
            _nativeMethods.DisableEditText(this, _faxAdditionalEt);
            _nativeMethods.DisableEditText(this, _companyEmailEt);
            _nativeMethods.DisableEditText(this, _corporativeSiteEt);
            //_companyAddressTv.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColorDisabled));
            //_companyAddressRl.Enabled = false;
            _logoTv.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColorDisabled));
            _logoRl.Enabled = false;
            //_createCardBn.Enabled = false;
            //_createCardBn.SetBackgroundColor(Resources.GetColor(Resource.Color.editTextLineColorDisabled));
        }

        private void CallKeyboard(EditText editText)
        {
            InputMethodManager inputMethodManager = this.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.ShowSoftInput(editText, ShowFlags.Forced);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
            editText.RequestFocus();
        }

        private async Task<bool> CacheLogo()
        {
            var delRes = await _nativeMethods.RemoveLogo();
            if (CroppedResult != null)
            {
                var res = await _nativeMethods.CacheLogo(CroppedResult);
            }

            return true;
        }

        void CachOtherData()
        {
            var timestampUtc = DateTime.UtcNow.ToString();
            //CardDoneActivity.variant_displaying = 1;

            CompanyPhone = _companyPhoneEt.Text;
            if (!String.IsNullOrEmpty(_companyPhoneEt.Text))
                if (!String.IsNullOrEmpty(_companyAdditionalPhoneEt.Text))
                    CompanyPhone += "#" + _companyAdditionalPhoneEt.Text;
            CorporativePhone = _corporativePhoneEt.Text;
            if (!String.IsNullOrEmpty(_corporativePhoneEt.Text))
                if (!String.IsNullOrEmpty(_corporativePhoneAdditionalEt.Text))
                    CorporativePhone += "#" + _corporativePhoneAdditionalEt.Text;
            Fax = _faxEt.Text;
            if (!String.IsNullOrEmpty(_faxEt.Text))
                if (!String.IsNullOrEmpty(_faxAdditionalEt.Text))
                    Fax += "#" + _faxAdditionalEt.Text;

            _databaseMethods.InsertCompanyCard(
            CompanyName,
            LinesOfBusiness,
            Position,
            FoundationYear,
            Clients,
            CompanyPhone,
            CorporativePhone,
            CompanyEmail,
            Fax,
            CorporativeSite,
            CompanyAddressActivity.Country,
            CompanyAddressActivity.Region,
            CompanyAddressActivity.City,
            CompanyAddressActivity.FullCompanyAddressStatic,
            CompanyAddressActivity.Index,
            CompanyAddressActivity.Notation,
            CompanyAddressMapActivity.CompanyLat,
            CompanyAddressMapActivity.CompanyLng,
            timestampUtc
            );
            _databaseMethods.GetDataFromCompanyCard();
        }

        private void hang_locks()
        {
            _addressLockIv.Visibility = ViewStates.Visible;
            _linesOfBusinessLockIv.Visibility = ViewStates.Visible;
            _yearLockIv.Visibility = ViewStates.Visible;
            _clientsLockIv.Visibility = ViewStates.Visible;
            _companyPhoneLockIv.Visibility = ViewStates.Visible;
            _companyAdditPhoneLockIv.Visibility = ViewStates.Visible;
            _corporativePhoneLockIv.Visibility = ViewStates.Visible;
            _corporativeAdditPhoneLockIv.Visibility = ViewStates.Visible;
            _faxLockIv.Visibility = ViewStates.Visible;
            _faxAdditLockIv.Visibility = ViewStates.Visible;
            _emailLockIv.Visibility = ViewStates.Visible;
            _siteLockIv.Visibility = ViewStates.Visible;
        }

        private void FillFields()
        {

            if (
                String.IsNullOrEmpty(CompanyAddressActivity.FullCompanyAddressStatic) &&
                String.IsNullOrEmpty(CompanyAddressActivity.Country) &&
                String.IsNullOrEmpty(CompanyAddressActivity.Region) &&
                String.IsNullOrEmpty(CompanyAddressActivity.City) &&
                String.IsNullOrEmpty(CompanyAddressActivity.Index) &&
                String.IsNullOrEmpty(CompanyAddressActivity.Notation) &&
                String.IsNullOrEmpty(CompanyAddressMapActivity.CompanyLat) &&
                String.IsNullOrEmpty(CompanyAddressMapActivity.CompanyLng)
            )
            {
                //addressMainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                _companyAddressTv.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColor));
            }
            else
            {
                _companyAddressTv.SetTextColor(Color.White);
            }


            if (!String.IsNullOrEmpty(CompanyName))
                _companyNameEt.Text = CompanyName;
            if (!String.IsNullOrEmpty(LinesOfBusiness))
                _linesOfBusinessEt.Text = LinesOfBusiness;
            if (!String.IsNullOrEmpty(Position))
                _positionEt.Text = Position;
            if (!String.IsNullOrEmpty(FoundationYear))
                _foundationYearEt.Text = FoundationYear;
            if (!String.IsNullOrEmpty(Clients))
                _clientsEt.Text = Clients;
            if (!String.IsNullOrEmpty(CompanyPhone))
            {
                if (!CompanyPhone.Contains("#"))
                    _companyPhoneEt.Text = CompanyPhone;
                else
                {
                    var array = CompanyPhone.Split("#");
                    try
                    {
                        _companyPhoneEt.Text = array[0];
                        _companyAdditionalPhoneEt.Text = array[1];
                    }
                    catch
                    {
                        _companyPhoneEt.Text = array[0];
                    }
                }
            }
            if (!String.IsNullOrEmpty(CorporativePhone))
            {
                if (!CorporativePhone.Contains("#"))
                    _corporativePhoneEt.Text = CorporativePhone;
                else
                {
                    var array = CorporativePhone.Split("#");
                    try
                    {
                        _corporativePhoneEt.Text = array[0];
                        _corporativePhoneAdditionalEt.Text = array[1];
                    }
                    catch
                    {
                        _corporativePhoneEt.Text = array[0];
                    }
                }
            }
            if (!String.IsNullOrEmpty(Fax))
            {
                if (!Fax.Contains("#"))
                    _faxEt.Text = Fax;
                else
                {
                    var array = Fax.Split("#");
                    try
                    {
                        _faxEt.Text = array[0];
                        _faxAdditionalEt.Text = array[1];
                    }
                    catch
                    {
                        _faxEt.Text = array[0];
                    }
                }
            }
            if (!String.IsNullOrEmpty(CompanyEmail))
            {
                _companyEmailEt.Text = CompanyEmail;
            }
            if (!String.IsNullOrEmpty(CorporativeSite))
            {
                if (CorporativeSite.ToLower().Contains("https://"))
                    CorporativeSite = CorporativeSite.Remove(0, "https://".Length);
                _corporativeSiteEt.Text = CorporativeSite;
            }
            //UIApplication.SharedApplication.KeyWindow.EndEditing(true);
            //await Task.Delay(300);
            //scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height / 2));
            //if (deviceModel.Contains("e 5") || deviceModel.Contains("e 4") || deviceModel.ToLower().Contains("se"))
            //    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 1.8));
            //else
            //    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 2 - 300));
            //view_in_scroll.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(scrollView.ContentSize.Height));
            //scrollView.Hidden = false;
            //activityIndicator.Hidden = true;
            if (CroppedResult != null)
            {
                _logoWithImageIv.SetImageBitmap(CroppedResult);
                _logoWithImageIv.Visibility = ViewStates.Visible;
                _logoTv.Visibility = ViewStates.Gone;
                _companyLogoimageIv.Visibility = ViewStates.Gone;
                //companyLogoMainBn.Hidden = true;
            }
            else
            {
                //companyLogoMainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                _logoTv.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColor));
            }
            _initFinished = true;
        }

        void ShowAlternativePopup()
        {
            Android.Support.V7.Widget.PopupMenu popupMenu = new Android.Support.V7.Widget.PopupMenu(this, _logoRl);
            var menuOpts = popupMenu.Menu;
            popupMenu.Inflate(Resource.Layout.photo_option);

            popupMenu.MenuItemClick += async (s1, arg1) =>
            {
                ChangedCompanyData = true;
                //if (_nativeMethods.AreStorageAndCamPermissionsGranted(this))
                //{
                if (arg1.Item.TitleFormatted.ToString() == this.GetString(Resource.String.edit))
                {
                    _activityLogoIndicator.Visibility = ViewStates.Visible;
                    _logoWithImageIv.Visibility = ViewStates.Gone;

                    //logo_with_imageIV.BuildDrawingCache(true);
                    //logo_with_imageIV.GetDrawingCache(true)

                    var uri = await _nativeMethods.ExportBitmapAsJpegAndGetUri(CroppedResult);

                    _activityLogoIndicator.Visibility = ViewStates.Gone;
                    _logoWithImageIv.Visibility = ViewStates.Visible;
                    try
                    {
                        StartCropActivity(uri);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (arg1.Item.TitleFormatted.ToString() == this.GetString(Resource.String.removePhoto))
                {
                    var res = await ResetLogo();
                    popupMenu?.Dismiss();
                }
                //}
                //else
                //_nativeMethods.CheckStoragePermissions(this);
            };
            popupMenu.Show();
        }


        public void StartCropActivity(Android.Net.Uri uri)
        {
            UCrop uCrop;
            uCrop = UCrop.Of(uri, Android.Net.Uri.FromFile(new Java.IO.File(CacheDir, Constants.destinationFileName)));
            uCrop.WithOptions(_nativeMethods.UCropOptions());
            uCrop.WithAspectRatio(1, 1);
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
                        _companyLogoimageIv.Visibility = ViewStates.Gone;
                        _logoWithImageIv.Visibility = ViewStates.Visible;
                        _logoTv.Visibility = ViewStates.Gone;
                        try
                        {
                            CroppedResult = NativeMethods.GetBitmapFromUrl(resultUri.Path);
                            _logoWithImageIv.SetImageBitmap(CroppedResult);
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

                return;
            }
            if (PictureMethods.CameraOrGalleryIndicator == Constants.gallery)
            {
                if (resultCode == Result.Ok)
                {
                    //FindViewById<ImageView>(Resource.Id.imageView1).SetImageURI(data.Data);
                    var res = AreStorageAndCamPermissionsGranted();
                    if (!res)
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
                    var res = AreStorageAndCamPermissionsGranted();
                    if (!res)
                        return;
                }

                if (input == null)
                {
                    //Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", ci), ToastLength.Long).Show();
                    //Task.Delay(1000);
                    return;
                }

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

        void ShowPhotoOrGalleryPopup()
        {
            Android.Support.V7.Widget.PopupMenu popupMenu = new Android.Support.V7.Widget.PopupMenu(this, _logoRl);
            var menuOpts = popupMenu.Menu;
            popupMenu.Inflate(Resource.Layout.camOrGalleryMenu);

            popupMenu.MenuItemClick += (s1, arg1) =>
            {
                ChangedCompanyData = true;
                if (arg1.Item.TitleFormatted.ToString() == GetString(Resource.String.takePhoto))
                {
                    try
                    {
                        if (_pictureMethods.IsThereAnAppToTakePictures(this))
                        {
                            PictureMethods.CameraOrGalleryIndicator = Constants.camera;
                            _pictureMethods.CreateDirectoryForPictures();
                            _pictureMethods.TakeAPicture(this);
                        }
                    }
                    catch
                    {
                        Toast.MakeText(this, TranslationHelper.GetString("failedToOpenTheCamera", _ci), ToastLength.Long).Show();
                    }
                }
                if (arg1.Item.TitleFormatted.ToString() == GetString(Resource.String.uploadFromGallery))
                {
                    PictureMethods.CameraOrGalleryIndicator = "gallery";
                    var imageIntent = new Intent();
                    imageIntent.SetType("image/*");
                    //imageIntent.SetType("file/*");
                    imageIntent.SetAction(Intent.ActionGetContent);
                    StartActivityForResult(
                        Intent.CreateChooser(imageIntent, "Select photo"), 0);
                }
            };
            popupMenu.Show();
        }
        //public void ShowRemovePopup()
        //{
        //    Android.Support.V7.Widget.PopupMenu popup_menu = new Android.Support.V7.Widget.PopupMenu(this, logoRL);
        //    var menuOpts = popup_menu.Menu;
        //    popup_menu.Inflate(Resource.Layout.delete_photo_popup);

        //    popup_menu.MenuItemClick += async (s1, arg1) =>
        //    {
        //        var res = await ResetLogo();
        //    };
        //    popup_menu.Show();
        //}

        private async Task<bool> ResetLogo()
        {
            CroppedResult = null;
            var delRes = await _nativeMethods.RemoveLogo();
            _logoWithImageIv.SetImageBitmap(null);
            _companyLogoimageIv.Visibility = ViewStates.Visible;
            _logoWithImageIv.Visibility = ViewStates.Gone;
            _logoTv.Visibility = ViewStates.Visible;
            return true;
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _scrollView = FindViewById<ScrollView>(Resource.Id.scrollView1);
            _companyNameEt = FindViewById<EditText>(Resource.Id.companyNameET);
            _linesOfBusinessEt = FindViewById<EditText>(Resource.Id.linesOfBusinessET);
            _positionEt = FindViewById<EditText>(Resource.Id.positionET);
            _foundationYearEt = FindViewById<EditText>(Resource.Id.foundationYearET);
            _clientsEt = FindViewById<EditText>(Resource.Id.clientsET);
            _companyPhoneEt = FindViewById<EditText>(Resource.Id.companyPhoneET);
            _companyAdditionalPhoneEt = FindViewById<EditText>(Resource.Id.companyAdditionalPhoneET);
            _corporativePhoneEt = FindViewById<EditText>(Resource.Id.corporativePhoneET);
            _corporativePhoneAdditionalEt = FindViewById<EditText>(Resource.Id.corporativePhoneAdditionalET);
            _faxEt = FindViewById<EditText>(Resource.Id.faxET);
            _faxAdditionalEt = FindViewById<EditText>(Resource.Id.faxAdditionalET);
            _companyEmailEt = FindViewById<EditText>(Resource.Id.companyEmailET);
            _corporativeSiteEt = FindViewById<EditText>(Resource.Id.corporativeSiteET);
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _logoTv = FindViewById<TextView>(Resource.Id.logoTV);
            _companyAddressTv = FindViewById<TextView>(Resource.Id.companyAddressTV);
            _conditionsTv = FindViewById<TextView>(Resource.Id.conditionsTV);
            _backRl = FindViewById<RelativeLayout>(Resource.Id.backRL);
            _logoWithImageIv = FindViewById<ImageView>(Resource.Id.logo_with_imageIV);
            _addressLockIv = FindViewById<ImageView>(Resource.Id.address_lockIV);
            _linesOfBusinessLockIv = FindViewById<ImageView>(Resource.Id.linesOfBusiness_lockIV);
            _yearLockIv = FindViewById<ImageView>(Resource.Id.year_lockIV);
            _clientsLockIv = FindViewById<ImageView>(Resource.Id.clients_lockIV);
            _companyPhoneLockIv = FindViewById<ImageView>(Resource.Id.company_phone_lockIV);
            _companyAdditPhoneLockIv = FindViewById<ImageView>(Resource.Id.company_addit_phone_lockIV);
            _corporativePhoneLockIv = FindViewById<ImageView>(Resource.Id.corporative_phone_lockIV);
            _corporativeAdditPhoneLockIv = FindViewById<ImageView>(Resource.Id.corporative_addit_phone_lockIV);
            _faxLockIv = FindViewById<ImageView>(Resource.Id.fax_lockIV);
            _faxAdditLockIv = FindViewById<ImageView>(Resource.Id.fax_addit_lockIV);
            _emailLockIv = FindViewById<ImageView>(Resource.Id.email_lockIV);
            _siteLockIv = FindViewById<ImageView>(Resource.Id.site_lockIV);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _activityIndicator.Visibility = ViewStates.Gone;
            _addressLockIv.Visibility = ViewStates.Gone;
            _linesOfBusinessLockIv.Visibility = ViewStates.Gone;
            _yearLockIv.Visibility = ViewStates.Gone;
            _clientsLockIv.Visibility = ViewStates.Gone;
            _companyPhoneLockIv.Visibility = ViewStates.Gone;
            _companyAdditPhoneLockIv.Visibility = ViewStates.Gone;
            _corporativePhoneLockIv.Visibility = ViewStates.Gone;
            _corporativeAdditPhoneLockIv.Visibility = ViewStates.Gone;
            _faxLockIv.Visibility = ViewStates.Gone;
            _faxAdditLockIv.Visibility = ViewStates.Gone;
            _emailLockIv.Visibility = ViewStates.Gone;
            _siteLockIv.Visibility = ViewStates.Gone;
            _companyLogoimageIv = FindViewById<ImageView>(Resource.Id.companyLogoimageIV);
            _activityLogoIndicator = FindViewById<ProgressBar>(Resource.Id.activityLogoIndicator);
            _resetBn = FindViewById<Button>(Resource.Id.resetBn);
            _logoRl = FindViewById<RelativeLayout>(Resource.Id.logoRL);
            _createCardBn = FindViewById<Button>(Resource.Id.createCardBn);
            _companyAddressRl = FindViewById<RelativeLayout>(Resource.Id.companyAddressRL);
            _activityLogoIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _activityLogoIndicator.Visibility = ViewStates.Gone;
            _headerTv.Text = TranslationHelper.GetString("aboutCompany", _ci);
            _resetBn.Text = TranslationHelper.GetString("reset", _ci);
            _companyAddressTv.Text = TranslationHelper.GetString("companyAddress", _ci);
            _logoTv.Text = TranslationHelper.GetString("companyLogo", _ci);
            _createCardBn.Text = TranslationHelper.GetString("save", _ci);
            _logoTv.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColor));
            /// Doing this to decrease space. Not Visibility Gone in this case
            _conditionsTv.TextSize = 1F;
            _conditionsTv.Text = "";
            _companyNameTil = FindViewById<TextInputLayout>(Resource.Id.companyNameTIL);

            _companyNameEt.SetTypeface(tf, TypefaceStyle.Normal);
            _linesOfBusinessEt.SetTypeface(tf, TypefaceStyle.Normal);
            _positionEt.SetTypeface(tf, TypefaceStyle.Normal);
            _foundationYearEt.SetTypeface(tf, TypefaceStyle.Normal);
            _clientsEt.SetTypeface(tf, TypefaceStyle.Normal);
            _companyPhoneEt.SetTypeface(tf, TypefaceStyle.Normal);
            _companyAdditionalPhoneEt.SetTypeface(tf, TypefaceStyle.Normal);
            _corporativePhoneEt.SetTypeface(tf, TypefaceStyle.Normal);
            _corporativePhoneAdditionalEt.SetTypeface(tf, TypefaceStyle.Normal);
            _faxEt.SetTypeface(tf, TypefaceStyle.Normal);
            _faxAdditionalEt.SetTypeface(tf, TypefaceStyle.Normal);
            _companyEmailEt.SetTypeface(tf, TypefaceStyle.Normal);
            _corporativeSiteEt.SetTypeface(tf, TypefaceStyle.Normal);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _logoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _companyAddressTv.SetTypeface(tf, TypefaceStyle.Normal);
            _conditionsTv.SetTypeface(tf, TypefaceStyle.Normal);
            _resetBn.SetTypeface(tf, TypefaceStyle.Normal);
            _createCardBn.SetTypeface(tf, TypefaceStyle.Normal);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _resetBn.SetTypeface(tf, TypefaceStyle.Normal);
            _companyAddressTv.SetTypeface(tf, TypefaceStyle.Normal);
            _logoTv.SetTypeface(tf, TypefaceStyle.Normal);
            _createCardBn.SetTypeface(tf, TypefaceStyle.Normal);

            _companyPhoneEt.FocusChange += (s, e) =>
            {
                if (!_companyPhoneEt.IsFocused)
                    if (_companyPhoneEt.Text.Length <= 2)
                    {
                        CompanyPhone = null;
                        _companyPhoneEt.Text = null;
                    }
                if (_companyPhoneEt.IsFocused)
                    if (String.IsNullOrEmpty(CompanyPhone))
                        _companyPhoneEt.Text = "+7";
            };
            _corporativePhoneEt.FocusChange += (s, e) =>
            {
                if (!_corporativePhoneEt.IsFocused)
                    if (_corporativePhoneEt.Text.Length <= 2)
                    {
                        CorporativePhone = null;
                        _corporativePhoneEt.Text = null;
                    }
                if (_corporativePhoneEt.IsFocused)
                    if (String.IsNullOrEmpty(CorporativePhone))
                        _corporativePhoneEt.Text = "+7";
            };
            _faxEt.FocusChange += (s, e) =>
            {
                if (!_faxEt.IsFocused)
                    if (_faxEt.Text.Length <= 2)
                    {
                        Fax = null;
                        _faxEt.Text = null;
                    }
                if (_faxEt.IsFocused)
                    if (String.IsNullOrEmpty(Fax))
                        _faxEt.Text = "+7";
            };
            _logoWithImageIv.Visibility = ViewStates.Gone;
        }

        void ResetData()
        {
            CroppedResult = null;
            CompanyName = null;
            LinesOfBusiness = null;
            Position = null;
            FoundationYear = null;
            Clients = null;
            CompanyPhone = null;
            CorporativePhone = null;
            Fax = null;
            CompanyEmail = null;
            CorporativeSite = null;
            _databaseMethods.ClearCompanyCardTable();
            NativeMethods.ResetCompanyAddress();

            EditCompanyDataActivity.Position = null;
            //if (Directory.Exists(cards_cache_dir))
            //    Directory.Delete(cards_cache_dir, true);
            _nativeMethods.RemoveLogo();
            _companyNameEt.Text = null;
            _linesOfBusinessEt.Text = null;
            _positionEt.Text = null;
            _foundationYearEt.Text = null;
            _clientsEt.Text = null;
            _companyPhoneEt.Text = null;
            _corporativePhoneEt.Text = null;
            _faxEt.Text = null;
            _companyEmailEt.Text = null;
            _corporativeSiteEt.Text = null;
            _corporativePhoneAdditionalEt.Text = null;
            _companyAdditionalPhoneEt.Text = null;
            _faxAdditionalEt.Text = null;
            ResetLogo();
            FillFields();
        }

        private void BackClicked()
        {
            if (ChangedSomething)
            {
                ShowBackAlert();
                return;
            }

            base.OnBackPressed();
        }

        private void ShowBackAlert()
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("goBackWithoutSavingData", _ci));
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("confirm", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
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
            builder.SetPositiveButton(TranslationHelper.GetString("confirm", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
                ChangedCompanyData = true;
                ResetData();
            });
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        public override void OnBackPressed()
        {
            BackClicked();
        }

        private const int REQUEST_PERMISSION_CODE = 1000;
        public bool AreStorageAndCamPermissionsGranted()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (
                             this.CheckSelfPermission(Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted
                          || this.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted
                          || this.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                   {
                                Manifest.Permission.Camera,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                   }, REQUEST_PERMISSION_CODE);
                    return false;
                }
            return true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case REQUEST_PERMISSION_CODE:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                        {
                            if (_logoWithImageIv.Visibility == ViewStates.Gone)
                                ShowPhotoOrGalleryPopup();
                            else
                                ShowAlternativePopup();
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
