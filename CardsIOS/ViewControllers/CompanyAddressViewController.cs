using Foundation;
using System;
using System.Drawing;
using UIKit;
using CardsIOS.NativeClasses;
using System.Threading.Tasks;
using System.Globalization;
using CoreLocation;
using CardsPCL;
using CardsPCL.CommonMethods;

namespace CardsIOS
{
    public partial class CompanyAddressViewController : UIViewController
    {
        protected FloatingTextField CountryTextField { get; private set; }
        protected FloatingTextField RegionTextField { get; private set; }
        protected FloatingTextField CityTextField { get; private set; }
        protected FloatingTextField DetailAddressTextField { get; private set; }
        protected FloatingTextField IndexTextField { get; private set; }
        protected FloatingTextField NotationTextField { get; private set; }
        protected FloatingTextField CoordsTextField { get; private set; }
        protected UIButton CoordsRemoveBn { get; private set; }

        public static bool came_from_map { get; set; }
        public static bool changedSomething;
        public static string FullCompanyAddressTemp { get; set; }
        public static string FullCompanyAddressStatic { get; set; }
        public static string country, region, city, index, notation;
        public static string countryTemp, regionTemp, cityTemp, indexTemp, notationTemp;

        NSObject keyboardShowObserver;
        NSObject keyboardHideObserver;
        RectangleF keyboardBounds;
        float keyboard_height;
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        Methods methods = new Methods();
        NativeMethods _nativeMethods = new NativeMethods();
        public CompanyAddressViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();

            resetBn.TouchUpInside += (s, e) =>
            {
                var option = UIAlertController.Create("Сбросить?", null, UIAlertControllerStyle.ActionSheet);
                option.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) => ConfirmResetData()));
                option.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null));
                this.PresentViewController(option, true, null);
            };

            changedSomething = false;

            backBn.TouchUpInside += (s, e) => backTouched();

            mapAddressBn.TouchUpInside += (s, e) =>
            {
                //FullCompanyAddressStatic 
                FullCompanyAddressTemp = CityTextField.Text + " " + DetailAddressTextField.Text;
                var vc = sb.InstantiateViewController(nameof(CompanyAddressMapViewController));
                this.NavigationController.PushViewController(vc, true);
            };
            applyAddressBn.TouchUpInside += (s, e) =>
            {
                EditCompanyDataViewControllerNew.changedCompanyData = true;
                apply_variables();
                this.NavigationController.PopViewController(true);
            };

            CountryTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                countryTemp = CountryTextField.Text;
            };
            RegionTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                regionTemp = RegionTextField.Text;
            };
            CityTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                cityTemp = CityTextField.Text;
            };
            DetailAddressTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                //FullCompanyAddressStatic
                FullCompanyAddressTemp = DetailAddressTextField.Text;
            };
            IndexTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                indexTemp = IndexTextField.Text;
            };
            NotationTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                notationTemp = NotationTextField.Text;
            };
            CoordsRemoveBn.TouchUpInside += (s, e) => removeCoordsQuestion();

            if (EditViewController.IsCompanyReadOnly)
                DisableFields();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            FillFields();
        }

        private void FillFields()
        {
            if (!String.IsNullOrEmpty(FullCompanyAddressTemp) && FullCompanyAddressTemp != " " && !String.IsNullOrEmpty(cityTemp))
            {
                if (FullCompanyAddressTemp.Contains(cityTemp))
                {
                    var str = FullCompanyAddressTemp.Substring(0, cityTemp.Length);
                    if (str == cityTemp)
                    {
                        FullCompanyAddressTemp = FullCompanyAddressTemp.Remove(0, cityTemp.Length + 1);
                    }
                }
            }
            else
                FullCompanyAddressTemp = null;
            var timer = new System.Timers.Timer();
            timer.Interval = 50;
            timer.Elapsed += delegate
            {
                timer.Stop();
                timer.Dispose();
                InvokeOnMainThread(async () =>
                {
                    if (!String.IsNullOrEmpty(country))
                    {
                        CountryTextField.FloatLabelTop();
                        CountryTextField.Text = country;
                    }
                    if (!String.IsNullOrEmpty(city))
                    {
                        CityTextField.FloatLabelTop();
                        CityTextField.Text = city;
                    }
                    if (!String.IsNullOrEmpty(region))
                    {
                        RegionTextField.FloatLabelTop();
                        RegionTextField.Text = region;
                    }
                    if (!String.IsNullOrEmpty(index))
                    {
                        IndexTextField.FloatLabelTop();
                        IndexTextField.Text = index;
                    }
                    if (!String.IsNullOrEmpty(FullCompanyAddressStatic))
                    {
                        DetailAddressTextField.FloatLabelTop();
                        DetailAddressTextField.Text = FullCompanyAddressStatic;
                    }
                    if (!String.IsNullOrEmpty(notation))
                    {
                        NotationTextField.FloatLabelTop();
                        NotationTextField.Text = notation;
                        if (!String.IsNullOrEmpty(notationTemp))
                            NotationTextField.Text = notationTemp;
                    }

                    CoordsTextField.UserInteractionEnabled = true;
                    if (!String.IsNullOrEmpty(CompanyAddressMapViewController.company_lat) && !String.IsNullOrEmpty(CompanyAddressMapViewController.company_lng))
                    {
                        CoordsTextField.FloatLabelTop();
                        CoordsTextField.Text = "N" + CompanyAddressMapViewController.company_lat + " E" + CompanyAddressMapViewController.company_lng;
                    }
                    CoordsTextField.UserInteractionEnabled = false;
                    UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                    //await Task.Delay(300);
                    SetScrollViewContentSize();
                    view_in_scroll.Frame = new Rectangle(0, 0/*Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height*1.2)*/, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(applyAddressBn.Frame.Y + applyAddressBn.Frame.Height));
                    scrollView.Hidden = false;
                    activityIndicator.Hidden = true;
                    if (came_from_map)
                        addressChangeRequest();
                });
            };
            timer.Start();
        }

        private void DisableFields()
        {
            _nativeMethods.DisableFloatingTextField(CountryTextField);
            _nativeMethods.DisableFloatingTextField(RegionTextField);
            //_nativeMethods.DisableFloatingTextField(PositionTextField);
            _nativeMethods.DisableFloatingTextField(CityTextField);
            _nativeMethods.DisableFloatingTextField(DetailAddressTextField);
            _nativeMethods.DisableFloatingTextField(IndexTextField);
            _nativeMethods.DisableFloatingTextField(NotationTextField);
            _nativeMethods.DisableFloatingTextField(CoordsTextField);
            var inactiveColor = UIColor.FromRGBA(146, 150, 155, 80);
            //addressMainBn.SetTitleColor(inactiveColor, UIControlState.Normal);
            //addressMainBn.UserInteractionEnabled = false;
            CoordsRemoveBn.SetTitleColor(inactiveColor, UIControlState.Normal);
            CoordsRemoveBn.UserInteractionEnabled = false;
            resetBn.SetTitleColor(inactiveColor, UIControlState.Normal);
            resetBn.UserInteractionEnabled = false;
            //mapAddressBn.SetTitleColor(inactiveColor, UIControlState.Normal);
            //mapAddressBn.UserInteractionEnabled = false;
            //applyAddressBn.SetTitleColor(inactiveColor, UIControlState.Normal);
            applyAddressBn.UserInteractionEnabled = false;
            applyAddressBn.BackgroundColor = inactiveColor;
            applyAddressBn.SetTitleColor(inactiveColor.ColorWithAlpha(50), UIControlState.Normal);
            //createCardBn.UserInteractionEnabled = false;
            //logo_with_imageBn.UserInteractionEnabled = false;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            scrollView.Hidden = true;
            activityIndicator.Hidden = false;
        }

        void backTouched()
        {
            var option_back = UIAlertController.Create("Выйти без сохранения введенных данных?",
                                      null,
                                      UIAlertControllerStyle.ActionSheet);
            option_back.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) => this.NavigationController.PopViewController(true)));
            option_back.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null/*, (action) => Console.WriteLine("Cancel button pressed.")*/));
            if (PersonalDataViewControllerNew.images_list != null)
                if (PersonalDataViewControllerNew.images_list.Count > 0)
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
            if (changedSomething)
            {
                this.PresentViewController(option_back, true, null);
                return;
            }

            this.NavigationController.PopViewController(true);
        }

        void resetData()
        {
            FullCompanyAddressStatic = null;
            country = null;
            region = null;
            city = null;
            index = null;
            notation = null;

            FullCompanyAddressTemp = null;
            countryTemp = null;
            regionTemp = null;
            cityTemp = null;
            indexTemp = null;
            notationTemp = null;

            CompanyAddressMapViewController.company_lat = null;
            CompanyAddressMapViewController.company_lng = null;
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;
            keyboard_height = (float)(View.Frame.Height / 2);
            // Fires when keyboard shows.
            //keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, (notification) =>
            //{
            //    NSValue nsKeyboardBounds = (NSValue)notification.UserInfo.ObjectForKey(UIKeyboard.BoundsUserInfoKey);
            //    keyboardBounds = nsKeyboardBounds.RectangleFValue;
            //    keyboard_height = keyboardBounds.Height;
            //    //if (needToScroll)
            //    //{
            //    //if ((View.Frame.Height - keyboardBounds.Height - 50) < 360)
            //    //{
            //    //scrollView.ContentOffset = new CoreGraphics.CGPoint(0, keyboardBounds.Height - 100);
            //    //}
            //    //}

            //});
            //// Fires when keyboard hides.
            //keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, (notification) =>
            //{
            //    //centering scroll
            //    //scrollView.ContentOffset = new CoreGraphics.CGPoint(0, 0);
            //});

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            //image_bgIV.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));
            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
                resetBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), Convert.ToInt32(View.Frame.Width) / 12 + 20, Convert.ToInt32(View.Frame.Width) / 4, Convert.ToInt32(View.Frame.Width) / 19);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
                resetBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), Convert.ToInt32(View.Frame.Width) / 12, Convert.ToInt32(View.Frame.Width) / 4, Convert.ToInt32(View.Frame.Width) / 19);
            }
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Адрес компании";
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(headerView.Frame.Height * 2), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));

            applyAddressBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            scrollView.Frame = new Rectangle(0, Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height - headerView.Frame.Height)));

            scrollView.Hidden = true;

            view_in_scroll.Frame = new Rectangle(0, 0/*Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height*1.2)*/, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(scrollView.Frame.Height/* - headerView.Frame.Height * 1.2*/));

            scrollView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;


            CountryTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Страна",
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Next
            };
            CountryTextField.ShouldReturn = _ => RegionTextField.BecomeFirstResponder();
            RegionTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Регион",
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Next
            };
            RegionTextField.ShouldReturn = _ => CityTextField.BecomeFirstResponder();
            CityTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Город",
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Next
            };
            CityTextField.ShouldReturn = _ => DetailAddressTextField.BecomeFirstResponder();

            DetailAddressTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Улица, Дом, Корпус, Офис",
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Next
            };
            DetailAddressTextField.ShouldReturn = _ => IndexTextField.BecomeFirstResponder();

            IndexTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Индекс",
                KeyboardType = UIKeyboardType.NumberPad,
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Next
            };
            IndexTextField.ShouldReturn = _ => NotationTextField.BecomeFirstResponder();

            NotationTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Примечание",
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Done
            };
            NotationTextField.ShouldReturn = _ => View.EndEditing(true);

            CoordsTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Координаты",
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Done

            };
            CoordsTextField.ShouldReturn = _ => View.EndEditing(true);

            CoordsRemoveBn = new UIButton
            {
                BackgroundColor = UIColor.Clear
            };
            CoordsRemoveBn.Frame = new CoreGraphics.CGRect(0, 430, View.Frame.Width, 60);
            //ApartmentTextField = new FloatingTextField
            //         {
            //             TranslatesAutoresizingMaskIntoConstraints = false,
            //             Placeholder = "Квартира",
            //             TextColor = UIColor.White,
            //  ReturnKeyType = UIReturnKeyType.Done
            //         };
            //ApartmentTextField.ShouldReturn = _ => View.EndEditing(true);

            //CountryTextField.EditingDidBegin += SetInsets;
            //RegionTextField.EditingDidBegin += SetInsets;
            //CityTextField.EditingDidBegin += SetInsets;
            //DetailAddressTextField.EditingDidBegin += SetInsets;
            //IndexTextField.EditingDidBegin += SetInsets;
            //NotationTextField.EditingDidBegin += SetInsets;


            view_in_scroll.AddSubviews(CountryTextField, RegionTextField, CityTextField, DetailAddressTextField, IndexTextField, NotationTextField, CoordsTextField, CoordsRemoveBn);
            view_in_scroll.AddConstraints(new NSLayoutConstraint[]
            {
                CountryTextField.TopAnchor.ConstraintEqualTo(view_in_scroll.TopAnchor, 12),
                CountryTextField.LeadingAnchor.ConstraintEqualTo(view_in_scroll.LeadingAnchor, 16),
                CountryTextField.TrailingAnchor.ConstraintEqualTo(view_in_scroll.TrailingAnchor, -16),
                CountryTextField.HeightAnchor.ConstraintEqualTo(48),
                RegionTextField.TopAnchor.ConstraintEqualTo(CountryTextField.TopAnchor, 70),
                RegionTextField.LeadingAnchor.ConstraintEqualTo(CountryTextField.LeadingAnchor, 0),
                RegionTextField.TrailingAnchor.ConstraintEqualTo(CountryTextField.TrailingAnchor, 0),
                RegionTextField.HeightAnchor.ConstraintEqualTo(48),
                CityTextField.TopAnchor.ConstraintEqualTo(RegionTextField.TopAnchor, 70),
                CityTextField.LeadingAnchor.ConstraintEqualTo(RegionTextField.LeadingAnchor, 0),
                CityTextField.TrailingAnchor.ConstraintEqualTo(RegionTextField.TrailingAnchor, 0),
                CityTextField.HeightAnchor.ConstraintEqualTo(48),
                DetailAddressTextField.TopAnchor.ConstraintEqualTo(CityTextField.TopAnchor, 70),
                DetailAddressTextField.LeadingAnchor.ConstraintEqualTo(CityTextField.LeadingAnchor, 0),
                DetailAddressTextField.TrailingAnchor.ConstraintEqualTo(CityTextField.TrailingAnchor, 0),
                DetailAddressTextField.HeightAnchor.ConstraintEqualTo(48),
                IndexTextField.TopAnchor.ConstraintEqualTo(DetailAddressTextField.TopAnchor, 70),
                IndexTextField.LeadingAnchor.ConstraintEqualTo(DetailAddressTextField.LeadingAnchor, 0),
                IndexTextField.TrailingAnchor.ConstraintEqualTo(DetailAddressTextField.TrailingAnchor, 0),
                IndexTextField.HeightAnchor.ConstraintEqualTo(48),
                NotationTextField.TopAnchor.ConstraintEqualTo(IndexTextField.TopAnchor, 70),
                NotationTextField.LeadingAnchor.ConstraintEqualTo(IndexTextField.LeadingAnchor, 0),
                NotationTextField.TrailingAnchor.ConstraintEqualTo(IndexTextField.TrailingAnchor, 0),
                NotationTextField.HeightAnchor.ConstraintEqualTo(48),
                CoordsTextField.TopAnchor.ConstraintEqualTo(NotationTextField.TopAnchor, 70),
                CoordsTextField.LeadingAnchor.ConstraintEqualTo(NotationTextField.LeadingAnchor, 0),
                CoordsTextField.TrailingAnchor.ConstraintEqualTo(NotationTextField.TrailingAnchor, 0),
                CoordsTextField.HeightAnchor.ConstraintEqualTo(48),
                CoordsRemoveBn.TopAnchor.ConstraintEqualTo(NotationTextField.TopAnchor, 70),
                CoordsRemoveBn.LeadingAnchor.ConstraintEqualTo(NotationTextField.LeadingAnchor, 0),
                CoordsRemoveBn.TrailingAnchor.ConstraintEqualTo(NotationTextField.TrailingAnchor, 0),
                CoordsRemoveBn.HeightAnchor.ConstraintEqualTo(48),
            });

            resetBn.SetTitle("Сбросить", UIControlState.Normal);
            applyAddressBn.SetTitle("ПОДТВЕРДИТЬ АДРЕС", UIControlState.Normal);
            mapAddressBn.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
            mapAddressBn.Layer.BorderWidth = 1f;
            mapAddressBn.SetTitle("ВЫБРАТЬ НА КАРТЕ", UIControlState.Normal);
            applyAddressBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            mapAddressBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            resetBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            if (!deviceModel.Contains("e 5") && !deviceModel.Contains("e 4") && !deviceModel.ToLower().Contains("e se"))
            {
                applyAddressBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                                     600,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
                mapAddressBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                                   Convert.ToInt32(applyAddressBn.Frame.Y - (int)(applyAddressBn.Frame.Height + 10)),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);

            }
            else
            {
                applyAddressBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                                      Convert.ToInt32(view_in_scroll.Frame.Height + 60 - headerView.Frame.Height / 3),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
                mapAddressBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                                   Convert.ToInt32(applyAddressBn.Frame.Y - (int)(applyAddressBn.Frame.Height + 10)),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
                var diff = view_in_scroll.Frame.Height - (applyAddressBn.Frame.Y + applyAddressBn.Frame.Height);
                if (diff < 0)
                    view_in_scroll.Frame = new Rectangle(0, 0/*Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height*1.2)*/, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(applyAddressBn.Frame.Y + applyAddressBn.Frame.Height));
            }

            SetScrollViewContentSize();
        }

        void removeCoordsQuestion()
        {
            if (!String.IsNullOrEmpty(CoordsTextField.Text))
            {
                var option = UIAlertController.Create("Удалить координаты?",
                                             null,
                                             UIAlertControllerStyle.ActionSheet);
                option.AddAction(UIAlertAction.Create("Удалить", UIAlertActionStyle.Default, (action) =>
                {
                    CompanyAddressMapViewController.lat = null;
                    CompanyAddressMapViewController.lng = null;
                    CompanyAddressMapViewController.company_lat = null;
                    CompanyAddressMapViewController.company_lng = null;
                    //CoordsTextField.UserInteractionEnabled = true;
                    CoordsTextField.Text = null;
                    CoordsTextField.FloatLabelBack();
                    //CoordsTextField.UserInteractionEnabled = false;
                }));

                option.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null));
                this.PresentViewController(option, true, null);
            }
            else
            {
                var option = UIAlertController.Create("Выберите координаты на карте",
                                             null,
                                             UIAlertControllerStyle.ActionSheet);
                option.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
                this.PresentViewController(option, true, null);
            }
        }
        void apply_variables()
        {
            if (!String.IsNullOrEmpty(countryTemp))
                country = countryTemp;
            else
                country = CountryTextField.Text;
            if (!String.IsNullOrEmpty(regionTemp))
                region = regionTemp;
            else
                region = RegionTextField.Text;
            if (!String.IsNullOrEmpty(cityTemp))
                city = cityTemp;
            else
                city = CityTextField.Text;
            if (!String.IsNullOrEmpty(indexTemp))
                index = indexTemp;
            else
                index = IndexTextField.Text;
            if (!String.IsNullOrEmpty(notationTemp))
                notation = notationTemp;
            else
                notation = NotationTextField.Text;
            if (!String.IsNullOrEmpty(FullCompanyAddressTemp))
                FullCompanyAddressStatic = FullCompanyAddressTemp;
            else
                FullCompanyAddressStatic = DetailAddressTextField.Text;
        }
        //void SetInsets(object sender, EventArgs e)
        //{
        //    var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboard_height, 0.0f);
        //    scrollView.ContentInset = contentInsets;
        //    scrollView.ScrollIndicatorInsets = contentInsets;
        //}
        async void addressChangeRequest()
        {
            var option = UIAlertController.Create("Изменить адрес в соответствии с отмеченной точкой?",
                                              null,
                                              UIAlertControllerStyle.ActionSheet);
            option.AddAction(UIAlertAction.Create("Заменить", UIAlertActionStyle.Default, async (action) =>
            {
                try
                {
                    var res_ = await ReverseGeocodeToConsoleAsync();
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
                //var vc = NavigationController;
                //NavigationController.PopViewController(false);
                InitializeReverseValues();
            }));

            option.AddAction(UIAlertAction.Create("Не менять", UIAlertActionStyle.Cancel, null));
            came_from_map = false;
            if (!String.IsNullOrEmpty(FullCompanyAddressStatic))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(FullCompanyAddressTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(country))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(region))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(city))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(index))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(notation))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(countryTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(regionTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(cityTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(indexTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(notationTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(CountryTextField.Text))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(RegionTextField.Text))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(CityTextField.Text))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(DetailAddressTextField.Text))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(IndexTextField.Text))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(NotationTextField.Text))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            try
            {
                var res = await ReverseGeocodeToConsoleAsync();
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
            InitializeReverseValues();
        }

        void InitializeReverseValues()
        {
            //scrollView.Hidden = true;
            //activityIndicator.Hidden = false;
            //var timer = new System.Timers.Timer();
            //timer.Interval = 50;
            //timer.Elapsed += delegate
            //{
            //timer.Stop();
            //timer.Dispose();
            InvokeOnMainThread(async () =>
            {
                CountryTextField.Text = null;
                RegionTextField.Text = null;
                CityTextField.Text = null;
                DetailAddressTextField.Text = null;
                //IndexTextField.Text = null;
                //NotationTextField.Text = null;
                if (!String.IsNullOrEmpty(countryTemp))
                {
                    CountryTextField.FloatLabelTop();
                    CountryTextField.Text = countryTemp;
                }
                if (!String.IsNullOrEmpty(cityTemp))
                {
                    CityTextField.FloatLabelTop();
                    CityTextField.Text = cityTemp;
                }
                if (!String.IsNullOrEmpty(regionTemp))
                {
                    RegionTextField.FloatLabelTop();
                    RegionTextField.Text = regionTemp;
                }
                if (!String.IsNullOrEmpty(indexTemp))
                {
                    IndexTextField.FloatLabelTop();
                    IndexTextField.Text = indexTemp;
                }
                if (!String.IsNullOrEmpty(FullCompanyAddressTemp))
                {
                    DetailAddressTextField.FloatLabelTop();
                    DetailAddressTextField.Text = FullCompanyAddressTemp;
                }

                UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                CoordsTextField.UserInteractionEnabled = false;
                //await Task.Delay(300);
                SetScrollViewContentSize();
                scrollView.Hidden = false;
                activityIndicator.Hidden = true;
            });
            //};
            //timer.Start();
        }

        void SetScrollViewContentSize()
        {
            scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, applyAddressBn.Frame.Y + applyAddressBn.Frame.Height + 15);
        }

        async Task<string> ReverseGeocodeToConsoleAsync()
        {
            if (String.IsNullOrEmpty(CompanyAddressMapViewController.company_lat) || String.IsNullOrEmpty(CompanyAddressMapViewController.company_lng))
                return null;
            double lat = Convert.ToDouble(CompanyAddressMapViewController.company_lat, CultureInfo.InvariantCulture);
            double lng = Convert.ToDouble(CompanyAddressMapViewController.company_lng, CultureInfo.InvariantCulture);
            CLLocation location = new CLLocation(lat, lng);
            var geoCoder = new CLGeocoder();
            CLPlacemark[] placemarks = null;
            try
            {
                placemarks = await geoCoder.ReverseGeocodeLocationAsync(location);
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
                //notationTemp = null;
                regionTemp = null;
                indexTemp = placemark.PostalCode;
                countryTemp = placemark.Country;
                cityTemp = placemark.SubAdministrativeArea;
                FullCompanyAddressTemp = placemark.Thoroughfare;
                if (placemark.SubThoroughfare != null)
                    if (!placemark.SubThoroughfare.Contains(placemark.Thoroughfare))
                        FullCompanyAddressTemp += " " + placemark.SubThoroughfare;


                break;

                //Console.WriteLine(placemark);
                //return placemark.Description;
            }
            return "";
        }

        void ConfirmResetData()
        {
            resetData();
            var vc = NavigationController;
            NavigationController.PopViewController(false);
            vc.PushViewController(sb.InstantiateViewController(nameof(CompanyAddressViewController)), false);
        }
    }
}