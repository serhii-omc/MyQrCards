using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
    [Activity(NoHistory = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CardsCreatingProcessActivity : Activity
    {
        ProgressBar _activityIndicator;
        TextView _mainTextTv;
        Companies _companies = new Companies();
        Cards _cards = new Cards();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        Attachments _attachments = new Attachments();
        Methods _methods = new Methods();
        NativeMethods _nativeMethods = new NativeMethods();
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        public static int PersonalCardId;
        public static string CameFrom { get; set; }
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

            if (QrActivity.CardsRemaining > 0)
            {
                await CreateCardProcess();
            }
            else
            {
                StartActivity(typeof(QrActivity));
                QrActivity.JustCreatedCardName = _databaseMethods.get_card_name();
                ClearAll();
            }
        }
        void ClearAll()
        {
            var taskA = new Task(async () =>
            {
                await _nativeMethods.RemovePersonalImages();
                await _nativeMethods.RemoveLogo();
                //await nativeMethods.RemoveOfflineCache();
                //var QRs_cache_dir = Path.Combine(docs, Constants.QRs_cache_dir);
            });
            taskA.Start();
            _databaseMethods.CleanPersonalNetworksTable();
            _databaseMethods.ClearCompanyCardTable();
            _databaseMethods.ClearUsersCardTable();
            //CropCompanyLogoViewController.currentImage = null;
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
            EditCompanyDataActivity.Position = null;
            EditCompanyDataActivity.LogoId = null;
        }

        private async Task<bool> CreateCardProcess()
        {
            #region uploading photos
            bool photosExist = true;
            var personalImages = _nativeMethods.GetPersonalImages();
            if (personalImages.Result == null)
                photosExist = false;
            else
                photosExist = true;
            var documentsLogo = _nativeMethods.GetDocumentsLogo();
            if (documentsLogo.Result != null)
                photosExist = true;
            int? logoId = null;
            List<int> attachmentsIdsList = new List<int>();
            if (photosExist)
            {
                _mainTextTv.Text = TranslationHelper.GetString("photosAreBeingUploaded", _ci);
                AttachmentsUploadModel resPhotos = null;
                try
                {
                    resPhotos = await _attachments.UploadAndroid(_databaseMethods.GetAccessJwt(), clientName, personalImages.Result, documentsLogo.Result);
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
                if (resPhotos == null)
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        return false;
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
                        return false;
                    }
                }
            }
            #endregion uploading photos
            _mainTextTv.Text = TranslationHelper.GetString("cardIsSynchronizing", _ci);
            string companyCardRes = "";
            if (!CompanyDataActivity.CompanyNull)
            {
                try
                {
                    if (logoId != null)
                        companyCardRes = await _companies.CreateCompanyCard(_databaseMethods.GetAccessJwt(), clientName,_databaseMethods.GetDataFromCompanyCard(), logoId);
                    else
                        companyCardRes = await _companies.CreateCompanyCard(_databaseMethods.GetAccessJwt(), clientName,_databaseMethods.GetDataFromCompanyCard());
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
                if (String.IsNullOrEmpty(companyCardRes))
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        return false;
                    }
                }
                if (companyCardRes == Constants.image_upload_status_code401.ToString())
                {
                    ShowSeveralDevicesRestriction();
                    return false;
                }
            }
            try
            {
                string userCardRes = null;
                try
                {
                    if (!CompanyDataActivity.CompanyNull)
                    {
                        var deserialized = JsonConvert.DeserializeObject<CompanyCardResponse>(companyCardRes);
                        userCardRes = await _cards.CreatePersonalCard(_databaseMethods.GetAccessJwt(),
                                                                       _databaseMethods.GetDataFromUsersCard(deserialized.id,
                                                                                                            _databaseMethods.GetLastSubscription(),
                                                                                                            EditCompanyDataActivity.Position,
                                                                                                            EditCompanyDataActivity.CorporativePhone),
                                                                       attachmentsIdsList,
                                                                       clientName);


                    }
                    else
                        userCardRes = await _cards.CreatePersonalCard(_databaseMethods.GetAccessJwt(),
                                                                       _databaseMethods.GetDataFromUsersCard(null,
                                                                                                            _databaseMethods.GetLastSubscription(),
                                                                                                            EditCompanyDataActivity.Position,
                                                                                                            EditCompanyDataActivity.CorporativePhone),
                                                                       attachmentsIdsList,
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
                if (String.IsNullOrEmpty(userCardRes))
                {
                    if (!_methods.IsConnected())
                    {
                        NoConnectionActivity.ActivityName = this;
                        StartActivity(typeof(NoConnectionActivity));
                        Finish();
                        return false;
                    }
                }
                if (userCardRes == Constants.image_upload_status_code401.ToString())
                {
                    ShowSeveralDevicesRestriction();
                    return false;
                }
                try
                {
                    var usersCardDes = JsonConvert.DeserializeObject<CompanyCardResponse>(userCardRes);
                    PersonalCardId = usersCardDes.id;
                    QrActivity.JustCreatedCardName = _databaseMethods.get_card_name();
                    _nativeMethods.ClearAll();

                    CardDoneActivity.CardId = PersonalCardId;
                    //CardDoneViewController.variant_displaying = 1;

                    _databaseMethods.InsertLastCloudSync(DateTime.Now);
                    ClearAll();
                    StartActivity(typeof(CardDoneActivity));
                }
                catch
                {
                    try
                    {
                        var deserializedError = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(userCardRes);
                        //if (deserialized_error[0].message == Constants.alreadyDone)
                        if (deserializedError[0].code == Constants.alreadyDone)
                            Toast.MakeText(this, TranslationHelper.GetString("cardWithThisNameExists", _ci), ToastLength.Long).Show();
                    }
                    catch { }
                    Pop();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var deserializedError = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(companyCardRes);
                    if (deserializedError != null)
                    {
                        Toast.MakeText(this, deserializedError[0].message, ToastLength.Long).Show();
                        Pop();
                    }
                    else
                    {
                        Toast.MakeText(this, TranslationHelper.GetString("tryOnceMore", _ci), ToastLength.Long).Show();
                        OnBackPressed();
                    }
                }
                catch { }
                //StartActivity(typeof(CardDoneActivity));
            }
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

        void Pop()
        {
            //if (came_from == Constants.creating)
            //{
            //    //this.NavigationController.PopViewController(false);
            //    // Nothing to do in this case.
            //}
            //else 
            //if (came_from == Constants.email_confirmation_waiting)
            //{
            //var vc_list = this.NavigationController.ViewControllers.ToList();
            //this.NavigationController.PopToViewController(vc_list[vc_list.Count - 4], false);
            OnBackPressed();
            //}
            //else //if (CropCompanyLogoViewController.came_from == Constants.attention_purge || CropCompanyLogoViewController.came_from == Constants.main_sync)
            //{
            //    // Nothing to do in this case.
            //}
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
