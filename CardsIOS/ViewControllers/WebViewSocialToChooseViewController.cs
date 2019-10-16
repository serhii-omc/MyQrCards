using System;
using System.Drawing;
using System.Threading.Tasks;
using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Enums;
using Foundation;
using UIKit;
using VKontakte;
using WebKit;

namespace CardsIOS
{
    public partial class WebViewSocialToChooseViewController : UIViewController, IWKNavigationDelegate
    {
        WKWebView webView;
        UIButton showBn;
        //UIWebView webView;
        UIActionSheet option;
        public static string headerValue;
        public static string urlString;
        public static string urlRoot;
        //public static string VkRetrievedData;
        //public static string FbRetrievedData;
        Methods methods = new Methods();
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        AppleVkService appleVkService = new AppleVkService();

        protected FloatingTextField UrlTextField { get; private set; }
        public WebViewSocialToChooseViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        //public override void ViewWillAppear(bool animated)
        //{
        //    base.ViewWillAppear(animated);


        //}

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();

            string[] tableItems = new string[] { "Да" };

            option = new UIActionSheet(null, null, "Нет", null, tableItems);
            option.Title = "Удалить ссылку на профиль в социальной сети?";
            option.Clicked += (btn_sender, args) => //Console.WriteLine("{0} Clicked", args.ButtonIndex);
            {
                if (args.ButtonIndex == 0)
                {
                    SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.RemoveAll(x => x.SocialNetworkID == SocialNetworkData.SampleData()[SocialNetworkTableViewSource<int, int>.currentIndex].Id);
                    SocialNetworkTableViewSource<int, int>.selectedIndexes.RemoveAll(x => x == SocialNetworkTableViewSource<int, int>.currentIndex);
                    NavigationController.PopViewController(true);
                    SocialNetworkTableViewSource<int, int>._checkedRows.Clear();
                }
            };
            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            if (!methods.IsConnected())
            {
                NoConnectionViewController.view_controller_name = GetType().Name;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                return;
            }
            acceptBn.TouchUpInside += AcceptBn_TouchUpInside;
            removeBn.TouchUpInside += (s, e) =>
              {
                  option.ShowInView(View);
              };
            //InitElements();
            //View.Hidden = true;
            try
            {
                var res = await checkAccounts();
            }
            catch
            {
                if (!methods.IsConnected())
                    InvokeOnMainThread(() =>
                    {
                        NoConnectionViewController.view_controller_name = GetType().Name;
                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                        return;
                    });
                return;
            }
            UpdateWebView();
            //webView.LoadFinished += (s, e) =>
            //{
            //    using (s as UIWebView)
            //    {
            //        UrlTextField.Text = ((UIWebView)s).Request.Url.AbsoluteString;
            //        urlString = ((UIWebView)s).Request.Url.AbsoluteString;
            //        UrlTextField.FloatLabelTop();
            //    }
            //};
            //var gkgmg = new WKWebView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            SetFields();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            activityIndicator.Hidden = true;
            SetFields();
        }

        void SetFields()
        {
            if (!String.IsNullOrEmpty(urlString))
            {
                UrlTextField.FloatLabelTop();
                UrlTextField.Text = urlString;
                if (urlRoot != SocialNetworkData.SampleData()[2].ContactUrl && urlRoot != SocialNetworkData.SampleData()[0].ContactUrl)
                    webView.Hidden = false;
                else
                    webView.Hidden = true;
                showBn.Hidden = true;
                UpdateWebView();
            }
            else
            {
                showBn.Hidden = true;
                webView.Hidden = true;
                UpdateWebView();
            }
        }

        void AcceptBn_TouchUpInside(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(UrlTextField.Text))
                return;

            int i = 0;
            foreach (var item in SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl)
            {
                if (item.SocialNetworkID == SocialNetworkData.SampleData()[SocialNetworkTableViewSource<int, int>.currentIndex].Id)
                {
                    SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.RemoveAt(i);
                    break;
                }
                i++;
            }

            // Check if the user entered id of his profile.
            if (!UrlTextField.Text.ToLower().Contains("http"))
                JoinIdAndLink();
            SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Add(
                                            new CardsPCL.Models.SocialNetworkModel
                                            {
                                                SocialNetworkID = SocialNetworkData.SampleData()[SocialNetworkTableViewSource<int, int>.currentIndex].Id,
                                                ContactUrl = UrlTextField.Text
                                            });

            bool index_exists = false;
            foreach (var index in SocialNetworkTableViewSource<int, int>.selectedIndexes)
            {
                if (index == SocialNetworkTableViewSource<int, int>.currentIndex)
                {
                    index_exists = true;
                    break;
                }
            }
            if (!index_exists)
                SocialNetworkTableViewSource<int, int>.selectedIndexes.Add(SocialNetworkTableViewSource<int, int>.currentIndex);
            NavigationController.PopViewController(true);
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;
            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
                removeBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), Convert.ToInt32(View.Frame.Width) / 12 + 20, Convert.ToInt32(View.Frame.Width) / 4, Convert.ToInt32(View.Frame.Width) / 19);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
                removeBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), Convert.ToInt32(View.Frame.Width) / 12, Convert.ToInt32(View.Frame.Width) / 4, Convert.ToInt32(View.Frame.Width) / 19);
            }
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = headerValue;
            removeBn.SetTitle("Удалить", UIControlState.Normal);

            acceptBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height / 2 - View.Frame.Width / 20), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));

            var hintLabel = new UILabel();
            hintLabel.TextColor = UIColor.White;
            hintLabel.Text = "Укажите свой аккаунт в социальной сети (id) или вставьте ссылку на свой профиль";

            hintLabel.Frame = new CoreGraphics.CGRect(20, headerView.Frame.Height + 10, headerView.Frame.Width - 40, headerView.Frame.Height / 1.65);
            hintLabel.Lines = 2;
            View.AddSubview(hintLabel);
            hintLabel.Font = UIFont.FromName(Constants.fira_sans, 14f);
            hintLabel.TextColor = UIColor.FromRGB(146, 150, 155);
            //hintLabel.TextAlignment = UITextAlignment.;
            //hintLabel.BackgroundColor = UIColor.Red;

            webView = new WKWebView(View.Frame, new WKWebViewConfiguration());
            //webView = new UIWebView();
            webView.NavigationDelegate = this;
            View.AddSubview(webView);
            //webView.Hidden = true;
            UpdateWebView();
            acceptBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 9,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            acceptBn.SetTitle("СОХРАНИТЬ", UIControlState.Normal);
            acceptBn.Font = acceptBn.Font.WithSize(15f);
            var val = View.Frame.Height - acceptBn.Frame.Y - acceptBn.Frame.Height;
            webView.Frame = new Rectangle(0,
                                          (int)headerView.Frame.Height * 2 + 51,
                                          (int)View.Frame.Width,
                                          (int)(acceptBn.Frame.Y - headerView.Frame.Height * 3 - val));
            acceptBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            removeBn.Font = UIFont.FromName(Constants.fira_sans, 17f);

            UrlTextField = new FloatingTextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Ссылка на ваш профиль",
                TextColor = UIColor.White,
                ReturnKeyType = UIReturnKeyType.Done,
                AutocapitalizationType = UITextAutocapitalizationType.None
            };
            UrlTextField.ShouldReturn = _ => View.EndEditing(true);

            UrlTextField.EditingChanged += (s, e) =>
            {
                urlString = UrlTextField.Text;
                UpdateWebView();
                webView.Hidden = true;
                // Second condition means that we do not need facebook and linkedin previews.
                if (!String.IsNullOrEmpty(UrlTextField.Text) && urlRoot != SocialNetworkData.SampleData()[2].ContactUrl && urlRoot != SocialNetworkData.SampleData()[0].ContactUrl)
                    showBn.Hidden = false;
                else
                    showBn.Hidden = true;

                //if (!UrlTextField.Text.ToLower().Contains("http"))
                //JoinIdAndLink();
            };
            View.AddSubview(UrlTextField);
            View.AddConstraints(new NSLayoutConstraint[]
            {
                UrlTextField.TopAnchor.ConstraintEqualTo(View.TopAnchor, hintLabel.Frame.Y+hintLabel.Frame.Height+10),
                UrlTextField.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 16),
                UrlTextField.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -16),
                UrlTextField.HeightAnchor.ConstraintEqualTo(48)
            });

            showBn = new UIButton();
            showBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            View.AddSubview(showBn);
            showBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         Convert.ToInt32(webView.Frame.Y),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            showBn.SetTitle("ПРОСМОТР", UIControlState.Normal);
            showBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            showBn.TouchUpInside += (s, e) =>
              {
                  // Check if the user entered id of his profile.
                  if (!UrlTextField.Text.ToLower().Contains("http"))
                      JoinIdAndLink();
                  webView.Hidden = false;
                  showBn.Hidden = true;
              };
        }
        // If the user enters id of his profile.
        private void JoinIdAndLink()
        {
            // Facebook.
            if (urlRoot == SocialNetworkData.SampleData()[0].ContactUrl)
                UrlTextField.Text = SocialNetworkData.SampleData()[0].ContactUrl /*+ Constants.faceBookUrlPart*/ + UrlTextField.Text;
            // Instagram.
            else if (urlRoot == SocialNetworkData.SampleData()[1].ContactUrl)
                UrlTextField.Text = SocialNetworkData.SampleData()[1].ContactUrl + UrlTextField.Text;
            // LinkedIn.
            else if (urlRoot == SocialNetworkData.SampleData()[2].ContactUrl)
            {
                if (UrlTextField.Text.Contains("/"))
                {
                    var linkedIdArray = UrlTextField.Text.Split("/");
                    UrlTextField.Text = SocialNetworkData.SampleData()[2].ContactUrl + Constants.linkedInUrlPart + linkedIdArray[linkedIdArray.Length - 1];
                }
                else
                    UrlTextField.Text = SocialNetworkData.SampleData()[2].ContactUrl + Constants.linkedInUrlPart + UrlTextField.Text;
            }
            // Twitter.
            else if (urlRoot == SocialNetworkData.SampleData()[3].ContactUrl)
                UrlTextField.Text = SocialNetworkData.SampleData()[3].ContactUrl + UrlTextField.Text;
            // Vkontakte.
            else if (urlRoot == SocialNetworkData.SampleData()[4].ContactUrl)
                UrlTextField.Text = SocialNetworkData.SampleData()[4].ContactUrl + UrlTextField.Text;
            urlString = UrlTextField.Text;
            UpdateWebView();
        }

        private void UpdateWebView()
        {
            try
            {
                using (NSUrl url = new NSUrl(urlString))
                {
                    var request = new NSMutableUrlRequest(url);
                    webView.LoadRequest(request);
                }
            }
            catch
            {

            }
        }

        private async Task<bool> checkAccounts()
        {
            webView.Hidden = false;
            showBn.Hidden = true;
            string result = null;
            if (urlRoot == SocialNetworkData.SampleData()[4].ContactUrl)
                try
                {
                    result = await GetVkData();
                }
                catch
                {
                    if (!methods.IsConnected())
                        InvokeOnMainThread(() =>
                        {
                            NoConnectionViewController.view_controller_name = GetType().Name;
                            this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                            return;
                        });
                    return false;
                }
            //else if (urlRoot == SocialNetworkData.SampleData()[0].ContactUrl)
            //result = await GetFbData();
            if (!String.IsNullOrEmpty(result))
            {
                webView.Hidden = false;
                showBn.Hidden = true;
                urlString = result;
                UrlTextField.Text = urlString;
                UrlTextField.FloatLabelTop();
            }
            else
            {
                //showBn.Hidden = true;
                webView.Hidden = true;
                activityIndicator.Hidden = true;
                if (String.IsNullOrEmpty(UrlTextField.Text))
                    showBn.Hidden = true;
            }
            Dispose();
            return true;
        }

        async Task<string> GetVkData()
        {
            try
            {
                AppleVkService.thisNavigationController = this.NavigationController;
                if (/*String.IsNullOrEmpty(VkRetrievedData) && */String.IsNullOrEmpty(urlString))
                {
                    //VKSdk.Instance.RegisterDelegate(this.ViewController);
                    //VKSdk.Instance.UiDelegate = this.ViewController;

                    activityIndicator.Hidden = false;
                    CardsPCL.Models.LoginResult loginResult = null;
                    try
                    {
                        loginResult = await appleVkService.Login();
                    }
                    catch
                    {
                        if (!methods.IsConnected())
                            InvokeOnMainThread(() =>
                            {
                                NoConnectionViewController.view_controller_name = GetType().Name;
                                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                                return;
                            });
                        return null;
                    }
                    activityIndicator.Hidden = true;
                    if (loginResult.LoginState != LoginState.Canceled)
                        if (!String.IsNullOrEmpty(loginResult.UserId))
                        {
                            //VkRetrievedData
                            urlString = SocialNetworkData.SampleData()[4].ContactUrl + "id" + loginResult.UserId;
                            return urlString;//VkRetrievedData;
                        }
                }
                else
                    return urlString;//VkRetrievedData;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        //async Task<string> GetFbData()
        //{
        //    if (String.IsNullOrEmpty(FbRetrievedData) && String.IsNullOrEmpty(urlString))
        //    {
        //        try
        //        {
        //            var loginResult = await new AppleFacebookService().Login();
        //            if (loginResult.LoginState != LoginState.Canceled)
        //                if (!String.IsNullOrEmpty(loginResult.UserId))
        //                    FbRetrievedData = SocialNetworkData.SampleData()[0].ContactUrl +/*Constants.faceBookUrlPart +*/ loginResult.UserId;
        //            if (loginResult.LoginState == LoginState.UnexpectedCrashed)
        //            {
        //                //var res = await GetFbData();
        //                //FbRetrievedData = 
        //                return FbRetrievedData;
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    return FbRetrievedData;
        //}

        [Export("webView:didFinishNavigation:")]
        public void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            //urlString = webView.Url.ToString();
            //UrlTextField.Text = urlString;
            //UrlTextField.FloatLabelTop();
            Console.WriteLine("DidFinishNavigation");
        }

        [Export("webView:didFailNavigation:withError:")]
        public void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            // If navigation fails, this gets called
            Console.WriteLine("DidFailNavigation");
        }

        [Export("webView:didFailProvisionalNavigation:withError:")]
        public void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            // If navigation fails, this gets called
            Console.WriteLine("DidFailProvisionalNavigation");
        }

        [Export("webView:didStartProvisionalNavigation:")]
        public void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            // When navigation starts, this gets called
            Console.WriteLine("DidStartProvisionalNavigation");
        }
    }
}