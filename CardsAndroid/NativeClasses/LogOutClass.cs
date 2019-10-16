using System.Threading.Tasks;
using Android.App;
using Android.Webkit;
using CardsAndroid.Activities;
using CardsAndroid.Adapters;
using CardsPCL.Database;

namespace CardsAndroid.NativeClasses
{
    public class LogOutClass
    {
        static DatabaseMethods _databaseMethods = new DatabaseMethods();
        static NativeMethods _nativeMethods = new NativeMethods();
        public static void log_out(Activity context)
        {
            //string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ormdemo.db3");
            //var db = new SQLiteConnection(dbPath);
            //var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //var cards_cache_dir = Path.Combine(docs, Constants.CardsPersonalImages);
            //var logo_cache_dir = Path.Combine(docs, Constants.CardsLogo);



            #region clearing tables, variables and photos
            var taskA = new Task(async () =>
            {
                await _nativeMethods.RemovePersonalImages();
                await _nativeMethods.RemoveLogo();
                await _nativeMethods.RemoveOfflineCache();
                //var QRs_cache_dir = Path.Combine(docs, Constants.QRs_cache_dir);
            });
            taskA.Start();
            NativeMethods.ResetSocialNetworkList();
            _databaseMethods.CleanPersonalNetworksTable();
            _databaseMethods.ClearCompanyCardTable();
            _databaseMethods.ClearUsersCardTable();
            _databaseMethods.ClearValidTillRepeatAfterTable();
            _databaseMethods.CleanCloudSynTable();
            _databaseMethods.CleanCardNames();
            _databaseMethods.CleanEtagTable();
            //clearing table
            try
            {
                _databaseMethods.CleanDifferentPurposesTable();
            }
            catch { }

            //clearing table
            try
            {
                _databaseMethods.CleanLoginAfterTable();
            }
            catch { }

            //clearing table
            try
            {
                _databaseMethods.CleanLoginedFromTable();
            }
            catch { }

            //CompanyDataActivity.currentImage = null;
            CompanyDataActivity.CroppedResult = null;
            //CompanyAddressMapViewController.lat = null;
            //CompanyAddressMapViewController.lng = null;
            CompanyAddressMapActivity.CompanyLat = null;
            CompanyAddressMapActivity.CompanyLng = null;
            CompanyAddressActivity.FullCompanyAddressStatic = null;
            CompanyAddressActivity.Country = null;
            CompanyAddressActivity.Region = null;
            CompanyAddressActivity.City = null;
            CompanyAddressActivity.Index = null;
            CompanyAddressActivity.Notation = null;
            CompanyDataActivity.CompanyName = null;
            CompanyDataActivity.LinesOfBusiness = null;
            CompanyDataActivity.Position = null;
            CompanyDataActivity.FoundationYear = null;
            CompanyDataActivity.Clients = null;
            CompanyDataActivity.CompanyPhone = null;
            CompanyDataActivity.CorporativePhone = null;
            CompanyDataActivity.Fax = null;
            CompanyDataActivity.CompanyEmail = null;
            CompanyDataActivity.CorporativeSite = null;
            PersonalDataActivity.MySurname = null;
            PersonalDataActivity.MyName = null;
            PersonalDataActivity.MyMiddlename = null;
            PersonalDataActivity.MyPhone = null;
            PersonalDataActivity.MyEmail = null;
            PersonalDataActivity.MyHomePhone = null;
            PersonalDataActivity.MySite = null;
            PersonalDataActivity.MyDegree = null;
            PersonalDataActivity.MyCardName = null;
            try { PersonalImageAdapter.Photos.Clear(); } catch { }
            PersonalDataActivity.MyBirthdate = null;
            NativeMethods.ResetHomeAddress();
            NativeMethods.ResetCompanyAddress();
            try { CreatingCardActivity.Datalist.Clear(); } catch { }
            try { QrActivity.CardNames.Clear(); } catch { }
            EditCompanyDataActivity.Position = null;
            EditCompanyDataActivity.LogoId = null;
            QrActivity.CardsRemaining = 0;
            QrActivity.IsPremium = false;
            QrActivity.ExtraPersonData = null;
            QrActivity.ExtraEmploymentData = null;
            QrActivity.CompanyLogoInQr = null;
            WaitingEmailConfirmActivity.CameFromPurge = false;
            //clearing webView cookies
            CookieSyncManager.CreateInstance(context);
            CookieManager cookieManager = CookieManager.Instance;
            cookieManager.RemoveAllCookies(null);
            #endregion clearing tables, variables and photos
        }
    }
}
