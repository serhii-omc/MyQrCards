using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using CardsAndroid.Adapters;
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
    public class EditActivity : Activity
    {
        TextView _personalTv, _companyTv, _headerTv, _makeMainTv, _mainTextTv;
        LinearLayout _loadingLl;
        ProgressBar _activityIndicator;
        Switch _switchSw;
        Button _switchBlockBn;
        CardsDataModel _desCardData;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        public static int CardId { get; set; }
        public static bool IsCompanyReadOnly { get; set; }
        public static List<int> IdsOfAttachments = new List<int>();
        public static List<Bitmap> ImagesFromServerList = new List<Bitmap>();
        NativeMethods _nativeMethods = new NativeMethods();
        //UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        //bool isFirstAppearance;
        Cards _cards = new Cards();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        Methods _methods = new Methods();
        string clientName;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.edit);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            try { EditPersonalImageAdapter.Photos.Clear(); } catch { }
            EditCompanyDataActivity.ChangedCompanyData = false;
            FindViewById<TextView>(Resource.Id.companyTV).Click += (s, e) => StartActivity(typeof(EditCompanyDataActivity));
            FindViewById<TextView>(Resource.Id.personalTV).Click += (s, e) =>
            {
                if (FindViewById<Switch>(Resource.Id.switchSw).Checked)
                    EditPersonalDataActivity.IsPrimary = true;
                else
                    EditPersonalDataActivity.IsPrimary = false;
                StartActivity(typeof(EditPersonalDataActivity));
            };
            FindViewById<Switch>(Resource.Id.switchSw).CheckedChange += (s, e) =>
            {
                if (_desCardData != null)
                    if (!_desCardData.isPrimary)
                        if (FindViewById<Switch>(Resource.Id.switchSw).Checked)
                        {
                            EditPersonalProcessActivity.FromPrimarySet = true; ;
                            EditPersonalDataActivity.IsPrimary = true;
                            QrActivity.ClickedPosition = 0;
                            QrActivity.CurrentPosition = 0;
                            cache_data();
                            StartActivity(typeof(EditPersonalProcessActivity));
                        }
            };
            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();

            EditPersonalImageAdapter.Photos?.Clear();
            InitElements();

            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }
            ClearAll();
            string resCardData = null;
            try
            {
                resCardData = await _cards.CardDataGet(_databaseMethods.GetAccessJwt(), CardId, clientName);
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
            if (String.IsNullOrEmpty(resCardData))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            if (/*res_card_data == Constants.status_code409 ||*/ resCardData == Constants.status_code401)
            {
                ShowSeveralDevicesRestriction();
                return;
            }
            _desCardData = JsonConvert.DeserializeObject<CardsDataModel>(resCardData);

            IsCompanyReadOnly = false;

            try { IsCompanyReadOnly = _desCardData.employment.company.isReadOnly; } catch { }
            // TODO
            //IsCompanyReadOnly = true;

            await FillVariables();

            _loadingLl.Visibility = ViewStates.Gone;
            //mainTextTV.Hidden = true;
            //activityIndicator.Hidden = true;
            //emailLogo.Hidden = true;
            //switchSw.Hidden = false;
            //makeMainBn.Hidden = false;
            //company_forw_bn.Hidden = false;
            //aboutCompanyBn.Hidden = false;
            //personal_forw_bn.Hidden = false;
            //personalDataBn.Hidden = false;
        }

        protected override async void OnResume()
        {
            base.OnResume();
            //EditPersonalImageAdapter.Photos?.Clear();
            //InitElements();

            //if (!_methods.IsConnected())
            //{
            //    NoConnectionActivity.ActivityName = this;
            //    StartActivity(typeof(NoConnectionActivity));
            //    Finish();
            //    return;
            //}
            //ClearAll();
            //string resCardData = null;
            //try
            //{
            //     resCardData = await _cards.CardDataGet(_databaseMethods.GetAccessJwt(), CardId);
            //}
            //catch (Exception ex)
            //{
            //    if (!_methods.IsConnected())
            //    {
            //        NoConnectionActivity.ActivityName = this;
            //        StartActivity(typeof(NoConnectionActivity));
            //        Finish();
            //        return;
            //    }
            //}
            //if (String.IsNullOrEmpty(resCardData))
            //{
            //    if (!_methods.IsConnected())
            //    {
            //        NoConnectionActivity.ActivityName = this;
            //        StartActivity(typeof(NoConnectionActivity));
            //        Finish();
            //        return;
            //    }
            //}
            //if (/*res_card_data == Constants.status_code409 ||*/ resCardData == Constants.StatusCode401)
            //{
            //    ShowSeveralDevicesRestriction();
            //    return;
            //}
            //_desCardData = JsonConvert.DeserializeObject<CardsDataModel>(resCardData);

            //await FillVariables();

            //_loadingLl.Visibility = ViewStates.Gone;
            ////mainTextTV.Hidden = true;
            ////activityIndicator.Hidden = true;
            ////emailLogo.Hidden = true;
            ////switchSw.Hidden = false;
            ////makeMainBn.Hidden = false;
            ////company_forw_bn.Hidden = false;
            ////aboutCompanyBn.Hidden = false;
            ////personal_forw_bn.Hidden = false;
            ////personalDataBn.Hidden = false;
        }


        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _personalTv = FindViewById<TextView>(Resource.Id.personalTV);
            _companyTv = FindViewById<TextView>(Resource.Id.companyTV);
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _makeMainTv = FindViewById<TextView>(Resource.Id.makeMainTV);
            _mainTextTv = FindViewById<TextView>(Resource.Id.mainTextTV);
            _switchSw = FindViewById<Switch>(Resource.Id.switchSw);
            _loadingLl = FindViewById<LinearLayout>(Resource.Id.loadingLL);
            _switchBlockBn = FindViewById<Button>(Resource.Id.switchBlockBn);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _personalTv.Text = TranslationHelper.GetString("personalData", _ci);
            _companyTv.Text = TranslationHelper.GetString("aboutCompany", _ci);
            _headerTv.Text = TranslationHelper.GetString("editing", _ci);
            _makeMainTv.Text = TranslationHelper.GetString("makeTheMain", _ci);
            _mainTextTv.Text = TranslationHelper.GetString("dataAcquisition", _ci);
            _loadingLl.Visibility = ViewStates.Visible;
            _switchBlockBn.Visibility = ViewStates.Gone;
            _personalTv.SetTypeface(tf, TypefaceStyle.Normal);
            _companyTv.SetTypeface(tf, TypefaceStyle.Normal);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            _makeMainTv.SetTypeface(tf, TypefaceStyle.Normal);
            _mainTextTv.SetTypeface(tf, TypefaceStyle.Normal);
            //switchSw.Hidden = true;
            //makeMainBn.Hidden = true;
            //company_forw_bn.Hidden = true;
            //aboutCompanyBn.Hidden = true;
            //personal_forw_bn.Hidden = true;
            //personalDataBn.Hidden = true;
        }

        public override void OnBackPressed()
        {
            ClearAll();
            base.OnBackPressed();
        }

        private async Task<bool> FillVariables()
        {
            if (_desCardData == null)
                return false;
            if (_desCardData.isPrimary)
            {
                //switchSw.SetHighlightColor(Color.AliceBlue);
                _switchSw.Checked = true;
                EditPersonalDataActivity.IsPrimary = true;
                _switchBlockBn.Visibility = ViewStates.Visible;
            }
            else
            {
                _switchSw.Checked = false;
                EditPersonalDataActivity.IsPrimary = false;
                //switchSw.Enabled = true;
            }
            try { EditPersonalProcessActivity.CompanyId = _desCardData.employment.company.id; } catch { }

            IdsOfAttachments.Clear();
            ImagesFromServerList.Clear();
            //NativeMethods.ResetSocialNetworkList();
            //SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
            //SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
            //SocialNetworkTableViewSource<int, int>._checkedRows.Clear();
            if (_desCardData.gallery != null)
            {
                _mainTextTv.Text = TranslationHelper.GetString("previousPhotosAcquisition", _ci);
                foreach (var item in _desCardData.gallery)
                {
                    IdsOfAttachments.Add(item.id);
                    try
                    {
                        await Task.Run(() => ImagesFromServerList.Add(NativeMethods.GetBitmapFromUrl(item.url)));
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
                }
            }

            //des_card_data.employment.company.logo.url
            //ids_of_attachments = des_card_data.gallery.;
            //var position = des_card_data.employment.position;
            //if (!String.IsNullOrEmpty(QRActivity.CompanyLogoInQr))
                try
                {
                    if (_desCardData?.employment?.company?.logo != null)
                    {
                        await Task.Run(() => EditCompanyDataActivity.CroppedResult = NativeMethods.GetBitmapFromUrl(_desCardData?.employment?.company?.logo?.url));
                        //UIImage image_logo;
                        //using (var url = new NSUrl(des_card_data.employment.company.logo.url))
                        //using (var data = NSData.FromUrl(url))
                        //CropCompanyLogoViewController.cropped_result = UIImage.LoadFromData(data);
                    }
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
            //else
            //{

            //}
            if (_desCardData?.person != null)
                if (_desCardData?.person?.socialNetworks != null)
                {
                    try
                    {
                        NativeMethods.ResetSocialNetworkList();
                    }
                    catch { }

                    foreach (var item in _desCardData?.person?.socialNetworks)
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
                    }
                    //i++;
                }

            try { EditPersonalDataActivity.MySurname = _desCardData.person.lastName; } catch { }
            try { EditPersonalDataActivity.MyName = _desCardData.person.firstName; } catch { }
            try { EditPersonalDataActivity.MyMiddlename = _desCardData.person.middleName; } catch { }
            try { EditPersonalDataActivity.MyPhone = _desCardData.person.mobilePhone; } catch { }
            try { EditPersonalDataActivity.MyEmail = _desCardData.person.email; } catch { }
            try { EditPersonalDataActivity.MyHomePhone = _desCardData.person.homePhone; } catch { }
            try { EditPersonalDataActivity.MySite = _desCardData.person.siteUrl; } catch { }
            try { EditPersonalDataActivity.MyDegree = _desCardData.person.education; } catch { }
            try { EditPersonalDataActivity.MyCardName = _desCardData.name; } catch { }
            try { EditPersonalDataActivity.MyCardNameOriginal = _desCardData.name; } catch { }
            try { EditPersonalDataActivity.MyBirthDate = _desCardData.person.birthdate.Substring(0, 10); } catch { }
            try { PersonalImageAdapter.Photos.Clear(); } catch { }
            try { HomeAddressActivity.FullAddressStatic = _desCardData.person.location.address; } catch { }
            try { HomeAddressActivity.MyCountry = _desCardData.person.location.country; } catch { }
            try { HomeAddressActivity.MyRegion = _desCardData.person.location.region; } catch { }
            try { HomeAddressActivity.MyCity = _desCardData.person.location.city; } catch { }
            try { HomeAddressActivity.MyIndex = _desCardData.person.location.postalCode; } catch { }
            try { HomeAddressActivity.MyNotation = _desCardData.person.location.notes; } catch { }
            try { NewCardAddressMapActivity.Lat = _desCardData.person.location.latitude.ToString().Replace(',', '.'); } catch { }
            try { NewCardAddressMapActivity.Lng = _desCardData.person.location.longitude.ToString().Replace(',', '.'); } catch { }

            try { EditCompanyDataActivity.CompanyName = _desCardData.employment.company.name; } catch { }
            try { EditCompanyDataActivity.LinesOfBusiness = _desCardData.employment.company.activity; } catch { }
            try { EditCompanyDataActivity.Position = _desCardData.employment.position; } catch { }
            try { EditCompanyDataActivity.FoundationYear = _desCardData.employment.company.foundedYear.ToString(); } catch { }
            try { EditCompanyDataActivity.Clients = _desCardData.employment.company.customers; } catch { }
            try { EditCompanyDataActivity.CompanyPhone = _desCardData.employment.company.phone; } catch { }
            try { EditCompanyDataActivity.CorporativePhone = _desCardData.employment.phone; } catch { }
            try { EditCompanyDataActivity.Fax = _desCardData.employment.company.fax; } catch { }
            try { EditCompanyDataActivity.CompanyEmail = _desCardData.employment.company.email; } catch { }
            try { EditCompanyDataActivity.CorporativeSite = _desCardData.employment.company.siteUrl; } catch { }
            try { CompanyAddressActivity.FullCompanyAddressStatic = _desCardData.employment.company.location.address; } catch { }
            try { CompanyAddressActivity.Country = _desCardData.employment.company.location.country; } catch { }
            try { CompanyAddressActivity.Region = _desCardData.employment.company.location.region; } catch { }
            try { CompanyAddressActivity.City = _desCardData.employment.company.location.city; } catch { }
            try { CompanyAddressActivity.Index = _desCardData.employment.company.location.postalCode; } catch { }
            try { CompanyAddressActivity.Notation = _desCardData.employment.company.location.notes; } catch { }
            try { CompanyAddressMapActivity.CompanyLat = _desCardData.employment.company.location.latitude.ToString().Replace(',', '.'); } catch { }
            try { CompanyAddressMapActivity.CompanyLng = _desCardData.employment.company.location.longitude.ToString().Replace(',', '.'); } catch { }
            try { EditCompanyDataActivity.LogoId = _desCardData.employment.company.logo.id; } catch { EditCompanyDataActivity.LogoId = null; }
            return true;
        }

        private void ClearAll()
        {
            #region clearing tables, variables and photos
            var taskA = new Task(async () =>
            {
                await _nativeMethods.RemovePersonalImages();
                await _nativeMethods.RemoveLogo();
            });
            taskA.Start();
            _databaseMethods.CleanPersonalNetworksTable();
            _databaseMethods.ClearCompanyCardTable();
            _databaseMethods.ClearUsersCardTable();
            IdsOfAttachments.Clear();
            ImagesFromServerList.Clear();
            try
            {
                NativeMethods.ResetSocialNetworkList();
            }
            catch { }
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
            EditPersonalDataActivity.MySurname = null;
            EditPersonalDataActivity.MyName = null;
            EditPersonalDataActivity.MyMiddlename = null;
            EditPersonalDataActivity.MyPhone = null;
            EditPersonalDataActivity.MyEmail = null;
            EditPersonalDataActivity.MyHomePhone = null;
            EditPersonalDataActivity.MySite = null;
            EditPersonalDataActivity.MyDegree = null;
            EditPersonalDataActivity.MyCardName = null;
            EditPersonalDataActivity.MyBirthDate = null;
            EditCompanyDataActivity.LogoId = null;
            EditCompanyDataActivity.CroppedResult = null;
            #endregion clearing tables, variables and photos
        }

        void cache_data()
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
