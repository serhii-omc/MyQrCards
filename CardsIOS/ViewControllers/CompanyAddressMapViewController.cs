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
    public partial class CompanyAddressMapViewController : UIViewController
    {
        MapView mapView;

        float zoom = 14;
        public static string lat = null;
        public static string lng = null;
        public static string company_lat = null;
        public static string company_lng = null;
        string lat_temporary, lng_temporary;
        CameraPosition camera;
        CLLocationManager mgr;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Methods methods = new Methods();
        UIStoryboard sb = UIStoryboard.FromName("Main", null);

        public CompanyAddressMapViewController(IntPtr handle) : base(handle)
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

            okBn.TouchUpInside += (s, e) =>
            {
                hintView.Hidden = true;
            };
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            neverShowBn.TouchUpInside += (s, e) =>
            {
                hintView.Hidden = true;
                databaseMethods.InsertMapHint(false);
            };

            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }

            if (lat == "")
                lat = null;
            if (lng == "")
                lng = null;
            if (company_lat == "")
                company_lat = null;
            if (company_lng == "")
                company_lng = null;

            address_textView.Hidden = true;

            View.AddSubviews(mapView, centerPositionBn, applyAddressBn, hintView);

             mgr = new CLLocationManager();
            try
            {
                lat = mgr.Location.Coordinate.Latitude.ToString().Replace(',', '.');
                lng = mgr.Location.Coordinate.Longitude.ToString().Replace(',', '.');

                camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lat, CultureInfo.InvariantCulture),
                                                   longitude: Convert.ToDouble(lng, CultureInfo.InvariantCulture),
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
                        lat = e.Locations[0].Coordinate.Latitude.ToString().Replace(',', '.');
                        lng = e.Locations[0].Coordinate.Longitude.ToString().Replace(',', '.');
                        camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lat, CultureInfo.InvariantCulture),
                                                       longitude: Convert.ToDouble(lng, CultureInfo.InvariantCulture),
                                                                zoom: zoom);

                        try
                        {
                            if (!String.IsNullOrEmpty(company_lat) && !String.IsNullOrEmpty(company_lng))
                            {
                                company_lat = company_lat.Replace(',', '.');
                                company_lng = company_lng.Replace(',', '.');
                                camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(company_lat, CultureInfo.InvariantCulture),
                                                                   longitude: Convert.ToDouble(company_lng, CultureInfo.InvariantCulture),
                                                                        zoom: zoom);
                            }

                        }
                        catch { }
                        if (String.IsNullOrEmpty(lat_temporary) && String.IsNullOrEmpty(lng_temporary))
                            mapView.Camera = camera;
                    }

                updated = true;
            };
            setMoscowCameraView();
            mgr.StartUpdatingLocation();
            centerPositionBn.TouchUpInside += (s, e) =>
            {
                mgr = new CLLocationManager();
                try
                {
                    lat = mgr.Location.Coordinate.Latitude.ToString().Replace(',', '.');
                    lng = mgr.Location.Coordinate.Longitude.ToString().Replace(',', '.');

                    camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lat, CultureInfo.InvariantCulture),
                                                       longitude: Convert.ToDouble(lng, CultureInfo.InvariantCulture),
                                                            zoom: zoom);
                    mapView.Camera = camera;
                }
                catch { }
            };

            try
            {
                if (company_lat == null && company_lng == null)
                {
                    try
                    {
                        var sd = await GeocodeToConsoleAsync(CompanyAddressViewController.FullCompanyAddressTemp);
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
                    CLLocationCoordinate2D coord = new CLLocationCoordinate2D(Convert.ToDouble(company_lat, CultureInfo.InvariantCulture), Convert.ToDouble(company_lng, CultureInfo.InvariantCulture));
                    var marker = Marker.FromPosition(coord);
                    //marker.Title = string.Format("Marker 1");
                    marker.Icon = UIImage.FromBundle("add_loc_marker.png");
                    marker.Map = mapView;
                    company_lat = company_lat.Replace(',', '.');
                    company_lng = company_lng.Replace(',', '.');
                    camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(company_lat, CultureInfo.InvariantCulture),
                                                   longitude: Convert.ToDouble(company_lng, CultureInfo.InvariantCulture),
                                                            zoom: zoom);
                    mapView.Camera = camera;
                }
            }
            catch { }
            applyAddressBn.TouchUpInside += (s, e) =>
            {
                CompanyAddressViewController.changedSomething = true;
                if (!String.IsNullOrEmpty(lat_temporary) && !String.IsNullOrEmpty(lng_temporary))
                {
                    company_lat = lat_temporary;
                    company_lng = lng_temporary;
                }
                else
                {
                    try
                    {
                        company_lat = mgr.Location.Coordinate.Latitude.ToString().Replace(',', '.');
                        company_lng = mgr.Location.Coordinate.Longitude.ToString().Replace(',', '.');
                    }
                    catch { }
                }
                CompanyAddressViewController.came_from_map = true;
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
            headerLabel.Text = "Адрес компании";

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
            applyAddressBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            if (!databaseMethods.GetShowMapHint())
                hintView.Hidden = true;
            camera = CameraPosition.FromCamera(latitude: Convert.ToDouble(lat, CultureInfo.InvariantCulture),
                                               longitude: Convert.ToDouble(lng, CultureInfo.InvariantCulture),
                                                   zoom: zoom);
            mapView = MapView.FromCamera(CGRect.Empty, camera);
            mapView.Frame = new Rectangle(0, Convert.ToInt32(headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));
            mapView.MyLocationEnabled = true;
            mapView.CoordinateLongPressed += HandleLongPress;
            mapView.UserInteractionEnabled = true;

            applyAddressBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                                 (Convert.ToInt32(View.Frame.Height - View.Frame.Height / 10)),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            applyAddressBn.Font = applyAddressBn.Font.WithSize(15f);
            applyAddressBn.SetTitle("ПОДТВЕРДИТЬ АДРЕС", UIControlState.Normal);
            address_textView.Frame = new Rectangle(0, (int)headerView.Frame.Height, (int)(View.Frame.Width), (int)(headerView.Frame.Height));
            centerPositionBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - ((View.Frame.Width / 8))), Convert.ToInt32(address_textView.Frame.Height), Convert.ToInt32(View.Frame.Width / 8), Convert.ToInt32(View.Frame.Width / 8));
            address_textLabel.Frame = new Rectangle(10, Convert.ToInt32(address_textView.Frame.Height / 2 - (int)(View.Frame.Width / 36)), (Convert.ToInt32(View.Frame.Width) - 20), Convert.ToInt32(View.Frame.Width) / 18);
            okBn.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 17f);
            neverShowBn.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 17f);
            applyAddressBn.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 15f);
            hintTV.Font = UIFont.FromName(CardsPCL.Constants.fira_sans, 13.5f);
            if (EditViewController.IsCompanyReadOnly)
                DisableFields();
        }

        private void DisableFields()
        {
            var inactiveColor = UIColor.FromRGBA(146, 150, 155, 80);
            hintView.Hidden = true;
            mapView.CoordinateLongPressed -= HandleLongPress;
            applyAddressBn.BackgroundColor = inactiveColor;
            applyAddressBn.SetTitleColor(inactiveColor.ColorWithAlpha(50), UIControlState.Normal);
            applyAddressBn.UserInteractionEnabled = false;
        }

        //async Task<string> ReverseGeocodeToConsoleAsync(CLLocation location)
        //{
        //    var geoCoder = new CLGeocoder();
        //    var placemarks = await geoCoder.ReverseGeocodeLocationAsync(location);
        //    foreach (var placemark in placemarks)
        //    {
        //        Console.WriteLine(placemark);
        //        return placemark.Description;
        //    }
        //    return "";
        //}

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
            marker.Icon = UIImage.FromBundle("add_loc_marker.png");
            marker.Map = mapView;

            lat_temporary = e.Coordinate.Latitude.ToString().Replace(',', '.');
            lng_temporary = e.Coordinate.Longitude.ToString().Replace(',', '.');

            CLLocation temp_coords = new CLLocation(e.Coordinate.Latitude, e.Coordinate.Longitude);
            //CompanyAddressViewController.FullCompanyAddressStatic = await ReverseGeocodeToConsoleAsync(temp_coords);
            if (!String.IsNullOrEmpty(CompanyAddressViewController.FullCompanyAddressTemp) && CompanyAddressViewController.FullCompanyAddressTemp != " ")
            {
                address_textLabel.Text = CompanyAddressViewController.FullCompanyAddressTemp;
                //address_textView.Hidden = false;
            }
        }
    }
}