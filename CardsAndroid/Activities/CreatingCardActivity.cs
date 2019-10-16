
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CardsAndroid.Adapters;
using CardsAndroid.Models;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;
using CardsPCL.Models;
using Newtonsoft.Json;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class CreatingCardActivity : Activity
    {
        public static List<CardListModel> Datalist;
        TextView _headerTv, _copyFromExistingTv, _mainTextTv;
        ProgressBar _activityIndicator;
        Button _createNewBn;
        RecyclerView _recyclerView;
        CreatingCardAdapter _creatingCardAdapter;
        RecyclerView.LayoutManager _layoutManager;
        static LinearLayout _loadingLl;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        static Cards _cards = new Cards();
        static DatabaseMethods _databaseMethods = new DatabaseMethods();
        NativeMethods _nativeMethods = new NativeMethods();
        List<CardListModel> _deserializedCardsList;
        List<CardListModel> _reverseList;
        static Methods _methods = new Methods();
        static Activity _contextStatic;
        static string clientName;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            SetContentView(Resource.Layout.creating_card);
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            InitElements(tf);

            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();
            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }
            string resCardsList = "";
            _loadingLl.Visibility = ViewStates.Visible;
            try
            {
                resCardsList = await _cards.CardsListGet(_databaseMethods.GetAccessJwt(), clientName);
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            if (String.IsNullOrEmpty(resCardsList))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            if (/*res_card_data == Constants.status_code409 ||*/ resCardsList == Constants.status_code401)
            {
                ShowSeveralDevicesRestriction();
                return;
            }

            _deserializedCardsList = JsonConvert.DeserializeObject<List<CardListModel>>(resCardsList);

            _reverseList = _deserializedCardsList.AsEnumerable().Reverse().ToList();
            Datalist = _reverseList;
            _loadingLl.Visibility = ViewStates.Gone;
            var cardNames = new List<CreatingCardModel>();
            foreach (var item in _reverseList)
            {
                cardNames.Add(new CreatingCardModel { Id = item.id, CardName = item.name });
            }

            _createNewBn.Click += (s, e) =>
            {
                ClearData();
                StartActivity(typeof(PersonalDataActivity));
            };

            _creatingCardAdapter = new CreatingCardAdapter(this, cardNames, tf);
            _recyclerView.SetAdapter(_creatingCardAdapter);
        }

        protected override void OnResume()
        {
            base.OnResume();
            _contextStatic = this;
            ClearData();
        }

        private void InitElements(Typeface tf)
        {
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _copyFromExistingTv = FindViewById<TextView>(Resource.Id.copyFromExistingTV);
            _createNewBn = FindViewById<Button>(Resource.Id.create_newBn);
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _mainTextTv.Text = TranslationHelper.GetString("dataAcquisition", _ci);
            _headerTv.Text = TranslationHelper.GetString("creatingCard", _ci);
            _copyFromExistingTv.Text = TranslationHelper.GetString("orCopyFromExisting", _ci);
            _createNewBn.Text = TranslationHelper.GetString("createNewCard", _ci);
            _loadingLl = FindViewById<LinearLayout>(Resource.Id.loadingLL);
            _layoutManager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            _recyclerView.SetLayoutManager(_layoutManager);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _copyFromExistingTv.SetTypeface(tf, TypefaceStyle.Normal);
            _createNewBn.SetTypeface(tf, TypefaceStyle.Normal);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
        }

        public static async Task<bool> show_loader(int cardId)
        {
            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = _contextStatic;
                _contextStatic.StartActivity(typeof(NoConnectionActivity));
                _contextStatic.Finish();
                return false;
            }
            _loadingLl.Visibility = ViewStates.Visible;

            string resCardData = null;
            try
            {
                resCardData = await _cards.CardDataGet(_databaseMethods.GetAccessJwt(), cardId, clientName);
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = _contextStatic;
                    _contextStatic.StartActivity(typeof(NoConnectionActivity));
                    _contextStatic.Finish();
                    return false;
                }
            }
            if (String.IsNullOrEmpty(resCardData))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = _contextStatic;
                    _contextStatic.StartActivity(typeof(NoConnectionActivity));
                    _contextStatic.Finish();
                    return false;
                }
            }
            if (/*res_card_data == Constants.status_code409 ||*/ resCardData == Constants.status_code401)
            {
                ShowSeveralDevicesRestriction();
                return false;
            }
            var desCardData = JsonConvert.DeserializeObject<CardsDataModel>(resCardData);
            try
            {
                if (!String.IsNullOrEmpty(desCardData?.employment?.company?.logo?.url))
                    CompanyDataActivity.CroppedResult = NativeMethods.GetBitmapFromUrl(desCardData?.employment?.company?.logo?.url);
            }
            catch (Exception ex)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = _contextStatic;
                    _contextStatic.StartActivity(typeof(NoConnectionActivity));
                    _contextStatic.Finish();
                    return false;
                }
            }
            try
            {
                PersonalImageAdapter.Photos.Clear();
            }
            catch
            {
            }
            if (desCardData?.gallery != null)
            {
                try
                {
                    foreach (var item in desCardData?.gallery)
                    {
                        PersonalImageAdapter.Photos.Add(NativeMethods.GetBitmapFromUrl(item.url));
                    }
                }
                catch (Exception ex)
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = _contextStatic;
                        _contextStatic.StartActivity(typeof(NoConnectionActivity));
                        _contextStatic.Finish();
                        return false;
                    }
                }
            }
            PersonalDataActivity.MySurname = desCardData?.person?.lastName;
            PersonalDataActivity.MyName = desCardData?.person?.firstName;
            PersonalDataActivity.MyMiddlename = desCardData?.person?.middleName;
            PersonalDataActivity.MyPhone = desCardData?.person?.mobilePhone;
            PersonalDataActivity.MyEmail = desCardData?.person?.email;
            PersonalDataActivity.MyHomePhone = desCardData?.person?.homePhone;
            PersonalDataActivity.MySite = desCardData?.person?.siteUrl;
            PersonalDataActivity.MyDegree = desCardData?.person?.education;
            PersonalDataActivity.MyCardName = desCardData?.name;
            try { PersonalDataActivity.MyBirthdate = desCardData.person.birthdate.Substring(0, 10); } catch { }

            HomeAddressActivity.FullAddressStatic = desCardData?.person?.location?.address;
            HomeAddressActivity.MyCountry = desCardData?.person?.location?.country;
            HomeAddressActivity.MyRegion = desCardData?.person?.location?.region;
            HomeAddressActivity.MyCity = desCardData?.person?.location?.city;
            HomeAddressActivity.MyIndex = desCardData?.person?.location?.postalCode;
            HomeAddressActivity.MyNotation = desCardData?.person?.location?.notes;
            NewCardAddressMapActivity.Lat = desCardData?.person?.location?.latitude?.ToString()?.Replace(',', '.');
            NewCardAddressMapActivity.Lng = desCardData?.person?.location?.longitude?.ToString()?.Replace(',', '.');
            try { CompanyDataActivity.Position = desCardData?.employment?.position; } catch { }
            try { CompanyDataActivity.CompanyName = desCardData.employment.company.name; } catch { }
            try { CompanyDataActivity.LinesOfBusiness = desCardData.employment.company.activity; } catch { }
            try { CompanyDataActivity.FoundationYear = desCardData.employment.company.foundedYear.ToString(); } catch { }
            try { CompanyDataActivity.Clients = desCardData.employment.company.customers; } catch { }
            try { CompanyDataActivity.CompanyPhone = desCardData.employment.company.phone; } catch { }
            try { CompanyDataActivity.CorporativePhone = desCardData.employment.phone; } catch { }
            try { CompanyDataActivity.Fax = desCardData.employment.company.fax; } catch { }
            try { CompanyDataActivity.CompanyEmail = desCardData.employment.company.email; } catch { }
            try { CompanyDataActivity.CorporativeSite = desCardData.employment.company.siteUrl; } catch { }
            try { CompanyAddressActivity.FullCompanyAddressStatic = desCardData.employment.company.location.address; } catch { }
            try { CompanyAddressActivity.Country = desCardData.employment.company.location.country; } catch { }
            try { CompanyAddressActivity.Region = desCardData.employment.company.location.region; } catch { }
            try { CompanyAddressActivity.City = desCardData.employment.company.location.city; } catch { }
            try { CompanyAddressActivity.Index = desCardData.employment.company.location.postalCode; } catch { }
            try { CompanyAddressActivity.Notation = desCardData.employment.company.location.notes; } catch { }
            try { CompanyAddressMapActivity.CompanyLat = desCardData.employment.company.location.latitude.ToString().Replace(',', '.'); } catch { }
            try { CompanyAddressMapActivity.CompanyLng = desCardData.employment.company.location.longitude.ToString().Replace(',', '.'); } catch { }
            if (desCardData?.person?.socialNetworks != null)
            {
                try
                {
                    NativeMethods.ResetSocialNetworkList();
                }
                catch { }
                //foreach (var item in des_card_data.person.socialNetworks)
                //{
                //    SocialNetworkTableViewSource<int, int>.selectedIndexes.Add(item.id);
                //}
                try
                {
                    int i = 0;
                    foreach (var item in desCardData?.person?.socialNetworks)
                    {
                        if (item.id == 1)
                            SocialNetworkAdapter.SocialNetworks[0].UsersUrl = item.contactUrl;
                        if (item.id == 4)
                            SocialNetworkAdapter.SocialNetworks[1].UsersUrl = item.contactUrl;
                        if (item.id == 3)
                            SocialNetworkAdapter.SocialNetworks[2].UsersUrl = item.contactUrl;
                        if (item.id == 5)
                            SocialNetworkAdapter.SocialNetworks[3].UsersUrl = item.contactUrl;
                        if (item.id == 2)
                            SocialNetworkAdapter.SocialNetworks[4].UsersUrl = item.contactUrl;
                        if (!String.IsNullOrEmpty(item.contactUrl))
                            _databaseMethods.InsertPersonalNetwork(new CardsPCL.Models.SocialNetworkModel { SocialNetworkID = item.id, ContactUrl = item.contactUrl });
                    }
                }
                catch { }
            }
            _loadingLl.Visibility = ViewStates.Gone;
            _contextStatic.StartActivity(typeof(PersonalDataActivity));
            return true;
        }

        void ClearData()
        {
            Task.Run(() =>
            {
                _nativeMethods.RemovePersonalImages();
                _nativeMethods.RemoveLogo();
                //await nativeMethods.RemoveOfflineCache();
            });
            _databaseMethods.CleanPersonalNetworksTable();
            _databaseMethods.ClearCompanyCardTable();
            _databaseMethods.ClearUsersCardTable();
            try { PersonalImageAdapter.Photos.Clear(); } catch { }
            //CompanyDataActivity.currentImage = null;
            CompanyDataActivity.CroppedResult = null;
            //CompanyAddressMapActivity.lat = null;
            //CompanyAddressMapActivity.lng = null;
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
            //try { PersonalDataViewController.images_list.Clear(); } catch { }
            PersonalDataActivity.MyBirthdate = null;
            HomeAddressActivity.FullAddressStatic = null;
            HomeAddressActivity.MyCountry = null;
            HomeAddressActivity.MyRegion = null;
            HomeAddressActivity.MyCity = null;
            HomeAddressActivity.MyIndex = null;
            HomeAddressActivity.MyNotation = null;
            NewCardAddressMapActivity.Lat = null;
            NewCardAddressMapActivity.Lng = null;
            EditCompanyDataActivity.Position = null;
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
            try
            {
                NativeMethods.ResetSocialNetworkList();
            }
            catch { }
        }

        public override void OnBackPressed()
        {
            ClearData();
            base.OnBackPressed();
        }

        static void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out(_contextStatic);
            MyCardActivity.DeviceRestricted = true;
            Intent intent = new Intent(_contextStatic, typeof(MyCardActivity));
            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
            _contextStatic.StartActivity(intent);
        }
    }
}
