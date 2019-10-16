using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using static Android.Gms.Maps.GoogleMap;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CompanyAddressMapActivity : Activity, IOnMapReadyCallback, IOnMapLongClickListener
    {
        public static string CompanyLat = null;
        public static string CompanyLng = null;
        Methods _methods = new Methods();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        //NativeMethods nativeMethods = new NativeMethods();
        private GoogleMap _map;
        private MapFragment _mapFragment;
        RelativeLayout _hintRl, _backRl;
        Button _okBn, _neverShowBn, _centerPositionBn, _applyAddressBn;
        TextView _hintTv, _headerTv;
        float _zoom = 16;
        LatLng _moscowPosition, _point;
        MarkerOptions _moscowMarker, _myMarker;
        Marker _workMarker;
        BitmapDescriptor _userMarker, _workDescriptor;
        Position _myPosition = new Position();
        MarkerOptions _markerOptions = new MarkerOptions();
        private CameraUpdate _cameraUpdate;
        //public static string lat = null;
        //public static string lng = null;
        string _latTemporary, _lngTemporary;
        bool _launched, _userDeclinedPermission;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            try
            {
                SetContentView(Resource.Layout.NewCardAddressMap);
            }
            catch
            {
                base.OnBackPressed();
                return;
            }
            InitElements();
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }

            _mapFragment =
                    FragmentManager.FindFragmentByTag("map") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeNormal)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true);

                Android.App.FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.map, _mapFragment, "map");
                fragTx.Commit();
            }
            _mapFragment.GetMapAsync(this);

            InitElements();
            if (!_databaseMethods.GetShowMapHint())
                _hintRl.Visibility = ViewStates.Gone;
            _okBn.Click += (s, e) =>
            {
                _hintRl.Visibility = ViewStates.Gone;
            };
            _neverShowBn.Click += (s, e) =>
            {
                _hintRl.Visibility = ViewStates.Gone;
                _databaseMethods.InsertMapHint(false);
            };

            _backRl.Click += (s, e) =>
            {
                OnBackPressed();
            };

            _centerPositionBn.Click += CenterPositionBn_Click;

            _applyAddressBn.Click += (s, e) =>
            {
                CompanyAddressActivity.ChangedSomething = true; if (!String.IsNullOrEmpty(_latTemporary) && !String.IsNullOrEmpty(_lngTemporary))
                {
                    CompanyLat = _latTemporary;
                    CompanyLng = _lngTemporary;
                }
                else
                {
                    CompanyLat = _myPosition?.Latitude.ToString();
                    CompanyLng = _myPosition?.Longitude.ToString();
                }
                CompanyAddressActivity.CameFromMap = true;
                OnBackPressed();
            };
            if (EditActivity.IsCompanyReadOnly)
                DisableFields();
        }

        private void DisableFields()
        {
            _hintRl.Visibility = ViewStates.Gone;
            _applyAddressBn.Enabled = false;
            _applyAddressBn.SetBackgroundColor(Resources.GetColor(Resource.Color.editTextLineColorDisabled));
        }

        private async void CheckGeocode()
        {
            try
            {
                if (String.IsNullOrEmpty(CompanyLat) && String.IsNullOrEmpty(CompanyLng))
                {
                    var sd = await GeocodeToConsoleAsync(CompanyAddressActivity.FullCompanyAddressTemp);
                }
                else
                {
                    SetWorkMarker(null);
                }
            }
            catch { }
        }

        async Task<string> GeocodeToConsoleAsync(string address)
        {
            var geo = new Geocoder(this);

            var addresses = geo.GetFromLocationName(address, 1);
            RunOnUiThread(() =>
            {
                addresses.ToList().ForEach((addr) =>
                {
                    SetWorkMarker(addr);
                });
            });
            return "";
        }

        void SetWorkMarker(Android.Locations.Address addr)
        {
            LatLng searchLocation;
            if (addr != null)
                searchLocation = new LatLng(
                                            Convert.ToDouble(addr.Latitude, (CultureInfo.InvariantCulture)),
                                            Convert.ToDouble(addr.Longitude, (CultureInfo.InvariantCulture)));
            else
                searchLocation = new LatLng(
                                            Convert.ToDouble(CompanyLat, CultureInfo.InvariantCulture),
                                            Convert.ToDouble(CompanyLng, CultureInfo.InvariantCulture));
            CameraPosition.Builder builderSearch = CameraPosition.InvokeBuilder();
            builderSearch.Target(searchLocation);
            builderSearch.Zoom(_zoom);
            CameraPosition searchCameraPosition = builderSearch.Build();
            _cameraUpdate = CameraUpdateFactory.NewCameraPosition(searchCameraPosition);
            _map.MoveCamera(_cameraUpdate);
            _workDescriptor = BitmapDescriptorFactory.FromResource(Resource.Drawable.add_loc_marker);
            if (_workMarker != null)
            {
                _workMarker.Remove();
            }
            MarkerOptions markerOptions = new MarkerOptions();
            markerOptions.SetPosition(searchLocation);
            markerOptions.SetIcon(_workDescriptor);
            _workMarker = _map.AddMarker(markerOptions);
            //work_marker.Title = GetString(Resource.String.work_location);
            _latTemporary = searchLocation.Latitude.ToString();
            _lngTemporary = searchLocation.Longitude.ToString();
        }

        void CenterPositionBn_Click(object sender, EventArgs e)
        {
            CheckLocPermission();
            if (_userMarker == null)
            {
                FindLocation();
                return;
            }

            CameraPosition.Builder targetBuilder = CameraPosition.InvokeBuilder();
            LatLng myPos = new LatLng(Convert.ToDouble(_myPosition.Latitude, CultureInfo.InvariantCulture), Convert.ToDouble(_myPosition.Longitude, CultureInfo.InvariantCulture));
            targetBuilder.Target(myPos);
            targetBuilder.Zoom(_zoom);
            CameraPosition cameraPosition = targetBuilder.Build();
            _cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            _map.MoveCamera(_cameraUpdate);
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _hintRl = FindViewById<RelativeLayout>(Resource.Id.hintRL);
            _backRl = FindViewById<RelativeLayout>(Resource.Id.backRL);
            _okBn = FindViewById<Button>(Resource.Id.okBn);
            _neverShowBn = FindViewById<Button>(Resource.Id.neverShowBn);
            _centerPositionBn = FindViewById<Button>(Resource.Id.centerPositionBn);
            _applyAddressBn = FindViewById<Button>(Resource.Id.applyAddressBn);
            _hintTv = FindViewById<TextView>(Resource.Id.hintTV);
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _hintTv.Text = TranslationHelper.GetString("holdFingerToPutMarker", _ci);
            _neverShowBn.Text = TranslationHelper.GetString("neverShowAgain", _ci);
            _headerTv.Text = TranslationHelper.GetString("companyAddress", _ci);
            _hintTv.SetTypeface(tf, TypefaceStyle.Normal);
            _neverShowBn.SetTypeface(tf, TypefaceStyle.Normal);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _applyAddressBn.SetTypeface(tf, TypefaceStyle.Normal);
            _okBn.SetTypeface(tf, TypefaceStyle.Normal);
        }

        private void SetMoscowDefaultCameraPos()
        {
            CameraPosition.Builder targetBuilder = CameraPosition.InvokeBuilder();
            _moscowPosition = new LatLng(Convert.ToDouble(Constants.moscow_lat, CultureInfo.InvariantCulture), Convert.ToDouble(Constants.moscow_lng, CultureInfo.InvariantCulture));
            targetBuilder.Target(_moscowPosition);
            targetBuilder.Zoom(Constants.moscow_zoom);
            CameraPosition cameraPosition = targetBuilder.Build();
            _cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            _map.MoveCamera(_cameraUpdate);
        }

        public async void OnMapReady(GoogleMap map)
        {
            _map = map;
            if (_map != null)
            {
                _map.UiSettings.ZoomControlsEnabled = false;
                _map.UiSettings.CompassEnabled = true;
                if (!EditActivity.IsCompanyReadOnly)
                    _map.SetOnMapLongClickListener(this);
                //_map.MoveCamera(cameraUpdate);
                //_map.CameraIdle += (s, e) => OnCameraIdle();
                //var locator = CrossGeolocator.Current;
                //try
                //{
                //    await locator.StartListeningAsync(TimeSpan.FromSeconds(30), 20);
                //}
                //catch { }
                //locator.PositionChanged += (sender, e) =>
                //{
                //    // Position position=new Position();
                //    var position = e.Position as Position;
                //    edit.PutString("latitude", Convert.ToDouble(position.Latitude/*, (CultureInfo.InvariantCulture)*/).ToString());
                //    edit.PutString("longitude", Convert.ToDouble(position.Longitude/*, (CultureInfo.InvariantCulture)*/).ToString());
                //    edit.Apply();
                //    my_position = new LatLng(
                //               Convert.ToDouble(pref.GetString("latitude", String.Empty)/*, (CultureInfo.InvariantCulture)*/),
                //               Convert.ToDouble(pref.GetString("longitude", String.Empty)/*, (CultureInfo.InvariantCulture)*/));
                //};

                SetMoscowDefaultCameraPos();
                _map.MoveCamera(_cameraUpdate);
                FindLocation();
                CheckGeocode();
            }
        }

        async void FindLocation()
        {
            var granted = await CheckLocPermission();
            if (!granted)
            {
                //var res = await checkLocPermission();
                return;
            }
            CheckLocationOn();
        }

        private bool CheckLocationOn()
        {
            if (IsGeolocationEnabled())
            {
                var outer = Task.Factory.StartNew(() =>
                {
                    var inner = Task.Factory.StartNew(() =>
                    {
                        Geolocation();
                    });
                });
                return true;
            }
            else
            {
                TurnGpSon();
                return false;
            }
        }

        // My method to turn the GPS on.
        public void TurnGpSon()
        {
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);//, Resource.Style.AlertStyle);
            builder.SetTitle(TranslationHelper.GetString("locationServicesDisabled", _ci));
            builder.SetMessage(TranslationHelper.GetString("turnThemOnQuestion", _ci));
            builder.SetCancelable(false);
            builder.SetPositiveButton(TranslationHelper.GetString("no", _ci), (object sender1, DialogClickEventArgs e1) =>
            { });
            builder.SetNegativeButton(TranslationHelper.GetString("yes", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
                StartActivity(new Intent(Android.Provider.Settings.ActionLocationSourceSettings));
            });
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        public async Task<string> Geolocation()
        {
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 50;

                _myPosition = await locator.GetLastKnownLocationAsync();
                if (_myPosition == null && locator.IsGeolocationAvailable && locator.IsGeolocationEnabled)
                    _myPosition = await locator.GetPositionAsync(TimeSpan.FromSeconds((double)30000));
            }
            catch
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 50;
                _myPosition = await locator.GetPositionAsync(TimeSpan.FromSeconds((double)30000));
            }
            if (_myPosition != null)
            {
                SetMyMarker();
            }
            if (_myPosition == null)
                return "";

            return "";
        }

        private void SetMyMarker()
        {
            RunOnUiThread(() =>
            {
                LatLng myPos = new LatLng(
                                   Convert.ToDouble(_myPosition.Latitude/*, (CultureInfo.InvariantCulture)*/),
                                   Convert.ToDouble(_myPosition.Longitude/*, (CultureInfo.InvariantCulture)*/));
                _myMarker = new MarkerOptions();
                _myMarker.SetPosition(myPos);
                _userMarker = BitmapDescriptorFactory.FromResource(Resource.Drawable.myMarker);
                _myMarker.SetIcon(_userMarker);
                _map.AddMarker(_myMarker);
                if (//!launched 
                String.IsNullOrWhiteSpace(CompanyAddressActivity.FullCompanyAddressTemp) && String.IsNullOrEmpty(CompanyLat) && String.IsNullOrEmpty(CompanyLng))
                {
                    CenterPositionBn_Click(null, null);
                    _launched = true;
                }
            });
        }

        // Method to detect if GPS is enabled
        public bool IsGeolocationEnabled()
        {
            return CrossGeolocator.Current.IsGeolocationEnabled;
        }
        async Task<bool> CheckLocPermission()
        {
            PermissionStatus locationStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);

            if (locationStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
                locationStatus = results[Permission.Location];
                if (!_userDeclinedPermission)
                    request_runtime_permissions();
                return false;
            }

            return true;
        }

        private const int requestPermissionCode = 1000;
        void request_runtime_permissions()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                if (
                               CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted
                          || CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted
                          || CheckSelfPermission(Manifest.Permission.AccessMockLocation) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                    {
                                Manifest.Permission.AccessFineLocation,
                                Manifest.Permission.AccessCoarseLocation,
                                Manifest.Permission.AccessMockLocation,
                    }, requestPermissionCode);
                }
                else
                {
                    ActivityCompat.RequestPermissions(this, new String[]
                    {
                                Manifest.Permission.AccessFineLocation,
                                Manifest.Permission.AccessCoarseLocation,
                                Manifest.Permission.AccessMockLocation,
                    }, requestPermissionCode);
                }
        }
        bool _shown = false;
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case requestPermissionCode:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
                        {
                            CheckLocationOn();
                        }
                        else
                        {
                            _userDeclinedPermission = true;
                            if (!_shown)
                                Toast.MakeText(this, TranslationHelper.GetString("permissionsNeeded", _ci), ToastLength.Long).Show();
                            _shown = true;
                            //request_runtime_permissions();
                        }
                        break;
                    }
            }
        }
        void IOnMapLongClickListener.OnMapLongClick(LatLng point)
        {
            //this.point = point;
            _workDescriptor = BitmapDescriptorFactory.FromResource(Resource.Drawable.add_loc_marker);
            if (_workMarker != null)
            {
                _workMarker.Remove();
            }
            //edit_city_coord_for_edit = city_coord_for_edit_prefs.Edit();
            //apply_cancelLL.Visibility = ViewStates.Visible;
            var markerOptions = new MarkerOptions();
            markerOptions.SetPosition(point);
            markerOptions.SetIcon(_workDescriptor);

            _workMarker = _map.AddMarker(markerOptions);
            _latTemporary = point.Latitude.ToString().Replace(',', '.');
            _lngTemporary = point.Longitude.ToString().Replace(',', '.');

            //work_marker.Title = GetString(Resource.String.work_location);
            //edit_city_coord_for_edit.PutString("lat", point.Latitude.ToString());
            //edit_city_coord_for_edit.PutString("lng", point.Longitude.ToString());
            //edit_city_coord_for_edit.Apply();
            //chosen_lat = point.Latitude.ToString();
            //chosen_lng = point.Longitude.ToString();
        }
    }

}