using System;
using System.Drawing;
using System.IO;
using CardsIOS.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CoreGraphics;
using Foundation;
using UIKit;

namespace CardsIOS
{
    public partial class CompanyDataViewControllerNew : UIViewController
    {

        UIImagePickerController picker;
        string _deviceModel, logo_cache_dir;
        public static string companyName, linesOfBusiness, position, foundationYear, clients, companyPhone, corporativePhone, fax, companyEmail, corporativeSite;
        public static bool company_null;
        UIActionSheet normal;
        UIActionSheet option;
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        NativeMethods nativeMethods = new NativeMethods();

        NSObject keyboardShowObserver;
        NSObject keyboardHideObserver;
        RectangleF keyboardBounds;
        float keyboard_height;

        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Methods methods = new Methods();
        public CompanyDataViewControllerNew(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
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

            CropCompanyLogoViewController.came_from = "create";
            var documentsLogo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            logo_cache_dir = Path.Combine(documentsLogo, Constants.CardsLogo);
            InitElements(logo_cache_dir);

            logo_with_imageBn.TouchUpInside += (s, e) =>
            {
                CropCompanyLogoViewController.currentImage = logo_with_imageBn.CurrentBackgroundImage;

                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(CropCompanyLogoViewController)), true);
            };
            companyLogoMainBn.TouchUpInside += (s, e) =>
            {
                option.ShowInView(View);
                CropCompanyLogoViewController.came_from = "create";
                //this.NavigationController.PushViewController(sb.InstantiateViewController("CropCompanyLogoViewController"), true);
            };
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            resetBn.TouchUpInside += (s, e) =>
            {
                var option = UIAlertController.Create("Сбросить?", null, UIAlertControllerStyle.ActionSheet);
                option.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) => ConfirmResetData()));
                option.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null));
                this.PresentViewController(option, true, null);
            };

            conditionsBn.TouchUpInside += (s, e) =>
            {
                NSString urlString = new NSString(Constants.licenseUrl);
                NSUrl myFileUrl = new NSUrl(urlString);
                UIApplication.SharedApplication.OpenUrl(myFileUrl);
                //var vc = sb.InstantiateViewController(nameof(LicenseViewController));
                //this.NavigationController.PushViewController(vc, true);
            };
            createCardBn.TouchUpInside += (s, e) =>
            {
                string error_message = "";
                if (String.IsNullOrEmpty(companyName))
                    error_message += "Введите название компании. ";
                try
                {
                    if (!String.IsNullOrEmpty(foundationYear))
                    {
                        int year_temp = Convert.ToInt32(foundationYear);
                        if (year_temp < 1700 || year_temp > 2999)
                            error_message += "Год основания некорректен. ";
                    }
                }
                catch
                {
                    error_message += "Год основания некорректен. ";
                }
                try
                {
                    if (!String.IsNullOrEmpty(companyPhone))
                    {
                        if (companyPhone.Length > 31)
                            error_message += "Телефон компании некорректен. ";
                    }
                }
                catch
                {
                    error_message += "Телефон компании некорректен. ";
                }
                try
                {
                    if (!String.IsNullOrEmpty(fax))
                    {
                        if (fax.Length > 31)
                            error_message += "Факс некорректен. ";
                    }
                }
                catch
                {
                    error_message += "Факс компании некорректен. ";
                }
                if (!String.IsNullOrEmpty(companyEmail))
                {
                    try
                    {
                        CompanyEmailTextField.Text = methods.EmailValidation(companyEmail);
                        companyEmail = CompanyEmailTextField.Text;
                    }
                    catch
                    {
                        error_message += "Email некорректен. ";
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
                company_null = false;
                if (
                  String.IsNullOrEmpty(companyName) &&
                  String.IsNullOrEmpty(linesOfBusiness) &&
                  String.IsNullOrEmpty(position) &&
                  String.IsNullOrEmpty(foundationYear) &&
                  String.IsNullOrEmpty(clients) &&
                  String.IsNullOrEmpty(companyPhone) &&
                  String.IsNullOrEmpty(corporativePhone) &&
                  String.IsNullOrEmpty(fax) &&
                  String.IsNullOrEmpty(companyEmail) &&
                  String.IsNullOrEmpty(corporativeSite) &&
                  String.IsNullOrEmpty(CompanyAddressViewController.country) &&
                  String.IsNullOrEmpty(CompanyAddressViewController.region) &&
                  String.IsNullOrEmpty(CompanyAddressViewController.city) &&
                  String.IsNullOrEmpty(CompanyAddressViewController.index) &&
                  String.IsNullOrEmpty(CompanyAddressViewController.notation) &&
                  String.IsNullOrEmpty(CompanyAddressMapViewController.lat) &&
                  String.IsNullOrEmpty(CompanyAddressMapViewController.lng) &&
                  String.IsNullOrEmpty(CompanyAddressMapViewController.company_lat) &&
                  String.IsNullOrEmpty(CompanyAddressMapViewController.company_lng) &&
                    logo_with_imageBn.Hidden == true)
                {
                    if (databaseMethods.userExists())
                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(CardsCreatingProcessViewController)), true);
                    else
                    {
                        databaseMethods.InsertLoginedFrom(Constants.from_card_creating);
                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(ConfirmEmailViewControllerNew)), true);
                    }
                    company_null = true;
                }
                if (String.IsNullOrEmpty(error_message))
                {
                    var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    var cards_cache_dir = Path.Combine(documents, Constants.CardsLogo);
                    if (!Directory.Exists(cards_cache_dir))
                        Directory.CreateDirectory(cards_cache_dir);
                    else
                    {
                        Directory.Delete(cards_cache_dir, true);
                        Directory.CreateDirectory(cards_cache_dir);
                    }

                    UIImage resultImage;
                    var _img = logo_with_imageBn.CurrentBackgroundImage;
                    if (_img != null)
                    {
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

                        string filenames = "logo.jpeg";
                        var fileName = Path.Combine(cards_cache_dir, filenames);
                        var img = UIImage.FromFile(fileName);
                        NSError err = null;
                        image_jpeg.Save(fileName, false, out err);
                    }

                    var timestampUTC = DateTime.UtcNow.ToString();
                    //CardDoneViewController.variant_displaying = 1;

                    //if(!String.IsNullOrEmpty(companyPhone))
                    //    if (!String.IsNullOrEmpty(CompanyAdditionalPhoneTextField.Text))
                    //        companyPhone += "#" + CompanyAdditionalPhoneTextField.Text;
                    //if (!String.IsNullOrEmpty(fax))
                    //if (!String.IsNullOrEmpty(FaxAdditionalTextField.Text))
                    companyPhone = CompanyPhoneTextField.Text;
                    if (!String.IsNullOrEmpty(CompanyPhoneTextField.Text))
                        if (!String.IsNullOrEmpty(CompanyAdditionalPhoneTextField.Text))
                            companyPhone += "#" + CompanyAdditionalPhoneTextField.Text;
                    corporativePhone = CorporativePhoneTextField.Text;
                    if (!String.IsNullOrEmpty(CorporativePhoneTextField.Text))
                        if (!String.IsNullOrEmpty(CorporativePhoneAdditionalPhoneTextField.Text))
                            corporativePhone += "#" + CorporativePhoneAdditionalPhoneTextField.Text;
                    fax = FaxTextField.Text;
                    if (!String.IsNullOrEmpty(FaxTextField.Text))
                        if (!String.IsNullOrEmpty(FaxAdditionalTextField.Text))
                            fax += "#" + FaxAdditionalTextField.Text;

                    databaseMethods.InsertCompanyCard(
                      companyName,
                      linesOfBusiness,
                      position,
                      foundationYear,
                      clients,
                      companyPhone,
                      corporativePhone,
                      companyEmail,
                      fax,
                      corporativeSite,
                      CompanyAddressViewController.country,
                      CompanyAddressViewController.region,
                      CompanyAddressViewController.city,
                      CompanyAddressViewController.FullCompanyAddressStatic,
                      CompanyAddressViewController.index,
                      CompanyAddressViewController.notation,
                      CompanyAddressMapViewController.company_lat,
                      CompanyAddressMapViewController.company_lng,
                      timestampUTC
                      );
                    //databaseMethods.GetDataFromCompanyCard();
                    if (databaseMethods.userExists())
                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(CardsCreatingProcessViewController)), true);
                    else
                    {
                        databaseMethods.InsertLoginedFrom(Constants.from_card_creating);
                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(ConfirmEmailViewControllerNew)), true);
                    }
                }
                else
                {
                    UIAlertView alert = new UIAlertView()
                    {
                        Title = "Ошибка",
                        Message = error_message
                    };
                    alert.AddButton("OK");
                    if (!company_null)
                    {
                        alert.Show();
                        if (error_message.Contains("название компании"))
                        {
                            EndEditingForAll();
                            scrollView.SetContentOffset(new CGPoint(0, keyboard_height), true);
                        }
                    }
                }
            };

            addressMainBn.TouchUpInside += (s, e) =>
            {
                var vc = sb.InstantiateViewController(nameof(CompanyAddressViewController));
                if (QRViewController.ExtraEmploymentData != null)
                    this.NavigationController.PushViewController(vc, true);
                else
                {
                    call_premium_option_menu();
                }
            };

            CompanyNameTextField.EditingChanged += (s, e) =>
            {
                companyName = CompanyNameTextField.Text;
            };
            LinesOfBusinessTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                    linesOfBusiness = LinesOfBusinessTextField.Text;
                else
                {
                    LinesOfBusinessTextField.Text = null;
                    call_premium_option_menu();
                    LinesOfBusinessTextField.EndEditing(true);
                }
            };
            PositionTextField.EditingChanged += (s, e) =>
            {
                position = PositionTextField.Text;
            };
            FoundationYearTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                    foundationYear = FoundationYearTextField.Text;
                else
                {
                    FoundationYearTextField.Text = null;
                    call_premium_option_menu();
                    FoundationYearTextField.EndEditing(true);
                }
            };
            ClientsTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                    clients = ClientsTextField.Text;
                else
                {
                    ClientsTextField.Text = null;
                    call_premium_option_menu();
                    ClientsTextField.EndEditing(true);
                }
            };
            CompanyPhoneTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(CompanyPhoneTextField.Text))
                    {
                        try
                        {
                            char l = CompanyPhoneTextField.Text[CompanyPhoneTextField.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                companyPhone = CompanyPhoneTextField.Text;
                            else
                                CompanyPhoneTextField.Text = CompanyPhoneTextField.Text.Remove(CompanyPhoneTextField.Text.Length - 1);
                        }
                        catch { }
                    }
                    else
                        companyPhone = CompanyPhoneTextField.Text;
                }
                else
                {
                    CompanyPhoneTextField.Text = null;
                    call_premium_option_menu();
                    CompanyPhoneTextField.EndEditing(true);
                }
            };
            CompanyAdditionalPhoneTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(CompanyAdditionalPhoneTextField.Text))
                    {
                        try
                        {
                            char l = CompanyAdditionalPhoneTextField.Text[CompanyAdditionalPhoneTextField.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                return;
                            else
                                CompanyAdditionalPhoneTextField.Text = CompanyAdditionalPhoneTextField.Text.Remove(CompanyAdditionalPhoneTextField.Text.Length - 1);
                        }
                        catch { }
                    }

                }
                else
                {
                    CompanyAdditionalPhoneTextField.Text = null;
                    call_premium_option_menu();
                    CompanyAdditionalPhoneTextField.EndEditing(true);
                }
            };
            FaxAdditionalTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(FaxAdditionalTextField.Text))
                    {
                        try
                        {
                            char l = FaxAdditionalTextField.Text[FaxAdditionalTextField.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                return;
                            else
                                FaxAdditionalTextField.Text = FaxAdditionalTextField.Text.Remove(FaxAdditionalTextField.Text.Length - 1);
                        }
                        catch { }
                    }
                }
                else
                {
                    FaxAdditionalTextField.Text = null;
                    call_premium_option_menu();
                    FaxAdditionalTextField.EndEditing(true);
                }
            };
            CorporativePhoneTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(CorporativePhoneTextField.Text))
                    {
                        try
                        {
                            char l = CorporativePhoneTextField.Text[CorporativePhoneTextField.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                corporativePhone = CorporativePhoneTextField.Text;
                            else
                                CorporativePhoneTextField.Text = CorporativePhoneTextField.Text.Remove(CorporativePhoneTextField.Text.Length - 1);
                        }
                        catch { }
                    }
                    else
                        corporativePhone = CorporativePhoneTextField.Text;
                }
                else
                {
                    CorporativePhoneTextField.Text = null;
                    call_premium_option_menu();
                    CorporativePhoneTextField.EndEditing(true);
                }
            };
            CorporativePhoneTextField.ShouldReturn = _ => CorporativePhoneAdditionalPhoneTextField.BecomeFirstResponder();
            CorporativePhoneAdditionalPhoneTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(CorporativePhoneAdditionalPhoneTextField.Text))
                    {
                        try
                        {
                            char l = CorporativePhoneAdditionalPhoneTextField.Text[CorporativePhoneAdditionalPhoneTextField.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                return;
                            else
                                CorporativePhoneAdditionalPhoneTextField.Text = CorporativePhoneAdditionalPhoneTextField.Text.Remove(CorporativePhoneAdditionalPhoneTextField.Text.Length - 1);
                        }
                        catch { }
                    }

                }
                else
                {
                    CorporativePhoneAdditionalPhoneTextField.Text = null;
                    call_premium_option_menu();
                    CorporativePhoneAdditionalPhoneTextField.EndEditing(true);
                }
            };
            CorporativePhoneAdditionalPhoneTextField.ShouldReturn = _ => FaxTextField.BecomeFirstResponder();
            FaxTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                {
                    if (!String.IsNullOrEmpty(FaxTextField.Text))
                    {
                        try
                        {
                            char l = FaxTextField.Text[FaxTextField.Text.Length - 1];
                            if (l == '+' || l == '-' || l == '(' || l == ')' || l == '0' || l == '1' || l == '2' || l == '3' || l == '4' || l == '5' || l == '6' || l == '7' || l == '8' || l == '9')
                                fax = FaxTextField.Text;
                            else
                                FaxTextField.Text = FaxTextField.Text.Remove(FaxTextField.Text.Length - 1);
                        }
                        catch { }
                    }
                    else
                        fax = FaxTextField.Text;
                }
                else
                {
                    FaxTextField.Text = null;
                    call_premium_option_menu();
                    FaxTextField.EndEditing(true);
                }
            };
            CompanyEmailTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                    companyEmail = CompanyEmailTextField.Text;
                else
                {
                    CompanyEmailTextField.Text = null;
                    call_premium_option_menu();
                    CompanyEmailTextField.EndEditing(true);
                }
            };
            CorporativeSiteTextField.EditingChanged += (s, e) =>
            {
                if (QRViewController.ExtraEmploymentData != null)
                    corporativeSite = CorporativeSiteTextField.Text;
                else
                {
                    CorporativeSiteTextField.Text = null;
                    call_premium_option_menu();
                    CorporativeSiteTextField.EndEditing(true);
                }
            };

            if (String.IsNullOrEmpty(QRViewController.ExtraPersonData))
                hang_locks();
        }

        private void EndEditingForAll()
        {
            CompanyNameTextField.EndEditing(true);
            LinesOfBusinessTextField.EndEditing(true);
            PositionTextField.EndEditing(true);
            FoundationYearTextField.EndEditing(true);
            ClientsTextField.EndEditing(true);
            CompanyPhoneTextField.EndEditing(true);
            CompanyAdditionalPhoneTextField.EndEditing(true);
            CorporativePhoneTextField.EndEditing(true);
            CorporativePhoneAdditionalPhoneTextField.EndEditing(true);
            FaxTextField.EndEditing(true);
            FaxAdditionalTextField.EndEditing(true);
            CompanyEmailTextField.EndEditing(true);
            CorporativeSiteTextField.EndEditing(true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            CardsCreatingProcessViewController.came_from = Constants.creating;
            FillFields();
        }

        private void FillFields()
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 50;

            if (
                String.IsNullOrEmpty(CompanyAddressViewController.FullCompanyAddressStatic) &&
                String.IsNullOrEmpty(CompanyAddressViewController.country) &&
                String.IsNullOrEmpty(CompanyAddressViewController.region) &&
                String.IsNullOrEmpty(CompanyAddressViewController.city) &&
                String.IsNullOrEmpty(CompanyAddressViewController.index) &&
                String.IsNullOrEmpty(CompanyAddressViewController.notation) &&
                String.IsNullOrEmpty(CompanyAddressMapViewController.company_lat) &&
                String.IsNullOrEmpty(CompanyAddressMapViewController.company_lng)
            )
            {
                //addressMainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                addressMainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
            }
            else
            {
                addressMainBn.SetBackgroundImage(null, UIControlState.Normal);
                addressMainBn.SetTitleColor(UIColor.White, UIControlState.Normal);
            }

            timer.Elapsed += delegate
            {
                timer.Stop();
                timer.Dispose();
                InvokeOnMainThread(async () =>
                {
                    if (!String.IsNullOrEmpty(companyName))
                    {
                        CompanyNameTextField.FloatLabelTop();
                        CompanyNameTextField.Text = companyName;
                    }
                    if (!String.IsNullOrEmpty(linesOfBusiness))
                    {
                        LinesOfBusinessTextField.FloatLabelTop();
                        LinesOfBusinessTextField.Text = linesOfBusiness;
                    }
                    if (!String.IsNullOrEmpty(position))
                    {
                        PositionTextField.FloatLabelTop();
                        PositionTextField.Text = position;
                    }
                    if (!String.IsNullOrEmpty(foundationYear))
                    {
                        FoundationYearTextField.FloatLabelTop();
                        FoundationYearTextField.Text = foundationYear;
                    }
                    if (!String.IsNullOrEmpty(clients))
                    {
                        ClientsTextField.FloatLabelTop();
                        ClientsTextField.Text = clients;
                    }
                    if (!String.IsNullOrEmpty(companyPhone))
                    {
                        CompanyPhoneTextField.FloatLabelTop();
                        if (!companyPhone.Contains("#"))
                            CompanyPhoneTextField.Text = companyPhone;
                        else
                        {
                            var array = companyPhone.Split("#");
                            try
                            {
                                CompanyPhoneTextField.Text = array[0];
                                CompanyAdditionalPhoneTextField.Text = array[1];
                                CompanyAdditionalPhoneTextField.FloatLabelTop();
                            }
                            catch
                            {
                                CompanyPhoneTextField.Text = array[0];
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(corporativePhone))
                    {
                        CorporativePhoneTextField.FloatLabelTop();
                        if (!corporativePhone.Contains("#"))
                            CorporativePhoneTextField.Text = corporativePhone;
                        else
                        {
                            var array = corporativePhone.Split("#");
                            try
                            {
                                CorporativePhoneTextField.Text = array[0];
                                CorporativePhoneAdditionalPhoneTextField.Text = array[1];
                                CorporativePhoneAdditionalPhoneTextField.FloatLabelTop();
                            }
                            catch
                            {
                                CorporativePhoneTextField.Text = array[0];
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(fax))
                    {
                        FaxTextField.FloatLabelTop();
                        if (!fax.Contains("#"))
                            FaxTextField.Text = fax;
                        else
                        {
                            var array = fax.Split("#");
                            try
                            {
                                FaxTextField.Text = array[0];
                                FaxAdditionalTextField.Text = array[1];
                                FaxAdditionalTextField.FloatLabelTop();
                            }
                            catch
                            {
                                FaxTextField.Text = array[0];
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(companyEmail))
                    {
                        CompanyEmailTextField.FloatLabelTop();
                        CompanyEmailTextField.Text = companyEmail;
                    }
                    if (!String.IsNullOrEmpty(corporativeSite))
                    {
                        if (corporativeSite.ToLower().Contains("https://"))
                            corporativeSite = corporativeSite.Remove(0, "https://".Length);
                        CorporativeSiteTextField.FloatLabelTop();
                        CorporativeSiteTextField.Text = corporativeSite;
                    }
                    UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                    //await Task.Delay(300);
                    //scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height / 2));
                    //if (_deviceModel.Contains("e 5") || _deviceModel.Contains("e 4") || _deviceModel.ToLower().Contains("se"))
                    //    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 1.8));
                    //else
                    //scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 2 - 300));
                    //view_in_scroll.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(scrollView.ContentSize.Height));
                    scrollView.Hidden = false;
                    activityIndicator.Hidden = true;
                });
            };
            timer.Start();
            if (CropCompanyLogoViewController.cropped_result != null)
            {
                logo_with_imageBn.SetBackgroundImage(CropCompanyLogoViewController.cropped_result, UIControlState.Normal);
                try
                {
                    logo_with_imageBn.LayoutIfNeeded();
                    logo_with_imageBn.Subviews[0].ContentMode = UIViewContentMode.ScaleAspectFit;
                }
                catch
                {

                }
                logo_with_imageBn.Hidden = false;
                companyLogoForwBn.Hidden = true;
                companyLogoMainBn.Hidden = true;
                companyLogoImageBn.Hidden = true;
            }
            else
            {
                //companyLogoMainBn.SetBackgroundImage(UIImage.FromBundle("button_inactive.png"), UIControlState.Normal);
                companyLogoMainBn.SetTitleColor(UIColor.FromRGB(146, 150, 155), UIControlState.Normal);
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            scrollView.Hidden = true;
            activityIndicator.Hidden = false;
        }


        private void InitElements(string logo_cache_dir)
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;
            scrollView.Hidden = true;
            new AppDelegate().disableAllOrientation = true;

            company_null = false;

            string[] tableItems = new string[] { "Сделать снимок", "Выбрать фотографию" };
            //normal = new UIActionSheet("Simple ActionSheet", null, "Cancel", "Delete", null);
            option = new UIActionSheet(null, null, "Отменить", null, tableItems);
            option.Clicked += (btn_sender, args) => //Console.WriteLine("{0} Clicked", args.ButtonIndex);
            {
                if (args.ButtonIndex == 1)
                {
                    CropCompanyLogoViewController.came_from_gallery_or_camera = Constants.gallery;
                    picker = new UIImagePickerController();
                    picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                    picker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
                    picker.FinishedPickingMedia += Finished;
                    picker.Canceled += Canceled;

                    PresentViewController(picker, animated: true, completionHandler: null);
                }
                if (args.ButtonIndex == 0)
                {
                    CropCompanyLogoViewController.came_from_gallery_or_camera = Constants.camera;
                    var storyboard = UIStoryboard.FromName("Main", NSBundle.MainBundle);
                    CameraController.came_from = Constants.company_logo;
                    var vc = storyboard.InstantiateViewController(nameof(CameraController));
                    this.NavigationController.PushViewController(vc, true);
                }
            };


            _deviceModel = Xamarin.iOS.DeviceHardware.Model;
            _headerConstraintTop.Constant = UIApplication.SharedApplication.StatusBarFrame.Height;
            _headerConstraintHeight.Constant = View.Frame.Height / 12;
            //image_bgIV.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            scrollView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            //if (_deviceModel.Contains("X"))
            //{
            //    headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
            //    backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            //    headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            //    resetBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), Convert.ToInt32(View.Frame.Width) / 12 + 20, Convert.ToInt32(View.Frame.Width) / 4, Convert.ToInt32(View.Frame.Width) / 19);
            //}
            //else
            //{
            //headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
            backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            resetBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), Convert.ToInt32(View.Frame.Width) / 12, Convert.ToInt32(View.Frame.Width) / 4, Convert.ToInt32(View.Frame.Width) / 19);
            //}
            createCardBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            //View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            //View.BackgroundColor = UIColor.Red;
            //headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "О компании";
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(headerView.Frame.Height * 2), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));

            scrollView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3F, backBn.Frame.Width / 2.5F, backBn.Frame.Height / 3F, backBn.Frame.Width / 2.5F);
            resetBn.SetTitle("Сбросить", UIControlState.Normal);
            //scrollView.Frame = new Rectangle(0, Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height - headerView.Frame.Height)));
            //if (deviceModel.Contains("e 5") || deviceModel.Contains("e 4"))
            //    scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 1.8));
            //else
            //scrollView.ContentSize = new CoreGraphics.CGSize(View.Frame.Width, Convert.ToInt32(View.Frame.Height * 2 - 300));
            //profileIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - (View.Frame.Width / 2.5) * 2), 20, Convert.ToInt32(View.Frame.Width - View.Frame.Width / 2.5), Convert.ToInt32(View.Frame.Width - View.Frame.Width / 1.8));
            //flapperBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - (View.Frame.Width / 2.5) * 2), 20, Convert.ToInt32(View.Frame.Width - View.Frame.Width / 2.5), Convert.ToInt32(View.Frame.Width - View.Frame.Width / 1.8));
            resetBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            companyLogoMainBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            conditionsBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            createCardBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            addressMainBn.Font = UIFont.FromName(Constants.fira_sans, 15f);

            CompanyNameTextField.Placeholder = "Название компании\u002A";
            CompanyNameTextField.TextColor = UIColor.White;
            CompanyNameTextField.ReturnKeyType = UIReturnKeyType.Next;
            CompanyNameTextField.ShouldReturn = _ => LinesOfBusinessTextField.BecomeFirstResponder();

            LinesOfBusinessTextField.Placeholder = "Направления деятельности";
            LinesOfBusinessTextField.TextColor = UIColor.White;
            LinesOfBusinessTextField.ReturnKeyType = UIReturnKeyType.Next;
            LinesOfBusinessTextField.ShouldReturn = _ => PositionTextField.BecomeFirstResponder();

            PositionTextField.Placeholder = "Должность";
            PositionTextField.TextColor = UIColor.White;
            PositionTextField.ReturnKeyType = UIReturnKeyType.Next;
            PositionTextField.ShouldReturn = _ => FoundationYearTextField.BecomeFirstResponder();

            FoundationYearTextField.Placeholder = "Год основания";
            FoundationYearTextField.TextColor = UIColor.White;
            FoundationYearTextField.ReturnKeyType = UIReturnKeyType.Next;
            FoundationYearTextField.ShouldReturn = _ => ClientsTextField.BecomeFirstResponder();

            ClientsTextField.Placeholder = "Клиенты";
            ClientsTextField.TextColor = UIColor.White;
            ClientsTextField.ReturnKeyType = UIReturnKeyType.Next;
            ClientsTextField.ShouldReturn = _ => CompanyPhoneTextField.BecomeFirstResponder();

            CompanyPhoneTextField.Placeholder = "Телефон компании";
            CompanyPhoneTextField.TextColor = UIColor.White;
            CompanyPhoneTextField.ReturnKeyType = UIReturnKeyType.Next;

            CompanyPhoneTextField.ShouldReturn = _ => CompanyAdditionalPhoneTextField.BecomeFirstResponder();

            CompanyAdditionalPhoneTextField.Placeholder = "Добавочный";
            CompanyAdditionalPhoneTextField.TextColor = UIColor.White;
            CompanyAdditionalPhoneTextField.ReturnKeyType = UIReturnKeyType.Next;
            CompanyAdditionalPhoneTextField.ShouldReturn = _ => CorporativePhoneTextField.BecomeFirstResponder();

            CorporativePhoneTextField.Placeholder = "Рабочий телефон";
            CorporativePhoneTextField.TextColor = UIColor.White;
            CorporativePhoneTextField.ReturnKeyType = UIReturnKeyType.Next;
            CorporativePhoneTextField.ShouldReturn = _ => CorporativePhoneAdditionalPhoneTextField.BecomeFirstResponder();

            CorporativePhoneAdditionalPhoneTextField.Placeholder = "Добавочный";
            CorporativePhoneAdditionalPhoneTextField.TextColor = UIColor.White;
            CorporativePhoneAdditionalPhoneTextField.ReturnKeyType = UIReturnKeyType.Next;
            CorporativePhoneAdditionalPhoneTextField.ShouldReturn = _ => FaxTextField.BecomeFirstResponder();

            FaxTextField.Placeholder = "Факс";
            FaxTextField.TextColor = UIColor.White;
            //KeyboardType = UIKeyboardType.PhonePad,
            FaxTextField.ReturnKeyType = UIReturnKeyType.Next;
            FaxTextField.ShouldReturn = _ => FaxAdditionalTextField.BecomeFirstResponder();

            FaxAdditionalTextField.Placeholder = "Добавочный";
            FaxAdditionalTextField.TextColor = UIColor.White;
            FaxAdditionalTextField.ReturnKeyType = UIReturnKeyType.Next;
            FaxAdditionalTextField.ShouldReturn = _ => CompanyEmailTextField.BecomeFirstResponder();

            CompanyEmailTextField.Placeholder = "Email компании";
            CompanyEmailTextField.TextColor = UIColor.White;
            CompanyEmailTextField.ReturnKeyType = UIReturnKeyType.Next;
            CompanyEmailTextField.ShouldReturn = _ => CorporativeSiteTextField.BecomeFirstResponder();

            CorporativeSiteTextField.Placeholder = "Сайт компании";
            CorporativeSiteTextField.TextColor = UIColor.White;
            CorporativeSiteTextField.ReturnKeyType = UIReturnKeyType.Done;
            CorporativeSiteTextField.ShouldReturn = _ => View.EndEditing(true);


            CompanyPhoneTextField.EditingDidEnd += (s, e) =>
            {
                if (CompanyPhoneTextField.Text.Length <= 2)
                {
                    companyPhone = null;
                    CompanyPhoneTextField.Text = null;
                }
            };

            CorporativePhoneTextField.EditingDidEnd += (s, e) =>
            {
                if (CorporativePhoneTextField.Text.Length <= 2)
                {
                    corporativePhone = null;
                    CorporativePhoneTextField.Text = null;
                }
            };

            FaxTextField.EditingDidEnd += (s, e) =>
            {
                if (FaxTextField.Text.Length <= 2)
                {
                    fax = null;
                    FaxTextField.Text = null;
                }
            };
            CorporativePhoneTextField.EditingDidBegin += (s, e) =>
            {
                if (String.IsNullOrEmpty(corporativePhone))
                    CorporativePhoneTextField.Text = "+7";
            };
            CompanyPhoneTextField.EditingDidBegin += (s, e) =>
            {
                if (String.IsNullOrEmpty(companyPhone))
                    CompanyPhoneTextField.Text = "+7";
            };
            FaxTextField.EditingDidBegin += (s, e) =>
            {
                if (String.IsNullOrEmpty(fax))
                    FaxTextField.Text = "+7";
            };
            addressMainBn.TitleEdgeInsets = new UIEdgeInsets(0, Convert.ToInt32(View.Frame.Width / 7), 0, 0);
            addressMainBn.SetTitle("Адрес компании", UIControlState.Normal);
            addressMainBn.Frame = new Rectangle(0, 700, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            //birthday_mainBn.Font = birthday_mainBn.Font.WithSize(19f);
            addressForwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
                                                 Convert.ToInt32((addressMainBn.Frame.Y /*+ birthday_mainBn.Frame.Height*/) + (addressMainBn.Frame.Height / 2) - View.Frame.Width / 50),
                                                    Convert.ToInt32(View.Frame.Width / 42),
                                                    Convert.ToInt32(View.Frame.Width) / 25);
            address_imageBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 23),
                                                  Convert.ToInt32(addressMainBn.Frame.Y + addressMainBn.Frame.Height / 4),
                                                  Convert.ToInt32(addressMainBn.Frame.Height / 2.45),
                                                  Convert.ToInt32(addressMainBn.Frame.Height / 2));

            companyLogoMainBn.TitleEdgeInsets = new UIEdgeInsets(0, Convert.ToInt32(View.Frame.Width / 7), 0, 0);
            companyLogoMainBn.SetTitle("Лого компании", UIControlState.Normal);
            companyLogoMainBn.Frame = new Rectangle(0, (int)(addressMainBn.Frame.Y + addressMainBn.Frame.Height), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            //birthday_mainBn.Font = birthday_mainBn.Font.WithSize(19f);
            companyLogoForwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
                                                     Convert.ToInt32((companyLogoMainBn.Frame.Y /*+ birthday_mainBn.Frame.Height*/) + (companyLogoMainBn.Frame.Height / 2) - View.Frame.Width / 50),
                                                     Convert.ToInt32(View.Frame.Width / 42),
                                                     Convert.ToInt32(View.Frame.Width) / 25);
            companyLogoImageBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 25),
                                                     Convert.ToInt32(companyLogoMainBn.Frame.Y + companyLogoMainBn.Frame.Height / 3.5),
                                                     Convert.ToInt32(companyLogoMainBn.Frame.Height / 2.5),
                                                     Convert.ToInt32(companyLogoMainBn.Frame.Height / 2.5));
            logo_with_imageBn.Frame = new CGRect(companyLogoImageBn.Frame.X, companyLogoMainBn.Frame.Y, companyLogoMainBn.Frame.Height, companyLogoMainBn.Frame.Height);
            logo_with_imageBn.Hidden = true;

            var mutableStr = new NSMutableAttributedString("Нажимая кнопку \u00ABСоздать визитку\u00BB, вы соглашаетесь с условиями и даёте согласие на обработку ваших персональных данных");
            var range = mutableStr.MutableString.LocalizedStandardRangeOfString(new NSString("условиями"));
            mutableStr.AddAttribute(UIStringAttributeKey.ForegroundColor, UIColor.FromRGB(255, 99, 62), range);
            //mutableStr.AddAttribute(UIStringAttributeKey.Font, UIFont.BoldSystemFontOfSize(30), range);
            conditionsBn.SetAttributedTitle(mutableStr, UIControlState.Normal);
            conditionsBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width / 25),
                                                     (int)(companyLogoMainBn.Frame.Y + companyLogoMainBn.Frame.Height + View.Frame.Width / 10),
                                                     (int)(View.Frame.Width - (View.Frame.Width / 25) * 2),
                                                     (int)(View.Frame.Width) / 25);
            conditionsBn.TitleLabel.Lines = 5;
            createCardBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (int)(conditionsBn.Frame.Y + conditionsBn.Frame.Height + View.Frame.Width / 8),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            createCardBn.SetTitle("СОЗДАТЬ ВИЗИТКУ", UIControlState.Normal);
        }

        void hang_locks()
        {
            var lock_height = 25;
            var lock_width = lock_height / 1.6;
            var linesOfBusiness_lock = new UIImageView();
            linesOfBusiness_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, 10, lock_width, lock_height);
            linesOfBusiness_lock.Image = UIImage.FromBundle("orange_lock.png");
            var year_lock = new UIImageView();
            year_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, 10, lock_width, lock_height);
            year_lock.Image = UIImage.FromBundle("orange_lock.png");
            var clients_lock = new UIImageView();
            clients_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, 10, lock_width, lock_height);
            clients_lock.Image = UIImage.FromBundle("orange_lock.png");
            var company_phone_lock = new UIImageView();
            company_phone_lock.Frame = new CGRect(View.Frame.Width / 1.85, 10, lock_width, lock_height);
            company_phone_lock.Image = UIImage.FromBundle("orange_lock.png");
            var company_addit_phone_lock = new UIImageView();
            company_addit_phone_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, 10, lock_width, lock_height);
            company_addit_phone_lock.Image = UIImage.FromBundle("orange_lock.png");
            var corporative_phone_lock = new UIImageView();
            corporative_phone_lock.Frame = new CGRect(View.Frame.Width / 1.85, 10, lock_width, lock_height);
            corporative_phone_lock.Image = UIImage.FromBundle("orange_lock.png");
            var corporative_addit_phone_lock = new UIImageView();
            corporative_addit_phone_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, 10, lock_width, lock_height);
            corporative_addit_phone_lock.Image = UIImage.FromBundle("orange_lock.png");
            var fax_lock = new UIImageView();
            fax_lock.Frame = new CGRect(View.Frame.Width / 1.85, 10, lock_width, lock_height);
            fax_lock.Image = UIImage.FromBundle("orange_lock.png");
            var fax_addit_lock = new UIImageView();
            fax_addit_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, 10, lock_width, lock_height);
            fax_addit_lock.Image = UIImage.FromBundle("orange_lock.png");
            var email_lock = new UIImageView();
            email_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, 10, lock_width, lock_height);
            email_lock.Image = UIImage.FromBundle("orange_lock.png");
            var site_lock = new UIImageView();
            site_lock.Frame = new CGRect(View.Frame.Width - lock_width * 2.3, 10, lock_width, lock_height);
            site_lock.Image = UIImage.FromBundle("orange_lock.png");
            var address_lock = new UIImageView();
            address_lock.Frame = new CGRect(View.Frame.Width - lock_width * 3.1, (60 - lock_height) / 2, lock_width, lock_height);
            address_lock.Image = UIImage.FromBundle("orange_lock.png");

            _linesOfBusinessView.Add(linesOfBusiness_lock);
            _foundationYearView.Add(year_lock);
            _clientsView.Add(clients_lock);
            _companyPhoneView.Add(company_phone_lock);
            _companyPhoneView.Add(company_addit_phone_lock);
            _corporporativePhoneView.Add(corporative_phone_lock);
            _corporporativePhoneView.Add(corporative_addit_phone_lock);
            _faxView.Add(fax_lock);
            _faxView.Add(fax_addit_lock);
            _companyEmailView.Add(email_lock);
            _corporativeSiteView.Add(site_lock);
            addressMainBn.Add(address_lock);

            var linesOfBusinessLockButton = new UIButton();
            var yearLockButton = new UIButton();
            var clientsLockButton = new UIButton();
            var companyPhoneLockButton = new UIButton();
            var corporativePhoneLockButton = new UIButton();
            var faxLockButton = new UIButton();
            var emailLockButton = new UIButton();
            var siteLockButton = new UIButton();
            var addressLockButton = new UIButton();

            linesOfBusinessLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 55);
            yearLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 55);
            clientsLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 55);
            companyPhoneLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 55);
            corporativePhoneLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 55);
            faxLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 55);
            emailLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 55);
            siteLockButton.Frame = new CGRect(0, 0, View.Frame.Width, 55);
            addressLockButton.Frame = addressMainBn.Frame;


            scrollView.AddSubviews(
                 linesOfBusinessLockButton,
                 yearLockButton,
                 clientsLockButton,
                 companyPhoneLockButton,
                 corporativePhoneLockButton,
                 faxLockButton,
                 emailLockButton,
                 siteLockButton,
                 addressLockButton
            );
            _linesOfBusinessView.Add(linesOfBusinessLockButton);
            _foundationYearView.Add(yearLockButton);
            _clientsView.Add(clientsLockButton);
            _companyPhoneView.Add(companyPhoneLockButton);
            _corporporativePhoneView.Add(corporativePhoneLockButton);
            _faxView.Add(faxLockButton);
            _companyEmailView.Add(emailLockButton);
            _corporativeSiteView.Add(siteLockButton);
            scrollView.Add(addressLockButton);

            linesOfBusinessLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
            yearLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
            clientsLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
            companyPhoneLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
            corporativePhoneLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
            faxLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
            emailLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
            siteLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
            addressLockButton.TouchUpInside += (s, e) => call_premium_option_menu();
        }

        void reset_data(string logo_cache_dir)
        {
            companyName = null;
            linesOfBusiness = null;
            position = null;
            foundationYear = null;
            clients = null;
            companyPhone = null;
            corporativePhone = null;
            fax = null;
            companyEmail = null;
            corporativeSite = null;
            databaseMethods.ClearCompanyCardTable();
            CropCompanyLogoViewController.cropped_result = null;
            CompanyAddressViewController.FullCompanyAddressStatic = null;
            CompanyAddressViewController.country = null;
            CompanyAddressViewController.region = null;
            CompanyAddressViewController.city = null;
            CompanyAddressViewController.index = null;
            CompanyAddressViewController.notation = null;
            CompanyAddressMapViewController.company_lat = null;
            CompanyAddressMapViewController.company_lng = null;
            EditCompanyDataViewControllerNew.logo_id = null;

            CompanyAddressViewController.FullCompanyAddressTemp = null;
            CompanyAddressViewController.countryTemp = null;
            CompanyAddressViewController.regionTemp = null;
            CompanyAddressViewController.cityTemp = null;
            CompanyAddressViewController.indexTemp = null;
            CompanyAddressViewController.notationTemp = null;

            if (Directory.Exists(logo_cache_dir))
                Directory.Delete(logo_cache_dir, true);
        }
        //void SetInsets(object sender, EventArgs e)
        //{
        //    var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboard_height, 0.0f);
        //    scrollView.ContentInset = contentInsets;
        //    scrollView.ScrollIndicatorInsets = contentInsets;
        //}

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
                    CropCompanyLogoViewController.currentImage = originalImage;
                    var vc = sb.InstantiateViewController(nameof(CropCompanyLogoViewController));
                    CropCompanyLogoViewController.came_from = "create";
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

        void ConfirmResetData()
        {
            reset_data(logo_cache_dir);
            var vc = NavigationController;
            NavigationController.PopViewController(false);
            vc.PushViewController(sb.InstantiateViewController(nameof(CompanyDataViewControllerNew)), false);
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
    }
}