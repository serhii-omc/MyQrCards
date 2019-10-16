using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UIKit;

namespace CardsIOS
{
    public partial class EditPersonalProcessViewController : UIViewController
    {
        Attachments attachments = new Attachments();
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        Methods methods = new Methods();
        Cards cards = new Cards();
        public static int? company_id;
        string UDID;
        public EditPersonalProcessViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();

            emailLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                        Convert.ToInt32(View.Frame.Width) / 3,
                                        Convert.ToInt32(View.Frame.Width) / 3,
                                        Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), 26);
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            mainTextTV.Text = "Визитка синхронизируется";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }
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
                        if (res_photos.attachments_ids != null)
                            attachments_ids_list = res_photos.attachments_ids;

                        if (res_photos.logo_id == Constants.image_upload_status_code401)
                        {
                            InvokeOnMainThread(() =>
                            {
                                ShowSeveralDevicesRestriction();
                                return;
                            });
                            return;
                        }
                    }

                    InvokeOnMainThread(() =>
                    {
                        mainTextTV.Text = "Визитка синхронизируется";
                    });
                }
                #endregion uploading photos
                var temp_ids = EditViewController.ids_of_attachments;//.AddRange(attachments_ids_list);
                temp_ids.AddRange(attachments_ids_list);
                System.Net.Http.HttpResponseMessage res_user = null;
                try
                {
                    res_user = await cards.CardUpdate(databaseMethods.GetAccessJwt(),
                                                           EditViewController.card_id,
                                                           databaseMethods.GetDataFromUsersCard(company_id, databaseMethods.GetLastSubscription(), EditCompanyDataViewControllerNew.position, EditCompanyDataViewControllerNew.corporativePhone),
                                                           EditPersonalDataViewControllerNew.is_primary,
                                                           SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl,
                                                           temp_ids,
                                                           UDID);
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
                if (res_user.StatusCode.ToString().Contains("401") || res_user.StatusCode.ToString().ToLower().Contains(Constants.status_code401))
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSeveralDevicesRestriction();
                        return;
                    });
                    return;
                }
                InvokeOnMainThread(() =>
                {
                    Clear();

                    var vc = sb.InstantiateViewController(nameof(RootQRViewController));
                    this.NavigationController.PushViewController(vc, true);
                });
            });

        }

        private void Clear()
        {
            databaseMethods.CleanPersonalNetworksTable();
            databaseMethods.ClearCompanyCardTable();
            databaseMethods.ClearUsersCardTable();
            SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
            SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
            SocialNetworkTableViewSource<int, int>._checkedRows.Clear();
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
            EditPersonalDataViewControllerNew.mySurname = null;
            EditPersonalDataViewControllerNew.myName = null;
            EditPersonalDataViewControllerNew.myMiddlename = null;
            EditPersonalDataViewControllerNew.myPhone = null;
            EditPersonalDataViewControllerNew.myEmail = null;
            EditPersonalDataViewControllerNew.myHomePhone = null;
            EditPersonalDataViewControllerNew.mySite = null;
            EditPersonalDataViewControllerNew.myDegree = null;
            EditPersonalDataViewControllerNew.myCardName = null;
            try { PersonalDataViewControllerNew.images_list.Clear(); } catch { }
            EditPersonalDataViewControllerNew.myBirthDate = null;
            HomeAddressViewController.FullAddressStatic = null;
            HomeAddressViewController.myCountry = null;
            HomeAddressViewController.myRegion = null;
            HomeAddressViewController.myCity = null;
            HomeAddressViewController.myIndex = null;
            HomeAddressViewController.myNotation = null;
            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
            EditCompanyDataViewControllerNew.position = null;
            EditCompanyDataViewControllerNew.logo_id = null;

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
        }

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}