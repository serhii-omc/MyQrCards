using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace CardsIOS
{
    public partial class PersonalDataViewControllerNew : UIViewController
    {
        static System.Timers.Timer timer1;
        string _deviceModel, cards_cache_dir;
        UIImagePickerController picker;
        public static string mySurname, myName, myMiddlename, myPhone, myEmail, myHomePhone, mySite, myDegree, myCardName, myBirthdate;
        UIActionSheet option;
        UIButton button = new UIButton();
        UIButton siteLockButton = new UIButton();
        static int margin_left_for_button = 0;
        //private object profileIV;
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        public static List<UIImage> images_list = new List<UIImage>();
        public static int current_img_id;
        static UIButton current_imgBn;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Attachments attachments = new Attachments();
        Methods methods = new Methods();
        NativeMethods nativeMethods = new NativeMethods();

        NSObject keyboardShowObserver;
        NSObject keyboardHideObserver;
        RectangleF keyboardBounds;
        float keyboard_height;

        public PersonalDataViewControllerNew(IntPtr handle) : base(handle)
        {
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        void OnImageClick(object sender, EventArgs e)
        {
            int index = 0;
            current_imgBn = sender as UIButton;
            foreach (var img in images_list)
            {
                if (current_imgBn.CurrentImage == img)
                    current_img_id = index;
                index++;
            }

            CropGalleryViewController.came_from_adding = false;
            CropGalleryViewController.currentImage = current_imgBn.CurrentImage;
            var vc = sb.InstantiateViewController(nameof(CropGalleryViewController));
            this.NavigationController.PushViewController(vc, true);
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
            //base.ViewDidLoad();
            EditViewController.IsCompanyReadOnly = false;
            //fires when keyboard shows
            keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, (notification) =>
            {
                NSValue nsKeyboardBounds = (NSValue)notification.UserInfo.ObjectForKey(UIKeyboard.BoundsUserInfoKey);
                keyboardBounds = nsKeyboardBounds.RectangleFValue;
                keyboard_height = keyboardBounds.Height;
            });
            // Fires when keyboard hides.
            keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, (notification) =>
            {
                keyboard_height = 0;
            });

            CropGalleryViewController.came_from = Constants.create;
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            cards_cache_dir = Path.Combine(documents, Constants.CardsPersonalImages);

            InitElements();

            birthday_mainBn.TouchUpInside += (s, e) =>
            {
                //hide keyboard
                UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                scrollView.Hidden = true;
                resetBn.Hidden = true;
                view_for_picker.Hidden = false;
                if (!String.IsNullOrEmpty(myBirthdate))
                {
                    date_picker.SetDate((NSDate)DateTime.SpecifyKind(DateTime.Parse(myBirthdate), DateTimeKind.Utc), false);
                }
            };
            date_picker.Frame = new CGRect(0, 0, view_for_picker.Frame.Width, view_for_picker.Frame.Height / 2);
            reset_pickerBn.Frame = new CGRect(View.Frame.Width / 5, date_picker.Frame.Height, View.Frame.Width / 5, social_netw_mainBn.Frame.Height);
            confirm_pickerBn.Frame = new CGRect(View.Frame.Width / 5 * 3, date_picker.Frame.Height, View.Frame.Width / 5, social_netw_mainBn.Frame.Height);

            confirm_pickerBn.SetTitle("Выбрать", UIControlState.Normal);
            reset_pickerBn.SetTitle("Сброс", UIControlState.Normal);
            if (String.IsNullOrEmpty(myBirthdate))
            {
                birthday_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //birthday_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                birthday_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155)/*FromRGB(75, 75, 75)*/, UIControlState.Normal);
                birthday_valueBn.SetTitle("", UIControlState.Normal);
            }
            else
            {
                birthday_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                birthday_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                SetBirthdateTitle();
            }
            resetBn.TouchUpInside += (s, e) =>
            {
                var option = UIAlertController.Create("Сбросить?", null, UIAlertControllerStyle.ActionSheet);
                option.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) => ConfirmResetData()));
                option.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null));
                this.PresentViewController(option, true, null);
            };

            pickPhotoBn.TouchUpInside += delegate
            {
                if (images_list.Count < 10)
                    option.ShowInView(View);
                else
                {
                    UIAlertView alert = new UIAlertView()
                    {
                        Title = "Достигнут лимит количества фото",
                        Message = "Нельзя прикрепить больше 10 фото к одной визитке"
                    };
                    alert.AddButton("OK");
                    alert.Show();
                }
            };
            confirm_pickerBn.TouchUpInside += (s, e) =>
            {
                myBirthdate = date_picker.Date.Description.Substring(0, 10);
                scrollView.Hidden = false;
                resetBn.Hidden = false;
                view_for_picker.Hidden = true;
                birthday_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                birthday_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                SetBirthdateTitle();
            };
            reset_pickerBn.TouchUpInside += (s, e) =>
            {
                date_picker.SetDate((NSDate)DateTime.SpecifyKind(DateTime.Parse("1990-01-01"), DateTimeKind.Utc), false);
                myBirthdate = null;
                scrollView.Hidden = false;
                resetBn.Hidden = false;
                view_for_picker.Hidden = true;
                //birthday_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                birthday_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155)/*FromRGB(75, 75, 75)*/, UIControlState.Normal);
                birthday_valueBn.SetTitle("", UIControlState.Normal);
            };
            address_mainBn.TouchUpInside += (s, e) =>
            {
                var vc = sb.InstantiateViewController(nameof(HomeAddressViewController));
                this.NavigationController.PushViewController(vc, true);
            };
            social_netw_mainBn.TouchUpInside += (s, e) =>
            {
                var vc = sb.InstantiateViewController(nameof(SocialNetworkViewController));
                SocialNetworkViewController.came_from = Constants.personal;
                this.NavigationController.PushViewController(vc, true);
            };

            continueBn.TouchUpInside += (s, e) =>
            {
                string error_message = "";
                if (String.IsNullOrEmpty(mySurname))
                    error_message += "Введите фамилию. ";
                if (String.IsNullOrEmpty(NameMiddleNameTextField.Text))
                    error_message += "Введите имя ";
                if (String.IsNullOrEmpty(myCardName))
                    error_message += "Введите название визитки. ";
                else
                {
                    if (CreatingCardViewController.datalist != null)
                    {
                        bool containsName = CreatingCardViewController.datalist.Any(item => item.name == myCardName);
                        if (containsName)
                            error_message += "Визитка с таким названием существует. ";
                    }
                }
                try
                {
                    if (!String.IsNullOrEmpty(myPhone))
                    {
                        if (myPhone.Length > 16)
                            error_message += "Телефон некорректен. ";
                    }
                }
                catch
                {
                    error_message += "Телефон некорректен. ";
                }
                if (!String.IsNullOrEmpty(myEmail))
                {
                    try
                    {
                        EmailTextField.Text = methods.EmailValidation(myEmail);
                        myEmail = EmailTextField.Text;
                    }
                    catch
                    {
                        error_message += "Email некорректен. ";
                    }
                }

                if (String.IsNullOrEmpty(error_message))
                {
                    //we need this to substitute email value for login
                    var date_time_stub = new DateTime();
                    databaseMethods.InsertValidTillRepeatAfter(date_time_stub, date_time_stub, EmailTextField.Text);

                    //caching card to db
                    var TaskA = new Task(() =>
                    {
                        CacheData(cards_cache_dir);
                    });
                    TaskA.Start();
                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(CompanyDataViewControllerNew)), true);
                }
                else
                {
                    EndEditingForAll();
                    scrollView.SetContentOffset(new CGPoint(0, keyboard_height), true);
                    UIAlertView alert = new UIAlertView()
                    {
                        Title = "Ошибка",
                        Message = error_message
                    };
                    alert.AddButton("OK");
                    alert.Show();
                }
            };
            InitializeImages();
            SurnameTextField.EditingChanged += (s, e) =>
            {
                mySurname = SurnameTextField.Text;
            };
            NameMiddleNameTextField.EditingChanged += (s, e) =>
            {
                if (NameMiddleNameTextField.Text.Contains(" ") && NameMiddleNameTextField.Text.IndexOf(" ", 1) > 1)
                {
                    String[] name_middle = NameMiddleNameTextField.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    myName = name_middle[0];
                    try
                    {
                        myMiddlename = name_middle[1];
                    }
                    catch { myMiddlename = string.Empty; }
                }
                else
                {
                    myMiddlename = string.Empty;
                    myName = NameMiddleNameTextField.Text;
                }
            };

            PhoneTextField.EditingChanged += (s, e) =>
            {
                if (!String.IsNullOrEmpty(PhoneTextField.Text))
                {
                    try
                    {
                        char l = PhoneTextField.Text[PhoneTextField.Text.Length - 1];
                        if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                            myPhone = PhoneTextField.Text;
                        else
                            PhoneTextField.Text = PhoneTextField.Text.Remove(PhoneTextField.Text.Length - 1);
                    }
                    catch { }
                }
                else
                    myPhone = PhoneTextField.Text;
            };
            EmailTextField.EditingChanged += (s, e) =>
            {
                myEmail = EmailTextField.Text;
            };
            HomePhoneTextField.EditingChanged += (s, e) =>
            {
                if (!String.IsNullOrEmpty(HomePhoneTextField.Text))
                {
                    try
                    {
                        char l = HomePhoneTextField.Text[HomePhoneTextField.Text.Length - 1];
                        if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                            myHomePhone = HomePhoneTextField.Text;
                        else
                            HomePhoneTextField.Text = HomePhoneTextField.Text.Remove(HomePhoneTextField.Text.Length - 1);
                    }
                    catch { }
                }
                else
                    myHomePhone = HomePhoneTextField.Text;
            };
            SiteTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraPersonData != null)
                    mySite = SiteTextField.Text;
                else
                {
                    SiteTextField.Text = null;
                    call_premium_option_menu();
                    SiteTextField.EndEditing(true);
                }
            };
            siteLockButton.TouchUpInside += (s, e) =>
            {
                if (QRViewController.ExtraPersonData != null)
                    mySite = SiteTextField.Text;
                else
                {
                    SiteTextField.Text = null;
                    call_premium_option_menu();
                    SiteTextField.EndEditing(true);
                }
            };
            DegreeTextField.EditingChanged += (s, e) =>
            {
                myDegree = DegreeTextField.Text;
            };
            CardNameTextField.EditingChanged += (s, e) =>
            {
                myCardName = CardNameTextField.Text;
            };
        }

        private void EndEditingForAll()
        {
            CardNameTextField.EndEditing(true);
            SurnameTextField.EndEditing(true);
            NameMiddleNameTextField.EndEditing(true);
            PhoneTextField.EndEditing(true);
            EmailTextField.EndEditing(true);
            HomePhoneTextField.EndEditing(true);
            SiteTextField.EndEditing(true);
            DegreeTextField.EndEditing(true);
        }

        private void FillFields()
        {
            databaseMethods.InsertLoginedFrom(Constants.from_card_creating_premium);
            var timer = new System.Timers.Timer();
            timer.Interval = 50;
            try
            {
                if (SocialNetworkTableViewSource<int, int>._checkedRows != null)
                {
                    if (SocialNetworkTableViewSource<int, int>.selectedIndexes.Count != 0)
                    {
                        social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                        social_netw_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                    }
                    else
                    {
                        social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                        //social_netw_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                        social_netw_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
                    }
                }
                else
                {
                    social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                    //social_netw_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                    social_netw_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155)/*FromRGB(75, 75, 75)*/, UIControlState.Normal);
                }
            }
            catch { }
            if (
                String.IsNullOrEmpty(HomeAddressViewController.FullAddressStatic) &&
                String.IsNullOrEmpty(HomeAddressViewController.myCountry) &&
                String.IsNullOrEmpty(HomeAddressViewController.myRegion) &&
                String.IsNullOrEmpty(HomeAddressViewController.myCity) &&
                String.IsNullOrEmpty(HomeAddressViewController.myIndex) &&
                String.IsNullOrEmpty(HomeAddressViewController.myNotation) &&
                String.IsNullOrEmpty(NewCardAddressMapViewController.lat) &&
                String.IsNullOrEmpty(NewCardAddressMapViewController.lng)
            )
            {
                address_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //address_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                address_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155)/*FromRGB(75, 75, 75)*/, UIControlState.Normal);
            }
            else
            {
                address_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                address_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
            }
            if (!String.IsNullOrEmpty(myBirthdate))
            {
                birthday_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                birthday_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                SetBirthdateTitle();
            }
            timer.Elapsed += delegate
            {
                timer.Stop();
                timer.Dispose();
                InvokeOnMainThread(async () =>
                {
                    if (!String.IsNullOrEmpty(mySurname))
                    {
                        SurnameTextField.FloatLabelTop();
                        SurnameTextField.Text = mySurname;
                    }
                    if (!String.IsNullOrEmpty(myName))
                    {
                        NameMiddleNameTextField.FloatLabelTop();
                        NameMiddleNameTextField.Text = myName + " " + myMiddlename;
                    }
                    if (!String.IsNullOrEmpty(myPhone))
                    {
                        PhoneTextField.FloatLabelTop();
                        PhoneTextField.Text = myPhone;
                    }
                    if (!String.IsNullOrEmpty(myEmail))
                    {
                        EmailTextField.FloatLabelTop();
                        EmailTextField.Text = myEmail;
                    }
                    if (!String.IsNullOrEmpty(myHomePhone))
                    {
                        HomePhoneTextField.FloatLabelTop();
                        HomePhoneTextField.Text = myHomePhone;
                    }
                    if (!String.IsNullOrEmpty(mySite))
                    {
                        if (mySite.ToLower().Contains("https://"))
                            mySite = mySite.Remove(0, "https://".Length);
                        SiteTextField.FloatLabelTop();
                        SiteTextField.Text = mySite;
                    }
                    if (!String.IsNullOrEmpty(myDegree))
                    {
                        DegreeTextField.FloatLabelTop();
                        DegreeTextField.Text = myDegree;
                    }
                    if (!String.IsNullOrEmpty(myCardName))
                    {
                        CardNameTextField.FloatLabelTop();
                        CardNameTextField.Text = myCardName;
                    }
                    UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                    //await Task.Delay(300);
                    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, scrollView.ContentSize.Height);
                    //if (_deviceModel.Contains("e 5") || _deviceModel.Contains("e 4") || _deviceModel.ToLower().Contains("se"))
                    //    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 1.8));
                    //else
                        //scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 2 - 350));
                    scrollView.ContentOffset = new CGPoint(0, 0);
                    scrollView.Hidden = false;
                    activityIndicator.Hidden = true;
                });
            };
            timer.Start();
        }

        private void CacheData(string cards_cache_dir)
        {
            //var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //var cards_cache_dir = Path.Combine(documents, Constants.CardsPersonalImages);
            if (!Directory.Exists(cards_cache_dir))
                Directory.CreateDirectory(cards_cache_dir);
            else
            {
                Directory.Delete(cards_cache_dir, true);
                Directory.CreateDirectory(cards_cache_dir);
            }

            int count = 0;
            foreach (var _img in images_list)
            {
                UIImage resultImage;
                var width_original = _img.Size.Width;
                var height_original = _img.Size.Height;
                if (width_original > Constants.BitmapSide)
                {
                    var width = Constants.BitmapSide;
                    var height = Constants.BitmapSide;

                    nativeMethods.SqueezeImage(ref width, ref height, ref width_original, ref height_original);

                    UIGraphics.BeginImageContext(new CGSize(width, height));

                    _img.Draw(new CGRect(0, 0, width, height));
                    resultImage = UIGraphics.GetImageFromCurrentImageContext();
                    UIGraphics.EndImageContext();
                }
                else
                    resultImage = _img;

                NSData image_jpeg = null;
                image_jpeg = resultImage.AsJPEG();
                if (image_jpeg.Length > 8000000)
                    image_jpeg = resultImage.AsJPEG(0.8F);

                string filenames = "image" + count.ToString() + ".jpeg";
                var fileName = Path.Combine(cards_cache_dir, filenames);
                var img = UIImage.FromFile(fileName);
                NSError err = null;
                image_jpeg.Save(fileName, false, out err);
                count++;
            }

            databaseMethods.InsertUsersCard(
            myName,
            mySurname,
            myMiddlename,
            myPhone,
            myEmail,
            myHomePhone,
            mySite,
            myDegree,
            myCardName,
            myBirthdate,
            HomeAddressViewController.myCountry,
            HomeAddressViewController.myRegion,
            HomeAddressViewController.myCity,
            HomeAddressViewController.FullAddressStatic,
            HomeAddressViewController.myIndex,
            HomeAddressViewController.myNotation,
            NewCardAddressMapViewController.lat,
            NewCardAddressMapViewController.lng
            );
        }

        private void InitElements()
        {
            scrollView.Hidden = true;
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;
            NavigationController.InteractivePopGestureRecognizer.Enabled = true;
            new AppDelegate().disableAllOrientation = true;

            string[] tableItems = new string[] { "Сделать снимок", "Выбрать фотографию" };

            option = new UIActionSheet(null, null, "Отменить", null, tableItems);
            option.Clicked += (btn_sender, args) => //Console.WriteLine("{0} Clicked", args.ButtonIndex);
            {
                if (args.ButtonIndex == 1)
                {
                    picker = new UIImagePickerController();
                    picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                    picker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
                    picker.FinishedPickingMedia += Finished;
                    picker.Canceled += Canceled;

                    PresentViewController(picker, animated: true, completionHandler: null);
                }
                if (args.ButtonIndex == 0)
                {
                    var storyboard = UIStoryboard.FromName("Main", NSBundle.MainBundle);
                    CameraController.came_from = Constants.personal;
                    var vc = storyboard.InstantiateViewController(nameof(CameraController));
                    this.NavigationController.PushViewController(vc, true);
                }
            };
            _deviceModel = Xamarin.iOS.DeviceHardware.Model;
            _headerConstraintTop.Constant = UIApplication.SharedApplication.StatusBarFrame.Height;
            _headerConstraintHeight.Constant = View.Frame.Height / 12;
            backBn.Frame = new Rectangle(0, 0, (int)_headerConstraintHeight.Constant, (int)_headerConstraintHeight.Constant);
            //backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, 0, (Convert.ToInt32(View.Frame.Width) / 5) * 3, (int)_headerConstraintHeight.Constant);
            resetBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), 0, Convert.ToInt32(View.Frame.Width) / 4, (int)_headerConstraintHeight.Constant);

            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            //backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3F, backBn.Frame.Width / 4F, backBn.Frame.Height / 3F, backBn.Frame.Width / 1.8F);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Личные данные";

            backBn.TouchUpInside += (s, e) => backTouched();

            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(headerView.Frame.Height * 2), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            continueBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            view_for_picker.Hidden = true;
            //backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3F, backBn.Frame.Width / 2.5F, backBn.Frame.Height / 3F, backBn.Frame.Width / 2.5F);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3F, backBn.Frame.Width / 2.5F, backBn.Frame.Height / 3F, backBn.Frame.Width / 2.5F);
            resetBn.SetTitle("Сбросить", UIControlState.Normal);
            scrollView.Frame = new Rectangle(0, Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height - headerView.Frame.Height)));

            resetBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            continueBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            address_mainBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            birthday_mainBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            birthday_valueBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            confirm_pickerBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            reset_pickerBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            social_netw_mainBn.Font = UIFont.FromName(Constants.fira_sans, 15f);

            imageScroll.BackgroundColor = UIColor.FromRGB(36, 43, 52);

            PhoneTextField.EditingDidEnd += (s, e) =>
            {
                if (PhoneTextField.Text.Length <= 2)
                {
                    myPhone = null;
                    PhoneTextField.Text = null;
                }
            };

            HomePhoneTextField.EditingDidEnd += (s, e) =>
            {
                if (HomePhoneTextField.Text.Length <= 2)
                {
                    myHomePhone = null;
                    HomePhoneTextField.Text = null;
                }
            };

            //view_in_scroll.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            //view_in_scroll.BackgroundColor = UIColor.Purple;

            PhoneTextField.EditingDidBegin += (s, e) =>
            {
                if (String.IsNullOrEmpty(myPhone))
                    PhoneTextField.Text = "+7";
            };
            HomePhoneTextField.EditingDidBegin += (s, e) =>
            {
                if (String.IsNullOrEmpty(myHomePhone))
                    HomePhoneTextField.Text = "+7";
            };

            _imageScrollConstraintHeight.Constant = (int)((View.Frame.Width / 4) / 4 * 3);
            pickPhotoBn.Frame = new Rectangle(0, 0, (int)_imageScrollConstraintHeight.Constant / 3 * 4, (int)_imageScrollConstraintHeight.Constant);

            margin_left_for_button = 3 + (int)pickPhotoBn.Frame.Width;
            //TODO
            //scrollView.BackgroundColor = UIColor.Orange;
            scrollView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;

            CardNameTextField.Placeholder = "Название визитки\u002A";
            CardNameTextField.AutocapitalizationType = UITextAutocapitalizationType.Words;
            CardNameTextField.TextColor = UIColor.White;
            CardNameTextField.ReturnKeyType = UIReturnKeyType.Next;
            CardNameTextField.ShouldReturn = _ => SurnameTextField.BecomeFirstResponder();

            SurnameTextField.Placeholder = "Фамилия\u002A";
            SurnameTextField.AutocapitalizationType = UITextAutocapitalizationType.Words;
            SurnameTextField.TextColor = UIColor.White;
            SurnameTextField.ReturnKeyType = UIReturnKeyType.Next;
            SurnameTextField.ShouldReturn = _ => NameMiddleNameTextField.BecomeFirstResponder();

            NameMiddleNameTextField.AutocapitalizationType = UITextAutocapitalizationType.Words;
            NameMiddleNameTextField.Placeholder = "Имя/Отчество\u002A";
            NameMiddleNameTextField.TextColor = UIColor.White;
            NameMiddleNameTextField.ReturnKeyType = UIReturnKeyType.Next;
            NameMiddleNameTextField.ShouldReturn = _ => PhoneTextField.BecomeFirstResponder();

            PhoneTextField.Placeholder = "Мобильный телефон";
            PhoneTextField.TextColor = UIColor.White;
            PhoneTextField.ReturnKeyType = UIReturnKeyType.Next;
            PhoneTextField.ShouldReturn = _ => EmailTextField.BecomeFirstResponder();

            EmailTextField.Placeholder = "Email";
            EmailTextField.TextColor = UIColor.White;
            EmailTextField.ReturnKeyType = UIReturnKeyType.Next;
            EmailTextField.ShouldReturn = _ => HomePhoneTextField.BecomeFirstResponder();

            HomePhoneTextField.Placeholder = "Домашний телефон";
            HomePhoneTextField.TextColor = UIColor.White;
            HomePhoneTextField.ReturnKeyType = UIReturnKeyType.Next;
            HomePhoneTextField.ShouldReturn = _ => SiteTextField.BecomeFirstResponder();

            SiteTextField.Placeholder = "Личный сайт";
            SiteTextField.TextColor = UIColor.White;
            SiteTextField.ReturnKeyType = UIReturnKeyType.Next;
            SiteTextField.ShouldReturn = _ => DegreeTextField.BecomeFirstResponder();

            DegreeTextField.Placeholder = "Звание/Учёная степень";
            DegreeTextField.TextColor = UIColor.White;
            DegreeTextField.ReturnKeyType = UIReturnKeyType.Done;
            DegreeTextField.ShouldReturn = _ => View.EndEditing(true);

            birthday_mainBn.TitleEdgeInsets = new UIEdgeInsets(0, 17, 0, 0);
            birthday_mainBn.SetTitle("День рождения", UIControlState.Normal);
            //birthday_mainBn.Frame = new Rectangle(0, 650, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            ////birthday_mainBn.Font = birthday_mainBn.Font.WithSize(19f);
            //birthfday_forwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
            //                                       Convert.ToInt32((birthday_mainBn.Frame.Y /*+ birthday_mainBn.Frame.Height*/) + (birthday_mainBn.Frame.Height / 2) - View.Frame.Width / 50),
            //                                        Convert.ToInt32(View.Frame.Width / 42),
            //                                        Convert.ToInt32(View.Frame.Width) / 25);
            //birthday_valueBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 2), (int)birthday_mainBn.Frame.Y, Convert.ToInt32(View.Frame.Width / 2), (int)birthday_mainBn.Frame.Height);
            //birthday_valueBn.SetTitle("29.05.1984", UIControlState.Normal);
            _birthdayValueBnLeadingConstraint.Constant = View.Frame.Width / 2;
            _birthdateValueBnWidthConstraint.Constant = _birthdayValueBnLeadingConstraint.Constant;

            address_mainBn.TitleEdgeInsets = new UIEdgeInsets(0, Convert.ToInt32(View.Frame.Width / 7), 0, 0);
            address_mainBn.SetTitle("Домашний адрес", UIControlState.Normal);
            //address_mainBn.Frame = new Rectangle(0, (int)(birthday_mainBn.Frame.Y + birthday_mainBn.Frame.Height + 25), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            ////birthday_mainBn.Font = birthday_mainBn.Font.WithSize(19f);
            //address_forwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
            //                                     Convert.ToInt32((address_mainBn.Frame.Y /*+ birthday_mainBn.Frame.Height*/) + (address_mainBn.Frame.Height / 2) - View.Frame.Width / 50),
            //                                        Convert.ToInt32(View.Frame.Width / 42),
            //                                        Convert.ToInt32(View.Frame.Width) / 25);
            //address_imageBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 23),
            //Convert.ToInt32(address_mainBn.Frame.Y + address_mainBn.Frame.Height / 4),
            //Convert.ToInt32(address_mainBn.Frame.Height / 2.45),
            //Convert.ToInt32(address_mainBn.Frame.Height / 2));
            //address_mainBn.BackgroundColor = UIColor.Blue;
            social_netw_mainBn.TitleEdgeInsets = new UIEdgeInsets(0, Convert.ToInt32(View.Frame.Width / 7), 0, 0);
            social_netw_mainBn.SetTitle("Социальные сети", UIControlState.Normal);
            //social_netw_mainBn.Frame = new Rectangle(0, (int)(address_mainBn.Frame.Y + address_mainBn.Frame.Height + 25), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            //birthday_mainBn.Font = birthday_mainBn.Font.WithSize(19f);
            //social_netw_forwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
            //                                         Convert.ToInt32((social_netw_mainBn.Frame.Y /*+ birthday_mainBn.Frame.Height*/) + (social_netw_mainBn.Frame.Height / 2) - View.Frame.Width / 50),
            //                                        Convert.ToInt32(View.Frame.Width / 42),
            //                                        Convert.ToInt32(View.Frame.Width) / 25);
            //social_netw_imageBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 25),
                                                      //Convert.ToInt32(social_netw_mainBn.Frame.Y + social_netw_mainBn.Frame.Height / 3.5),
                                                      //Convert.ToInt32(social_netw_mainBn.Frame.Height / 2),
                                                      //Convert.ToInt32(social_netw_mainBn.Frame.Height / 2.2));
            //card_nameIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 25),
            //Convert.ToInt32(25),
            //Convert.ToInt32(social_netw_mainBn.Frame.Height / 2.3),
            //Convert.ToInt32(social_netw_mainBn.Frame.Height / 2.2));
            //continueBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         //(int)(social_netw_mainBn.Frame.Y + social_netw_mainBn.Frame.Height + 25),
                                         //Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         //Convert.ToInt32(View.Frame.Height) / 12);
            continueBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            //continueBn.Font = continueBn.Font.WithSize(15f);
            continueBn.SetTitle("ПРОДОЛЖИТЬ", UIControlState.Normal);
            view_for_picker.Frame = new CGRect(0, headerView.Frame.Height, View.Frame.Width, View.Frame.Height - headerView.Frame.Height);
            //view_for_picker.Frame = new CGRect(0, headerView.Frame.Y, View.Frame.Width, View.Frame.Height - headerView.Frame.Y);
            date_picker.SetDate((NSDate)DateTime.SpecifyKind(DateTime.Parse("1990-01-01"), DateTimeKind.Utc), false);

            var site_lock = new UIImageView();
            var lock_height = 25;
            var lock_width = lock_height / 1.6;
            site_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, lock_height / 2, lock_width, lock_height);
            siteLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 48);
            //siteLockButton.BackgroundColor = UIColor.Red;
            site_lock.Image = UIImage.FromBundle("orange_lock.png");
            if (String.IsNullOrEmpty(QRViewController.ExtraPersonData))
            {
                _personalSiteView.AddSubview(site_lock);
                _personalSiteView.AddSubview(siteLockButton);
            }
        }
        void backTouched()
        {
            if (view_for_picker.Hidden)
            {
                var option_back = UIAlertController.Create("Выйти без сохранения введенных данных?",
                                          null,
                                          UIAlertControllerStyle.ActionSheet);
                option_back.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) => this.NavigationController.PopViewController(true)));
                option_back.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null/*, (action) => Console.WriteLine("Cancel button pressed.")*/));
                if (images_list != null)
                    if (images_list.Count > 0)
                    {
                        this.PresentViewController(option_back, true, null);
                        return;
                    }
                if (!String.IsNullOrEmpty(SurnameTextField.Text))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(NameMiddleNameTextField.Text))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(PhoneTextField.Text))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(EmailTextField.Text))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(HomePhoneTextField.Text))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(SiteTextField.Text))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(DegreeTextField.Text))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(CardNameTextField.Text))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(HomeAddressViewController.FullAddressStatic))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(HomeAddressViewController.myCountry))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(HomeAddressViewController.myRegion))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(HomeAddressViewController.myCity))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(HomeAddressViewController.myIndex))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(HomeAddressViewController.myNotation))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(NewCardAddressMapViewController.lat))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(NewCardAddressMapViewController.lng))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (!String.IsNullOrEmpty(myBirthdate))
                {
                    this.PresentViewController(option_back, true, null);
                    return;
                }
                if (SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl != null)
                    if (SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Count > 0)
                    {
                        this.PresentViewController(option_back, true, null);
                        return;
                    }
                this.NavigationController.PopViewController(true);
            }
            else
            {
                resetBn.Hidden = false;
                view_for_picker.Hidden = true;
                scrollView.Hidden = false;
            }
        }
        // TODO remove
        //private void SetColorsToElements()
        //{
        //    //CardNameTextField.BackgroundColor = UIColor.White;
        //    //card_nameIV.BackgroundColor = UIColor.Orange;
        //    foreach (var subview in view_in_scroll.Subviews)
        //    {
        //        if (subview is FloatingTextField)
        //        {
        //            subview.BackgroundColor = UIColor.Red;
        //        }
        //    }
        //    view_in_scroll.BackgroundColor = UIColor.Green;

        //    foreach (var subview in View.Subviews)
        //    {
        //        if (subview is FloatingTextField)
        //        {
        //            subview.BackgroundColor = UIColor.Red;
        //        }


        //        if (!(subview is UIButton))
        //        {
        //            if (subview is UITextField)
        //            {
        //                subview.BackgroundColor = UIColor.Red;
        //            }
        //            if (subview as UIScrollView != null)
        //            {
        //                foreach (var subsubview in subview.Subviews)
        //                {
        //                    if (subsubview is UITextField || subsubview is FloatingTextField)
        //                    {
        //                        subsubview.BackgroundColor = UIColor.Red;
        //                    }
        //                    //subsubview.BackgroundColor = UIColor.Blue;
        //                }
        //            }
        //            subview.BackgroundColor = UIColor.White;
        //        }
        //    }
        //}

        void reset_data(string cards_cache_dir)
        {
            mySurname = null;
            myName = null;
            myMiddlename = null;
            myPhone = null;
            myEmail = null;
            myHomePhone = null;
            mySite = null;
            myDegree = null;
            myCardName = null;
            myBirthdate = null;
            images_list.Clear();
            HomeAddressViewController.FullAddressStatic = null;
            HomeAddressViewController.myCountry = null;
            HomeAddressViewController.myRegion = null;
            HomeAddressViewController.myCity = null;
            HomeAddressViewController.myIndex = null;
            HomeAddressViewController.myNotation = null;

            HomeAddressViewController.FullAddressTemp = null;
            HomeAddressViewController.myCountryTemp = null;
            HomeAddressViewController.myRegionTemp = null;
            HomeAddressViewController.myCityTemp = null;
            HomeAddressViewController.myIndexTemp = null;
            HomeAddressViewController.myNotationTemp = null;

            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
            SocialNetworkTableViewSource<int, int>._checkedRows.Clear();
            SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
            SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
            EditCompanyDataViewControllerNew.position = null;
            if (Directory.Exists(cards_cache_dir))
                Directory.Delete(cards_cache_dir, true);
            databaseMethods.CleanPersonalNetworksTable();
            databaseMethods.ClearUsersCardTable();
        }

        void SetBirthdateTitle()
        {
            var birth_array = myBirthdate.Split('-');
            try { birthday_valueBn.SetTitle(birth_array[2] + "." + birth_array[1] + "." + birth_array[0], UIControlState.Normal); }
            catch { birthday_valueBn.SetTitle(myBirthdate, UIControlState.Normal); }
        }

        public void Finished(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            bool isImage = false;
            switch (e.Info[UIImagePickerController.MediaType].ToString())
            {
                case "public.image":
                    isImage = true;
                    break;
                case "public.video":
                    break;
            }
            NSUrl referenceURL = e.Info[new NSString("UIImagePickerControllerReferenceUrl")] as NSUrl;
            if (referenceURL != null)
                Console.WriteLine("Url:" + referenceURL.ToString());
            if (isImage)
            {

                UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;

                if (originalImage != null)
                {
                    button = new UIButton();
                    //button.Frame = new CGRect((int)View.Frame.Width/, 0, 100, 100);
                    button.Frame = new CGRect(margin_left_for_button, 0, pickPhotoBn.Frame.Width, pickPhotoBn.Frame.Height);
                    //button.SetTitle(margin_left_for_button.ToString(), UIControlState.Normal);
                    //button.SetTitleColor(UIColor.Green, UIControlState.Normal);
                    imageScroll.AddSubview(button);
                    margin_left_for_button = 3 + margin_left_for_button + (int)(pickPhotoBn.Frame.Width);
                    button.SetImage(originalImage, UIControlState.Normal);
                    button.TouchUpInside += new EventHandler(this.OnImageClick);
                    imageScroll.ContentSize = new CoreGraphics.CGSize(margin_left_for_button + 15, _imageScrollConstraintHeight.Constant);
                    images_list.Add(originalImage);
                    current_img_id = images_list.Count - 1;
                    //profileIV.Image = originalImage;
                    CropGalleryViewController.currentImage = originalImage;
                    CropGalleryViewController.came_from_adding = true;
                    var vc = sb.InstantiateViewController(nameof(CropGalleryViewController));

                    this.NavigationController.PushViewController(vc, true);
                    //this.NavigationController.RemoveFromParentViewController();
                }
            }
            else
            {
                NSUrl mediaURL = e.Info[UIImagePickerController.MediaURL] as NSUrl;
                if (mediaURL != null)
                {
                    Console.WriteLine(mediaURL.ToString());
                }
            }
            picker.DismissModalViewController(true);
        }

        void Canceled(object sender, EventArgs e)
        {
            picker.DismissModalViewController(true);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
        void InitializeImages()
        {
            for (int k = 0; k < images_list.Count; k++)
            {
                button = new UIButton();
                //button.Frame = new CGRect((int)View.Frame.Width/, 0, 100, 100);
                button.Frame = new CGRect(margin_left_for_button, 0, pickPhotoBn.Frame.Height, pickPhotoBn.Frame.Height);
                //button.SetTitle(margin_left_for_button.ToString(), UIControlState.Normal);
                //button.SetTitleColor(UIColor.Green, UIControlState.Normal);
                imageScroll.AddSubview(button);
                margin_left_for_button = 3 + margin_left_for_button + (int)(pickPhotoBn.Frame.Height);
                button.SetImage(images_list[k], UIControlState.Normal);
                button.TouchUpInside += new EventHandler(this.OnImageClick);
                imageScroll.ContentSize = new CoreGraphics.CGSize(margin_left_for_button + 15, _imageScrollConstraintHeight.Constant);
            }
        }
        void call_premium_option_menu()
        {
            string[] constraintItems = new string[] { "Подробнее о Premium" };

            if (!databaseMethods.userExists())
                constraintItems = new string[] { "Подробнее о Premium", "Войти в учетную запись" };
            var option_const = new UIActionSheet(null, null, "Отменить", null, constraintItems);
            option_const.Title = "Заполнение этого поля доступно для Premium-подписки";
            option_const.Clicked += (btn_sender, args) =>
            {
                if (args.ButtonIndex == 0)
                {
                    NavigationController.PushViewController(sb.InstantiateViewController(nameof(PremiumViewController)), true);
                }
                if (!databaseMethods.userExists())
                    if (args.ButtonIndex == 1)
                    {
                        NavigationController.PushViewController(sb.InstantiateViewController(nameof(EmailViewControllerNew)), true);
                    }
            };
            option_const.ShowInView(View);
        }

        void ConfirmResetData()
        {
            reset_data(cards_cache_dir);
            var vc = NavigationController;
            NavigationController.PopViewController(false);
            vc.PushViewController(sb.InstantiateViewController(nameof(PersonalDataViewControllerNew)), false);
            //profileIV.Image=UIImage.FromBundle("photoTemplate");
        }
    }
}