using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;
using CardsPCL.Models;
using Newtonsoft.Json;
using ZXing.Mobile;
using static ZXing.Mobile.MobileBarcodeScanningOptions;

namespace CardsAndroid.Activities
{
    [Activity(Label = "LinkStickerActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class LinkStickerActivity : Activity
    {
        TextView _headerTv, _mainTextTV, _infoTV;
        Button _orderBn, _linkStickerBn;
        Typeface _tf;
        ProgressBar _activityIndicator;

        MobileBarcodeScanner _scanner;
        MobileBarcodeScanningOptions _optionsCustom;
        Button _flashButton, _cancelButton;
        View _zxingOverlay;

        DatabaseMethods _databaseMethods = new DatabaseMethods();
        Methods _methods = new Methods();
        CardLinks _cardLinks = new CardLinks();
        string clientName;
        public static int CardId;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.LinkSticker);

            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;

            InitElements();

            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();

            _linkStickerBn.Click += async (s, e) =>
            {
                if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                    RequestCameraPermissions();

                MobileBarcodeScanner.Initialize(Application);
                _optionsCustom = new MobileBarcodeScanningOptions();
                _scanner = new MobileBarcodeScanner();

                _scanner = new MobileBarcodeScanner();
                _scanner.UseCustomOverlay = true;

                //Inflate our custom overlay from a resource layout
                _zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlay, null);

                //Find the button from our resource layout and wire up the click event
                _flashButton = _zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingFlash);
                _cancelButton = _zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingCancel);
                _flashButton.Text = TranslationHelper.GetString("flash", GetCurrentCulture.GetCurrentCultureInfo());
                _cancelButton.Text = TranslationHelper.GetString("cancel", GetCurrentCulture.GetCurrentCultureInfo());
                _flashButton.SetTypeface(_tf, TypefaceStyle.Normal);
                _cancelButton.SetTypeface(_tf, TypefaceStyle.Normal);
                _flashButton.Click += (sender, e1) => _scanner.ToggleTorch();
                _cancelButton.Click += (sender, e1) => _scanner.Cancel();

                //Set our custom overlay
                _scanner.CustomOverlay = _zxingOverlay;
                try
                {
                    _optionsCustom.CameraResolutionSelector = new CameraResolutionSelectorDelegate(SelectLowestResolutionMatchingDisplayAspectRatio);//new CameraResolution { Height = 1920, Width = 1080 };
                }
                catch (Exception ex)
                {

                }
                _optionsCustom.AutoRotate = false;
                if (Build.VERSION.SdkInt < Build.VERSION_CODES.M)
                    await LaunchScanner();
            };

            _orderBn.Click += OpenOrderLink;
        }

        private void InitElements()
        {
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _mainTextTV = FindViewById<TextView>(Resource.Id.mainTextTV);
            _infoTV = FindViewById<TextView>(Resource.Id.infoTV);
            _orderBn = FindViewById<Button>(Resource.Id.orderBn);
            _linkStickerBn = FindViewById<Button>(Resource.Id.linkStickerBn);
            _activityIndicator = FindViewById<ProgressBar>(Resource.Id.activityIndicator);
            _activityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _activityIndicator.Visibility = ViewStates.Gone;

            _tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _headerTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _mainTextTV.SetTypeface(_tf, TypefaceStyle.Normal);
            _infoTV.SetTypeface(_tf, TypefaceStyle.Normal);
            _orderBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _linkStickerBn.SetTypeface(_tf, TypefaceStyle.Normal);

            _headerTv.Text = TranslationHelper.GetString("linkQrSticker", GetCurrentCulture.GetCurrentCultureInfo());
            _mainTextTV.Text = TranslationHelper.GetString("shareCardWithoutOpeningApp", GetCurrentCulture.GetCurrentCultureInfo());
            _infoTV.Text = TranslationHelper.GetString("infoShareText", GetCurrentCulture.GetCurrentCultureInfo());
            _orderBn.Text = TranslationHelper.GetString("orderOrSticker", GetCurrentCulture.GetCurrentCultureInfo())?.ToUpper();
            _linkStickerBn.Text = TranslationHelper.GetString("linkQrSticker", GetCurrentCulture.GetCurrentCultureInfo())?.ToUpper();
        }

        private async Task LaunchScanner()
        {
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
            {
                if (!CameraGranted())
                    return;
            }
            var scanResult = await _scanner.Scan(_optionsCustom);
            HandleScanResult(scanResult);
        }

        async void HandleScanResult(ZXing.Result result)
        {
            HttpResponseMessage res = null;
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                if (!result.Text.ToLower().Contains("https"))
                {
                    CallAlert(TranslationHelper.GetString("wrongQr", GetCurrentCulture.GetCurrentCultureInfo()));
                    return;
                }
                if (!result.Text.ToLower().Contains("card.myqrcards.com/links/"))
                {
                    CallAlert(TranslationHelper.GetString("wrongQr", GetCurrentCulture.GetCurrentCultureInfo()));
                    return;
                }
                try
                {
                    var scannedString = result.Text;

                    string cardLinkID = "";
                    try
                    {
                        var splitted = scannedString.Split("/");
                        var count = scannedString.Count(x => x == '/');
                        cardLinkID = splitted[count];
                        if (string.IsNullOrEmpty(cardLinkID))
                            cardLinkID = splitted[count - 1];
                    }
                    catch
                    {
                        //CallAlert();
                    }
                    _linkStickerBn.Visibility = ViewStates.Gone;
                    _orderBn.Visibility = ViewStates.Gone;
                    _activityIndicator.Visibility = ViewStates.Visible;
                    res = await _cardLinks.CardsLinksGet(_databaseMethods.GetAccessJwt(), clientName, cardLinkID);
                    _linkStickerBn.Visibility = ViewStates.Visible;
                    _orderBn.Visibility = ViewStates.Visible;
                    _activityIndicator.Visibility = ViewStates.Gone;

                    if (res?.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        CallAlert(TranslationHelper.GetString("thisQrCannotBeUsedAsCard", GetCurrentCulture.GetCurrentCultureInfo()));
                        return;
                    }
                    if (res?.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        CallAlert(TranslationHelper.GetString("thisQrCannotBeUsedAsCard", GetCurrentCulture.GetCurrentCultureInfo()));
                        return;
                    }
                    if (res?.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        CallAlert(TranslationHelper.GetString("thisQrAlreadyBelongsToAnotherUser", GetCurrentCulture.GetCurrentCultureInfo()), orderButtonShown: false);
                        return;
                    }
                    if (res?.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await res.Content.ReadAsStringAsync();
                        var deserialized = JsonConvert.DeserializeObject<CardLinkModel>(content);
                        if (CardId == deserialized?.card?.id)
                        {
                            CallAlert(TranslationHelper.GetString("qrAlreadyLinkedToThisCard", GetCurrentCulture.GetCurrentCultureInfo()), orderButtonShown: false);
                            return;
                        }
                        if (CardId != deserialized?.card?.id && deserialized?.isDefault == false)
                        {
                            CallAlert($"{TranslationHelper.GetString("qrAlreadyLinkedToCard", GetCurrentCulture.GetCurrentCultureInfo())} {deserialized?.card?.name}. {TranslationHelper.GetString("rebind", GetCurrentCulture.GetCurrentCultureInfo())}", cardLinkID);
                            return;
                        }
                        if (deserialized?.isDefault == false)
                        {
                            CallAlert(TranslationHelper.GetString("impossibleToLinkMainQr", GetCurrentCulture.GetCurrentCultureInfo()));
                            return;
                        }
                    }
                    if (res?.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        await LinkCard(cardLinkID);
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
            }
            else
                return;
        }

        private async Task LinkCard(string cardLinkID)
        {
            try
            {
                _linkStickerBn.Visibility = ViewStates.Gone;
                _orderBn.Visibility = ViewStates.Gone;
                _activityIndicator.Visibility = ViewStates.Visible;
                var linkres = await _cardLinks.LinkCard(_databaseMethods.GetAccessJwt(), clientName, CardId.ToString(), cardLinkID);
                if (linkres.ToLower().Contains(Constants.status_code202))
                {
                    CallAlert(TranslationHelper.GetString("qrLinkedSuccessfully", GetCurrentCulture.GetCurrentCultureInfo()), linkedSuccessfully: true, orderButtonShown: false);
                    _linkStickerBn.Visibility = ViewStates.Visible;
                    _orderBn.Visibility = ViewStates.Visible;
                    _activityIndicator.Visibility = ViewStates.Gone;
                    return;
                }
                else
                {
                    CallAlert(TranslationHelper.GetString("impossibleLinkThisQrToThisCard", GetCurrentCulture.GetCurrentCultureInfo()));
                    _linkStickerBn.Visibility = ViewStates.Visible;
                    _orderBn.Visibility = ViewStates.Visible;
                    _activityIndicator.Visibility = ViewStates.Gone;
                    return;
                }
                _linkStickerBn.Visibility = ViewStates.Visible;
                _orderBn.Visibility = ViewStates.Visible;
                _activityIndicator.Visibility = ViewStates.Gone;
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
                else
                {
                    CallAlert(TranslationHelper.GetString("impossibleLinkThisQrToThisCard", GetCurrentCulture.GetCurrentCultureInfo()));
                    return;
                }
                _linkStickerBn.Visibility = ViewStates.Visible;
                _orderBn.Visibility = ViewStates.Visible;
                _activityIndicator.Visibility = ViewStates.Gone;
            }
        }

        void CallAlert(string title, string cardLinkId = null, bool orderButtonShown = true, bool linkedSuccessfully = false)
        {
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
            if (cardLinkId != null)
            {
                builder.SetPositiveButton(TranslationHelper.GetString("yes", GetCurrentCulture.GetCurrentCultureInfo()), async (object s, DialogClickEventArgs e) => { await LinkCard(cardLinkId); });
                builder.SetNegativeButton(TranslationHelper.GetString("no", GetCurrentCulture.GetCurrentCultureInfo()), (object s, DialogClickEventArgs e) => { });
            }
            else
            {
                builder.SetPositiveButton("OK", (object s, DialogClickEventArgs e) =>
                {
                    if (linkedSuccessfully)
                        OnBackPressed();
                });
                if (orderButtonShown)
                    builder.SetNeutralButton(TranslationHelper.GetString("orderStickerWithQr", GetCurrentCulture.GetCurrentCultureInfo()), (object s, DialogClickEventArgs e) => OpenOrderLink(s, e));
            }

            builder.SetMessage(title);
            Android.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        void OpenOrderLink(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("https://myqrcards.com/sticker");
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }

        public CameraResolution SelectLowestResolutionMatchingDisplayAspectRatio(List<CameraResolution> availableResolutions)
        {
            CameraResolution result = null;
            try
            {
                double aspectTolerance = 0.1;
                var displayOrientationHeight = Resources.DisplayMetrics.HeightPixels;
                var displayOrientationWidth = Resources.DisplayMetrics.WidthPixels;
                var targetRatio = displayOrientationHeight / displayOrientationWidth;
                var targetHeight = displayOrientationHeight;
                result = availableResolutions?[0];
            }
            catch
            {

            }
            return result;
        }

        private const int requestPermissionCode = 1000;
        public void RequestCameraPermissions()
        {
            if (!CameraGranted())
            {
                ActivityCompat.RequestPermissions(this, new String[]
                {
                                Manifest.Permission.Camera,
                }, requestPermissionCode);
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[]
                {
                                Manifest.Permission.Camera,
                }, requestPermissionCode);
            }
        }

        private bool CameraGranted()
        {
            return CheckSelfPermission(Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted;
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            if (CameraGranted())
                await LaunchScanner();
        }
    }
}
