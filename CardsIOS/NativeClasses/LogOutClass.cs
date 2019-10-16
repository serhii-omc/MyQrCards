using System;
using System.IO;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.Database;
using CardsPCL.Database.Tables;
using Foundation;
using SQLite;

namespace CardsIOS.NativeClasses
{
    public class LogOutClass
    {
        static DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        public static void log_out()
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ormdemo.db3");
            var db = new SQLiteConnection(dbPath);
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var cards_cache_dir = Path.Combine(docs, Constants.CardsPersonalImages);
            var logo_cache_dir = Path.Combine(docs, Constants.CardsLogo);
            var QRs_cache_dir = Path.Combine(docs, Constants.QRs_cache_dir);
            #region clearing tables, variables and photos
            if (Directory.Exists(cards_cache_dir))
                Directory.Delete(cards_cache_dir, true);
            if (Directory.Exists(logo_cache_dir))
                Directory.Delete(logo_cache_dir, true);
            if (Directory.Exists(QRs_cache_dir))
                Directory.Delete(QRs_cache_dir, true);
            try { SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear(); } catch { }
            try { SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear(); } catch { }
            try { SocialNetworkTableViewSource<int, int>._checkedRows.Clear(); } catch { }
            databaseMethods.CleanPersonalNetworksTable();
            databaseMethods.ClearCompanyCardTable();
            databaseMethods.ClearUsersCardTable();
            databaseMethods.ClearValidTillRepeatAfterTable();
            databaseMethods.CleanCloudSynTable();
            databaseMethods.CleanCardNames();
            databaseMethods.CleanEtagTable();
            //View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            var differentPurposesTable = db.Table<DifferentPurposesTable>();
            //clearing table
            try
            {
                foreach (var differentPurposeItem in differentPurposesTable)
                    databaseMethods.RemoveDifferentPurpose(differentPurposeItem.Id);
            }
            catch { }

            var loginFromTable = db.Table<LoginedFromTable>();

            //clearing table
            try
            {
                foreach (var loginFromItem in loginFromTable)
                    databaseMethods.RemoveLoginFrom(loginFromItem.Id);
            }
            catch { }

            var loginAfterTable = db.Table<LoginAfterTable>();
            //clearing table
            try
            {
                foreach (var loginAfterItem in loginAfterTable)
                    databaseMethods.RemoveLoginAfter(loginAfterItem.Id);
            }
            catch { }

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
            try { CreatingCardViewController.datalist.Clear(); } catch { }
            try { QRViewController.card_names.Clear(); } catch { }
            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
            EditCompanyDataViewControllerNew.position = null;
            EditCompanyDataViewControllerNew.logo_id = null;
            QRViewController.cards_remaining = 0;
            QRViewController.is_premium = false;
            QRViewController.ExtraPersonData = null;
            QRViewController.ExtraEmploymentData = null;
            QRViewController.CompanyLogoInQr = null;
            WaitingEmailConfirmViewController.cameFromPurge = false;
            //clearing webView cookies
            NSHttpCookieStorage CookieStorage = NSHttpCookieStorage.SharedStorage;
            foreach (var cookie in CookieStorage.Cookies)
                CookieStorage.DeleteCookie(cookie);
            #endregion clearing tables, variables and photos
        }
    }
}
