using System;
using System.Drawing;
using System.Text;
using CardsIOS.NativeClasses;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using CoreGraphics;
using Foundation;
using Newtonsoft.Json;
using Plugin.InAppBilling.Abstractions;
using UIKit;

namespace CardsIOS
{
    public partial class PremiumViewController : UIViewController//, Iina
    {
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        DateTime? maxValTill = null;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Methods methods = new Methods();
        CardsPCL.CommonMethods.Accounts accounts = new CardsPCL.CommonMethods.Accounts();
        string UDID;
        public PremiumViewController(IntPtr handle) : base(handle)
        {

        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
            InitElements();

            if (databaseMethods.userExists())
            {
                if (!methods.IsConnected())
                {
                    InvokeOnMainThread(() =>
                    {
                        NoConnectionViewController.view_controller_name = GetType().Name;
                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                        return;
                    });
                    return;
                }
                InvokeInBackground(async () =>
                {
                    try
                    {
                        var purchase_info = await accounts.AccountAuthorize(databaseMethods.GetAccessJwt(), UDID);
                        AuthorizeAfterPurchase(purchase_info);
                    }
                    catch
                    {
                        InvokeOnMainThread(() =>
                        {
                            NoConnectionViewController.view_controller_name = GetType().Name;
                            this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                            return;
                        });
                        try { return; } catch { }
                    }
                });
            }

            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };

            month_bn.TouchUpInside += (s, e) =>
            {
                if (!methods.IsConnected())
                {
                    NoConnectionViewController.view_controller_name = GetType().Name;
                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                    return;
                }
                BuySubs("com.myqrcards.iphone.month_auto_subscription");
            };
            year_bn.TouchUpInside += (s, e) =>
            {
                if (!methods.IsConnected())
                {
                    NoConnectionViewController.view_controller_name = GetType().Name;
                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                    return;
                }
                BuySubs("com.myqrcards.iphone.year_auto_subscription");
            };
            termsOfUseBn.TouchUpInside += (s, e) =>
              {
                  this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(/* LicenseViewController*/AboutAppViewController)), false);
              };
            privacyPolicyBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(PrivacyPolicyViewController)), false);
            };
        }

        private void BuySubs(string product_id)
        {
            if (!databaseMethods.userExists())
            {
                call_login_menu();
                return;
            }
            activityIndicator.Hidden = false;
            month_bn.Hidden = true;
            month_subscriptionTV.Hidden = true;
            year_bn.Hidden = true;
            year_subscriptionTV.Hidden = true;
            InvokeInBackground(async () =>
            {
                InAppBillingService inAppBillingService = new InAppBillingService();
                //var gkgj = await inAppBillingService.PurchaseItemAsync();
                //var detais = await inAppBillingService.GetProductDetails(product_id);
                //var was_purchased_before = await inAppBillingService.WasItemPurchasedAsync(product_id);
                InAppBillingPurchase purchase = new InAppBillingPurchase();
                //if (!was_purchased_before)
                {
                    InvokeOnMainThread(() =>
                    {
                        activityIndicator.Hidden = false;
                        month_bn.Hidden = true;
                        month_subscriptionTV.Hidden = true;
                        InvokeInBackground(async () =>
                        {
                            string authorize_check = null;
                            try
                            {
                                authorize_check = await accounts.AccountAuthorize(databaseMethods.GetAccessJwt(), UDID);
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
                            if (/*res_card_data == Constants.status_code409 ||*/ authorize_check == Constants.status_code401)
                            {
                                InvokeOnMainThread(() =>
                                {
                                    ShowSeveralDevicesRestriction();
                                    return;
                                });
                                return;
                            }
                            var deserialized = JsonConvert.DeserializeObject<AuthorizeRootObject>(authorize_check);
                            if (deserialized != null)
                            {
                                var jsonObj = JsonConvert.SerializeObject(new { accountID = deserialized.accountID.ToString() });
                                var textBytes = Encoding.UTF8.GetBytes(jsonObj);
                                var base64 = Convert.ToBase64String(textBytes);

                                if (deserialized.accountID != null)
                                    purchase = await inAppBillingService.PurchaseSubscription(product_id, base64);
                            }
                            //else
                            //purchase = await inAppBillingService.PurchaseSubscription(product_id, String.Empty);
                            if (purchase != null)
                            {
                                InvokeOnMainThread(() =>
                                {
                                    ShowAttentionSyncWaiting();
                                });
                                // Inform our server.
                                var UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
                                string authorize_check_after_purchase = null;
                                try
                                {
                                    var notify_server = await accounts.AccountSubscribe(
                                                    databaseMethods.GetAccessJwt(),
                                                    Constants.personalApple.ToString(),
                                                    purchase,
                                                    DateTime.UtcNow.AddDays(1), // ATTENTION with days.
                                                    UDID);

                                    authorize_check_after_purchase = await accounts.AccountAuthorize(databaseMethods.GetAccessJwt(), UDID);
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
                                InvokeOnMainThread(() =>
                                {
                                    AuthorizeAfterPurchase(authorize_check_after_purchase);
                                    ShowSyncSuccessful();
                                });
                            }

                            InvokeOnMainThread(() =>
                            {
                                activityIndicator.Hidden = true;
                                month_bn.Hidden = false;
                                month_subscriptionTV.Hidden = false;
                                year_bn.Hidden = false;
                                year_subscriptionTV.Hidden = false;
                            });
                        });
                    });
                }
            });
        }



        private void ShowSyncSuccessful()
        {
            UIAlertView alert = new UIAlertView()
            {
                Title = "Покупка проведена",
                Message = "Синхронизация успешно завершена."
            };
            alert.AddButton("OK");
            alert.Show();
            InitElements();
        }

        private void ShowAttentionSyncWaiting()
        {
            ratesTV.Hidden = true;
            premium_advantagesTV.TextColor = UIColor.Red;
            premium_detailsTV.TextColor = UIColor.Red;
            premium_advantagesTV.Text = "Внимание!";
            premium_detailsTV.Text = "Дождитесь синхронизации с сервером!";
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = true;
            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            //image_bgIV.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height));
            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Premium";
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            premium_advantagesTV.Frame = new Rectangle((int)(View.Frame.Width / 17), (int)(headerView.Frame.Height + headerView.Frame.Y + 10), (int)View.Frame.Width, 25);
            premium_advantagesTV.TextColor = UIColor.White;
            premium_advantagesTV.Text = "Преимущества Premium";
            premium_detailsTV.Frame = new Rectangle(
                (int)(View.Frame.Width / 17),
                (int)(premium_advantagesTV.Frame.Y + premium_advantagesTV.Frame.Height + 12),
                (int)(View.Frame.Width - View.Frame.Width / 17),
                (int)(View.Frame.Height / 3));
            premium_detailsTV.TextColor = UIColor.FromRGB(190, 192, 195);
            premium_detailsTV.Text =
                "1. Возможность добавить логотип вашей компании в центр QR-кода визитки" + "\r\n" +
                "2. Возможность создать несколько визиток для всех видов вашей деятельности" + "\r\n" +
                "3. Облачное хранение визиток – используйте созданную визитку на всех ваших устройствах" + "\r\n" +
                "4. Доступны дополнительные поля – максимум информации о вас и компании" + "\r\n" +
                "5. Отсутствие рекламы" + "\r\n" +
                Constants.generalLicenseRu;

            termsOfUseBn.Frame = new CGRect(View.Frame.Width / 17,
                                            premium_detailsTV.Frame.Y + premium_detailsTV.Frame.Height + 7,
                                            View.Frame.Width / 2 - View.Frame.Width / 17 - View.Frame.Width / 34,
                                            50);
            termsOfUseBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            termsOfUseBn.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            termsOfUseBn.TitleLabel.TextAlignment = UITextAlignment.Center;
            termsOfUseBn.SetTitle("Условия\nиспользования", UIControlState.Normal);
            privacyPolicyBn.Frame = new CGRect(termsOfUseBn.Frame.X + termsOfUseBn.Frame.Width + View.Frame.Width / 17,
                                                termsOfUseBn.Frame.Y,
                                                termsOfUseBn.Frame.Width,
                                                termsOfUseBn.Frame.Height);
            privacyPolicyBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            privacyPolicyBn.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            privacyPolicyBn.TitleLabel.TextAlignment = UITextAlignment.Center;
            privacyPolicyBn.SetTitle("Политика\nконфиденциальности", UIControlState.Normal);

            ratesTV.Frame = new Rectangle((int)(View.Frame.Width / 17), (int)(View.Frame.Height - View.Frame.Height / 2 + 100), (int)View.Frame.Width, 25);
            subscriptionTillTV.Frame = new Rectangle((int)(View.Frame.Width / 17), (int)(View.Frame.Height - View.Frame.Height / 2 + 100), (int)View.Frame.Width - (int)(View.Frame.Width / 17) * 2, 100);
            ratesTV.Text = "Тарифы";
            ratesTV.Hidden = false;
            month_subscriptionTV.Frame = new Rectangle((int)(View.Frame.Width / 17), (int)(ratesTV.Frame.Height + ratesTV.Frame.Y + 10), (int)View.Frame.Width / 2 - 10, 50);
            month_subscriptionTV.Text = "Ежемесячная" + "\r\n" +
                "подписка";
            year_subscriptionTV.Frame = new Rectangle((int)(View.Frame.Width / 17), (int)(month_subscriptionTV.Frame.Height + month_subscriptionTV.Frame.Y + 10), (int)View.Frame.Width / 2 - 10, 50);
            year_subscriptionTV.Text = "Ежегодная" + "\r\n" +
                "подписка";
            month_bn.Frame = new Rectangle((int)(View.Frame.Width / 2 + View.Frame.Width / 17), (int)(ratesTV.Frame.Height + ratesTV.Frame.Y + 20), (int)View.Frame.Width / 2 - (int)(View.Frame.Width / 17 * 2), 40);
            year_bn.Frame = new Rectangle((int)(View.Frame.Width / 2 + View.Frame.Width / 17), (int)(month_subscriptionTV.Frame.Height + month_subscriptionTV.Frame.Y + 20), (int)View.Frame.Width / 2 - (int)(View.Frame.Width / 17 * 2), 40);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            activityIndicator.Hidden = true;

            month_bn.SetTitle("29 \u20BD", UIControlState.Normal);
            year_bn.SetTitle("299 \u20BD", UIControlState.Normal);
            month_bn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            year_bn.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            //year_bn.Hidden = true;
            //year_subscription.Hidden = true;

            premium_advantagesTV.Font = UIFont.FromName(Constants.fira_sans, 23f);
            subscriptionTillTV.Font = UIFont.FromName(Constants.fira_sans, 23f);
            ratesTV.Font = UIFont.FromName(Constants.fira_sans, 23f);
            premium_detailsTV.Font = UIFont.FromName(Constants.fira_sans, 17f);
            month_subscriptionTV.Font = UIFont.FromName(Constants.fira_sans, 17f);
            year_subscriptionTV.Font = UIFont.FromName(Constants.fira_sans, 17f);
            termsOfUseBn.Font = UIFont.FromName(Constants.fira_sans, 12f);
            privacyPolicyBn.Font = UIFont.FromName(Constants.fira_sans, 12f);
            month_bn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            year_bn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            subscriptionTillTV.Hidden = true;
            if (databaseMethods.userExists())
                HideBuyElements();
        }

        void call_login_menu()
        {
            string[] constraintItems = new string[] { "Подробнее о Premium" };

            if (!databaseMethods.userExists())
                constraintItems = new string[] { "Войти в учетную запись" };
            var option_const = new UIActionSheet(null, null, "Отменить", null, constraintItems);
            option_const.Title = "Для прибретения Premium-подписки необходимо войти в свою учетную запись";
            option_const.Clicked += (btn_sender, args) =>
            {
                if (args.ButtonIndex == 0)
                {
                    NavigationController.PushViewController(sb.InstantiateViewController(nameof(EmailViewControllerNew)), true);
                }
            };
            option_const.ShowInView(View);
        }
        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }

        void AuthorizeAfterPurchase(string cards_remainingcontent)
        {
            var des_auth = JsonConvert.DeserializeObject<AuthorizeRootObject>(cards_remainingcontent);
            try
            {
                try
                {
                    foreach (var subscription in des_auth.subscriptions)
                    {
                        if (subscription.isTrial)
                        {
                            HideSubscriptionTill();
                            break;
                        }
                        if (subscription.id != 1)
                        {
                            if (maxValTill != null)
                            {
                                var res = DateTime.Compare(subscription.validTill, (DateTime)maxValTill);
                                if (res < 0)
                                    maxValTill = subscription.validTill;
                            }
                            else
                                maxValTill = subscription.validTill;
                        }
                        InvokeOnMainThread(() =>
                        {
                            if (maxValTill != null)
                                ShowSubscriptionTill();
                            else
                                ShowBuyElements();
                        });
                    }
                }
                catch (Exception ex)
                {
                }
                foreach (var subs in des_auth.subscriptions)
                {
                    if (subs.limitations != null)
                        if (subs.limitations.allowMultiClients)
                        {
                            QRViewController.is_premium = true;
                            break;
                        }
                }
                //if (!is_premium)
                foreach (var subscription in des_auth.subscriptions)
                {
                    if (subscription.limitations?.cardsRemaining == null)
                    {
                        QRViewController.cards_remaining = 10;
                        break;
                    }
                    else
                    {
                        if (subscription.limitations != null)
                            if (subscription.limitations.cardsRemaining > QRViewController.cards_remaining)
                                QRViewController.cards_remaining = subscription.limitations.cardsRemaining.Value;
                    }
                }
                try
                {
                    NativeMethods.ClearFeatures();
                    foreach (var subscription in des_auth.subscriptions)
                    {
                        if (subscription.features != null)
                        {
                            foreach (var feature in subscription.features)
                            {
                                if (String.IsNullOrEmpty(QRViewController.ExtraEmploymentData))
                                    if (feature == Constants.ExtraEmploymentData)
                                        QRViewController.ExtraEmploymentData = feature;
                                if (String.IsNullOrEmpty(QRViewController.CompanyLogoInQr))
                                    if (feature == Constants.CompanyLogoInQr)
                                        QRViewController.CompanyLogoInQr = feature;
                                if (String.IsNullOrEmpty(QRViewController.ExtraPersonData))
                                    if (feature == Constants.ExtraPersonData)
                                        QRViewController.ExtraPersonData = feature;
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
            subscriptionTillTV.Hidden = false;
            var dateTill = maxValTill?.ToLocalTime().Date.ToString().Split(' ')[0].Replace('/', '.');//.Substring(0, 10);
            subscriptionTillTV.Text = $"{"Подписка оплачена до:"} {dateTill}";
            HideBuyElements();
        }
        void HideSubscriptionTill()
        {
            subscriptionTillTV.Hidden = true;
            ShowBuyElements();
        }
        void ShowBuyElements()
        {
            ratesTV.Hidden = false;
            month_bn.Hidden = false;
            month_subscriptionTV.Hidden = false;
            year_bn.Hidden = false;
            year_subscriptionTV.Hidden = false;
        }
        void HideBuyElements()
        {
            ratesTV.Hidden = true;
            month_bn.Hidden = true;
            month_subscriptionTV.Hidden = true;
            year_bn.Hidden = true;
            year_subscriptionTV.Hidden = true;
        }
    }
}
