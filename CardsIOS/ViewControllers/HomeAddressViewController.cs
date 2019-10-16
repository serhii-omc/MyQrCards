using Foundation;
using System;
using System.Drawing;
using UIKit;
using CardsIOS.NativeClasses;
using System.Threading.Tasks;
using CoreLocation;
using System.Globalization;
using CardsPCL;
using CardsPCL.CommonMethods;

namespace CardsIOS
{
    public partial class HomeAddressViewController : UIViewController
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

        public static string FullAddressStatic { get; set; }
        public static string FullAddressTemp { get; set; }
        public static string myCountry, myRegion, myCity, myIndex, myNotation;
        public static string myCountryTemp, myRegionTemp, myCityTemp, myIndexTemp, myNotationTemp;
        Methods methods = new Methods();

        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);

        NSObject keyboardShowObserver;
        NSObject keyboardHideObserver;
        RectangleF keyboardBounds;
        float keyboard_height;
        public HomeAddressViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            FillFields();
        }
        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            scrollView.Hidden = true;
            activityIndicator.Hidden = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();

            changedSomething = false;

            backBn.TouchUpInside += (s, e) => backTouched();

            resetBn.TouchUpInside += (s, e) =>
            {
                var option = UIAlertController.Create("Сбросить?", null, UIAlertControllerStyle.ActionSheet);
                option.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) => ConfirmResetData()));
                option.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null));
                this.PresentViewController(option, true, null);
            };

            mapAddressBn.TouchUpInside += (s, e) =>
            {
                //FullAddressStatic
                FullAddressTemp = CityTextField.Text + " " + DetailAddressTextField.Text;
                var vc = sb.InstantiateViewController(nameof(NewCardAddressMapViewController));
                this.NavigationController.PushViewController(vc, true);
            };
            applyAddressBn.TouchUpInside += (s, e) =>
            {
                apply_variables();
                this.NavigationController.PopViewController(true);
            };
            CountryTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                myCountryTemp = CountryTextField.Text;
            };
            RegionTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                myRegionTemp = RegionTextField.Text;
            };
            CityTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                myCityTemp = CityTextField.Text;
            };
            DetailAddressTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                //FullAddressStatic
                FullAddressTemp = DetailAddressTextField.Text;
            };
            IndexTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                myIndexTemp = IndexTextField.Text;
            };
            NotationTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                myNotationTemp = NotationTextField.Text;
            };
            CoordsTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                //myNotationTemp = CoordsTextField.Text;
            };
            CoordsRemoveBn.TouchUpInside += (s, e) => removeCoordsQuestion();
        }

        void SetScrollViewContentSize()
        {
            scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, applyAddressBn.Frame.Y + applyAddressBn.Frame.Height + 15);
        }

        private void FillFields()
        {
            if (!String.IsNullOrEmpty(FullAddressTemp) && FullAddressTemp != " " && !String.IsNullOrEmpty(myCityTemp))
            {
                if (FullAddressTemp.Contains(myCityTemp))
                {
                    var str = FullAddressTemp.Substring(0, myCityTemp.Length);
                    if (str == myCityTemp)
                    {
                        FullAddressTemp = FullAddressTemp.Remove(0, myCityTemp.Length + 1);
                    }
                }
            }
            else
                FullAddressTemp = null;

            var timer = new System.Timers.Timer();
            timer.Interval = 10;
            timer.Elapsed += delegate
            {
                timer.Stop();
                timer.Dispose();
                InvokeOnMainThread(async () =>
                {
                    if (!String.IsNullOrEmpty(myCountry))
                    {
                        CountryTextField.FloatLabelTop();
                        CountryTextField.Text = myCountry;
                    }
                    if (!String.IsNullOrEmpty(myCity))
                    {
                        CityTextField.FloatLabelTop();
                        CityTextField.Text = myCity;
                    }
                    if (!String.IsNullOrEmpty(myRegion))
                    {
                        RegionTextField.FloatLabelTop();
                        RegionTextField.Text = myRegion;
                    }
                    if (!String.IsNullOrEmpty(myIndex))
                    {
                        IndexTextField.FloatLabelTop();
                        IndexTextField.Text = myIndex;
                    }
                    if (!String.IsNullOrEmpty(FullAddressStatic))
                    {
                        DetailAddressTextField.FloatLabelTop();
                        DetailAddressTextField.Text = FullAddressStatic;
                    }
                    if (!String.IsNullOrEmpty(myNotation))
                    {
                        NotationTextField.FloatLabelTop();
                        NotationTextField.Text = myNotation;
                        if (!String.IsNullOrEmpty(myNotationTemp))
                            NotationTextField.Text = myNotationTemp;
                    }
                    CoordsTextField.UserInteractionEnabled = true;
                    if (!String.IsNullOrEmpty(NewCardAddressMapViewController.lat) && !String.IsNullOrEmpty(NewCardAddressMapViewController.lng))
                    {
                        CoordsTextField.FloatLabelTop();
                        CoordsTextField.Text = "N" + NewCardAddressMapViewController.lat + " E" + NewCardAddressMapViewController.lng;
                    }
                    UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                    CoordsTextField.UserInteractionEnabled = false;
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

        void resetData()
        {
            FullAddressStatic = null;
            myCountry = null;
            myRegion = null;
            myCity = null;
            myIndex = null;
            myNotation = null;
            FullAddressTemp = null;
            myCountryTemp = null;
            myRegionTemp = null;
            myCityTemp = null;
            myIndexTemp = null;
            myNotationTemp = null;

            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
        }

        void backTouched()
        {
            var option_back = UIAlertController.Create("Выйти без сохранения введенных данных?",
                                      null,
                                      UIAlertControllerStyle.ActionSheet);
            option_back.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) => this.NavigationController.PopViewController(true)));
            option_back.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null/*, (action) => Console.WriteLine("Cancel button pressed.")*/));
            //if (PersonalDataViewController.images_list != null)
            //if (PersonalDataViewController.images_list.Count > 0)
            //{
            //    this.PresentViewController(option_back, true, null);
            //    return;
            //}
            if (changedSomething)
            {
                this.PresentViewController(option_back, true, null);
                return;
            }

            this.NavigationController.PopViewController(true);
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;
            keyboard_height = (float)(View.Frame.Height / 2);
            // Fires when keyboard shows.
            keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, (notification) =>
            {
                NSValue nsKeyboardBounds = (NSValue)notification.UserInfo.ObjectForKey(UIKeyboard.BoundsUserInfoKey);
                keyboardBounds = nsKeyboardBounds.RectangleFValue;
                keyboard_height = keyboardBounds.Height;
                //if (needToScroll)
                //{
                //if ((View.Frame.Height - keyboardBounds.Height - 50) < 360)
                //{
                //scrollView.ContentOffset = new CoreGraphics.CGPoint(0, keyboardBounds.Height - 100);
                //}
                //}

            });
            // Fires when keyboard hides.
            keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, (notification) =>
            {
                //centering scroll
                //scrollView.ContentOffset = new CoreGraphics.CGPoint(0, 0);
            });

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
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Домашний адрес";
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(headerView.Frame.Height * 2), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            scrollView.Frame = new Rectangle(0, Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height - headerView.Frame.Height)));
            scrollView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;

            scrollView.Hidden = true;

            applyAddressBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            view_in_scroll.Frame = new Rectangle(0, 0/*Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height*1.2)*/, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(scrollView.Frame.Height /*- headerView.Frame.Height * 1.2)*/));

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
                Placeholder = "Улица, Дом, Корпус, Квартира",
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Next
            };
            DetailAddressTextField.ShouldReturn = _ => IndexTextField.BecomeFirstResponder();

            IndexTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Индекс",
                TextColor = UIColor.White,
                KeyboardType = UIKeyboardType.NumberPad,
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
            NotationTextField.ShouldReturn = _ => View.EndEditing(true);//CoordsTextField.BecomeFirstResponder();

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
                    NewCardAddressMapViewController.lat = null;
                    NewCardAddressMapViewController.lng = null;
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
            if (!String.IsNullOrEmpty(myCountryTemp))
                myCountry = myCountryTemp;
            else
                myCountry = CountryTextField.Text;
            if (!String.IsNullOrEmpty(myRegionTemp))
                myRegion = myRegionTemp;
            else
                myRegion = RegionTextField.Text;
            if (!String.IsNullOrEmpty(myCityTemp))
                myCity = myCityTemp;
            else
                myCity = CityTextField.Text;
            if (!String.IsNullOrEmpty(myIndexTemp))
                myIndex = myIndexTemp;
            else
                myIndex = IndexTextField.Text;
            if (!String.IsNullOrEmpty(myNotationTemp))
                myNotation = myNotationTemp;
            else
                myNotation = NotationTextField.Text;
            if (!String.IsNullOrEmpty(FullAddressTemp))
                FullAddressStatic = FullAddressTemp;
            else
                FullAddressStatic = DetailAddressTextField.Text;
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
            if (!String.IsNullOrEmpty(FullAddressStatic))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(FullAddressTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myCountry))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myRegion))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myCity))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myIndex))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myNotation))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myCountryTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myRegionTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myCityTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myIndexTemp))
            {
                this.PresentViewController(option, true, null);
                return;
            }
            if (!String.IsNullOrEmpty(myNotationTemp))
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
            }catch
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
                if (!String.IsNullOrEmpty(myCountryTemp))
                {
                    CountryTextField.FloatLabelTop();
                    CountryTextField.Text = myCountryTemp;
                }
                if (!String.IsNullOrEmpty(myCityTemp))
                {
                    CityTextField.FloatLabelTop();
                    CityTextField.Text = myCityTemp;
                }
                if (!String.IsNullOrEmpty(myRegionTemp))
                {
                    RegionTextField.FloatLabelTop();
                    RegionTextField.Text = myRegionTemp;
                }
                if (!String.IsNullOrEmpty(myIndexTemp))
                {
                    IndexTextField.FloatLabelTop();
                    IndexTextField.Text = myIndexTemp;
                }
                if (!String.IsNullOrEmpty(FullAddressTemp))
                {
                    DetailAddressTextField.FloatLabelTop();
                    DetailAddressTextField.Text = FullAddressTemp;
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

        async Task<string> ReverseGeocodeToConsoleAsync()
        {
            if (String.IsNullOrEmpty(NewCardAddressMapViewController.lat) || String.IsNullOrEmpty(NewCardAddressMapViewController.lng))
                return null;
            double lat = Convert.ToDouble(NewCardAddressMapViewController.lat, CultureInfo.InvariantCulture);
            double lng = Convert.ToDouble(NewCardAddressMapViewController.lng, CultureInfo.InvariantCulture);
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
                //myNotationTemp = null;
                myRegionTemp = null;
                myIndexTemp = placemark.PostalCode;
                myCountryTemp = placemark.Country;
                myCityTemp = placemark.SubAdministrativeArea;
                FullAddressTemp = placemark.Thoroughfare;
                if (placemark.SubThoroughfare != null)
                    if (!placemark.SubThoroughfare.Contains(placemark.Thoroughfare))
                        FullAddressTemp += " " + placemark.SubThoroughfare;


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
            vc.PushViewController(sb.InstantiateViewController(nameof(HomeAddressViewController)), false);
        }
    }
}
