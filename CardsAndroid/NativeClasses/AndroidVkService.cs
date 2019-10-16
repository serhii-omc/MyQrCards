using System;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using CardsPCL.Enums;
using CardsPCL.Interfaces;
using CardsPCL.Models;
using VKontakte;
using VKontakte.API;

namespace CardsAndroid.NativeClasses
{
    public class AndroidVkService : Java.Lang.Object, IVkService
    {
        //public static AndroidVkService Instance => DependencyService.Get<IVkService>() as AndroidVkService;
        public static Activity Context;
        readonly string[] _permissions = {
            VKScope.Email,
            VKScope.Offline
        };

        TaskCompletionSource<LoginResult> _completionSource;
        LoginResult _loginResult;

        public Task<LoginResult> Login()
        {
            _completionSource = new TaskCompletionSource<LoginResult>();
            //VKSdk.Login(Forms.Context as Activity, _permissions);
            VKSdk.Login(/*Application.Context*/Context as Activity, _permissions);
            return _completionSource.Task;
        }

        public void Logout()
        {
            _loginResult = null;
            _completionSource = null;
            VKSdk.Logout();
            Toast.MakeText(Context, "logout", ToastLength.Short).Show();
        }

        public void SetUserToken(VKAccessToken token)
        {
            Toast.MakeText(Context, "setusertoken", ToastLength.Short).Show();
            _loginResult = new LoginResult
            {
                Email = token.Email,
                Token = token.AccessToken,
                UserId = token.UserId,
                //ExpireAt = Utils.FromMsDateTime(token.ExpiresIn)
            };

            Task.Run(GetUserInfo);
        }

        async Task GetUserInfo()
        {
            Toast.MakeText(Context, "getuserinfo", ToastLength.Short).Show();
            var request = VKApi.Users.Get(VKParameters.From(VKApiConst.Fields, @"photo_400_orig,"));
            var response = await request.ExecuteAsync();
            var jsonArray = response.Json.OptJSONArray(@"response");
            var account = jsonArray?.GetJSONObject(0);
            if (account != null && _loginResult != null)
            {
                _loginResult.FirstName = account.OptString(@"first_name");
                _loginResult.LastName = account.OptString(@"last_name");
                _loginResult.ImageUrl = account.OptString(@"photo_400_orig");
                _loginResult.LoginState = LoginState.Success;
                SetResult(_loginResult);
            }
            else
                SetErrorResult(@"Unable to complete the request of user info");
        }

        public void SetErrorResult(string errorMessage)
        {
            Toast.MakeText(Context, "seterrorresult", ToastLength.Short).Show();
            SetResult(new LoginResult { LoginState = LoginState.Failed, ErrorString = errorMessage });
        }

        public void SetCanceledResult()
        {
            Toast.MakeText(Context, "setcanceledresult", ToastLength.Short).Show();
            SetResult(new LoginResult { LoginState = LoginState.Canceled });
        }

        void SetResult(LoginResult result)
        {
            Toast.MakeText(Context, "setresult", ToastLength.Short).Show();
            _completionSource?.TrySetResult(result);
            _loginResult = null;
            _completionSource = null;
        }
    }
}