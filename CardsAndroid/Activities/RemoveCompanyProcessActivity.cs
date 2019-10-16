using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using CardsAndroid.Adapters;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class RemoveCompanyProcessActivity : Activity
    {
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        Cards _cards = new Cards();
        Companies _companies = new Companies();
        Attachments _attachments = new Attachments();
        Methods _methods = new Methods();
        ProgressBar _activityIndicator;
        TextView _mainTextTv;
        string clientName;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            SetContentView(Resource.Layout.LoadingLayout);

            InitElements();

            if (_methods.IsConnected())
            {
                await Sync();
            }
            else
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }
        }

        async Task<bool> Sync()
        {
            //caching card to db
            _databaseMethods.InsertUsersCard(
            EditPersonalDataActivity.MyName,
            EditPersonalDataActivity.MySurname,
            EditPersonalDataActivity.MyMiddlename,
            EditPersonalDataActivity.MyPhone,
            EditPersonalDataActivity.MyEmail,
            EditPersonalDataActivity.MyHomePhone,
            EditPersonalDataActivity.MySite,
            EditPersonalDataActivity.MyDegree,
            EditPersonalDataActivity.MyCardName,
            EditPersonalDataActivity.MyBirthDate,
            HomeAddressActivity.MyCountry,
            HomeAddressActivity.MyRegion,
            HomeAddressActivity.MyCity,
            HomeAddressActivity.FullAddressStatic,
            HomeAddressActivity.MyIndex,
            HomeAddressActivity.MyNotation,
            NewCardAddressMapActivity.Lat,
            NewCardAddressMapActivity.Lng,
            true
            );
            HttpResponseMessage res = null;
            try
            {
                res = await _cards.CardUpdate(_databaseMethods.GetAccessJwt(),
                                                 EditActivity.CardId,
                                                 _databaseMethods.GetDataFromUsersCard(null,
                                                                                      _databaseMethods.GetLastSubscription(),
                                                                                      EditCompanyDataActivity.Position, EditCompanyDataActivity.CorporativePhone),
                                                 EditPersonalDataActivity.IsPrimary,
                                                 GetPersonalNetworks(),
                                                 EditActivity.IdsOfAttachments,
                                                 clientName);
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return false;
                }
            }
            if (res == null)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return false;
                }
            }
            if (res.StatusCode.ToString().Contains("401") || res.StatusCode.ToString().ToLower().Contains(Constants.status_code401))
            {
                ShowSeveralDevicesRestriction();
                return false;
            }

            ClearAll();

            try
            {
                StartActivity(typeof(QrActivity));
                //var vc_list = this.NavigationController.ViewControllers.ToList();
                //try { vc_list.RemoveAt(vc_list.Count - 2); } catch { }
                //this.NavigationController.ViewControllers = vc_list.ToArray();
            }
            catch { }
            return true;
        }

        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _mainTextTv.Text = TranslationHelper.GetString("cardIsSynchronizing", _ci);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
        }

        List<CardsPCL.Models.SocialNetworkModel> GetPersonalNetworks()
        {
            List<CardsPCL.Models.SocialNetworkModel> socialNetworks = new List<CardsPCL.Models.SocialNetworkModel>();
            foreach (var item/*index*/ in SocialNetworkAdapter.SocialNetworks)//.selectedIndexes)
            {
                int socialnetworkId = 0;
                if (item.SocialNetworkName == Constants.facebook)
                    socialnetworkId = 1;
                else if (item.SocialNetworkName == Constants.instagram)
                    socialnetworkId = 4;
                else if (item.SocialNetworkName == Constants.linkedin)
                    socialnetworkId = 3;
                else if (item.SocialNetworkName == Constants.twitter)
                    socialnetworkId = 5;
                else if (item.SocialNetworkName == Constants.vkontakte)
                    socialnetworkId = 2;
                if (!String.IsNullOrEmpty(item.UsersUrl))
                    //databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = socialnetworkId, ContactUrl = item.usersUrl });
                    socialNetworks.Add(new CardsPCL.Models.SocialNetworkModel { SocialNetworkID = socialnetworkId, ContactUrl = item.UsersUrl });
                //databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = datalist[index].Id, ContactUrl = datalist[index].ContactUrl });

            }
            return socialNetworks;
        }

        private void ClearAll()
        {
            _databaseMethods.CleanPersonalNetworksTable();
            _databaseMethods.ClearCompanyCardTable();
            _databaseMethods.ClearUsersCardTable();
            NativeMethods.ResetSocialNetworkList();
            CompanyDataActivity.CroppedResult = null;
            EditCompanyDataActivity.CroppedResult = null;
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
            EditPersonalDataActivity.MySurname = null;
            EditPersonalDataActivity.MyName = null;
            EditPersonalDataActivity.MyMiddlename = null;
            EditPersonalDataActivity.MyPhone = null;
            EditPersonalDataActivity.MyEmail = null;
            EditPersonalDataActivity.MyHomePhone = null;
            EditPersonalDataActivity.MySite = null;
            EditPersonalDataActivity.MyDegree = null;
            EditPersonalDataActivity.MyCardName = null;
            try { PersonalImageAdapter.Photos.Clear(); } catch { }
            try { EditPersonalImageAdapter.Photos.Clear(); } catch { }
            EditPersonalDataActivity.MyBirthDate = null;
            EditCompanyDataActivity.CompanyName = null;
            EditCompanyDataActivity.LinesOfBusiness = null;
            EditCompanyDataActivity.Position = null;
            EditCompanyDataActivity.FoundationYear = null;
            EditCompanyDataActivity.Clients = null;
            EditCompanyDataActivity.CompanyPhone = null;
            EditCompanyDataActivity.CorporativePhone = null;
            EditCompanyDataActivity.Fax = null;
            EditCompanyDataActivity.CompanyEmail = null;
            EditCompanyDataActivity.CorporativeSite = null;
            HomeAddressActivity.FullAddressStatic = null;
            HomeAddressActivity.MyCountry = null;
            HomeAddressActivity.MyRegion = null;
            HomeAddressActivity.MyCity = null;
            HomeAddressActivity.MyIndex = null;
            HomeAddressActivity.MyNotation = null;
            NewCardAddressMapActivity.Lat = null;
            NewCardAddressMapActivity.Lng = null;
            EditCompanyDataActivity.LogoId = null;

            HomeAddressActivity.FullAddressTemp = null;
            HomeAddressActivity.MyCountryTemp = null;
            HomeAddressActivity.MyRegionTemp = null;
            HomeAddressActivity.MyCityTemp = null;
            HomeAddressActivity.MyIndexTemp = null;
            HomeAddressActivity.MyNotationTemp = null;
            CompanyAddressActivity.FullCompanyAddressTemp = null;
            CompanyAddressActivity.CountryTemp = null;
            CompanyAddressActivity.RegionTemp = null;
            CompanyAddressActivity.CityTemp = null;
            CompanyAddressActivity.IndexTemp = null;
            CompanyAddressActivity.NotationTemp = null;
        }

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out(this);
            MyCardActivity.DeviceRestricted = true;
            Intent intent = new Intent(this, typeof(MyCardActivity));
            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
            StartActivity(intent);
        }
    }
}
