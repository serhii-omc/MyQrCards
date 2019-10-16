using System;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CoreGraphics;
using CoreLocation;
using Google.Maps;
using UIKit;
namespace CardsIOS
{
    public partial class NewCardAddressMapViewController : UIViewController
    {
        //MKMapView map_view; 
        MapView mapView;

        float zoom = 14;
        public static string lat = null;
        public static string lng = null;
        string lat_temporary, lng_temporary;
        CameraPosition camera;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Methods methods = new Methods();
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        public NewCardAddressMapViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();

            if (!databaseMethods.GetShowMapHint())
                hintView.Hidden = true;
            okBn.TouchUpInside += (s, e) =>
            {
                hintView.Hidden = true;
            };
            neverShowBn.TouchUpInside += (s, e) =>
            {
                hintView.Hidden = true;
                databaseMethods.InsertMapHint(false);
            };

            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lat, CultureInfo.InvariantCulture),
                                               longitude: Convert.ToDouble(lng, CultureInfo.InvariantCulture),
                                                   zoom: zoom);
            mapView = MapView.FromCamera(CGRect.Empty, camera);
            mapView.Frame = new Rectangle(0, Convert.ToInt32(headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));
            mapView.MyLocationEnabled = true;
            mapView.CoordinateLongPressed += HandleLongPress;
            mapView.UserInteractionEnabled = true;
            setMoscowCameraView();
            applyAddressBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            applyAddressBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                                 (Convert.ToInt32(View.Frame.Height - View.Frame.Height / 10)),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);

            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }
            address_textView.Hidden = true;
            View.AddSubviews(mapView, centerPositionBn, applyAddressBn, /*address_textView,*/ hintView);

            var mgr = new CLLocationManager();
            try
            {
                var lati = mgr.Location.Coordinate.Latitude.ToString().Replace(',', '.');
                var lngi = mgr.Location.Coordinate.Longitude.ToString().Replace(',', '.');

                camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lati, CultureInfo.InvariantCulture),
                                                   longitude: Convert.ToDouble(lngi, CultureInfo.InvariantCulture),
                                                        zoom: zoom);
                mapView.Camera = camera;
            }
            catch { }
            bool updated = false;
            mgr.LocationsUpdated += (sender, e) =>
            {
                if (!updated)
                    foreach (var locss in e.Locations)
                    {
                        var lati = e.Locations[0].Coordinate.Latitude.ToString().Replace(',', '.');
                        var lngi = e.Locations[0].Coordinate.Longitude.ToString().Replace(',', '.');
                        camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lati, CultureInfo.InvariantCulture),
                                                           longitude: Convert.ToDouble(lngi, CultureInfo.InvariantCulture),
                                                                zoom: zoom);

                        try
                        {
                            if (!String.IsNullOrEmpty(lat) && !String.IsNullOrEmpty(lng))
                                camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lat, CultureInfo.InvariantCulture),
                                                                   longitude: Convert.ToDouble(lng, CultureInfo.InvariantCulture),
                                                                        zoom: zoom);

                        }
                        catch { }
                        if (String.IsNullOrEmpty(lat_temporary) && String.IsNullOrEmpty(lng_temporary))
                            mapView.Camera = camera;
                    }
                updated = true;
            };
            mgr.StartUpdatingLocation();
            centerPositionBn.TouchUpInside += (s, e) =>
              {
                  mgr = new CLLocationManager();
                  try
                  {
                      var latitude = mgr.Location.Coordinate.Latitude.ToString().Replace(',', '.');
                      var longitude = mgr.Location.Coordinate.Longitude.ToString().Replace(',', '.');

                      camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(latitude, CultureInfo.InvariantCulture),
                                                         longitude: Convert.ToDouble(longitude, CultureInfo.InvariantCulture),
                                                                zoom: zoom);
                      mapView.Camera = camera;
                  }
                  catch { }
              };

            try
            {
                if (lat == null && lng == null)
                {
                    try
                    {
                        var sd = await GeocodeToConsoleAsync(HomeAddressViewController.FullAddressTemp);
                    }
                    catch
                    {
                        if (!methods.IsConnected())
                            InvokeOnMainThread(() =>
                            {
                                NoConnectionViewController.view_controller_name = GetType().Name;
                                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                                return;
                            });
                        return;
                    }
                }
                else
                {
                    CLLocationCoordinate2D coord = new CLLocationCoordinate2D(Convert.ToDouble(lat, CultureInfo.InvariantCulture), Convert.ToDouble(lng, CultureInfo.InvariantCulture));
                    var marker = Marker.FromPosition(coord);
                    //marker.Title = string.Format("Marker 1");
                    marker.Icon = UIImage.FromBundle("add_loc_marker.png");
                    marker.Map = mapView;
                    camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lat, CultureInfo.InvariantCulture),
                                                   longitude: Convert.ToDouble(lng, CultureInfo.InvariantCulture),
                                                            zoom: zoom);
                    mapView.Camera = camera;
                }
            }
            catch { }
            applyAddressBn.TouchUpInside += (s, e) =>
              {
                  HomeAddressViewController.changedSomething = true;
                  if (!String.IsNullOrEmpty(lat_temporary) && !String.IsNullOrEmpty(lng_temporary))
                  {
                      lat = lat_temporary;
                      lng = lng_temporary;
                  }
                  else
                  {
                      try
                      {
                          lat = mgr.Location.Coordinate.Latitude.ToString().Replace(',', '.');
                          lng = mgr.Location.Coordinate.Longitude.ToString().Replace(',', '.');
                      }
                      catch { }
                  }
                  HomeAddressViewController.came_from_map = true;
                  this.NavigationController.PopViewController(true);
              };
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;

            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Домашний адрес";

            if (lat == "")
                lat = null;
            if (lng == "")
                lng = null;

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            hintView.Frame = new Rectangle(0, 0, (int)(View.Frame.Width), (int)(View.Frame.Width / 2));
            hintView.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            fingerIV.Frame = new Rectangle((int)(View.Frame.Width / 2 - (View.Frame.Width / 24)), 25, (int)(View.Frame.Width / 12), (int)View.Frame.Width / 8);
            okBn.Layer.BorderColor = UIColor.White.CGColor;
            okBn.Layer.BorderWidth = 1f;
            neverShowBn.Layer.BorderColor = UIColor.White.CGColor;
            neverShowBn.Layer.BorderWidth = 1f;
            hintTV.Frame = new Rectangle(0, (int)(fingerIV.Frame.Y + fingerIV.Frame.Height + 10), (int)View.Frame.Width, (int)hintView.Frame.Height / 3);
            okBn.Frame = new Rectangle(10, (int)hintTV.Frame.Y + (int)hintTV.Frame.Height, (int)View.Frame.Width / 7, (int)View.Frame.Width / 10);
            neverShowBn.Frame = new Rectangle((int)(okBn.Frame.X + (int)okBn.Frame.Width + 20), (int)okBn.Frame.Y, (int)(View.Frame.Width - 40 - (int)okBn.Frame.Width), (int)View.Frame.Width / 10);
            applyAddressBn.SetTitle("ПОДТВЕРДИТЬ АДРЕС", UIControlState.Normal);
            address_textView.Frame = new Rectangle(0, (int)headerView.Frame.Height, (int)(View.Frame.Width), (int)(headerView.Frame.Height));
            centerPositionBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - ((View.Frame.Width / 8))), Convert.ToInt32(address_textView.Frame.Height), Convert.ToInt32(View.Frame.Width / 8), Convert.ToInt32(View.Frame.Width / 8));
            address_textLabel.Frame = new Rectangle(10, Convert.ToInt32(address_textView.Frame.Height / 2 - (int)(View.Frame.Width / 36)), (Convert.ToInt32(View.Frame.Width) - 20), Convert.ToInt32(View.Frame.Width) / 18);
            okBn.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 17f);
            neverShowBn.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 17f);
            applyAddressBn.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 15f);
            hintTV.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 13.5f);
        }

        private void setMoscowCameraView()
        {
            camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(CardsPCL.Constants.moscow_lat, CultureInfo.InvariantCulture),
                                                   longitude: Convert.ToDouble(CardsPCL.Constants.moscow_lng, CultureInfo.InvariantCulture),
                                                            zoom: CardsPCL.Constants.moscow_zoom);
            mapView.Camera = camera;
        }

        async Task<string> GeocodeToConsoleAsync(string address)
        {
            var geoCoder = new CLGeocoder();
            CLPlacemark[] placemarks = null;
            try
            {
                placemarks = await geoCoder.GeocodeAddressAsync(address);
            }
            catch
            {
                if (!methods.IsConnected())
                    InvokeOnMainThread(() =>
                    {
                        NoConnectionViewController.view_controller_name = GetType().Name;
                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                        return;
                    });
                return null;
            }
            foreach (var placemark in placemarks)
            {
                Console.WriteLine(placemark);
                CLLocationCoordinate2D coord = new CLLocationCoordinate2D(placemark.Location.Coordinate.Latitude, placemark.Location.Coordinate.Longitude);
                var marker = Marker.FromPosition(coord);
                //marker.Title = string.Format("Marker 1");
                marker.Icon = UIImage.FromBundle("add_loc_marker.png");
                marker.Map = mapView;
                camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(placemark.Location.Coordinate.Latitude, CultureInfo.InvariantCulture),
                                                   longitude: Convert.ToDouble(placemark.Location.Coordinate.Longitude, CultureInfo.InvariantCulture),
                                                            zoom: zoom);
                lat_temporary = placemark.Location.Coordinate.Latitude.ToString().Replace(',', '.');
                lng_temporary = placemark.Location.Coordinate.Longitude.ToString().Replace(',', '.');
                mapView.Camera = camera;
                break;
            }
            return "";
        }

        async void HandleLongPress(object sender, GMSCoordEventArgs e)
        {
            //clearing previous markers
            mapView.Clear();
            CLLocationCoordinate2D coord = new CLLocationCoordinate2D(e.Coordinate.Latitude, e.Coordinate.Longitude);
            var marker = Marker.FromPosition(coord);
            //marker.Title = string.Format("Marker 1");
            marker.Layer.Frame = new CGRect(0, 0, 10, 10);
            //marker.Icon.
            marker.Icon = UIImage.FromBundle("add_loc_marker.png");
            //marker.Icon.Scale(new CGSize(marker.Icon.Size.Width, marker.Icon.Size.Height), 25F);
            //SetMarkerSize(ref marker);
            marker.Map = mapView;
            lat_temporary = e.Coordinate.Latitude.ToString().Replace(',', '.');
            lng_temporary = e.Coordinate.Longitude.ToString().Replace(',', '.');
            CLLocation temp_coords = new CLLocation(e.Coordinate.Latitude, e.Coordinate.Longitude);
            //HomeAddressViewController.FullAddressStatic = await ReverseGeocodeToConsoleAsync(temp_coords);
            if (!String.IsNullOrEmpty(HomeAddressViewController.FullAddressTemp) && HomeAddressViewController.FullAddressTemp != " ")
            {
                address_textLabel.Text = HomeAddressViewController.FullAddressTemp;
                //address_textView.Hidden = false;
            }
        }

        private void SetMarkerSize(ref Marker marker)
        {
            //var myIcon = new Google.Maps.("images/marker-icon.png", null, null, null, new google.maps.Size(21, 30));

            //            var marker = new google.maps.Marker({
            //    position: myLatLng,
            //    map: myMap,
            //    flat: true,
            //    title: 'My location',
            //    icon: myIcon
            //});
            //marker.Icon.Scale(new CGSize(5, 5), 0.1F);
            //var kgkg = marker.Icon.Size.Height;
            //var kgkgq = marker.Icon.Size.Width;
            //marker.IconView.Frame = new CGRect(0, 0, 10, 10);
        }
    }
}