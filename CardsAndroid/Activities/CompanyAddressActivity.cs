using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL.CommonMethods;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CompanyAddressActivity : Activity
    {
        TextView _headerTv;
        Button _resetBn, _mapAddressBn, _applyAddressBn, _сoordsRemoveBn;
        RelativeLayout _backRl;
        EditText _countryEt, _regionEt, _cityEt, _detailAddressEt, _indexEt, _notationEt, _coordsEt;
        public static bool CameFromMap { get; set; }
        public static bool ChangedSomething;
        static bool _initFinished = false;
        public static string FullCompanyAddressStatic { get; set; }
        public static string FullCompanyAddressTemp { get; set; }
        public static string Country, Region, City, Index, Notation;
        public static string CountryTemp, RegionTemp, CityTemp, IndexTemp, NotationTemp;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        Methods _methods = new Methods();
        NativeMethods _nativeMethods = new NativeMethods();
        bool _reverseDoneYet;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (!EditActivity.IsCompanyReadOnly)
                SetContentView(Resource.Layout.CompanyAddress);
            else
                SetContentView(Resource.Layout.CompanyAddressDisabled);
            InitElements();
            ChangedSomething = false;
            _initFinished = false;
            _backRl.Click += (s, e) => BackClicked();
            _resetBn.Click += (s, e) => ShowResetAlert();

            _mapAddressBn.Click += (s, e) =>
            {
                //FullAddressStatic
                FullCompanyAddressTemp = _cityEt.Text + " " + _detailAddressEt.Text;
                StartActivity(typeof(CompanyAddressMapActivity));
            };
            _applyAddressBn.Click += (s, e) =>
            {
                EditCompanyDataActivity.ChangedCompanyData = true;
                apply_variables();
                base.OnBackPressed();
            };
            _countryEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    CountryTemp = _countryEt.Text;
            };
            _regionEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    RegionTemp = _regionEt.Text;
            };
            _cityEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    CityTemp = _cityEt.Text;
            };
            _detailAddressEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                //FullAddressStatic
                if (!_reverseDoneYet)
                    FullCompanyAddressTemp = _detailAddressEt.Text;
            };
            _indexEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    IndexTemp = _indexEt.Text;
            };
            _notationEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    NotationTemp = _notationEt.Text;
            };
            _coordsEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
            };
            _сoordsRemoveBn.Click += RemoveCoordsQuestion;

            if (EditActivity.IsCompanyReadOnly)
                DisableFields();
        }
        protected override void OnResume()
        {
            base.OnResume();
            _initFinished = false;
            FillFields();
        }

        private void DisableFields()
        {
            _resetBn.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColorDisabled));
            _resetBn.Enabled = false;
            _nativeMethods.DisableEditText(this, _countryEt);
            _nativeMethods.DisableEditText(this, _regionEt);
            _nativeMethods.DisableEditText(this, _cityEt);
            _nativeMethods.DisableEditText(this, _detailAddressEt);
            _nativeMethods.DisableEditText(this, _indexEt);
            _nativeMethods.DisableEditText(this, _notationEt);
            _nativeMethods.DisableEditText(this, _coordsEt);
            //_applyAddressBn.SetTextColor(Resources.GetColor(Resource.Color.editTextLineColorDisabled));
            _applyAddressBn.Enabled = false;
            _сoordsRemoveBn.Enabled = false;
            _applyAddressBn.SetBackgroundColor(Resources.GetColor(Resource.Color.editTextLineColorDisabled));
        }


        private void FillFields()
        {
            if (!String.IsNullOrEmpty(FullCompanyAddressTemp) && FullCompanyAddressTemp != " " && !String.IsNullOrEmpty(CityTemp))
            {
                if (FullCompanyAddressTemp.Contains(CityTemp))
                {
                    var str = FullCompanyAddressTemp.Substring(0, CityTemp.Length);
                    if (str == CityTemp)
                    {
                        FullCompanyAddressTemp = FullCompanyAddressTemp.Remove(0, CityTemp.Length + 1);
                    }
                }
            }
            else
                FullCompanyAddressTemp = null;


            if (!String.IsNullOrEmpty(Country))
            {
                _countryEt.Text = Country;
            }
            if (!String.IsNullOrEmpty(City))
            {
                _cityEt.Text = City;
            }
            if (!String.IsNullOrEmpty(Region))
            {
                _regionEt.Text = Region;
            }
            if (!String.IsNullOrEmpty(Index))
            {
                _indexEt.Text = Index;
            }
            if (!String.IsNullOrEmpty(FullCompanyAddressStatic))
            {
                _detailAddressEt.Text = FullCompanyAddressStatic;
            }
            if (!String.IsNullOrEmpty(Notation))
            {
                _notationEt.Text = Notation;
            }
            if (!String.IsNullOrEmpty(CompanyAddressMapActivity.CompanyLat) && !String.IsNullOrEmpty(CompanyAddressMapActivity.CompanyLng))
            {
                _coordsEt.Text = "N" + CompanyAddressMapActivity.CompanyLat + " E" + CompanyAddressMapActivity.CompanyLng;
            }
            if (CameFromMap)
                AddressChangeRequest();

            _initFinished = true;
        }

        async void AddressChangeRequest()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(TranslationHelper.GetString("changeAddressInAccordanceWithTheMarkedPoint", _ci));
            builder.SetPositiveButton(TranslationHelper.GetString("replace", _ci), async (object sender1, DialogClickEventArgs e1) =>
            {
                var res_ = await ReverseGeocodeToConsoleAsync();
                InitializeReverseValues();
            });
            builder.SetCancelable(true);
            builder.SetNegativeButton(TranslationHelper.GetString("doNotReplace", _ci), (object sender1, DialogClickEventArgs e1) => { });
            AlertDialog dialog = builder.Create();

            CameFromMap = false;
            if (!String.IsNullOrEmpty(FullCompanyAddressStatic))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(FullCompanyAddressTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(Country))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(Region))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(City))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(Index))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(Notation))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(CountryTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(RegionTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(CityTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(IndexTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(NotationTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(_countryEt.Text))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(_regionEt.Text))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(_cityEt.Text))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(_detailAddressEt.Text))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(_indexEt.Text))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(_notationEt.Text))
            {
                dialog.Show();
                return;
            }
            string res = null;
            try
            {
                res = await ReverseGeocodeToConsoleAsync();
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
            if (String.IsNullOrEmpty(res))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            InitializeReverseValues();
        }

        async Task<string> ReverseGeocodeToConsoleAsync()
        {
            RunOnUiThread(() =>
            {
                if (String.IsNullOrEmpty(CompanyAddressMapActivity.CompanyLat) || String.IsNullOrEmpty(CompanyAddressMapActivity.CompanyLng))
                    return;
                CompanyAddressMapActivity.CompanyLat = CompanyAddressMapActivity.CompanyLat.Replace(',', '.');
                CompanyAddressMapActivity.CompanyLng = CompanyAddressMapActivity.CompanyLng.Replace(',', '.');
                double lat = Convert.ToDouble(CompanyAddressMapActivity.CompanyLat, CultureInfo.InvariantCulture);
                double lng = Convert.ToDouble(CompanyAddressMapActivity.CompanyLng, CultureInfo.InvariantCulture);

                var geo = new Geocoder(this);

                var addresses = geo.GetFromLocation(lat, lng, 1);
                addresses.ToList().ForEach((addr) =>
                {
                    //myNotationTemp = null;
                    RegionTemp = null;
                    IndexTemp = addr.PostalCode;
                    CountryTemp = addr.CountryName;
                    CityTemp = addr.Locality;
                    FullCompanyAddressTemp = addr.Thoroughfare;
                    if (addr.SubThoroughfare != null)
                        if (!addr.SubThoroughfare.Contains(addr.Thoroughfare))
                            FullCompanyAddressTemp += " " + addr.SubThoroughfare;

                });
                _reverseDoneYet = true;
            });

            return "";
        }

        void ResetData()
        {
            FullCompanyAddressStatic = null;
            Country = null;
            Region = null;
            City = null;
            Index = null;
            Notation = null;
            FullCompanyAddressTemp = null;
            CountryTemp = null;
            RegionTemp = null;
            CityTemp = null;
            IndexTemp = null;
            NotationTemp = null;

            _countryEt.Text = null;
            _regionEt.Text = null;
            _cityEt.Text = null;
            _indexEt.Text = null;
            _notationEt.Text = null;
            _detailAddressEt.Text = null;
            _notationEt.Text = null;
            _coordsEt.Text = null;

            CompanyAddressMapActivity.CompanyLat = null;
            CompanyAddressMapActivity.CompanyLng = null;
        }

        void apply_variables()
        {
            if (!String.IsNullOrEmpty(CountryTemp))
                Country = CountryTemp;
            else
                Country = _countryEt.Text;
            if (!String.IsNullOrEmpty(RegionTemp))
                Region = RegionTemp;
            else
                Region = _regionEt.Text;
            if (!String.IsNullOrEmpty(CityTemp))
                City = CityTemp;
            else
                City = _cityEt.Text;
            if (!String.IsNullOrEmpty(IndexTemp))
                Index = IndexTemp;
            else
                Index = _indexEt.Text;
            if (!String.IsNullOrEmpty(NotationTemp))
                Notation = NotationTemp;
            else
                Notation = _notationEt.Text;
            if (!String.IsNullOrEmpty(FullCompanyAddressTemp))
                FullCompanyAddressStatic = FullCompanyAddressTemp;
            else
                FullCompanyAddressStatic = _detailAddressEt.Text;
        }

        void InitializeReverseValues()
        {
            _countryEt.Text = null;
            _regionEt.Text = null;
            _cityEt.Text = null;
            _detailAddressEt.Text = null;
            _indexEt.Text = null;
            //notationET.Text = null;
            if (!String.IsNullOrEmpty(CountryTemp))
            {
                _countryEt.Text = CountryTemp;
            }
            if (!String.IsNullOrEmpty(CityTemp))
            {
                _cityEt.Text = CityTemp;
            }
            if (!String.IsNullOrEmpty(RegionTemp))
            {
                _regionEt.Text = RegionTemp;
            }
            if (!String.IsNullOrEmpty(IndexTemp))
            {
                _indexEt.Text = IndexTemp;
            }
            if (!String.IsNullOrEmpty(FullCompanyAddressTemp))
            {
                _detailAddressEt.Text = FullCompanyAddressTemp;
            }

            _reverseDoneYet = false;
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _resetBn = FindViewById<Button>(Resource.Id.resetBn);
            _mapAddressBn = FindViewById<Button>(Resource.Id.mapAddressBn);
            _сoordsRemoveBn = FindViewById<Button>(Resource.Id.сoordsRemoveBn);
            _applyAddressBn = FindViewById<Button>(Resource.Id.applyAddressBn);
            _backRl = FindViewById<RelativeLayout>(Resource.Id.backRL);
            _countryEt = FindViewById<EditText>(Resource.Id.countryET);
            _regionEt = FindViewById<EditText>(Resource.Id.regionET);
            _cityEt = FindViewById<EditText>(Resource.Id.cityET);
            _detailAddressEt = FindViewById<EditText>(Resource.Id.detailAddressET);
            _indexEt = FindViewById<EditText>(Resource.Id.indexET);
            _notationEt = FindViewById<EditText>(Resource.Id.notationET);
            _coordsEt = FindViewById<EditText>(Resource.Id.coordsET);
            _resetBn.Text = TranslationHelper.GetString("reset", _ci);
            _mapAddressBn.Text = TranslationHelper.GetString("chooseOnMap", _ci);
            _applyAddressBn.Text = TranslationHelper.GetString("confirmAddress", _ci);
            _headerTv.Text = TranslationHelper.GetString("companyAddress", _ci);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _resetBn.SetTypeface(tf, TypefaceStyle.Normal);
            _mapAddressBn.SetTypeface(tf, TypefaceStyle.Normal);
            _сoordsRemoveBn.SetTypeface(tf, TypefaceStyle.Normal);
            _applyAddressBn.SetTypeface(tf, TypefaceStyle.Normal);
            _countryEt.SetTypeface(tf, TypefaceStyle.Normal);
            _regionEt.SetTypeface(tf, TypefaceStyle.Normal);
            _cityEt.SetTypeface(tf, TypefaceStyle.Normal);
            _detailAddressEt.SetTypeface(tf, TypefaceStyle.Normal);
            _indexEt.SetTypeface(tf, TypefaceStyle.Normal);
            _notationEt.SetTypeface(tf, TypefaceStyle.Normal);
            _coordsEt.SetTypeface(tf, TypefaceStyle.Normal);
        }

        private void ShowBackAlert()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("goBackWithoutSavingData", _ci));
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("confirm", _ci), (object sender1, DialogClickEventArgs e1) => base.OnBackPressed());
            AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        private void ShowResetAlert()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("reset", _ci) + "?");
            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("confirm", _ci), (object sender1, DialogClickEventArgs e1) => ResetData());
            AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        void RemoveCoordsQuestion(Object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            if (!String.IsNullOrEmpty(_coordsEt.Text))
            {
                builder.SetTitle(TranslationHelper.GetString("removeCoordsQuestion", _ci));
                builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
                builder.SetCancelable(true);
                builder.SetPositiveButton(TranslationHelper.GetString("remove", _ci), (object sender1, DialogClickEventArgs e1) =>
                {
                    CompanyAddressMapActivity.CompanyLat = null;
                    CompanyAddressMapActivity.CompanyLng = null;
                    //CoordsTextField.UserInteractionEnabled = true;
                    _coordsEt.Text = null;
                });
                AlertDialog dialog = builder.Create();
                dialog.Show();
            }
            else
            {
                builder.SetTitle(TranslationHelper.GetString("chooseCoordsOnMap", _ci));
                builder.SetNegativeButton("OK", (object sender1, DialogClickEventArgs e1) => { });
                builder.SetCancelable(true);
                AlertDialog dialog = builder.Create();
                dialog.Show();
            }
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

        public override void OnBackPressed()
        {
            BackClicked();
        }
    }
}
