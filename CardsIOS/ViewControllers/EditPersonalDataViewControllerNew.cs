using System;
using System.Drawing;
using System.IO;
using System.Linq;
using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CoreGraphics;
using Foundation;
using UIKit;

namespace CardsIOS
{
    public partial class EditPersonalDataViewControllerNew : UIViewController
    {
        UIImagePickerController picker;
        public static bool is_primary;
        public static bool changedSomething;
        string _deviceModel;
        public static string mySurname, myName, myMiddlename, myPhone, myEmail, myHomePhone, mySite, myDegree, myCardName, myBirthDate, myCardNameOriginal;
        UIActionSheet normal;
        UIActionSheet option;
        UIActionSheet option_delete;
        UIButton button = new UIButton();
        UIButton siteLockButton = new UIButton();
        static int margin_left_for_button = 0;
        private object profileIV;
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        //public static List<UIImage> images_list = new List<UIImage>();
        //public static int current_img_id;
        static UIButton current_imgBn;
        string cards_cache_dir;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Attachments attachments = new Attachments();
        Methods methods = new Methods();
        NativeMethods nativeMethods = new NativeMethods();

        NSObject keyboardShowObserver;
        NSObject keyboardHideObserver;
        RectangleF keyboardBounds;
        float keyboard_height;

        public EditPersonalDataViewControllerNew(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        void OnOldImageClick(object sender, EventArgs e)
        {
            int index = 0;
            current_imgBn = sender as UIButton;
            foreach (var img in EditViewController.images_from_server_list)
            {
                if (current_imgBn.CurrentImage == img)
                {
                    CropOldPersonalImageViewController.global_index = index;
                    CropOldPersonalImageViewController.came_from_adding = true;
                    CropOldPersonalImageViewController.currentImage = current_imgBn.CurrentImage;
                    var vc = sb.InstantiateViewController(nameof(CropOldPersonalImageViewController));
                    this.NavigationController.PushViewController(vc, true);
                    //string[] tableItems = new string[] { "Удалить фото" };

                    //var id = EditViewController.ids_of_attachments[index];
                    //normal = new UIActionSheet("Simple ActionSheet", null, "Cancel", "Delete", null);
                    //option_delete = new UIActionSheet(null, null, "Не удалять", null, tableItems);
                    //option_delete.Clicked += (btn_sender, args) => //Console.WriteLine("{0} Clicked", args.ButtonIndex);
                    //{
                    //    if (args.ButtonIndex == 0)
                    //    {
                    //        EditViewController.ids_of_attachments.RemoveAt(index);
                    //        EditViewController.images_from_server_list.RemoveAt(index);
                    //        var vc = NavigationController;
                    //        NavigationController.PopViewController(false);
                    //        vc.PushViewController(sb.InstantiateViewController(nameof(EditPersonalDataViewController)), false);
                    //    }
                    //};
                    //option_delete.ShowInView(View);
                    break;
                }
                //PersonalDataViewController.current_img_id = index;
                index++;
            }

            //int hashBn=sender.GetHashCode();
            //CropGalleryViewController.currentImage = current_imgBn.CurrentImage;
            //var vc = sb.InstantiateViewController("CropGalleryViewController");
            //this.NavigationController.PushViewController(vc, true);
        }

        void OnImageClick(object sender, EventArgs e)
        {
            int index = 0;
            current_imgBn = sender as UIButton;
            foreach (var img in PersonalDataViewControllerNew.images_list)
            {
                if (current_imgBn.CurrentImage == img)
                    PersonalDataViewControllerNew.current_img_id = index;
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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

            changedSomething = false;
            CropGalleryViewController.came_from = Constants.edit;
            CropOldPersonalImageViewController.came_from = Constants.edit;

            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            cards_cache_dir = Path.Combine(documents, Constants.CardsPersonalImages);

            InitElements();

            //backBn.TouchUpInside += (s, e) => backTouched();
            //{
            //    if (view_for_picker.Hidden)
            //        this.NavigationController.PopViewController(true);
            //    else
            //    {
            //        resetBn.Hidden = false;
            //        view_for_picker.Hidden = true;
            //        scrollView.Hidden = false;
            //    }
            //};
            resetBn.TouchUpInside += (s, e) =>
            {
                var option = UIAlertController.Create("Сбросить?", null, UIAlertControllerStyle.ActionSheet);
                option.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) => ConfirmResetData()));
                option.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null));
                this.PresentViewController(option, true, null);
                //profileIV.Image=UIImage.FromBundle("photoTemplate");
            };

            pickPhotoBn.TouchUpInside += delegate
            {
                if (PersonalDataViewControllerNew.images_list.Count < 10)
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
                //normal.ShowInView(View);
            };

            CardNameTextField.Placeholder = "Название визитки\u002A";
            CardNameTextField.TextColor = UIColor.White;
            CardNameTextField.AutocapitalizationType = UITextAutocapitalizationType.Words;
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
            birthday_mainBn.TitleEdgeInsets = new UIEdgeInsets(0, 17, 0, 0);
            birthday_mainBn.SetTitle("День рождения", UIControlState.Normal);
            birthday_mainBn.Frame = new Rectangle(0, 570, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            //birthday_mainBn.Font = birthday_mainBn.Font.WithSize(19f);
            birthday_forwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
                                                   Convert.ToInt32((birthday_mainBn.Frame.Y /*+ birthday_mainBn.Frame.Height*/) + (birthday_mainBn.Frame.Height / 2) - View.Frame.Width / 50),
                                                    Convert.ToInt32(View.Frame.Width / 42),
                                                    Convert.ToInt32(View.Frame.Width) / 25);
            birthday_valueBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 2), Convert.ToInt32(birthday_mainBn.Frame.Y), Convert.ToInt32(View.Frame.Width / 2), Convert.ToInt32(birthday_mainBn.Frame.Height));
            birthday_valueBn.SetTitle("29.05.1984", UIControlState.Normal);

            address_mainBn.TitleEdgeInsets = new UIEdgeInsets(0, Convert.ToInt32(View.Frame.Width / 7), 0, 0);
            address_mainBn.SetTitle("Домашний адрес", UIControlState.Normal);
            address_mainBn.Frame = new Rectangle(0, 628, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            //birthday_mainBn.Font = birthday_mainBn.Font.WithSize(19f);
            address_forwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
                                                 Convert.ToInt32((address_mainBn.Frame.Y /*+ birthday_mainBn.Frame.Height*/) + (address_mainBn.Frame.Height / 2) - View.Frame.Width / 50),
                                                    Convert.ToInt32(View.Frame.Width / 42),
                                                    Convert.ToInt32(View.Frame.Width) / 25);
            address_imageBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 23),
                                                  Convert.ToInt32(address_mainBn.Frame.Y + address_mainBn.Frame.Height / 4),
                                                  Convert.ToInt32(address_mainBn.Frame.Height / 2.45),
                                                  Convert.ToInt32(address_mainBn.Frame.Height / 2));

            social_netw_mainBn.TitleEdgeInsets = new UIEdgeInsets(0, Convert.ToInt32(View.Frame.Width / 7), 0, 0);
            social_netw_mainBn.SetTitle("Социальные сети", UIControlState.Normal);
            social_netw_mainBn.Frame = new Rectangle(0, 676, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            //birthday_mainBn.Font = birthday_mainBn.Font.WithSize(19f);
            social_netw_forwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
                                                     Convert.ToInt32((social_netw_mainBn.Frame.Y /*+ birthday_mainBn.Frame.Height*/) + (social_netw_mainBn.Frame.Height / 2) - View.Frame.Width / 50),
                                                    Convert.ToInt32(View.Frame.Width / 42),
                                                    Convert.ToInt32(View.Frame.Width) / 25);
            social_netw_imageBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 25),
                                                      Convert.ToInt32(social_netw_mainBn.Frame.Y + social_netw_mainBn.Frame.Height / 3.5),
                                                      Convert.ToInt32(social_netw_mainBn.Frame.Height / 2),
                                                      Convert.ToInt32(social_netw_mainBn.Frame.Height / 2.2));
            //cardNameIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 25),
            //Convert.ToInt32(25),
            //Convert.ToInt32(social_netw_mainBn.Frame.Height / 2.3),
            //Convert.ToInt32(social_netw_mainBn.Frame.Height / 2.2));
            continueBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         750,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            continueBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            //continueBn.Font = continueBn.Font.WithSize(15f);
            continueBn.SetTitle("СОХРАНИТЬ", UIControlState.Normal);
            //birthday_valueBn.SetTitle("29.05.1984", UIControlState.Normal);
            view_for_picker.Frame = new CGRect(0, headerView.Frame.Height, View.Frame.Width, View.Frame.Height - headerView.Frame.Height);
            date_picker.SetDate((NSDate)DateTime.SpecifyKind(DateTime.Parse("1990-01-01"), DateTimeKind.Utc), false);
            birthday_mainBn.TouchUpInside += (s, e) =>
            {
                //hide keyboard
                UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                scrollView.Hidden = true;
                resetBn.Hidden = true;
                view_for_picker.Hidden = false;
                if (!String.IsNullOrEmpty(myBirthDate))
                {
                    date_picker.SetDate((NSDate)DateTime.SpecifyKind(DateTime.Parse(myBirthDate), DateTimeKind.Utc), false);
                }
            };
            date_picker.Frame = new CGRect(0, 0, view_for_picker.Frame.Width, view_for_picker.Frame.Height / 2);
            reset_pickerBn.Frame = new CGRect(View.Frame.Width / 5, date_picker.Frame.Height, View.Frame.Width / 5, social_netw_mainBn.Frame.Height);
            confirm_pickerBn.Frame = new CGRect(View.Frame.Width / 5 * 3, date_picker.Frame.Height, View.Frame.Width / 5, social_netw_mainBn.Frame.Height);

            confirm_pickerBn.SetTitle("Выбрать", UIControlState.Normal);
            reset_pickerBn.SetTitle("Сброс", UIControlState.Normal);
            if (String.IsNullOrEmpty(myBirthDate))
            {
                birthday_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //birthday_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                birthday_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
                birthday_valueBn.SetTitle("", UIControlState.Normal);
            }
            else
            {
                birthday_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                birthday_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                SetBirthdateTitle();
            }
            confirm_pickerBn.TouchUpInside += (s, e) =>
            {
                myBirthDate = date_picker.Date.Description.Substring(0, 10);
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
                myBirthDate = null;
                scrollView.Hidden = false;
                resetBn.Hidden = false;
                view_for_picker.Hidden = true;
                birthday_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //birthday_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                birthday_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
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
                    if (QRViewController.card_names != null)
                    {
                        bool containsName = QRViewController.card_names.Any(item => item.ToLower() == myCardName.ToLower());
                        if (containsName)
                            if (myCardName != myCardNameOriginal)
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
                    QRViewController.current_card_name_header = myCardName;
                    //caching card to db
                    //var TaskA = new Task(() =>
                    //{
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
                    foreach (var _img in PersonalDataViewControllerNew.images_list)
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
                    //caching card to db
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
                    myBirthDate,
                    HomeAddressViewController.myCountry,
                    HomeAddressViewController.myRegion,
                    HomeAddressViewController.myCity,
                    HomeAddressViewController.FullAddressStatic,
                    HomeAddressViewController.myIndex,
                    HomeAddressViewController.myNotation,
                    NewCardAddressMapViewController.lat,
                    NewCardAddressMapViewController.lng,
                    true
                    );
                    //});
                    //TaskA.Start();
                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(EditPersonalProcessViewController)), true);
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
                changedSomething = true;
                mySurname = SurnameTextField.Text;
            };
            NameMiddleNameTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
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
                changedSomething = true;
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
                changedSomething = true;
                myEmail = EmailTextField.Text;
            };
            HomePhoneTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
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
                changedSomething = true;
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
                changedSomething = true;
                myDegree = DegreeTextField.Text;
            };
            CardNameTextField.EditingChanged += (s, e) =>
            {
                changedSomething = true;
                myCardName = CardNameTextField.Text;
            };
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

        private void FillFields()
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 50;

            if (SocialNetworkTableViewSource<int, int>.selectedIndexes != null)
            {
                if (SocialNetworkTableViewSource<int, int>.selectedIndexes.Count != 0)
                {
                    social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                    social_netw_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                }
                else
                {
                    social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                    //var img = UIImage.FromBundle("button_inactive.png");
                    //social_netw_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                    social_netw_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
                }
            }
            else
            {
                social_netw_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                //var img = UIImage.FromBundle("button_inactive.png");
                //social_netw_mainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                social_netw_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
            }

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
                address_mainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
            }
            else
            {
                address_mainBn.SetBackgroundImage(null, UIControlState.Normal);
                address_mainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
            }
            if (!String.IsNullOrEmpty(myBirthDate))
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
                    if (!String.IsNullOrEmpty(myCardName))
                    {
                        CardNameTextField.FloatLabelTop();
                        //CardNameTextField.BecomeFirstResponder();
                        CardNameTextField.Text = myCardName;
                    }
                    if (!String.IsNullOrEmpty(myDegree))
                    {
                        DegreeTextField.FloatLabelTop();
                        DegreeTextField.Text = myDegree;
                    }
                    if (!String.IsNullOrEmpty(mySite))
                    {
                        if (mySite.ToLower().Contains("https://"))
                            mySite = mySite.Remove(0, "https://".Length);
                        SiteTextField.FloatLabelTop();
                        SiteTextField.Text = mySite;
                    }
                    if (!String.IsNullOrEmpty(myHomePhone))
                    {
                        HomePhoneTextField.FloatLabelTop();
                        HomePhoneTextField.Text = myHomePhone;
                    }
                    if (!String.IsNullOrEmpty(myEmail))
                    {
                        EmailTextField.FloatLabelTop();
                        EmailTextField.Text = myEmail;
                    }
                    if (!String.IsNullOrEmpty(myPhone))
                    {
                        PhoneTextField.FloatLabelTop();
                        PhoneTextField.Text = myPhone;
                    }
                    if (!String.IsNullOrEmpty(myName))
                    {
                        NameMiddleNameTextField.FloatLabelTop();
                        NameMiddleNameTextField.Text = myName + " " + myMiddlename;
                    }
                    if (!String.IsNullOrEmpty(mySurname))
                    {
                        SurnameTextField.FloatLabelTop();
                        SurnameTextField.Text = mySurname;
                    }
                    UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                    //await Task.Delay(300);
                    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, scrollView.ContentSize.Height);
                    //scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height / 2));
                    //if (_deviceModel.Contains("e 5") || _deviceModel.Contains("e 4") || _deviceModel.ToLower().Contains("se"))
                    //    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 1.8));
                    //else
                    //scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 2 - 200));
                    //var frame = scrollView.Frame;
                    //scrollView.Frame = new CGRect(0,0,0,0);
                    //scrollView.Frame = frame;

                    //view_in_scroll.Frame = new Rectangle(0, Convert.ToInt32(imageScroll.Frame.Y + imageScroll.Frame.Height/* + View.Frame.Height / 20*/), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(scrollView.ContentSize.Height - imageScroll.Frame.Height));
                    scrollView.ContentOffset = new CGPoint(0, 0);
                    scrollView.Hidden = false;
                    activityIndicator.Hidden = true;
                });
            };
            timer.Start();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            scrollView.Hidden = true;
            activityIndicator.Hidden = false;
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
            CardNameTextField.TextColor = UIColor.White;
            CardNameTextField.ReturnKeyType = UIReturnKeyType.Next;
            CardNameTextField.ShouldReturn = _ => SurnameTextField.BecomeFirstResponder();

            SurnameTextField.Placeholder = "Фамилия\u002A";
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

            view_for_picker.Frame = new CGRect(0, headerView.Frame.Y, View.Frame.Width, View.Frame.Height - headerView.Frame.Y);

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
            else
            {
                resetBn.Hidden = false;
                view_for_picker.Hidden = true;
                scrollView.Hidden = false;
            }
        }

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
            myBirthDate = null;
            //images_list.Clear();
            try { EditViewController.images_from_server_list.Clear(); } catch { };
            try { PersonalDataViewControllerNew.images_list.Clear(); } catch { };
            try { EditViewController.ids_of_attachments.Clear(); } catch { };
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
            try { SocialNetworkTableViewSource<int, int>._checkedRows.Clear(); } catch { }
            try { SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear(); } catch { }
            try { SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear(); } catch { }
            //EditCompanyDataViewController.position = null;
            if (Directory.Exists(cards_cache_dir))
                Directory.Delete(cards_cache_dir, true);
            databaseMethods.CleanPersonalNetworksTable();
            databaseMethods.ClearUsersCardTable();
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

        void SetBirthdateTitle()
        {
            var birth_array = myBirthDate.Split('-');
            try { birthday_valueBn.SetTitle(birth_array[2] + "." + birth_array[1] + "." + birth_array[0], UIControlState.Normal); }
            catch { birthday_valueBn.SetTitle(myBirthDate, UIControlState.Normal); }
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
                    PersonalDataViewControllerNew.images_list.Add(originalImage);
                    PersonalDataViewControllerNew.current_img_id = PersonalDataViewControllerNew.images_list.Count - 1;
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
        private void InitializeImages()
        {
            InitializeServerImages();
            for (int k = 0; k < PersonalDataViewControllerNew.images_list.Count; k++)
            {
                button = new UIButton();
                //button.Frame = new CGRect((int)View.Frame.Width/, 0, 100, 100);
                button.Frame = new CGRect(margin_left_for_button, 0, pickPhotoBn.Frame.Height, pickPhotoBn.Frame.Height);
                //button.SetTitle(margin_left_for_button.ToString(), UIControlState.Normal);
                //button.SetTitleColor(UIColor.Green, UIControlState.Normal);
                imageScroll.AddSubview(button);
                margin_left_for_button = 3 + margin_left_for_button + (int)(pickPhotoBn.Frame.Height);
                button.SetImage(PersonalDataViewControllerNew.images_list[k], UIControlState.Normal);
                button.TouchUpInside += new EventHandler(this.OnImageClick);
                imageScroll.ContentSize = new CoreGraphics.CGSize(margin_left_for_button + 15, _imageScrollConstraintHeight.Constant);
            }
        }
        private void InitializeServerImages()
        {
            for (int k = 0; k < EditViewController.images_from_server_list.Count; k++)
            {
                button = new UIButton();
                //button.Frame = new CGRect((int)View.Frame.Width/, 0, 100, 100);
                button.Frame = new CGRect(margin_left_for_button, 0, pickPhotoBn.Frame.Height, pickPhotoBn.Frame.Height);
                //button.SetTitle(margin_left_for_button.ToString(), UIControlState.Normal);
                //button.SetTitleColor(UIColor.Green, UIControlState.Normal);
                imageScroll.AddSubview(button);
                margin_left_for_button = 3 + margin_left_for_button + (int)(pickPhotoBn.Frame.Height);
                button.SetImage(EditViewController.images_from_server_list[k], UIControlState.Normal);
                button.TouchUpInside += new EventHandler(this.OnOldImageClick);
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
            vc.PushViewController(sb.InstantiateViewController(nameof(EditPersonalDataViewControllerNew)), false);
        }
    }
}
