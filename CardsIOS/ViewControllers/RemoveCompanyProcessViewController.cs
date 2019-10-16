using System;
using System.Drawing;
using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using Foundation;
using UIKit;

namespace CardsIOS
{
    public partial class RemoveCompanyProcessViewController : UIViewController
    {
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Cards cards = new Cards();
        Companies companies = new Companies();
        Attachments attachments = new Attachments();
        Methods methods = new Methods();
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        string UDID;
        public RemoveCompanyProcessViewController(IntPtr handle) : base(handle)
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

            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();

            if (methods.IsConnected())
            {
                sync();
            }
            else
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
            }
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

        void sync()
        {
            InvokeInBackground(async () =>
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
                System.Net.Http.HttpResponseMessage res = null;
                try
                {
                     res = await cards.CardUpdate(databaseMethods.GetAccessJwt(),
                                                     EditViewController.card_id,
                                                     databaseMethods.GetDataFromUsersCard(null,
                                                                                          databaseMethods.GetLastSubscription(),
                                                                                          EditCompanyDataViewControllerNew.position, EditCompanyDataViewControllerNew.corporativePhone),
                                                     EditPersonalDataViewControllerNew.is_primary,
                                                     SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl,
                                                     EditViewController.ids_of_attachments,
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

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}