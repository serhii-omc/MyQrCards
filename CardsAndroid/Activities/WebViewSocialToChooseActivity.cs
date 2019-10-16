
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using CardsAndroid.Adapters;
using CardsAndroid.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Enums;
using CardsPCL.Localization;
using VKontakte;

namespace CardsAndroid.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustNothing)]
    public class WebViewSocialToChooseActivity : Activity
    {
        public static string HeaderValue;
        public static string UrlString;
        public static string UrlRoot;
        EditText _urlEt;
        TextView _hintTv, _headerTv;
        RelativeLayout _backRl;
        Button _removeBn, _acceptBn, _showBn;
        Android.Webkit.WebView _webView;
        CultureInfo _ci = GetCurrentCulture.GetCurrentCultureInfo();
        Methods _methods = new Methods();
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.WebViewSocialToChoose);

            InitElements();

            //VKSdk.Initialize(this).WithPayments();

            if (!_methods.IsConnected())
            {
                NoConnectionActivity.ActivityName = this;
                StartActivity(typeof(NoConnectionActivity));
                Finish();
                return;
            }

            _acceptBn.Click += AcceptBn_TouchUpInside;

            _removeBn.Click += (s, e) => ShowRemoveAlert();
            //InitElements();
            //View.Hidden = true;

            var res = await CheckAccounts();
            UpdateWebView();
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetFields();
        }

        void SetFields()
        {
            if (!String.IsNullOrEmpty(UrlString))
            {
                _urlEt.Text = UrlString;
                if (UrlRoot != Constants.linkedinUrl && UrlRoot != Constants.facebookUrl)
                    _webView.Visibility = ViewStates.Visible;
                else
                    _webView.Visibility = ViewStates.Gone;
                _showBn.Visibility = ViewStates.Gone;
                UpdateWebView();
            }
            else
            {
                _showBn.Visibility = ViewStates.Gone;
                _webView.Visibility = ViewStates.Gone;
                UpdateWebView();
            }
        }

        private async Task<bool> CheckAccounts()
        {
            _webView.Visibility = ViewStates.Visible;
            _showBn.Visibility = ViewStates.Gone;
            string result = null;
            if (UrlRoot == Constants.vkontakteUrl)
                result = await GetVkData();
            //else if (urlRoot == SocialNetworkData.SampleData()[0].ContactUrl)
            //result = await GetFbData();
            if (!String.IsNullOrEmpty(result))
            {
                _webView.Visibility = ViewStates.Visible;
                _showBn.Visibility = ViewStates.Gone;
                UrlString = result;
                _urlEt.Text = UrlString;
                //urlET.FloatLabelTop();
            }
            else
            {
                //showBn.Hidden = true;
                _webView.Visibility = ViewStates.Gone;
                //activityIndicator.Hidden = true;
                if (String.IsNullOrEmpty(_urlEt.Text))
                    _showBn.Visibility = ViewStates.Gone;
            }
            //Dispose();
            return true;
        }

        async Task<string> GetVkData()
        {
            try
            {
                AndroidVkService.Context = this;
                if (/*String.IsNullOrEmpty(VkRetrievedData) && */String.IsNullOrEmpty(UrlString))
                {
                    //VKSdk.Instance.RegisterDelegate(this.ViewController);
                    //VKSdk.Instance.UiDelegate = this.ViewController;
                    var loginResult = await new AndroidVkService().Login();
                    if (loginResult.LoginState != LoginState.Canceled)
                        if (!String.IsNullOrEmpty(loginResult.UserId))
                        {
                            //VkRetrievedData
                            UrlString = Constants.vkontakteUrl + "id" + loginResult.UserId;
                            return UrlString;//VkRetrievedData;
                        }

                }
                else
                    return UrlString;//VkRetrievedData;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        void AcceptBn_TouchUpInside(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_urlEt.Text))
                return;

            int i = 0;
            //foreach (var item in SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl)
            //{
            //    if (item.SocialNetworkID == SocialNetworkData.SampleData()[SocialNetworkTableViewSource<int, int>.currentIndex].Id)
            //    {
            //        SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.RemoveAt(i);
            //        break;
            //    }
            //    i++;
            //}

            // Check if the user entered id of his profile.
            if (!_urlEt.Text.ToLower().Contains("http"))
                JoinIdAndLink();

            if (UrlRoot == Constants.facebookUrl)
                SocialNetworkAdapter.SocialNetworks[0].UsersUrl = _urlEt.Text;
            else if (UrlRoot == Constants.instagramUrl)
                SocialNetworkAdapter.SocialNetworks[1].UsersUrl = _urlEt.Text;
            else if (UrlRoot == Constants.linkedinUrl)
                SocialNetworkAdapter.SocialNetworks[2].UsersUrl = _urlEt.Text;
            else if (UrlRoot == Constants.twitterUrl)
                SocialNetworkAdapter.SocialNetworks[3].UsersUrl = _urlEt.Text;
            else if (UrlRoot == Constants.vkontakteUrl)
                SocialNetworkAdapter.SocialNetworks[4].UsersUrl = _urlEt.Text;
            EditPersonalDataActivity.ChangedSomething = true;
            OnBackPressed();

            //SocialNetworkAdapter<int, int>.socialNetworkListWithMyUrl.Add(
            //                                new CardsPCL.Models.SocialNetworkModel
            //                                {
            //                                    SocialNetworkID = SocialNetworkData.SampleData()[SocialNetworkTableViewSource<int, int>.currentIndex].Id,
            //                                    ContactUrl = UrlTextField.Text
            //                                });

            //bool index_exists = false;
            //foreach (var index in SocialNetworkTableViewSource<int, int>.selectedIndexes)
            //{
            //    if (index == SocialNetworkTableViewSource<int, int>.currentIndex)
            //    {
            //        index_exists = true;
            //        break;
            //    }
            //}
            //if (!index_exists)
            //SocialNetworkTableViewSource<int, int>.selectedIndexes.Add(SocialNetworkTableViewSource<int, int>.currentIndex);
            //NavigationController.PopViewController(true);
        }
        private void InitElements()
        {
            Typeface tf = Typeface.CreateFromAsset(Assets, "FiraSansRegular.ttf");
            _urlEt = FindViewById<EditText>(Resource.Id.urlET);
            _hintTv = FindViewById<TextView>(Resource.Id.hintTV);
            _backRl = FindViewById<RelativeLayout>(Resource.Id.backRL);
            _removeBn = FindViewById<Button>(Resource.Id.removeBn);
            _acceptBn = FindViewById<Button>(Resource.Id.acceptBn);
            _showBn = FindViewById<Button>(Resource.Id.showBn);
            _webView = FindViewById<Android.Webkit.WebView>(Resource.Id.webView);
            _headerTv = FindViewById<TextView>(Resource.Id.headerTV);
            _hintTv.Text = TranslationHelper.GetString("specifyYourAccountInSocialNetwork", _ci);
            _removeBn.Text = TranslationHelper.GetString("remove", _ci);
            _showBn.Text = TranslationHelper.GetString("viewing", _ci);
            _acceptBn.Text = TranslationHelper.GetString("save", _ci);
            _backRl.Click += (s, e) => OnBackPressed();
            _headerTv.Text = HeaderValue;
            _urlEt.SetTypeface(tf, TypefaceStyle.Normal);
            _hintTv.SetTypeface(tf, TypefaceStyle.Normal);
            _removeBn.SetTypeface(tf, TypefaceStyle.Normal);
            _acceptBn.SetTypeface(tf, TypefaceStyle.Normal);
            _showBn.SetTypeface(tf, TypefaceStyle.Normal);
            _headerTv.SetTypeface(tf, TypefaceStyle.Normal);
            // Prevent redirecting to default browser.
            _webView.SetWebViewClient(new WebViewClient());
            _webView.Settings.JavaScriptEnabled = true;
            _webView.Clickable = false;
            _webView.Focusable = false;
            UpdateWebView();
            _urlEt.TextChanged += (s, e) =>
            {
                UrlString = _urlEt.Text;
                UpdateWebView();
                _webView.Visibility = ViewStates.Gone;
                // Second condition means that we do not need facebook and linkedin previews.
                if (!String.IsNullOrEmpty(_urlEt.Text) && UrlRoot != Constants.linkedinUrl && UrlRoot != Constants.facebookUrl)
                    _showBn.Visibility = ViewStates.Visible;
                else
                    _showBn.Visibility = ViewStates.Gone;
                //if (!UrlTextField.Text.ToLower().Contains("http"))
                //JoinIdAndLink();
            };
            //urlET.
            _showBn.Click += (s, e) =>
            {
                // Check if the user entered id of his profile.
                if (!_urlEt.Text.ToLower().Contains("http"))
                    JoinIdAndLink();
                _webView.Visibility = ViewStates.Visible;
                _showBn.Visibility = ViewStates.Gone;
            };
        }

        private void JoinIdAndLink()
        {
            // Facebook.
            if (UrlRoot == Constants.facebookUrl)
                _urlEt.Text = Constants.facebookUrl /*+ Constants.faceBookUrlPart*/ + _urlEt.Text;
            // Instagram.
            else if (UrlRoot == Constants.instagramUrl)
                _urlEt.Text = Constants.instagramUrl + _urlEt.Text;
            // LinkedIn.
            else if (UrlRoot == Constants.linkedinUrl)
            {
                if (_urlEt.Text.Contains("/"))
                {
                    var linkedIdArray = _urlEt.Text.Split("/");
                    _urlEt.Text = Constants.linkedinUrl + Constants.linkedInUrlPart + linkedIdArray[linkedIdArray.Length - 1];
                }
                else
                    _urlEt.Text = Constants.linkedinUrl + Constants.linkedInUrlPart + _urlEt.Text;
            }
            // Twitter.
            else if (UrlRoot == Constants.twitterUrl)
                _urlEt.Text = Constants.twitterUrl + _urlEt.Text;
            // Vkontakte.
            else if (UrlRoot == Constants.vkontakteUrl)
                _urlEt.Text = Constants.vkontakteUrl + _urlEt.Text;
            UrlString = _urlEt.Text;
            UpdateWebView();
        }

        private void ShowRemoveAlert()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(TranslationHelper.GetString("removeSocialNetworkLinkQuestion", _ci));
            builder.SetNegativeButton(TranslationHelper.GetString("no", _ci), (object sender1, DialogClickEventArgs e1) => { });
            builder.SetCancelable(true);
            builder.SetPositiveButton(TranslationHelper.GetString("yes", _ci), (object sender1, DialogClickEventArgs e1) =>
            {
                if (UrlRoot == Constants.facebookUrl)
                    SocialNetworkAdapter.SocialNetworks[0].UsersUrl = null;
                else if (UrlRoot == Constants.instagramUrl)
                    SocialNetworkAdapter.SocialNetworks[1].UsersUrl = null;
                else if (UrlRoot == Constants.linkedinUrl)
                    SocialNetworkAdapter.SocialNetworks[2].UsersUrl = null;
                else if (UrlRoot == Constants.twitterUrl)
                    SocialNetworkAdapter.SocialNetworks[3].UsersUrl = null;
                else if (UrlRoot == Constants.vkontakteUrl)
                    SocialNetworkAdapter.SocialNetworks[4].UsersUrl = null;
                base.OnBackPressed();
            });
            AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        void UpdateWebView()
        {
            try
            {
                _webView.LoadUrl(UrlString);
            }
            catch
            {
            }
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            bool vkResult;
            var task = VKSdk.OnActivityResultAsync(requestCode, resultCode, data, out vkResult);

            if (!vkResult)
            {
                base.OnActivityResult(requestCode, resultCode, data);
                //AndroidFacebookService.Instance.OnActivityResult(requestCode, (int)resultCode, data);
                return;
            }
            //InitElements();
            try
            {
                var token = await task;
                // Get token
                UrlString = Constants.vkontakteUrl + "id" + token.UserId;
            }
            catch (Exception e)
            {
                // Handle exception
            }
        }
    }
}
