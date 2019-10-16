using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using CoreGraphics;
using Foundation;
using Newtonsoft.Json;
using UIKit;
using ZXing.Mobile;

namespace CardsIOS
{
    public partial class LinkStickerViewController : UIViewController
    {
        MobileBarcodeScanner _scanner;
        MobileBarcodeScanningOptions _optionsCustom;
        DatabaseMethodsIOS _databaseMethods = new DatabaseMethodsIOS();
        CardLinks _cardLinks = new CardLinks();
        UIStoryboard sb = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        Methods methods = new Methods();
        string UDID;
        public static int CardId;

        public LinkStickerViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();

            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };

            linkStickerBn.TouchUpInside += async (s, e) =>
               {
                   _scanner = new MobileBarcodeScanner();
                   var customView = new UIView();
                   customView.Frame = View.Frame;
                   _scanner.UseCustomOverlay = true;
                   _scanner.CustomOverlay = customView;

                   var bottomView = new UIView { BackgroundColor = UIColor.FromRGB(36, 43, 52) };
                   customView.AddSubview(bottomView);
                   bottomView.Frame = new CGRect(0, customView.Frame.Height - 100, View.Frame.Width, 100);

                   var cancelBn = new UIButton();
                   cancelBn.SetTitle("Отмена", UIControlState.Normal);
                   cancelBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                   cancelBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
                   bottomView.AddSubview(cancelBn);
                   cancelBn.Frame = new CGRect(0, 0, View.Frame.Width / 3, 60);
                   cancelBn.TouchUpInside += (s1, e1) =>
                     {
                         _scanner.Cancel();
                     };

                   var flashBn = new UIButton();
                   flashBn.SetTitle("Вспышка", UIControlState.Normal);
                   flashBn.SetTitleColor(UIColor.White, UIControlState.Normal);
                   flashBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
                   bottomView.AddSubview(flashBn);
                   flashBn.Frame = new CGRect(View.Frame.Width - View.Frame.Width / 3, 0, View.Frame.Width / 3, 60);
                   flashBn.TouchUpInside += (s1, e1) =>
                   {
                       _scanner.Torch(!_scanner.IsTorchOn);
                   };

                   var shotRectangleIv = new UIImageView();
                   var rectangleSide = View.Frame.Width - View.Frame.Width / 3;
                   shotRectangleIv.Frame = new CGRect(View.Frame.Width / 6, (customView.Frame.Height - bottomView.Frame.Height) / 2 - rectangleSide / 2, rectangleSide, rectangleSide);
                   shotRectangleIv.Image = UIImage.FromBundle("scanner_rectangle.png");
                   customView.AddSubview(shotRectangleIv);

                   _optionsCustom = new MobileBarcodeScanningOptions();
                   _optionsCustom.AutoRotate = false;

                   await LaunchScanner();
               };

            orderStickerBn.TouchUpInside += (s, e) => OpenOrderLink();

            InitElements();
        }

        private void InitElements()
        {
            activityIndicator.Hidden = true;
            new AppDelegate().disableAllOrientation = true;
            View.BackgroundColor = UIColor.FromRGB(38, 46, 56);

            new AppDelegate().disableAllOrientation = true;
            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            bgImageView.Frame = View.Frame;
            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                backBn.Frame = new Rectangle(0, (Convert.ToInt32(View.Frame.Width) / 20) + 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width - View.Frame.Width / 2.5)), Convert.ToInt32(View.Frame.Width) / 18);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 6, Convert.ToInt32(View.Frame.Width) / 12, Convert.ToInt32(View.Frame.Width - View.Frame.Width / 3.5), Convert.ToInt32(View.Frame.Width) / 18);
            }
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Привязать наклейку с QR";
            headerLabel.TextAlignment = UITextAlignment.Center;
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);

            cardsLogo.Frame = new CGRect(Convert.ToInt32(View.Frame.Width) / 3,
                                            headerView.Frame.Y + headerView.Frame.Height + 20,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Text = "Делитесь визиткой" +
                              "\r\n" +
                              "не открывая приложения";

            mainTextTV.Font = UIFont.FromName(Constants.fira_sans, 22f);
            mainTextTV.Lines = 2;

            mainTextTV.Frame = new Rectangle(0, Convert.ToInt32(cardsLogo.Frame.Y) + Convert.ToInt32(cardsLogo.Frame.Height), Convert.ToInt32(View.Frame.Width), 60);
            mainTextTV.SizeToFit();
            mainTextTV.Frame = new CGRect(0, mainTextTV.Frame.Y + 20, View.Frame.Width, mainTextTV.Frame.Height);

            infoLabel.Lines = 8;
            infoLabel.Text = "1. Закажите специальный"
                + "\r\n" + "нестираемый QR-стикер"
                + "\r\n" +
                "\r\n" + "2. Привяжите к нему свою"
                + "\r\n" + "электронную визитку"
                 + "\r\n" +
                  "\r\n" +
                  "3. Наклейте стикер на чехол"
                + "\r\n" + "смартфона или ежедневник";
            infoLabel.TextAlignment = UITextAlignment.Left;

            orderStickerBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                         (Convert.ToInt32(View.Frame.Height) / 10) * 8,
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);

            infoLabel.Font = UIFont.FromName(Constants.fira_sans, 18f);
            infoLabel.Frame = new CGRect(0, Convert.ToInt32(mainTextTV.Frame.Y) + mainTextTV.Frame.Height, Convert.ToInt32(View.Frame.Width), 200);
            infoLabel.SizeToFit();
            infoLabel.Frame = new CGRect((View.Frame.Width - infoLabel.Frame.Width) / 2, Convert.ToInt32(mainTextTV.Frame.Y) + mainTextTV.Frame.Height, Convert.ToInt32(infoLabel.Frame.Width + 5), orderStickerBn.Frame.Y - (mainTextTV.Frame.Y + mainTextTV.Frame.Height));


            linkStickerBn.SetTitle("ПРИВЯЗАТЬ НАКЛЕЙКУ С QR", UIControlState.Normal);
            linkStickerBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 15,
                                                 (int)(orderStickerBn.Frame.Y + orderStickerBn.Frame.Height + 15),
                                         Convert.ToInt32(View.Frame.Width) - ((Convert.ToInt32(View.Frame.Width) / 15) * 2),
                                         Convert.ToInt32(View.Frame.Height) / 12);
            linkStickerBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            linkStickerBn.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
            linkStickerBn.Layer.BorderWidth = 1f;
            orderStickerBn.Font = UIFont.FromName(Constants.fira_sans, 15f);
            orderStickerBn.BackgroundColor = UIColor.FromRGB(255, 99, 62);

            linkStickerBn.Hidden = false;
            orderStickerBn.SetTitle("ЗАКАЗАТЬ НАКЛЕЙКУ С QR", UIControlState.Normal);
            cardsLogo.Image = UIImage.FromBundle("link_sticker_img");
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
        }


        private async Task LaunchScanner()
        {
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
                    CallAlert("Данный QR-код не может быть использован в качестве электронной визитки");
                    return;
                }
                if (!result.Text.ToLower().Contains("card.myqrcards.com/links/"))
                {
                    CallAlert("Данный QR-код не может быть использован в качестве электронной визитки");
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
                    linkStickerBn.Hidden = true;
                    orderStickerBn.Hidden = true;
                    activityIndicator.Hidden = false;
                    res = await _cardLinks.CardsLinksGet(_databaseMethods.GetAccessJwt(), UDID, cardLinkID);
                    linkStickerBn.Hidden = false;
                    orderStickerBn.Hidden = false;
                    activityIndicator.Hidden = true;

                    if (res?.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        CallAlert("Данный QR-код не может быть использован в качестве электронной визитки");
                        return;
                    }
                    if (res?.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        CallAlert("Данный QR-код не может быть использован в качестве электронной визитки");
                    }
                    if (res?.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        CallAlert("QR-код уже занят другим пользователем", orderButtonShown: false);
                    }
                    if (res?.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await res.Content.ReadAsStringAsync();
                        var deserialized = JsonConvert.DeserializeObject<CardLinkModel>(content);
                        if (CardId == deserialized?.card?.id)
                            CallAlert("QR-код уже привязан к данной визитке", orderButtonShown: false);
                        if (CardId != deserialized?.card?.id && deserialized?.isDefault == false)
                            CallAlert($"Данный QR-код уже привязан к визитке {deserialized?.card?.name}. Перепривязать?", cardLinkID);
                        if (deserialized?.isDefault == false)
                            CallAlert("Невозможно перепривязать основной QR-код визитки");
                    }
                    if (res?.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        await LinkCard(cardLinkID);
                    }
                }
                catch (Exception ex)
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
                }
            }
            else
                return;
        }

        private async Task LinkCard(string cardLinkID)
        {
            try
            {
                linkStickerBn.Hidden = true;
                orderStickerBn.Hidden = true;
                activityIndicator.Hidden = false;
                var linkres = await _cardLinks.LinkCard(_databaseMethods.GetAccessJwt(), UDID, CardId.ToString(), cardLinkID);
                if (linkres.ToLower().Contains(Constants.status_code202))
                    CallAlert("QR-код успешно привязан", linkedSuccessfully: true, orderButtonShown: false);
                else
                    CallAlert("Невозможно привязать этот QR-код к данной визитке");
                linkStickerBn.Hidden = false;
                orderStickerBn.Hidden = false;
                activityIndicator.Hidden = true;
            }
            catch (Exception ex)
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
                else
                    CallAlert("Невозможно привязать этот QR-код к данной визитке");
                linkStickerBn.Hidden = false;
                orderStickerBn.Hidden = false;
                activityIndicator.Hidden = true;
            }
        }

        void CallAlert(string title, string cardLinkId = null, bool orderButtonShown = true, bool linkedSuccessfully = false)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(async () =>
            {
                var alertController = UIAlertController.Create(title, null, UIAlertControllerStyle.Alert);

                if (cardLinkId != null)
                {
                    alertController.AddAction(UIAlertAction.Create("Да", UIAlertActionStyle.Default, async (action) => await LinkCard(cardLinkId)));
                    alertController.AddAction(UIAlertAction.Create("Нет", UIAlertActionStyle.Default, null));
                }
                else
                {
                    alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, (action) =>
                    {
                        if (linkedSuccessfully)
                            NavigationController.PopViewController(true);
                    }));
                    if (orderButtonShown)
                        alertController.AddAction(UIAlertAction.Create("Заказать наклейку с QR", UIAlertActionStyle.Default, (action) => OpenOrderLink()));
                }

                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
            });
        }

        private void OpenOrderLink()
        {
            NSString urlString = new NSString("https://myqrcards.com/sticker");
            NSUrl myFileUrl = new NSUrl(urlString);
            UIApplication.SharedApplication.OpenUrl(myFileUrl);
        }
    }
}