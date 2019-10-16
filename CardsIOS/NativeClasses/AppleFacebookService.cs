//using System;
//using System.Threading.Tasks;
//using CardsPCL.Enums;
//using CardsPCL.Interfaces;
//using CardsPCL.Models;
//using CoreGraphics;
////using Facebook.CoreKit;
////using Facebook.LoginKit;
//using Foundation;
//using UIKit;

//namespace CardsIOS.NativeClasses
//{
//    public class AppleFacebookService : IFacebookService
//    {
//        //readonly LoginManager _loginManager = new LoginManager();
//        readonly string[] _permissions = { @"public_profile", @"email"/*, @"user_about_me"*/ };

//        LoginResult _loginResult;
//        TaskCompletionSource<LoginResult> _completionSource;

//        public Task<LoginResult> Login()
//        {
//            _completionSource = new TaskCompletionSource<LoginResult>();
//            //_loginManager.LogInWithReadPermissions(_permissions, GetCurrentViewController(), LoginManagerLoginHandler);
//            return _completionSource.Task;
//        }

//        public void Logout()
//        {
//            //_loginManager.LogOut();
//        }

//        void LoginManagerLoginHandler(LoginManagerLoginResult result, NSError error)
//        {
//            if(result==null)
//            {
//                //_completionSource.TrySetResult(new LoginResult { LoginState = LoginState.UnexpectedCrashed, ErrorString = error.LocalizedDescription });
//                _completionSource.TrySetResult(new LoginResult { LoginState = LoginState.Canceled, ErrorString = error.LocalizedDescription });
//                return;
//            }
//            if (result.IsCancelled)
//                _completionSource.TrySetResult(new LoginResult { LoginState = LoginState.Canceled });
//            else if (error != null)
//                _completionSource.TrySetResult(new LoginResult { LoginState = LoginState.Failed, ErrorString = error.LocalizedDescription });
//            else
//            {
//                _loginResult = new LoginResult
//                {
//                    Token = result.Token.TokenString,
//                    UserId = result.Token.UserId,
//                    //ExpireAt = result.Token.ExpirationDate.ToDateTime()
//                    ExpireAt = (DateTime)result.Token.ExpirationDate
//                };

//                var request = new GraphRequest(@"me", new NSDictionary(@"fields", @"email"));
//                request.Start(GetEmailRequestHandler);
//            }
//        }

//        void GetEmailRequestHandler(GraphRequestConnection connection, NSObject result, NSError error)
//        {
//            if (error != null)
//                _completionSource.TrySetResult(new LoginResult { LoginState = LoginState.Failed, ErrorString = error.LocalizedDescription });
//            else
//            {
//                _loginResult.FirstName = Profile.CurrentProfile.FirstName;
//                _loginResult.LastName = Profile.CurrentProfile.LastName;
//                _loginResult.ImageUrl = Profile.CurrentProfile.ImageUrl(ProfilePictureMode.Square, new CGSize()).ToString();

//                var dict = result as NSDictionary;
//                var emailKey = new NSString(@"email");
//                if (dict != null && dict.ContainsKey(emailKey))
//                    _loginResult.Email = dict[emailKey]?.ToString();

//                _loginResult.LoginState = LoginState.Success;
//                _completionSource.TrySetResult(_loginResult);
//            }
//        }

//        static UIViewController GetCurrentViewController()
//        {
//            var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
//            while (viewController.PresentedViewController != null)
//                viewController = viewController.PresentedViewController;
//            return viewController;
//        }
//    }
//}
