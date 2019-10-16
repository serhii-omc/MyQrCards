using System;
using System.Drawing;
using System.Json;
using CardsIOS.NativeClasses;
using CardsPCL;
using CardsPCL.Database;
using SidebarNavigation;
using UIKit;
using Xamarin.Auth;

namespace CardsIOS
{
    public partial class ViewController : UIViewController
    {
        System.Timers.Timer timer;
        public SidebarController SidebarController { get; set; }
        public static UINavigationController navigationController;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            new AppDelegate().disableAllOrientation = true;
            navigationController = this.NavigationController;
            this.NavigationController.NavigationBarHidden = true;

            //facebook
            //try
            //{
            //    var loginResult = await new AppleFacebookService().Login();
            //    if (loginResult.LoginState.ToString() != Constants.canceled)
            //    {
            //        string result = "fail";
            //        var user_id = loginResult.UserId;
            //        if (String.IsNullOrEmpty(user_id))
            //            result = "fail";
            //        else
            //            result = "success";
            //    }
            //    int k = 87;
            //}
            //catch (Exception ex)
            //{

            //}

            //vkontakte
            //try
            //{
            //    AppleVkService.thisNavigationController = this.NavigationController;
            //    var loginResult = await new AppleVkService().Login();
            //    if (loginResult.LoginState.ToString() != Constants.canceled)
            //    {
            //        string result= "fail";
            //        var user_id = loginResult.UserId;
            //        if (String.IsNullOrEmpty(user_id))
            //            result = "fail";
            //        else
            //            result = "success";

            //    }
            //    int k = 87;
            //}
            //catch (Exception ex)
            //{

            //}

            //oauth2 = new Xamarin.Auth.Helpers.OAuth2()
            //{
            //    Description = "Instagram OAuth2",
            //    OAuth_IdApplication_IdAPI_KeyAPI_IdClient_IdCustomer = "",
            //    OAuth2_Scope = "basic",
            //    OAuth_UriAuthorization = new Uri("https://api.instagram.com/oauth/authorize/"),
            //    OAuth_UriCallbackAKARedirect = new Uri("http://xamarin.com"),
            //    AllowCancel = true,
            //};

            //if (TestCases.ContainsKey(oauth2.Description))
            //{
            //    TestCases[oauth2.Description] = oauth2;
            //}
            //else
            //{
            //    TestCases.Add(oauth2.Description, oauth2);
            //}

            //linkedin
            //var auth = new OAuth2Authenticator(
            //     clientId: "77czt68vpsssvi",
            //     clientSecret: "twNG31mvP7dhuQKJ",
            //     scope: "r_basicprofile",
            //     authorizeUrl: new Uri("https://www.linkedin.com/uas/oauth2/authorization"),
            //     redirectUrl: new Uri("https://www.youtube.com/c/HoussemDellai"),
            //     accessTokenUrl: new Uri("https://www.linkedin.com/uas/oauth2/accessToken"));

            //var ui = auth.GetUI();

            //auth.Completed += LinkedinAuth_Completed;
            ////auth.AllowCancel = false;
            //PresentViewController(ui, true, null);

            ////UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(auth.GetUI(), true, null);
            //auth.ShowErrors = false;
            //auth.Error += (sender, eventArgs) =>
            //{
            //    OAuth2Authenticator auth2 = (OAuth2Authenticator)sender;
            //    //auth2.ShowErrors = false;
            //    auth2.OnCancelled();
            //};

            //instagram
            //var auth = new OAuth2Authenticator(
            //     clientId: "77czt68vpsssvi",
            //     clientSecret: "twNG31mvP7dhuQKJ",
            //     scope: "r_basicprofile",
            //     authorizeUrl: new Uri("https://api.instagram.com/oauth/authorize/"),
            //     redirectUrl: new Uri("https://www.youtube.com/c/HoussemDellai"),
            //     accessTokenUrl: new Uri("https://api.instagram.com/oauth2/accessToken"));

            //var ui = auth.GetUI();

            //auth.Completed += LinkedinAuth_Completed;
            ////auth.AllowCancel = false;
            //PresentViewController(ui, true, null);

            ////UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(auth.GetUI(), true, null);
            ////auth.ShowErrors = false;
            //auth.Error += (sender, eventArgs) =>
            //{
            //    OAuth2Authenticator auth2 = (OAuth2Authenticator)sender;
            //    //auth2.ShowErrors = false;
            //    auth2.OnCancelled();
            //};


            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            // Perform any additional setup after loading the view, typically from a nib.
            splashIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Height) / 3, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 2);
        }

        //async void LinkedinAuth_Completed(object sender, AuthenticatorCompletedEventArgs e)
        //{
        //    if(e.IsAuthenticated)
        //    {
        //        if (e.IsAuthenticated)
        //        {
        //            var request = new OAuth2Request(
        //                "GET",
        //                new Uri("https://api.linkedin.com/v1/people/~:(id,firstName,lastName,headline,picture-url,summary,educations,three-current-positions,honors-awards,site-standard-profile-request,location,api-standard-profile-request,phone-numbers)?"
        //                      + "format=json"
        //                      + "&oauth2_access_token="
        //                      + e.Account.Properties["access_token"]),
        //                null,
        //                e.Account);

        //            var linkedInResponse = await request.GetResponseAsync();
        //            var json = linkedInResponse.GetResponseText();
        //            var linkedInUser = JsonValue.Parse(json);

        //            //var name = linkedInUser["firstName"] + " " + linkedInUser["lastName"];
        //            //var id = linkedInUser["id"];
        //            //var description = linkedInUser["headline"];
        //            //var picture = linkedInUser["pictureUrl"];

        //            var siteStandardProfileRequest = linkedInUser["siteStandardProfileRequest"];

        //            //var fsdsfd = name;
        //            //var fdfdf = id;
        //            //var ffsdf = description;

        //        }
        //    }
        //    else
        //    {

        //    }
        //    DismissViewController(true, null);
        //}

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            var sb = UIStoryboard.FromName("Main", null);
            var vc = sb.InstantiateViewController(nameof(OnBoarding1ViewController));//"QRViewController");//"RootMain_SyncViewController");//
            if (databaseMethods.userExists())
                vc = sb.InstantiateViewController(nameof(RootQRViewController));
            timer = new System.Timers.Timer();
            timer.Interval = 300;
            timer.Elapsed += delegate
            {
                timer.Stop();
                timer.Dispose();
                InvokeOnMainThread(() => { this.NavigationController.PushViewController(vc, true); });
            };
            timer.Start();
        }
        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
