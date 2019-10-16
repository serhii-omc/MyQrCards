using System;
using System.Threading.Tasks;
using CardsPCL.Enums;
using CardsPCL.Interfaces;
using CardsPCL.Models;
using Foundation;
using UIKit;
using VKontakte;
using VKontakte.API;
using VKontakte.API.Methods;
using VKontakte.API.Models;
using VKontakte.Core;
using VKontakte.Views;
namespace CardsIOS.NativeClasses
{
    public class AppleVkService: NSObject, IVkService, IVKSdkDelegate, IVKSdkUIDelegate
    {
        public static UINavigationController thisNavigationController;
        readonly string[] _permissions = {
            VKPermissions.Email,
            VKPermissions.Offline
        };

        LoginResult _loginResult;
        TaskCompletionSource<LoginResult> _completionSource;

        public AppleVkService()
        {
            //_completionSource = null;

            VKSdk.ForceLogout();
            VKSdk.Instance.RegisterDelegate(this);
            VKSdk.Instance.UiDelegate = this;
        }

        public Task<LoginResult> Login()
        {
            _completionSource = new TaskCompletionSource<LoginResult>();
            VKSdk.Authorize(_permissions);
            return _completionSource.Task;
        }

        public void Logout()
        {
            _loginResult = null;
            _completionSource = null;
        }

        [Export("vkSdkTokenHasExpired:")]
        public void TokenHasExpired(VKAccessToken expiredToken)
        {
            VKSdk.Authorize(_permissions);
        }

        public new void Dispose()
        {
            VKSdk.Instance.UnregisterDelegate(this);
            VKSdk.Instance.UiDelegate = null;
            SetCancelledResult();
        }

        public void AccessAuthorizationFinished(VKAuthorizationResult result)
        {
            if (result?.Token == null)
                SetErrorResult(result?.Error?.LocalizedDescription ?? @"VK authorization unknown error");
            else
            {
                _loginResult = new LoginResult
                {
                    Token = result.Token.AccessToken,
                    UserId = result.Token.UserId,
                    Email = result.Token.Email,
                    //ExpireAt = Utils.FromMsDateTime(result.Token.ExpiresIn),
                };
                Task.Run(GetUserInfo);
            }
        }

        async Task GetUserInfo()
        {
            var request = VKApi.Users.Get(NSDictionary.FromObjectAndKey((NSString)@"photo_400_orig", VKApiConst.Fields));
            var response = await request.ExecuteAsync();
            var users = response.ParsedModel as VKUsersArray;
            var account = users?.FirstObject as VKUser;
            if (account != null && _loginResult != null)
            {
                _loginResult.FirstName = account.first_name;
                _loginResult.LastName = account.last_name;
                _loginResult.ImageUrl = account.photo_400_orig;
                _loginResult.LoginState = LoginState.Success;
                SetResult(_loginResult);
            }
            else
                SetErrorResult(@"Unable to complete the request of user info");
        }

        public void UserAuthorizationFailed()
        {
            SetErrorResult(@"VK authorization unknown error");
        }

        public void ShouldPresentViewController(UIViewController controller)
        {
            //thisNavigationController.TopViewController.PresentViewController(controller, true, null);
            //thisNavigationController.ModalViewController.PresentViewController(controller, true, null);
            //thisNavigationController.PushViewController(controller, true);
            //controller.
            //NSString urlString = new NSString(des_card_data.url);
            //NSUrl myFileUrl = new NSUrl(urlString);
            //UIApplication.SharedApplication.OpenUrl(myFileUrl);
        }

        public void NeedCaptchaEnter(VKError captchaError)
        {
            var vc = VKCaptchaViewController.Create(captchaError);
            vc.PresentIn(thisNavigationController.TopViewController);
        }

        void SetCancelledResult()
        {
            SetResult(new LoginResult { LoginState = LoginState.Canceled });
        }

        void SetErrorResult(string errorString)
        {
            SetResult(new LoginResult { LoginState = LoginState.Failed, ErrorString = errorString });
        }

        void SetResult(LoginResult result)
        {
            _completionSource?.TrySetResult(result);
            _loginResult = null;
            _completionSource = null;
        }
    }
}