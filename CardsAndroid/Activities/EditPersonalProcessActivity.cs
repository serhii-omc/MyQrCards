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

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class EditPersonalProcessActivity : Activity
    {
        public static bool FromPrimarySet;
        public static int? CompanyId;
        Attachments _attachments = new Attachments();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        NativeMethods _nativeMethods = new NativeMethods();
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        Methods _methods = new Methods();
        Cards _cards = new Cards();
        ProgressBar _activityIndicator;
        TextView _mainTextTv;
        string clientName;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            SetContentView(Resource.Layout.LoadingLayout);
            InitElements();
            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }

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
                    resPhotos = await _attachments.UploadAndroid(_databaseMethods.GetAccessJwt(), clientName,personalImages, documentsLogo);
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

            //var temp_ids = new List<int>();//EditActivity.ids_of_attachments;//.AddRange(attachments_ids_list);
            //temp_ids.AddRange(attachments_ids_list);
            HttpResponseMessage resUser = null;
            try
            {
                if (!EditPersonalDataActivity.IsPrimary&& !FromPrimarySet)
                    resUser = await _cards.CardUpdate(_databaseMethods.GetAccessJwt(),
                                                         EditActivity.CardId,
                                                         _databaseMethods.GetDataFromUsersCard(CompanyId, _databaseMethods.GetLastSubscription(), EditCompanyDataActivity.Position, EditCompanyDataActivity.CorporativePhone),
                                                         EditPersonalDataActivity.IsPrimary,
                                                         GetPersonalNetworks(),
                                                         //temp_ids);
                                                         attachmentsIdsList,
                                                         clientName);
                else if(EditPersonalDataActivity.IsPrimary && FromPrimarySet)
                {
                    resUser = await _cards.CardUpdate(_databaseMethods.GetAccessJwt(),
                                                    EditActivity.CardId,
                                                    _databaseMethods.GetDataFromUsersCard(CompanyId, _databaseMethods.GetLastSubscription(), EditCompanyDataActivity.Position, EditCompanyDataActivity.CorporativePhone),
                                                    EditPersonalDataActivity.IsPrimary,
                                                    GetPersonalNetworks(),
                                                    //temp_ids);
                                                    //attachments_ids_list);
                                                    EditActivity.IdsOfAttachments,
                                                    clientName);
                    FromPrimarySet = false;
                }
                else
                    resUser = await _cards.CardUpdate(_databaseMethods.GetAccessJwt(),
                                                         EditActivity.CardId,
                                                         _databaseMethods.GetDataFromUsersCard(CompanyId, _databaseMethods.GetLastSubscription(), EditCompanyDataActivity.Position, EditCompanyDataActivity.CorporativePhone),
                                                         EditPersonalDataActivity.IsPrimary,
                                                         GetPersonalNetworks(),
                                                         //temp_ids);
                                                         attachmentsIdsList,
                                                         clientName);
                                                         //EditActivity.ids_of_attachments);
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
            if (resUser == null)
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
            }
            if (resUser.StatusCode.ToString().Contains("401") || resUser.StatusCode.ToString().ToLower().Contains(Constants.status_code401))
            {
                ShowSeveralDevicesRestriction();
                return;
            }

            await Clear();

            StartActivity(typeof(QrActivity));
        }

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out(this);
            MyCardActivity.DeviceRestricted = true;
            Intent intent = new Intent(this, typeof(MyCardActivity));
            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
            StartActivity(intent);
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
