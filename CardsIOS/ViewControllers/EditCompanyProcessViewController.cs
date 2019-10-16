using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
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
using UIKit;

namespace CardsIOS
{
    public partial class EditCompanyProcessViewController : UIViewController
    {
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Cards cards = new Cards();
        Companies companies = new Companies();
        Attachments attachments = new Attachments();
        Methods methods = new Methods();
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        string UDID;
        public EditCompanyProcessViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewDidLoad()
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
            InvokeInBackground(async () =>
             {
                 if (!EditCompanyDataViewControllerNew.changedCompanyData)
                     goto DoPersonalStuff;
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
                         //logo_id = deserialized_logo.id;
                         logo_id = res_photos.logo_id;
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
                 else
                     logo_id = EditCompanyDataViewControllerNew.logo_id;

                 #endregion uploading photos
                 InvokeOnMainThread(() =>
                         {
                             mainTextTV.Text = "Визитка синхронизируется";
                         });
                 string res_company = null;
                 try
                 {
                     if (logo_id != null)
                     {
                         if (EditPersonalProcessViewController.company_id != null)
                             res_company = await companies.UpdateCompanyCard(databaseMethods.GetAccessJwt(), UDID, databaseMethods.GetDataFromCompanyCard(), EditPersonalProcessViewController.company_id, logo_id);
                         else
                         {
                             res_company = await companies.CreateCompanyCard(databaseMethods.GetAccessJwt(), UDID, databaseMethods.GetDataFromCompanyCard(), logo_id);
                         }
                     }
                     else
                     {
                         if (EditPersonalProcessViewController.company_id != null)
                             res_company = await companies.UpdateCompanyCard(databaseMethods.GetAccessJwt(), UDID, databaseMethods.GetDataFromCompanyCard(), EditPersonalProcessViewController.company_id);
                         else
                         {
                             res_company = await companies.CreateCompanyCard(databaseMethods.GetAccessJwt(), UDID, databaseMethods.GetDataFromCompanyCard());
                         }
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
                 if (res_company == Constants.image_upload_status_code401.ToString())
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
                     var deserialized = JsonConvert.DeserializeObject<CompanyCardResponse>(res_company);
                 }
                 catch
                 {
                     databaseMethods.ClearCompanyCardTable();
                     InvokeOnMainThread(() =>
                     {
                         var deserialized_error = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(res_company);
                         UIAlertView alert = new UIAlertView()
                         {
                             Title = "Ошибка в данных компании",
                             Message = deserialized_error[0].message
                         };
                         alert.AddButton("OK");
                         alert.Show();
                         this.NavigationController.PopViewController(true);

                     });
                 }

                 if (res_company.Contains("id") && res_company.Length < 12)
                 {
                     EditPersonalProcessViewController.company_id = Convert.ToInt32(JsonConvert.DeserializeObject<CompanyCardResponse>(res_company).id);
                 }
             DoPersonalStuff:
                 //caching card to db
                 databaseMethods.InsertUsersCard(
                         EditPersonalDataViewControllerNew.myName,
                         EditPersonalDataViewControllerNew.mySurname,
                         EditPersonalDataViewControllerNew.myMiddlename,
                         EditPersonalDataViewControllerNew.myPhone,
                         EditPersonalDataViewControllerNew.myEmail,
                         EditPersonalDataViewControllerNew.myHomePhone,
                         EditPersonalDataViewControllerNew.mySite,
                         EditPersonalDataViewControllerNew.myDegree,
                         EditPersonalDataViewControllerNew.myCardName,
                         EditPersonalDataViewControllerNew.myBirthDate,
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
                 System.Net.Http.HttpResponseMessage res = null;
                 try
                 {
                     res = await cards.CardUpdate(databaseMethods.GetAccessJwt(),
                                                      EditViewController.card_id,
                                                      databaseMethods.GetDataFromUsersCard(EditPersonalProcessViewController.company_id,
                                                                                           databaseMethods.GetLastSubscription(),
                                                                                           EditCompanyDataViewControllerNew.position, EditCompanyDataViewControllerNew.corporativePhone),
                                                      EditPersonalDataViewControllerNew.is_primary,
                                                      SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl,
                                                      EditViewController.ids_of_attachments,
                                                      UDID);
                 }
                 catch (Exception ex)
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
                 if (res.StatusCode.ToString().Contains("401") || res.StatusCode.ToString().ToLower().Contains(Constants.status_code401))
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
                     ClearAll();

                     var vc = sb.InstantiateViewController(nameof(RootQRViewController));
                     try
                     {
                         this.NavigationController.PushViewController(vc, true);
                         //var vc_list = this.NavigationController.ViewControllers.ToList();
                         //try { vc_list.RemoveAt(vc_list.Count - 2); } catch { }
                         //this.NavigationController.ViewControllers = vc_list.ToArray();
                     }
                     catch { }
                 });
             });

        }

        private void ClearAll()
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
            EditCompanyDataViewControllerNew.companyName = null;
            EditCompanyDataViewControllerNew.linesOfBusiness = null;
            EditCompanyDataViewControllerNew.position = null;
            EditCompanyDataViewControllerNew.foundationYear = null;
            EditCompanyDataViewControllerNew.clients = null;
            EditCompanyDataViewControllerNew.companyPhone = null;
            EditCompanyDataViewControllerNew.corporativePhone = null;
            EditCompanyDataViewControllerNew.fax = null;
            EditCompanyDataViewControllerNew.companyEmail = null;
            EditCompanyDataViewControllerNew.corporativeSite = null;
            HomeAddressViewController.FullAddressStatic = null;
            HomeAddressViewController.myCountry = null;
            HomeAddressViewController.myRegion = null;
            HomeAddressViewController.myCity = null;
            HomeAddressViewController.myIndex = null;
            HomeAddressViewController.myNotation = null;
            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
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

        private void InitElements()
        {
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            emailLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), 26);
            mainTextTV.Text = "Визитка синхронизируется";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
        }

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}