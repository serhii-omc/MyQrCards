using System;
using System.Globalization;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Localization;
using CardsPCL.Models;
using Newtonsoft.Json;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class PremiumActivity : Activity
    {
        Typeface _tf;
        Button _monthBn, _yearBn, _termsBn, _politicsBn;
        DateTime? _maxValTill = null;
        TextView _premiumAdvantagesTv, _premiumDetailsTv, _ratesTv, _headerTv, _subscriptionTillTv, _monthTv, _yearTv;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        DatabaseMethods _databaseMethods = new DatabaseMethods();
        Methods _methods = new Methods();
        ProgressBar _monthActivityIndicator, _yearActivityIndicator;
        Accounts _accounts = new Accounts();
        string clientName;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            clientName = Android.OS.Build.Manufacturer + " " + Android.OS.Build.Model;
            SetContentView(Resource.Layout.premium);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;

            InitElements();

            if (_databaseMethods.UserExists())
            {
                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }
                var purchaseInfo = await _accounts.AccountAuthorize(_databaseMethods.GetAccessJwt(), clientName);
                //var deserialized_info = JsonConvert.DeserializeObject<AuthorizeRootObject>(purchase_info);
                AuthorizeAfterPurchase(purchaseInfo);
            }

            FindViewById<RelativeLayout>(Resource.Id.backRL).Click += (s, e) => OnBackPressed();

            _monthBn.Click += async (s, e) =>
            {
                if (!_databaseMethods.UserExists())
                {
                    call_login_menu();
                    return;
                }

                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }

                _monthActivityIndicator.Visibility = ViewStates.Visible;
                _monthBn.Visibility = ViewStates.Gone;

                string productId = "month_auto_subscription";
                InAppBillingService inAppBillingService = new InAppBillingService();
                InAppBillingPurchase purchase = new InAppBillingPurchase();

                var authorizeCheck = await _accounts.AccountAuthorize(_databaseMethods.GetAccessJwt(), clientName);
                if (/*res_card_data == Constants.status_code409 ||*/ authorizeCheck == Constants.status_code401)
                {
                    ShowSeveralDevicesRestriction();
                    return;
                }
                var deserialized = JsonConvert.DeserializeObject<AuthorizeRootObject>(authorizeCheck);
                if (deserialized != null)
                {
                    var jsonObj = JsonConvert.SerializeObject(new { accountID = deserialized.accountID.ToString() });
                    var textBytes = Encoding.UTF8.GetBytes(jsonObj);
                    var base64 = Convert.ToBase64String(textBytes);

                    if (deserialized.accountID != null)
                        purchase = await inAppBillingService.PurchaseSubscription(productId, base64);
                }
                //else
                //purchase = await inAppBillingService.PurchaseSubscription(product_id, String.Empty);
                if (purchase != null)
                {
                    Toast.MakeText(this, TranslationHelper.GetString("waitServerSync", _ci), ToastLength.Short).Show();
                    // Inform our server.

                    var notifyServer = await _accounts.AccountSubscribeAndroid(_databaseMethods.GetAccessJwt(),
                                                                               3,
                                                                               purchase.ProductId,
                                                                               purchase.PurchaseToken);

                    //var notify_server = await accounts.AccountSubscribe(
                    //databaseMethods.GetAccessJwt(),
                    //Constants.personalGoogle.ToString(),
                    //purchase,
                    //DateTime.UtcNow.AddDays(1), // ATTENTION with days.
                    //NativeMethods.GetDeviceId()
                    //);

                    var authorizeCheckAfterPurchase = await _accounts.AccountAuthorize(_databaseMethods.GetAccessJwt(), clientName);
                    AuthorizeAfterPurchase(authorizeCheckAfterPurchase);
                    _monthActivityIndicator.Visibility = ViewStates.Gone;
                    Toast.MakeText(this, TranslationHelper.GetString("syncDoneSuccessfully", _ci), ToastLength.Short).Show();
                    return;
                }
                _monthActivityIndicator.Visibility = ViewStates.Gone;
                _monthBn.Visibility = ViewStates.Visible;
            };

            _yearBn.Click += async (s, e) =>
            {
                if (!_databaseMethods.UserExists())
                {
                    call_login_menu();
                    return;
                }

                if (!_methods.IsConnected())
                {
                    NoConnectionActivity.ActivityName = this;
                    StartActivity(typeof(NoConnectionActivity));
                    Finish();
                    return;
                }

                _yearActivityIndicator.Visibility = ViewStates.Visible;
                _yearBn.Visibility = ViewStates.Gone;

                string productId = "year_auto_subscription";
                InAppBillingService inAppBillingService = new InAppBillingService();
                InAppBillingPurchase purchase = new InAppBillingPurchase();

                var authorizeCheck = await _accounts.AccountAuthorize(_databaseMethods.GetAccessJwt(), clientName);
                if (/*res_card_data == Constants.status_code409 ||*/ authorizeCheck == Constants.status_code401)
                {
                    ShowSeveralDevicesRestriction();
                    return;
                }
                var deserialized = JsonConvert.DeserializeObject<AuthorizeRootObject>(authorizeCheck);
                if (deserialized != null)
                {
                    var jsonObj = JsonConvert.SerializeObject(new { accountID = deserialized.accountID.ToString() });
                    var textBytes = Encoding.UTF8.GetBytes(jsonObj);
                    var base64 = Convert.ToBase64String(textBytes);

                    if (deserialized.accountID != null)
                        purchase = await inAppBillingService.PurchaseSubscription(productId, base64);
                }
                //else
                //purchase = await inAppBillingService.PurchaseSubscription(product_id, String.Empty);
                if (purchase != null)
                {
                    Toast.MakeText(this, TranslationHelper.GetString("waitServerSync", _ci), ToastLength.Short).Show();
                    // Inform our server.

                    var notifyServer = await _accounts.AccountSubscribeAndroid(_databaseMethods.GetAccessJwt(),
                                                                               3,
                                                                               purchase.ProductId,
                                                                               purchase.PurchaseToken);

                    //var notify_server = await accounts.AccountSubscribe(
                    //databaseMethods.GetAccessJwt(),
                    //Constants.personalGoogle.ToString(),
                    //purchase,
                    //DateTime.UtcNow.AddDays(1), // ATTENTION with days.
                    //NativeMethods.GetDeviceId()
                    //);

                    var authorizeCheckAfterPurchase = await _accounts.AccountAuthorize(_databaseMethods.GetAccessJwt(), clientName);
                    AuthorizeAfterPurchase(authorizeCheckAfterPurchase);
                    _yearActivityIndicator.Visibility = ViewStates.Gone;
                    Toast.MakeText(this, TranslationHelper.GetString("syncDoneSuccessfully", _ci), ToastLength.Short).Show();
                    return;
                }
                _yearActivityIndicator.Visibility = ViewStates.Gone;
                _yearBn.Visibility = ViewStates.Visible;
            };

            _termsBn.Click += (s, e) =>
              {
                  var uri = Android.Net.Uri.Parse("https://myqrcards.com/agreement/ru");
                  StartActivity(new Intent(Intent.ActionView, uri));
              };
            _politicsBn.Click += (s, e) =>
              {
                  var uri = Android.Net.Uri.Parse("https://myqrcards.com/privacy-policy/ru");
                  StartActivity(new Intent(Intent.ActionView, uri));
              };
        }

        private void InitElements()
        {
            _tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _premiumAdvantagesTv = FindViewById<TextView>(Resource.Id.premium_advantagesTV);
            _premiumDetailsTv = FindViewById<TextView>(Resource.Id.premium_detailsTV);
            _ratesTv = FindViewById<TextView>(Resource.Id.ratesTV);
            _subscriptionTillTv = FindViewById<TextView>(Resource.Id.subscriptionTillTV);
            _monthBn = FindViewById<Button>(Resource.Id.monthBn);
            _yearBn = FindViewById<Button>(Resource.Id.yearBn);
            _termsBn = FindViewById<Button>(Resource.Id.termsBn);
            _politicsBn = FindViewById<Button>(Resource.Id.politicsBn);
            _monthTv = FindViewById<TextView>(Resource.Id.monthTV);
            _yearTv = FindViewById<TextView>(Resource.Id.yearTV);
            _monthBn.Text = "29 \u20BD";
            _yearBn.Text = "299 \u20BD";
            _headerTv.Text = TranslationHelper.GetString("premium", _ci);
            _premiumAdvantagesTv.Text = TranslationHelper.GetString("premiumAdvantages", _ci);
            _premiumDetailsTv.Text = TranslationHelper.GetString("premiumDetails", _ci);
            _ratesTv.Text = TranslationHelper.GetString("rates", _ci);
            _monthTv.Text = TranslationHelper.GetString("monthSubscription", _ci);
            _yearTv.Text = TranslationHelper.GetString("yearSubscription", _ci);
            _termsBn.Text = TranslationHelper.GetString("termsOfService", _ci);
            _politicsBn.Text = TranslationHelper.GetString("politicsConidential", _ci);
            _monthBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _yearBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _headerTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _premiumAdvantagesTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _premiumDetailsTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _ratesTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _termsBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _politicsBn.SetTypeface(_tf, TypefaceStyle.Normal);
            _monthActivityIndicator = FindViewById<ProgressBar>(Resource.Id.monthActivityIndicator);
            _monthActivityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _monthActivityIndicator.Visibility = ViewStates.Gone;
            _yearActivityIndicator = FindViewById<ProgressBar>(Resource.Id.yearActivityIndicator);
            _yearActivityIndicator.IndeterminateDrawable.SetColorFilter(Resources.GetColor(Resource.Color.buttonOrangeColor), Android.Graphics.PorterDuff.Mode.Multiply);
            _yearActivityIndicator.Visibility = ViewStates.Gone;
            _subscriptionTillTv.Visibility = ViewStates.Gone;
            if (_databaseMethods.UserExists())
                HideBuyElements();
        }

        void call_login_menu()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(TranslationHelper.GetString("needToLoginToBuySubs", _ci));

            builder.SetNegativeButton(TranslationHelper.GetString("cancel", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("login", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
                StartActivity(typeof(EmailActivity));
            });
            AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
        }
        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out(this);
            MyCardActivity.DeviceRestricted = true;
            Intent intent = new Intent(this, typeof(MyCardActivity));
            intent.AddFlags(ActivityFlags.ClearTop); // Removes other Activities from stack
            StartActivity(intent);
        }

        void AuthorizeAfterPurchase(string cardsRemainingcontent)
        {
            var desAuth = JsonConvert.DeserializeObject<AuthorizeRootObject>(cardsRemainingcontent);
            try
            {
                try
                {
                    foreach (var subscription in desAuth.subscriptions)
                    {
                        if (subscription.isTrial)
                        {
                            HideSubscriptionTill();
                            break;
                        }
                        if (subscription.id != 1)
                        {
                            if (_maxValTill != null)
                            {
                                var res = DateTime.Compare(subscription.validTill, (DateTime)_maxValTill);
                                if (res < 0)
                                    _maxValTill = subscription.validTill;
                            }
                            else
                                _maxValTill = subscription.validTill;
                        }
                        if (_maxValTill != null)
                            ShowSubscriptionTill();
                        else
                            ShowBuyElements();
                    }
                }
                catch (Exception ex)
                {
                }
                foreach (var subs in desAuth.subscriptions)
                {
                    if (subs.limitations != null)
                        if (subs.limitations.allowMultiClients)
                        {
                            QrActivity.IsPremium = true;
                            break;
                        }
                }
                //if (!is_premium)
                foreach (var subscription in desAuth.subscriptions)
                {
                    if (subscription.limitations?.cardsRemaining == null)
                    {
                        QrActivity.CardsRemaining = 10;
                        break;
                    }
                    else
                    {
                        if (subscription.limitations != null)
                            if (subscription.limitations.cardsRemaining > QrActivity.CardsRemaining)
                                QrActivity.CardsRemaining = subscription.limitations.cardsRemaining.Value;
                    }
                }
                try
                {
                    foreach (var subscription in desAuth.subscriptions)
                    {
                        if (subscription.features != null)
                        {
                            foreach (var feature in subscription.features)
                            {
                                if (String.IsNullOrEmpty(QrActivity.ExtraEmploymentData))
                                    if (feature == Constants.ExtraEmploymentData)
                                        QrActivity.ExtraEmploymentData = feature;
                                if (String.IsNullOrEmpty(QrActivity.CompanyLogoInQr))
                                    if (feature == Constants.CompanyLogoInQr)
                                        QrActivity.CompanyLogoInQr = feature;
                                if (String.IsNullOrEmpty(QrActivity.ExtraPersonData))
                                    if (feature == Constants.ExtraPersonData)
                                        QrActivity.ExtraPersonData = feature;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            catch { }
            //SideMenuViewController.reloadOption();
        }

        void ShowSubscriptionTill()
        {
            _subscriptionTillTv.Visibility = ViewStates.Visible;
            var dateTill = _maxValTill?.ToLocalTime().Date.ToString().Split(' ')[0].Replace('/', '.');//.Substring(0, 10);
            _subscriptionTillTv.Text = $"{TranslationHelper.GetString("yourSubscriptionPaidTill", _ci)} {dateTill}";
            HideBuyElements();
            _monthTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _yearTv.SetTypeface(_tf, TypefaceStyle.Normal);
        }
        void HideSubscriptionTill()
        {
            _subscriptionTillTv.Visibility = ViewStates.Gone;
            ShowBuyElements();
            _monthTv.SetTypeface(_tf, TypefaceStyle.Normal);
            _yearTv.SetTypeface(_tf, TypefaceStyle.Normal);
        }
        void ShowBuyElements()
        {
            _ratesTv.Visibility = ViewStates.Visible;
            _monthBn.Visibility = ViewStates.Visible;
            _monthTv.Visibility = ViewStates.Visible;
            _yearBn.Visibility = ViewStates.Visible;
            _yearTv.Visibility = ViewStates.Visible;
        }
        void HideBuyElements()
        {
            _ratesTv.Visibility = ViewStates.Gone;
            _monthBn.Visibility = ViewStates.Gone;
            _monthTv.Visibility = ViewStates.Gone;
            _yearBn.Visibility = ViewStates.Gone;
            _yearTv.Visibility = ViewStates.Gone;
        }
    }
}
