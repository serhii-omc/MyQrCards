using CardsIOS.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using Foundation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using UIKit;

namespace CardsIOS
{
    public partial class CardsCreatingProcessViewController : UIViewController
    {
        Companies companies = new Companies();
        Cards cards = new Cards();
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Attachments attachments = new Attachments();
        Methods methods = new Methods();
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        public static int personal_card_id;
        public static string came_from { get; set; }
        string UDID;
        public CardsCreatingProcessViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
            InitElements();

            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }

            if (QRViewController.cards_remaining > 0)
            {
                CreateCardProcess();
            }
            else
            {
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootQRViewController)), true);
                QRViewController.just_created_card_name = databaseMethods.get_card_name();
                clearAll();
            }
        }

        private void CreateCardProcess()
        {
            InvokeInBackground(async () =>
            {
                #region uploading photos
                bool photos_exist = true;
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var cards_cache_dir = Path.Combine(documents, Constants.CardsPersonalImages);
                if (!Directory.Exists(cards_cache_dir))
                    photos_exist = false;
                else
                {
                    photos_exist = false;
                    string[] filenames = Directory.GetFiles(cards_cache_dir);
                    foreach (var img in filenames)
                    {
                        photos_exist = true;
                        break;
                    }
                }
                var documentsLogo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var logo_cache_dir = Path.Combine(documentsLogo, Constants.CardsLogo);
                if (Directory.Exists(logo_cache_dir))
                {
                    string[] filenames = Directory.GetFiles(logo_cache_dir);
                    foreach (var img in filenames)
                    {
                        photos_exist = true;
                        break;
                    }
                }
                int? logo_id = null;
                List<int> attachments_ids_list = new List<int>();
                if (photos_exist)
                {
                    InvokeOnMainThread(() =>
                    {
                        mainTextTV.Text = "Фотографии выгружаются";
                    });
                    AttachmentsUploadModel res_photos = null;
                    try
                    {
                        res_photos = await attachments.UploadIOS(databaseMethods.GetAccessJwt(), UDID);
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
                    if (res_photos != null)
                    {
                        //var deserialized_logo = JsonConvert.DeserializeObject<CompanyLogoModel>(res_photos);
                        logo_id = res_photos.logo_id;
                        attachments_ids_list = res_photos.attachments_ids;
                        if (logo_id == Constants.image_upload_status_code401)
                        {
                            InvokeOnMainThread(() =>
                            {
                                ShowSeveralDevicesRestriction();
                                return;
                            });
                            return;
                        }
                    }
                }
                #endregion uploading photos
                InvokeOnMainThread(() =>
                {
                    mainTextTV.Text = "Визитка синхронизируется";
                });
                string company_card_res = "";
                if (!CompanyDataViewControllerNew.company_null)
                {
                    try
                    {
                        if (logo_id != null)
                            company_card_res = await companies.CreateCompanyCard(databaseMethods.GetAccessJwt(), UDID, databaseMethods.GetDataFromCompanyCard(), logo_id);
                        else
                            company_card_res = await companies.CreateCompanyCard(databaseMethods.GetAccessJwt(), UDID, databaseMethods.GetDataFromCompanyCard());
                        if (company_card_res == Constants.image_upload_status_code401.ToString())
                        {
                            InvokeOnMainThread(() =>
                            {
                                ShowSeveralDevicesRestriction();
                                return;
                            });
                            return;
                        }
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
                try
                {
                    string user_card_res;
                    if (!CompanyDataViewControllerNew.company_null)
                    {
                        var deserialized = JsonConvert.DeserializeObject<CompanyCardResponse>(company_card_res);
                        try
                        {
                            user_card_res = await cards.CreatePersonalCard(databaseMethods.GetAccessJwt(),
                                                                           databaseMethods.GetDataFromUsersCard(deserialized.id,
                                                                                                                databaseMethods.GetLastSubscription(),
                                                                                                                EditCompanyDataViewControllerNew.position,
                                                                                                                EditCompanyDataViewControllerNew.corporativePhone),
                                                                           attachments_ids_list,
                                                                           UDID);
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
                        try
                        {
                            user_card_res = await cards.CreatePersonalCard(databaseMethods.GetAccessJwt(),
                                                                           databaseMethods.GetDataFromUsersCard(null,
                                                                                                                databaseMethods.GetLastSubscription(),
                                                                                                                EditCompanyDataViewControllerNew.position,
                                                                                                                EditCompanyDataViewControllerNew.corporativePhone),
                                                                           attachments_ids_list,
                                                                           UDID);
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
                    if (user_card_res == Constants.image_upload_status_code401.ToString())
                    {
                        InvokeOnMainThread(() =>
                        {
                            ShowSeveralDevicesRestriction();
                            return;
                        });
                        return;
                    }
                    try
                    {
                        var users_card_des = JsonConvert.DeserializeObject<CompanyCardResponse>(user_card_res);
                        personal_card_id = users_card_des.id;
                        QRViewController.just_created_card_name = databaseMethods.get_card_name();
                        clearAll();

                        CardDoneViewController.card_id = personal_card_id;
                        InvokeOnMainThread(() =>
                        {
                            //CardDoneViewController.variant_displaying = 1;

                            databaseMethods.InsertLastCloudSync(DateTime.Now);
                            this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(CardDoneViewController)), true);
                        });
                    }
                    catch
                    {
                        InvokeOnMainThread(() =>
                        {
                            var deserialized_error = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(user_card_res);
                            //if (deserialized_error[0].message == Constants.alreadyDone)
                            UIAlertView alert = new UIAlertView()
                            {
                                Title = "Ошибка",
                                Message = deserialized_error[0].message
                            };
                            if (deserialized_error[0].code == Constants.alreadyDone)
                                alert.Message = "Визитка с таким названием существует. ";
                            alert.AddButton("OK");
                            alert.Show();
                            pop();
                        });
                    }
                }
                catch
                {
                    InvokeOnMainThread(() =>
                    {
                        var deserialized_error = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(company_card_res);
                        if (deserialized_error != null)
                        {
                            UIAlertView alert = new UIAlertView()
                            {
                                Title = "Ошибка",
                                Message = deserialized_error[0].message
                            };
                            alert.AddButton("OK");
                            alert.Show();
                            pop();
                        }
                        else
                            this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(CardDoneViewController)), true);
                    });
                }
            });
        }

        private void InitElements()
        {
            backBn.Hidden = true;
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            //image_bgIV.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));
            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
            }
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            emailLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), 26);
            //var d = cardsLogo.Frame.X;
            mainTextTV.Text = "Визитка синхронизируется";
            //mainTextTV.Font = mainTextTV.Font.WithSize(22f);
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
        }

        void pop()
        {
            if (came_from == Constants.creating)
            {
                this.NavigationController.PopViewController(false);
            }
            else if (came_from == Constants.email_confirmation_waiting)
            {
                var vc_list = this.NavigationController.ViewControllers.ToList();
                this.NavigationController.PopToViewController(vc_list[vc_list.Count - 4], false);
            }
            else if (CropCompanyLogoViewController.came_from == Constants.attention_purge || CropCompanyLogoViewController.came_from == Constants.main_sync)
            {
                //nothing to do in this case
            }
        }
        void clearAll()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var cards_cache_dir = Path.Combine(documents, Constants.CardsPersonalImages);
            var documentsLogo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var logo_cache_dir = Path.Combine(documentsLogo, Constants.CardsLogo);
            #region clearing tables, variables and photos
            if (Directory.Exists(cards_cache_dir))
                Directory.Delete(cards_cache_dir, true);
            if (Directory.Exists(logo_cache_dir))
                Directory.Delete(logo_cache_dir, true);
            databaseMethods.CleanPersonalNetworksTable();
            databaseMethods.ClearCompanyCardTable();
            databaseMethods.ClearUsersCardTable();
            CropCompanyLogoViewController.currentImage = null;
            CropCompanyLogoViewController.cropped_result = null;
            CompanyAddressMapViewController.lat = null;
            CompanyAddressMapViewController.lng = null;
            CompanyAddressMapViewController.company_lat = null;
            CompanyAddressMapViewController.company_lng = null;
            CompanyAddressViewController.FullCompanyAddressStatic = null;
            CompanyAddressViewController.country = null;
            CompanyAddressViewController.region = null;
            CompanyAddressViewController.city = null;
            CompanyAddressViewController.index = null;
            CompanyAddressViewController.notation = null;
            CompanyDataViewControllerNew.companyName = null;
            CompanyDataViewControllerNew.linesOfBusiness = null;
            CompanyDataViewControllerNew.position = null;
            CompanyDataViewControllerNew.foundationYear = null;
            CompanyDataViewControllerNew.clients = null;
            CompanyDataViewControllerNew.companyPhone = null;
            CompanyDataViewControllerNew.corporativePhone = null;
            CompanyDataViewControllerNew.fax = null;
            CompanyDataViewControllerNew.companyEmail = null;
            CompanyDataViewControllerNew.corporativeSite = null;
            PersonalDataViewControllerNew.mySurname = null;
            PersonalDataViewControllerNew.myName = null;
            PersonalDataViewControllerNew.myMiddlename = null;
            PersonalDataViewControllerNew.myPhone = null;
            PersonalDataViewControllerNew.myEmail = null;
            PersonalDataViewControllerNew.myHomePhone = null;
            PersonalDataViewControllerNew.mySite = null;
            PersonalDataViewControllerNew.myDegree = null;
            PersonalDataViewControllerNew.myCardName = null;
            try { PersonalDataViewControllerNew.images_list.Clear(); } catch { }
            PersonalDataViewControllerNew.myBirthdate = null;
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
            CompanyAddressViewController.FullCompanyAddressTemp = null;
            CompanyAddressViewController.countryTemp = null;
            CompanyAddressViewController.regionTemp = null;
            CompanyAddressViewController.cityTemp = null;
            CompanyAddressViewController.indexTemp = null;
            CompanyAddressViewController.notationTemp = null;

            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
            EditCompanyDataViewControllerNew.position = null;
            EditCompanyDataViewControllerNew.logo_id = null;
            #endregion clearing tables, variables and photos

        }
        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}