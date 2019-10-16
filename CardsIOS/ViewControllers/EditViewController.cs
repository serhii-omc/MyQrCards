using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using CoreGraphics;
using Foundation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UIKit;

namespace CardsIOS
{
    public partial class EditViewController : UIViewController
    {
        public static int card_id;
        public static List<int> ids_of_attachments = new List<int>();
        public static List<UIImage> images_from_server_list = new List<UIImage>();
        public static bool IsCompanyReadOnly { get; set; }
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        //bool isFirstAppearance;
        Cards cards = new Cards();
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Methods methods = new Methods();
        string UDID;
        public EditViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);
            if (parent == null)
            {
                ClearAll();
            }
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
            EditCompanyDataViewControllerNew.changedCompanyData = false;
            aboutCompanyBn.TouchUpInside += (s, e) =>
            {
                var vc = sb.InstantiateViewController(nameof(EditCompanyDataViewControllerNew));
                this.NavigationController.PushViewController(vc, true);
            };
            personalDataBn.TouchUpInside += (s, e) =>
            {
                if (switchSw.On)
                    EditPersonalDataViewControllerNew.is_primary = true;
                else
                    EditPersonalDataViewControllerNew.is_primary = false;
                var vc = sb.InstantiateViewController(nameof(EditPersonalDataViewControllerNew));
                this.NavigationController.PushViewController(vc, true);
            };
            switchSw.ValueChanged += (s, e) =>
            {
                if (switchSw.On)
                {
                    EditPersonalDataViewControllerNew.is_primary = true;
                    QRViewController.content_offset = new CGPoint(0, 0);
                    cache_data();
                    var vc = sb.InstantiateViewController(nameof(EditPersonalProcessViewController));
                    this.NavigationController.PushViewController(vc, true);
                }
            };
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };

            InitElements();

            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }
            InvokeInBackground(async () =>
            {
                string res_card_data = null;
                try
                {
                    res_card_data = await cards.CardDataGet(databaseMethods.GetAccessJwt(), card_id, UDID);
                    if (String.IsNullOrEmpty(res_card_data))
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
                if (/*res_card_data == Constants.status_code409 ||*/ res_card_data == Constants.status_code401)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSeveralDevicesRestriction();
                        return;
                    });
                    return;
                }
                var des_card_data = JsonConvert.DeserializeObject<CardsDataModel>(res_card_data);

                IsCompanyReadOnly = false;

                try { IsCompanyReadOnly = des_card_data.employment.company.isReadOnly; } catch { }

                // TODO
                //IsCompanyReadOnly = true;

                FillVariables(des_card_data);
                InvokeOnMainThread(() =>
                {
                    mainTextTV.Hidden = true;
                    activityIndicator.Hidden = true;
                    emailLogo.Hidden = true;
                    switchSw.Hidden = false;
                    makeMainBn.Hidden = false;
                    company_forw_bn.Hidden = false;
                    aboutCompanyBn.Hidden = false;
                    personal_forw_bn.Hidden = false;
                    personalDataBn.Hidden = false;
                });
            });
        }

        //public override void ViewWillAppear(bool animated)
        //{
        //    base.ViewWillAppear(animated);


        //}

        private void ClearAll()
        {
            #region clearing tables, variables and photos
            //if (Directory.Exists(cards_cache_dir))
            //    Directory.Delete(cards_cache_dir, true);
            //if (Directory.Exists(logo_cache_dir))
            //Directory.Delete(logo_cache_dir, true);
            databaseMethods.CleanPersonalNetworksTable();
            databaseMethods.ClearCompanyCardTable();
            databaseMethods.ClearUsersCardTable();
            ids_of_attachments.Clear();
            images_from_server_list.Clear();
            SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
            SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
            SocialNetworkTableViewSource<int, int>._checkedRows.Clear();
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
            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
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
            EditPersonalDataViewControllerNew.mySurname = null;
            EditPersonalDataViewControllerNew.myName = null;
            EditPersonalDataViewControllerNew.myMiddlename = null;
            EditPersonalDataViewControllerNew.myPhone = null;
            EditPersonalDataViewControllerNew.myEmail = null;
            EditPersonalDataViewControllerNew.myHomePhone = null;
            EditPersonalDataViewControllerNew.mySite = null;
            EditPersonalDataViewControllerNew.myDegree = null;
            EditPersonalDataViewControllerNew.myCardName = null;
            EditPersonalDataViewControllerNew.myBirthDate = null;
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

            #endregion clearing tables, variables and photos
        }

        private void FillVariables(CardsDataModel des_card_data)
        {
            InvokeOnMainThread(() =>
            {
                if (des_card_data.isPrimary)
                {
                    switchSw.On = true;
                    EditPersonalDataViewControllerNew.is_primary = true;
                    switchSw.UserInteractionEnabled = false;
                }
                else
                {
                    switchSw.On = false;
                    EditPersonalDataViewControllerNew.is_primary = false;
                    switchSw.UserInteractionEnabled = true;
                }
            });
            try { EditPersonalProcessViewController.company_id = des_card_data.employment.company.id; } catch { }

            ids_of_attachments.Clear();
            images_from_server_list.Clear();
            //SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
            //SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
            //SocialNetworkTableViewSource<int, int>._checkedRows.Clear();
            if (des_card_data.gallery != null)
            {
                foreach (var item in des_card_data.gallery)
                {
                    ids_of_attachments.Add(item.id);
                    try
                    {
                        UIImage image;
                        using (var url = new NSUrl(item.url))
                        using (var data = NSData.FromUrl(url))
                            image = UIImage.LoadFromData(data);
                        images_from_server_list.Add(image);
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
            }

            //des_card_data.employment.company.logo.url
            //ids_of_attachments = des_card_data.gallery.;
            //var position = des_card_data.employment.position;
            if (String.IsNullOrEmpty(QRViewController.CompanyLogoInQr))
                try
                {
                    if (des_card_data.employment.company.logo != null)
                    {
                        try
                        {
                            UIImage image_logo;
                            using (var url = new NSUrl(des_card_data.employment.company.logo.url))
                            using (var data = NSData.FromUrl(url))
                                CropCompanyLogoViewController.cropped_result = UIImage.LoadFromData(data);
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
                }
                catch { }
            //else
            //{

            //}
            if (des_card_data.person.socialNetworks != null)
            {
                try
                {
                    SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
                    SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
                }
                catch { }
                //foreach (var item in des_card_data.person.socialNetworks)
                //{
                //    SocialNetworkTableViewSource<int, int>.selectedIndexes.Add(item.id);
                //}

                int i = 0;
                foreach (var item_ in SocialNetworkData.SampleData())
                {
                    foreach (var item in des_card_data.person.socialNetworks)
                    {
                        if (item.id == item_.Id)
                        {
                            SocialNetworkTableViewSource<int, int>.selectedIndexes.Add(i);
                            SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Add(new SocialNetworkModel { SocialNetworkID = item.id, ContactUrl = item.contactUrl });
                        }
                    }
                    i++;
                }
            }
            try { EditPersonalDataViewControllerNew.mySurname = des_card_data.person.lastName; } catch { }
            try { EditPersonalDataViewControllerNew.myName = des_card_data.person.firstName; } catch { }
            try { EditPersonalDataViewControllerNew.myMiddlename = des_card_data.person.middleName; } catch { }
            try { EditPersonalDataViewControllerNew.myPhone = des_card_data.person.mobilePhone; } catch { }
            try { EditPersonalDataViewControllerNew.myEmail = des_card_data.person.email; } catch { }
            try { EditPersonalDataViewControllerNew.myHomePhone = des_card_data.person.homePhone; } catch { }
            try { EditPersonalDataViewControllerNew.mySite = des_card_data.person.siteUrl; } catch { }
            try { EditPersonalDataViewControllerNew.myDegree = des_card_data.person.education; } catch { }
            try { EditPersonalDataViewControllerNew.myCardName = des_card_data.name; } catch { }
            try { EditPersonalDataViewControllerNew.myCardNameOriginal = des_card_data.name; } catch { }
            try { EditPersonalDataViewControllerNew.myBirthDate = des_card_data.person.birthdate.Substring(0, 10); } catch { }
            try { PersonalDataViewControllerNew.images_list.Clear(); } catch { }
            try { HomeAddressViewController.FullAddressStatic = des_card_data.person.location.address; } catch { }
            try { HomeAddressViewController.myCountry = des_card_data.person.location.country; } catch { }
            try { HomeAddressViewController.myRegion = des_card_data.person.location.region; } catch { }
            try { HomeAddressViewController.myCity = des_card_data.person.location.city; } catch { }
            try { HomeAddressViewController.myIndex = des_card_data.person.location.postalCode; } catch { }
            try { HomeAddressViewController.myNotation = des_card_data.person.location.notes; } catch { }
            try { NewCardAddressMapViewController.lat = des_card_data.person.location.latitude.ToString().Replace(',', '.'); } catch { }
            try { NewCardAddressMapViewController.lng = des_card_data.person.location.longitude.ToString().Replace(',', '.'); } catch { }

            try { EditCompanyDataViewControllerNew.companyName = des_card_data.employment.company.name; } catch { }
            try { EditCompanyDataViewControllerNew.linesOfBusiness = des_card_data.employment.company.activity; } catch { }
            try { EditCompanyDataViewControllerNew.position = des_card_data.employment.position; } catch { }
            try { EditCompanyDataViewControllerNew.foundationYear = des_card_data.employment.company.foundedYear.ToString(); } catch { }
            try { EditCompanyDataViewControllerNew.clients = des_card_data.employment.company.customers; } catch { }
            try { EditCompanyDataViewControllerNew.companyPhone = des_card_data.employment.company.phone; } catch { }
            try { EditCompanyDataViewControllerNew.corporativePhone = des_card_data.employment.phone; } catch { }
            try { EditCompanyDataViewControllerNew.fax = des_card_data.employment.company.fax; } catch { }
            try { EditCompanyDataViewControllerNew.companyEmail = des_card_data.employment.company.email; } catch { }
            try { EditCompanyDataViewControllerNew.corporativeSite = des_card_data.employment.company.siteUrl; } catch { }
            try { CompanyAddressViewController.FullCompanyAddressStatic = des_card_data.employment.company.location.address; } catch { }
            try { CompanyAddressViewController.country = des_card_data.employment.company.location.country; } catch { }
            try { CompanyAddressViewController.region = des_card_data.employment.company.location.region; } catch { }
            try { CompanyAddressViewController.city = des_card_data.employment.company.location.city; } catch { }
            try { CompanyAddressViewController.index = des_card_data.employment.company.location.postalCode; } catch { }
            try { CompanyAddressViewController.notation = des_card_data.employment.company.location.notes; } catch { }
            try { CompanyAddressMapViewController.company_lat = des_card_data.employment.company.location.latitude.ToString().Replace(',', '.'); } catch { }
            try { CompanyAddressMapViewController.company_lng = des_card_data.employment.company.location.longitude.ToString().Replace(',', '.'); } catch { }
            try { EditCompanyDataViewControllerNew.logo_id = des_card_data.employment.company.logo.id; } catch { EditCompanyDataViewControllerNew.logo_id = null; }
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;
            switchSw.Hidden = true;
            makeMainBn.Hidden = true;
            company_forw_bn.Hidden = true;
            aboutCompanyBn.Hidden = true;
            personal_forw_bn.Hidden = true;
            personalDataBn.Hidden = true;
            new AppDelegate().disableAllOrientation = true;
            switchSw.OnTintColor = UIColor.FromRGB(255, 99, 62);

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
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            internalView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Редактирование";

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);

            emailLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), 26);
            //var d = cardsLogo.Frame.X;
            mainTextTV.Text = "Получение данных";
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);

            personalDataBn.TitleEdgeInsets = new UIEdgeInsets(0, 17, 0, 0);
            personalDataBn.SetTitle("Личные данные", UIControlState.Normal);
            personalDataBn.Frame = new Rectangle(0, (int)(headerView.Frame.Y + headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);

            personal_forw_bn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
                                                   Convert.ToInt32((personalDataBn.Frame.Y) + (personalDataBn.Frame.Height / 2) - View.Frame.Width / 50),
                                                    Convert.ToInt32(View.Frame.Width / 42),
                                                    Convert.ToInt32(View.Frame.Width) / 25);
            aboutCompanyBn.SetTitle("О компании", UIControlState.Normal);
            aboutCompanyBn.Frame = new Rectangle(0, (int)(personalDataBn.Frame.Y + personalDataBn.Frame.Height + 2), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            aboutCompanyBn.TitleEdgeInsets = new UIEdgeInsets(0, 17, 0, 0);
            company_forw_bn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 15),
                                                   Convert.ToInt32((aboutCompanyBn.Frame.Y) + (aboutCompanyBn.Frame.Height / 2) - View.Frame.Width / 50),
                                                    Convert.ToInt32(View.Frame.Width / 42),
                                                    Convert.ToInt32(View.Frame.Width) / 25);
            makeMainBn.SetTitle("Сделать основной", UIControlState.Normal);
            makeMainBn.Frame = new Rectangle(0, (int)(aboutCompanyBn.Frame.Y + aboutCompanyBn.Frame.Height + 7), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            makeMainBn.TitleEdgeInsets = new UIEdgeInsets(0, 17, 0, 0);
            personalDataBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            aboutCompanyBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            makeMainBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            //switchSw.Frame = new CoreGraphics.CGRect(0, 0, 100, 100);
            var switchWidth = switchSw.Frame.Width;
            var switchHeight = switchSw.Frame.Height;
            switchSw.On = false;
            switchSw.Frame = new CoreGraphics.CGRect(View.Frame.Width - View.Frame.Width / 15 - switchWidth + View.Frame.Width / 25, makeMainBn.Frame.Y + ((makeMainBn.Frame.Height - switchHeight) / 2), switchWidth, switchHeight);

            mainTextTV.Hidden = false;
            activityIndicator.Hidden = false;
            emailLogo.Hidden = false;
            switchSw.Hidden = true;
            makeMainBn.Hidden = true;
            company_forw_bn.Hidden = true;
            aboutCompanyBn.Hidden = true;
            personal_forw_bn.Hidden = true;
            personalDataBn.Hidden = true;
        }

        void cache_data()
        {
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
        }
        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}