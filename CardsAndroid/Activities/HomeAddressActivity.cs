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
    public class HomeAddressActivity : Activity
    {
        TextView _headerTv;
        Button _resetBn, _mapAddressBn, _applyAddressBn, _сoordsRemoveBn;
        RelativeLayout _backRl;
        EditText _countryEt, _regionEt, _cityEt, _detailAddressEt, _indexEt, _notationEt, _coordsEt;
        public static bool CameFromMap { get; set; }
        public static bool ChangedSomething;
        static bool _initFinished = false;
        public static string FullAddressStatic { get; set; }
        public static string FullAddressTemp { get; set; }
        public static string MyCountry, MyRegion, MyCity, MyIndex, MyNotation;
        public static string MyCountryTemp, MyRegionTemp, MyCityTemp, MyIndexTemp, MyNotationTemp;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        Methods _methods = new Methods();
        bool _reverseDoneYet;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.HomeAddress);

            InitElements();
            ChangedSomething = false;
            _initFinished = false;
            _backRl.Click += (s, e) => BackClicked();
            _resetBn.Click += (s, e) => ShowResetAlert();

            _mapAddressBn.Click += (s, e) =>
            {
                //FullAddressStatic
                FullAddressTemp = _cityEt.Text + " " + _detailAddressEt.Text;
                StartActivity(typeof(NewCardAddressMapActivity));
            };
            _applyAddressBn.Click += (s, e) =>
            {
                apply_variables();
                base.OnBackPressed();
            };
            _countryEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    MyCountryTemp = _countryEt.Text;
            };
            _regionEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    MyRegionTemp = _regionEt.Text;
            };
            _cityEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    MyCityTemp = _cityEt.Text;
            };
            _detailAddressEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                //FullAddressStatic
                if (!_reverseDoneYet)
                    FullAddressTemp = _detailAddressEt.Text;
            };
            _indexEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    MyIndexTemp = _indexEt.Text;
            };
            _notationEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
                if (!_reverseDoneYet)
                    MyNotationTemp = _notationEt.Text;
            };
            _coordsEt.TextChanged += (s, e) =>
            {
                if (_initFinished)
                    ChangedSomething = true;
            };
            _сoordsRemoveBn.Click += RemoveCoordsQuestion;
        }
        protected override void OnResume()
        {
            base.OnResume();
            _initFinished = false;
            FillFields();
        }

        private void FillFields()
        {
            if (!String.IsNullOrEmpty(FullAddressTemp) && FullAddressTemp != " " && !String.IsNullOrEmpty(MyCityTemp))
            {
                if (FullAddressTemp.Contains(MyCityTemp))
                {
                    var str = FullAddressTemp.Substring(0, MyCityTemp.Length);
                    if (str == MyCityTemp)
                    {
                        FullAddressTemp = FullAddressTemp.Remove(0, MyCityTemp.Length + 1);
                    }
                }
            }
            else
                FullAddressTemp = null;


            if (!String.IsNullOrEmpty(MyCountry))
            {
                _countryEt.Text = MyCountry;
            }
            if (!String.IsNullOrEmpty(MyCity))
            {
                _cityEt.Text = MyCity;
            }
            if (!String.IsNullOrEmpty(MyRegion))
            {
                _regionEt.Text = MyRegion;
            }
            if (!String.IsNullOrEmpty(MyIndex))
            {
                _indexEt.Text = MyIndex;
            }
            if (!String.IsNullOrEmpty(FullAddressStatic))
            {
                _detailAddressEt.Text = FullAddressStatic;
            }
            if (!String.IsNullOrEmpty(MyNotation))
            {
                _notationEt.Text = MyNotation;
            }
            if (!String.IsNullOrEmpty(NewCardAddressMapActivity.Lat) && !String.IsNullOrEmpty(NewCardAddressMapActivity.Lng))
            {
                _coordsEt.Text = "N" + NewCardAddressMapActivity.Lat + " E" + NewCardAddressMapActivity.Lng;
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
            if (!String.IsNullOrEmpty(FullAddressStatic))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(FullAddressTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyCountry))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyRegion))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyCity))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyIndex))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyNotation))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyCountryTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyRegionTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyCityTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyIndexTemp))
            {
                dialog.Show();
                return;
            }
            if (!String.IsNullOrEmpty(MyNotationTemp))
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
                if (String.IsNullOrEmpty(NewCardAddressMapActivity.Lat) || String.IsNullOrEmpty(NewCardAddressMapActivity.Lng))
                    return;
                NewCardAddressMapActivity.Lat = NewCardAddressMapActivity.Lat.Replace(',', '.');
                NewCardAddressMapActivity.Lng = NewCardAddressMapActivity.Lng.Replace(',', '.');
                double lat = Convert.ToDouble(NewCardAddressMapActivity.Lat, CultureInfo.InvariantCulture);
                double lng = Convert.ToDouble(NewCardAddressMapActivity.Lng, CultureInfo.InvariantCulture);

                var geo = new Geocoder(this);

                var addresses = geo.GetFromLocation(lat, lng, 1);
                addresses.ToList().ForEach((addr) =>
                {
                    //myNotationTemp = null;
                    MyRegionTemp = null;
                    MyIndexTemp = addr.PostalCode;
                    MyCountryTemp = addr.CountryName;
                    MyCityTemp = addr.Locality;
                    FullAddressTemp = addr.Thoroughfare;
                    if (addr.SubThoroughfare != null)
                        if (!addr.SubThoroughfare.Contains(addr.Thoroughfare))
                            FullAddressTemp += " " + addr.SubThoroughfare;

                });
                _reverseDoneYet = true;
            });

            return "";
        }

        void ResetData()
        {
            FullAddressStatic = null;
            MyCountry = null;
            MyRegion = null;
            MyCity = null;
            MyIndex = null;
            MyNotation = null;
            FullAddressTemp = null;
            MyCountryTemp = null;
            MyRegionTemp = null;
            MyCityTemp = null;
            MyIndexTemp = null;
            MyNotationTemp = null;

            _countryEt.Text = null;
            _regionEt.Text = null;
            _cityEt.Text = null;
            _indexEt.Text = null;
            _notationEt.Text = null;
            _detailAddressEt.Text = null;
            _notationEt.Text = null;
            _coordsEt.Text = null;

            NewCardAddressMapActivity.Lat = null;
            NewCardAddressMapActivity.Lng = null;
        }

        void apply_variables()
        {
            if (!String.IsNullOrEmpty(MyCountryTemp))
                MyCountry = MyCountryTemp;
            else
                MyCountry = _countryEt.Text;
            if (!String.IsNullOrEmpty(MyRegionTemp))
                MyRegion = MyRegionTemp;
            else
                MyRegion = _regionEt.Text;
            if (!String.IsNullOrEmpty(MyCityTemp))
                MyCity = MyCityTemp;
            else
                MyCity = _cityEt.Text;
            if (!String.IsNullOrEmpty(MyIndexTemp))
                MyIndex = MyIndexTemp;
            else
                MyIndex = _indexEt.Text;
            if (!String.IsNullOrEmpty(MyNotationTemp))
                MyNotation = MyNotationTemp;
            else
                MyNotation = _notationEt.Text;
            if (!String.IsNullOrEmpty(FullAddressTemp))
                FullAddressStatic = FullAddressTemp;
            else
                FullAddressStatic = _detailAddressEt.Text;
        }

        void InitializeReverseValues()
        {
            _countryEt.Text = null;
            _regionEt.Text = null;
            _cityEt.Text = null;
            _detailAddressEt.Text = null;
            _indexEt.Text = null;
            //notationET.Text = null;
            if (!String.IsNullOrEmpty(MyCountryTemp))
            {
                _countryEt.Text = MyCountryTemp;
            }
            if (!String.IsNullOrEmpty(MyCityTemp))
            {
                _cityEt.Text = MyCityTemp;
            }
            if (!String.IsNullOrEmpty(MyRegionTemp))
            {
                _regionEt.Text = MyRegionTemp;
            }
            if (!String.IsNullOrEmpty(MyIndexTemp))
            {
                _indexEt.Text = MyIndexTemp;
            }
            if (!String.IsNullOrEmpty(FullAddressTemp))
            {
                _detailAddressEt.Text = FullAddressTemp;
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
            _headerTv.Text = TranslationHelper.GetString("homeAddress", _ci);
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
                    NewCardAddressMapActivity.Lat = null;
                    NewCardAddressMapActivity.Lng = null;
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
            //base.OnBackPressed();
            BackClicked();
        }
    }
}
