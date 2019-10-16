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
using CardsPCL.Models;
using Newtonsoft.Json;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class EditCompanyProcessActivity : Activity
    {
        ProgressBar _activityIndicator;
        TextView _mainTextTv;
        Methods _methods = new Methods();
        NativeMethods _nativeMethods = new NativeMethods();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        Attachments _attachments = new Attachments();
        Companies _companies = new Companies();
        Cards _cards = new Cards();
        string clientName;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.LoadingLayout);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            InitElements();
            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }

            if (!EditCompanyDataActivity.ChangedCompanyData)
                goto DoPersonalStuff;

            #region uploading photos
            bool photosExist = true;
            var personalImages = await _nativeMethods.GetPersonalImages();
            if (personalImages == null)
                photosExist = false;
            else
                photosExist = true;
            var documentsLogo = await _nativeMethods.GetDocumentsLogo();
            if (documentsLogo != null)
                photosExist = true;
            int? logoId = null;
            List<int> attachmentsIdsList = new List<int>();
            if (photosExist)
            {
                _mainTextTv.Text = TranslationHelper.GetString("photosAreBeingUploaded", _ci);
                AttachmentsUploadModel resPhotos = null;
                try
                {
                    resPhotos = await _attachments.UploadAndroid(_databaseMethods.GetAccessJwt(), clientName, personalImages, documentsLogo);
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
                if (resPhotos == null)
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        return;
                    }
                }
                if (resPhotos != null)
                {
                    //var deserialized_logo = JsonConvert.DeserializeObject<CompanyLogoModel>(res_photos);
                    logoId = resPhotos.logo_id;
                    attachmentsIdsList = resPhotos.attachments_ids;
                    if (logoId == Constants.image_upload_status_code401)
                    {
                        ShowSeveralDevicesRestriction();
                        return;
                    }
                }
            }
            _mainTextTv.Text = TranslationHelper.GetString("cardIsSynchronizing", _ci);
            #endregion uploading photos
            string resCompany = null;
            try
            {
                if (logoId != null)
                {
                    if (EditPersonalProcessActivity.CompanyId != null)
                        resCompany = await _companies.UpdateCompanyCard(_databaseMethods.GetAccessJwt(), clientName, _databaseMethods.GetDataFromCompanyCard(), EditPersonalProcessActivity.CompanyId, logoId);
                    else
                    {
                        resCompany = await _companies.CreateCompanyCard(_databaseMethods.GetAccessJwt(), clientName, _databaseMethods.GetDataFromCompanyCard(), logoId);
                    }
                }
                else
                {
                    if (EditPersonalProcessActivity.CompanyId != null)
                        resCompany = await _companies.UpdateCompanyCard(_databaseMethods.GetAccessJwt(), clientName, _databaseMethods.GetDataFromCompanyCard(), EditPersonalProcessActivity.CompanyId);
                    else
                    {
                        resCompany = await _companies.CreateCompanyCard(_databaseMethods.GetAccessJwt(), clientName, _databaseMethods.GetDataFromCompanyCard());
                    }
                }
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
            if (String.IsNullOrEmpty(resCompany))
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            if (resCompany == Constants.image_upload_status_code401.ToString())
            {
                ShowSeveralDevicesRestriction();
                return;
            }
            try
            {
                var deserialized = JsonConvert.DeserializeObject<CompanyCardResponse>(resCompany);
            }
            catch
            {
                _databaseMethods.ClearCompanyCardTable();
                var deserializedError = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(resCompany);
                Toast.MakeText(this, TranslationHelper.GetString("errorInCompanyData", _ci), ToastLength.Long).Show();
                base.OnBackPressed();
                return;
            }

            if (resCompany.Contains("id") && resCompany.Length < 12)
            {
                EditPersonalProcessActivity.CompanyId = Convert.ToInt32(JsonConvert.DeserializeObject<CompanyCardResponse>(resCompany).id);
            }
        DoPersonalStuff:
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
                                                _databaseMethods.GetDataFromUsersCard(EditPersonalProcessActivity.CompanyId,
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
                    return;
                }
            }
            if (res == null)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            if (res.StatusCode.ToString().Contains("401") || res.StatusCode.ToString().ToLower().Contains(Constants.status_code401))
            {
                ShowSeveralDevicesRestriction();
                return;
            }

            await Clear();

            try
            {
                StartActivity(typeof(QrActivity));
                //var vc_list = this.NavigationController.ViewControllers.ToList();
                //try { vc_list.RemoveAt(vc_list.Count - 2); } catch { }
                //this.NavigationController.ViewControllers = vc_list.ToArray();
            }
            catch { }

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

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out(this);
            MyCardActivity.DeviceRestricted = true;
            Intent intent = new Intent(this, typeof(MyCardActivity));
            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
            StartActivity(intent);
        }

        private async Task<bool> Clear()
        {
            await Task.Run(async () =>
            {
                await _nativeMethods.RemovePersonalImages();
                await _nativeMethods.RemoveLogo();
            });
            _databaseMethods.CleanPersonalNetworksTable();
            _databaseMethods.ClearCompanyCardTable();
            _databaseMethods.ClearUsersCardTable();
            NativeMethods.ResetSocialNetworkList();
            EditCompanyDataActivity.CroppedResult = null;
            CompanyDataActivity.CroppedResult = null;
            NativeMethods.ResetCompanyAddress();
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
            try { EditPersonalImageAdapter.Photos.Clear(); } catch { }
            try { PersonalImageAdapter.Photos.Clear(); } catch { }
            EditPersonalDataActivity.MyBirthDate = null;
            NativeMethods.ResetCompanyAddress();
            EditCompanyDataActivity.Position = null;
            EditCompanyDataActivity.LogoId = null;
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
    }
}
