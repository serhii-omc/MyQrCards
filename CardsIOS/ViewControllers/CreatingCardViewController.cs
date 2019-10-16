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
using System.Linq;
using UIKit;

namespace CardsIOS
{
    public partial class CreatingCardViewController : UIViewController
    {
        public static nfloat cellHeight, viewWidth;
        public static List<CardListModel> datalist;
        static UIView staticLoaderView;
        static UIImageView loaderLogo;
        static UIActivityIndicatorView loaderIndicator;
        static UILabel loaderLabel;
        static Cards cards = new Cards();
        static DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        static UIStoryboard sb = UIStoryboard.FromName("Main", null);
        static UINavigationController UINavigationController_;
        static UIViewController personal_dataVC;
        List<CardListModel> deserialized_cards_list;
        List<CardListModel> reverseList;
        static Methods methods = new Methods();
        string UDID;

        public CreatingCardViewController(IntPtr handle) : base(handle)
        {

        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            clearData();
        }
        public override void ViewDidLoad()
        {
            //base.ViewDidLoad();
            InitElements();
            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };

            //create_newBn.TouchUpInside += (s, e) =>
            //{
            //    clearData();
            //    //personal_dataVC = );
            //    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(PersonalDataViewController)), false);
            //};

            // TODO
            //return;
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), true);
                return;
            }
            string res_cards_list = "";
            InvokeInBackground(async () =>
            {
                try
                {
                    var accessJWT = databaseMethods.GetAccessJwt();
                    res_cards_list = await cards.CardsListGet(accessJWT, UDID);
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
                if (/*res_card_data == Constants.status_code409 ||*/ res_cards_list == Constants.status_code401)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSeveralDevicesRestriction();
                        return;
                    });
                    return;
                }

                deserialized_cards_list = JsonConvert.DeserializeObject<List<CardListModel>>(res_cards_list);

                reverseList = deserialized_cards_list.AsEnumerable().Reverse().ToList();
                //TODO
                InvokeOnMainThread(() => InitTable());
            });
        }

        private void InitTable()
        {
            var source = new MyCardsListTableViewSource(tableView);
            var items = new List<int>();
            try { datalist.Clear(); } catch { }
            datalist = reverseList;

            for (int i = 0; i < datalist.Count; i++)
            {
                items.Add(datalist[i].id);
            }
            source.Items = items;

            tableView.RowHeight = cellHeight;
            tableView.Source = source;
            headerView.Hidden = false;
            create_newBn.Hidden = false;
            create_new_forwBn.Hidden = false;
            copyFromExistingLabel.Hidden = false;
            tableView.Hidden = false;
            mainTextTV.Hidden = true;
            emailLogo.Hidden = true;
            activityIndicator.Hidden = true;

            create_newBn.TouchUpInside += (s, e) =>
            {
                clearData();
                personal_dataVC = sb.InstantiateViewController(nameof(PersonalDataViewControllerNew));
                this.NavigationController.PushViewController(personal_dataVC, true);
            };
            create_new_forwBn.TouchUpInside += (s, e) =>
            {
                clearData();
                personal_dataVC = sb.InstantiateViewController(nameof(PersonalDataViewControllerNew));
                this.NavigationController.PushViewController(personal_dataVC, true);
            };
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;

            UINavigationController_ = this.NavigationController;

            cellHeight = View.Frame.Height / 12;
            viewWidth = View.Frame.Width;
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
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Создание визитки";

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            create_newBn.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            inner_view.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            create_newBn.TitleEdgeInsets = new UIEdgeInsets(0, 17, 0, 0);
            create_newBn.SetTitle("Создать новую визитку", UIControlState.Normal);
            create_newBn.Frame = new Rectangle(0, Convert.ToInt32(headerView.Frame.Y + headerView.Frame.Height), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 12);
            create_newBn.Font = create_newBn.Font.WithSize(19f);
            create_new_forwBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 20),
                                                    Convert.ToInt32((headerView.Frame.Y + headerView.Frame.Height) + (create_newBn.Frame.Height / 2) - View.Frame.Width / 50),
                                                    Convert.ToInt32(View.Frame.Width / 42),
                                                    Convert.ToInt32(View.Frame.Width) / 25);
            copyFromExistingLabel.Frame = new Rectangle(17, Convert.ToInt32(create_newBn.Frame.Y + create_newBn.Frame.Height), Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height) / 16);
            copyFromExistingLabel.Text = "или скопировать из существующих";

            tableView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            tableView.Frame = new Rectangle(0,
                                            (int)(headerView.Frame.Height + create_newBn.Frame.Height + copyFromExistingLabel.Frame.Height),
                                            (int)(View.Frame.Width),
                                            (int)(View.Frame.Height - headerView.Frame.Height - create_newBn.Frame.Height - copyFromExistingLabel.Frame.Height));
            emailLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), 26);
            //var d = cardsLogo.Frame.X;
            mainTextTV.Text = "Получение данных";
            mainTextTV.Font = mainTextTV.Font.WithSize(22f);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            headerView.Hidden = true;
            create_newBn.Hidden = true;
            create_new_forwBn.Hidden = true;
            copyFromExistingLabel.Hidden = true;
            tableView.Hidden = true;
            mainTextTV.Hidden = false;
            emailLogo.Hidden = false;
            activityIndicator.Hidden = false;


            staticLoaderView = new UIView();
            staticLoaderView.Frame = new CoreGraphics.CGRect(0, 0, View.Frame.Width, View.Frame.Height);
            staticLoaderView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            loaderLogo = new UIImageView();
            loaderLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            loaderLogo.Image = UIImage.FromBundle("email_confirm_waiting.png");
            loaderIndicator = new UIActivityIndicatorView();
            loaderIndicator.Color = UIColor.FromRGB(255, 99, 62);
            loaderIndicator.StartAnimating();
            loaderIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            loaderLabel = new UILabel();
            loaderLabel.Frame = new Rectangle(0, (Convert.ToInt32(loaderLogo.Frame.Y) + Convert.ToInt32(loaderLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), 26);
            //var d = cardsLogo.Frame.X;
            loaderLabel.Text = "Получение данных";
            loaderLabel.TextColor = UIColor.White;
            loaderLabel.Font = loaderLabel.Font.WithSize(22f);
            loaderLabel.TextAlignment = UITextAlignment.Center;

            staticLoaderView.AddSubviews(loaderLogo, loaderLabel, loaderIndicator);
            staticLoaderView.Hidden = true;
            View.AddSubview(staticLoaderView);

            create_newBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 17f);
        }

        public static void show_loader(int card_id)
        {
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = "CreatingCardViewController";
                var vc = UINavigationController_;
                vc.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }
            staticLoaderView.Hidden = false;

            InvokeInBackground(async () =>
            {
                var UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
                var res_card_data = await cards.CardDataGet(databaseMethods.GetAccessJwt(), card_id, UDID);
                if (/*res_card_data == Constants.status_code409 ||*/ res_card_data == Constants.status_code401)
                {
                    UINavigationController_.InvokeOnMainThread(() =>
                    {
                        ShowSeveralDevicesRestriction();
                        return;
                    });
                    return;
                }
                var des_card_data = JsonConvert.DeserializeObject<CardsDataModel>(res_card_data);
                try
                {
                    using (var url = new NSUrl(des_card_data.employment.company.logo.url))
                    using (var data = NSData.FromUrl(url))
                        CropCompanyLogoViewController.cropped_result = UIImage.LoadFromData(data);
                }
                catch
                {

                }
                UINavigationController_.InvokeOnMainThread(() =>
                {
                    try
                    {
                        PersonalDataViewControllerNew.images_list.Clear();
                    }
                    catch
                    {
                    }
                });
                if (des_card_data.gallery != null)
                {
                    foreach (var item in des_card_data.gallery)
                    {
                        UIImage image;
                        using (var url = new NSUrl(item.url))
                        using (var data = NSData.FromUrl(url))
                            image = UIImage.LoadFromData(data);
                        UINavigationController_.InvokeOnMainThread(() =>
                        {
                            PersonalDataViewControllerNew.images_list.Add(image);
                        });
                    }
                }
                UINavigationController_.InvokeOnMainThread(() =>
                {
                    PersonalDataViewControllerNew.mySurname = des_card_data?.person?.lastName;
                    PersonalDataViewControllerNew.myName = des_card_data?.person?.firstName;
                    PersonalDataViewControllerNew.myMiddlename = des_card_data?.person?.middleName;
                    PersonalDataViewControllerNew.myPhone = des_card_data?.person?.mobilePhone;
                    PersonalDataViewControllerNew.myEmail = des_card_data?.person?.email;
                    PersonalDataViewControllerNew.myHomePhone = des_card_data?.person?.homePhone;
                    PersonalDataViewControllerNew.mySite = des_card_data?.person?.siteUrl;
                    PersonalDataViewControllerNew.myDegree = des_card_data?.person?.education;
                    PersonalDataViewControllerNew.myCardName = des_card_data?.name;
                    try { PersonalDataViewControllerNew.myBirthdate = des_card_data.person.birthdate.Substring(0, 10); } catch { }

                    HomeAddressViewController.FullAddressStatic = des_card_data?.person?.location?.address;
                    HomeAddressViewController.myCountry = des_card_data?.person?.location?.country;
                    HomeAddressViewController.myRegion = des_card_data?.person?.location?.region;
                    HomeAddressViewController.myCity = des_card_data?.person?.location?.city;
                    HomeAddressViewController.myIndex = des_card_data?.person?.location?.postalCode;
                    HomeAddressViewController.myNotation = des_card_data?.person?.location?.notes;
                    NewCardAddressMapViewController.lat = des_card_data?.person?.location?.latitude?.ToString()?.Replace(',', '.');
                    NewCardAddressMapViewController.lng = des_card_data?.person?.location?.longitude?.ToString()?.Replace(',', '.');
                    try { CompanyDataViewControllerNew.position = des_card_data.employment.position; } catch { }
                    try { CompanyDataViewControllerNew.companyName = des_card_data.employment.company.name; } catch { }
                    try { CompanyDataViewControllerNew.linesOfBusiness = des_card_data.employment.company.activity; } catch { }
                    try { CompanyDataViewControllerNew.foundationYear = des_card_data.employment.company.foundedYear.ToString(); } catch { }
                    try { CompanyDataViewControllerNew.clients = des_card_data.employment.company.customers; } catch { }
                    try { CompanyDataViewControllerNew.companyPhone = des_card_data.employment.company.phone; } catch { }
                    try { CompanyDataViewControllerNew.corporativePhone = des_card_data.employment.phone; } catch { }
                    try { CompanyDataViewControllerNew.fax = des_card_data.employment.company.fax; } catch { }
                    try { CompanyDataViewControllerNew.companyEmail = des_card_data.employment.company.email; } catch { }
                    try { CompanyDataViewControllerNew.corporativeSite = des_card_data.employment.company.siteUrl; } catch { }
                    try { CompanyAddressViewController.FullCompanyAddressStatic = des_card_data.employment.company.location.address; } catch { }
                    try { CompanyAddressViewController.country = des_card_data.employment.company.location.country; } catch { }
                    try { CompanyAddressViewController.region = des_card_data.employment.company.location.region; } catch { }
                    try { CompanyAddressViewController.city = des_card_data.employment.company.location.city; } catch { }
                    try { CompanyAddressViewController.index = des_card_data.employment.company.location.postalCode; } catch { }
                    try { CompanyAddressViewController.notation = des_card_data.employment.company.location.notes; } catch { }
                    try { CompanyAddressMapViewController.company_lat = des_card_data.employment.company.location.latitude.ToString().Replace(',', '.'); } catch { }
                    try { CompanyAddressMapViewController.company_lng = des_card_data.employment.company.location.longitude.ToString().Replace(',', '.'); } catch { }
                    if (des_card_data.person.socialNetworks != null)
                    {
                        try
                        {
                            SocialNetworkTableViewSource<int, int>.selectedIndexes?.Clear();
                            SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl?.Clear();
                        }
                        catch { }
                            //foreach (var item in des_card_data.person.socialNetworks)
                            //{
                            //    SocialNetworkTableViewSource<int, int>.selectedIndexes.Add(item.id);
                            //}
                            try
                        {
                            int i = 0;
                            foreach (var item_ in SocialNetworkData.SampleData())
                            {
                                foreach (var item in des_card_data?.person?.socialNetworks)
                                {
                                    if (item.id == item_.Id)
                                    {
                                        SocialNetworkTableViewSource<int, int>.selectedIndexes.Add(i);
                                        SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Add(new SocialNetworkModel { SocialNetworkID = item.id, ContactUrl = item.contactUrl });
                                    }
                                }
                                i++;
                            }
                            foreach (var item/*index*/ in SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl)//.selectedIndexes)
                            {
                                databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = item.SocialNetworkID, ContactUrl = item.ContactUrl });
                                //databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = datalist[index].Id, ContactUrl = datalist[index].ContactUrl });
                            }
                        }
                        catch { }
                    }
                    staticLoaderView.Hidden = true;
                    personal_dataVC = sb.InstantiateViewController(nameof(PersonalDataViewControllerNew));
                    UINavigationController_.PushViewController(personal_dataVC, true);
                });
            });
        }
        void clearData()
        {
            try
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
                try { PersonalDataViewControllerNew.images_list.Clear(); } catch { }
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
                //try { PersonalDataViewController.images_list.Clear(); } catch { }
                PersonalDataViewControllerNew.myBirthdate = null;
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
                try
                {
                    SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
                    SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
                }
                catch { }
                #endregion clearing tables, variables and photos
            }
            catch(Exception ex)
            {

            }
        }

        static void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            UINavigationController_.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}