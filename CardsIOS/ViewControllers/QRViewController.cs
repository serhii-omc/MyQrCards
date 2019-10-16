using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CardsIOS.Models;
using CardsIOS.NativeClasses;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using CoreGraphics;
using Foundation;
using Newtonsoft.Json;
using SidebarNavigation;
using TTGSnackBar;
using UIKit;
using ZXing;
using ZXing.Mobile;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace CardsIOS
{
    public partial class QRViewController : UIViewController
    {
        System.Timers.Timer connectionWaitingTimer;
        UIImage logoImageDemonstr;
        List<CardsIOS.Models.QR_Logo_ID_Model> qR_Logo_ID_List;
        Cards cards = new Cards();
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
        Attachments attachments = new Attachments();
        Companies companies = new Companies();
        UIButton QR_button = new UIButton();
        NativeMethods nativeMethods = new NativeMethods();
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        //static UIButton current_qrBn;
        List<QRListModel> deserialized_cards_list;
        List<QRListModel> reverseList;
        List<QRListModel> qrsList = new List<QRListModel>();
        int global_tag, cards_count;
        public static List<string> card_names;
        Methods methods = new Methods();
        UIActionSheet option;
        CardsPCL.CommonMethods.Accounts accounts = new CardsPCL.CommonMethods.Accounts();
        public static string just_created_card_name { get; set; }
        public static bool is_premium;
        public static bool ShowWeHaveNotDownloadedCard;
        bool ShowWeHaveNotDownloadedCardScrollDraggingInformer;
        public static int cards_remaining;
        public static string current_card_name_header { get; set; }
        static string edit_delete_indicator = "";
        int index;
        public static CGPoint content_offset;
        //public SidebarController SidebarController { get; private set; }
        public SidebarController SideBarController;
        public UIViewController holderVC;
        IntPtr handle;
        int count_cached = 0;
        FileInfo[] cached_files_array;
        //features
        public static string ExtraEmploymentData, CompanyLogoInQr, ExtraPersonData;
        private readonly string cacheMessage = "Дождитесь кэширования\nв файловую систему.\nЭто позволит быстро\nдемонстрировать визитки";
        string UDID;

        public QRViewController(IntPtr handle) : base(handle)
        {
            this.handle = handle;
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UDID = UIDevice.CurrentDevice.IdentifierForVendor.ToString();

            headerLabel.Text = current_card_name_header;
            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            if (deviceModel.ToLower().Contains("e 5") || deviceModel.ToLower().Contains("e 4") || deviceModel.ToLower().Contains("se"))
                headerLabel.Font = UIFont.FromName(Constants.fira_sans, 16f);
            activityIndicator.Color = UIColor.FromRGB(255, 99, 62);
            demonstrationView.Hidden = true;
            demonstrationView.Frame = new CGRect(0, UIApplication.SharedApplication.StatusBarFrame.Height, View.Frame.Width, View.Frame.Height);
            companyLogoIV.Hidden = true;
            companyLogoIV.BackgroundColor = UIColor.White;
            companyLogoIV.ContentMode = UIViewContentMode.ScaleAspectFit;
            //arrowsIV.Hidden = true;
            closeBn.Frame = new CoreGraphics.CGRect(View.Frame.Width / 15, View.Frame.Width / 10, View.Frame.Width / 10, View.Frame.Width / 10);
            qrIV.Frame = new CoreGraphics.CGRect(View.Frame.Width / 12, View.Frame.Height / 4, View.Frame.Width / 12 * 10, View.Frame.Width / 12 * 10);
            companyLogoIV.Frame = new CGRect(qrIV.Frame.X + qrIV.Frame.Width / 17 * 6, qrIV.Frame.Y + qrIV.Frame.Width / 17 * 6, qrIV.Frame.Width / 17 * 5, qrIV.Frame.Width / 17 * 5);
            closeBn.ImageEdgeInsets = new UIEdgeInsets(closeBn.Frame.Width / 4, closeBn.Frame.Width / 4, closeBn.Frame.Width / 4, closeBn.Frame.Width / 4);
            closeBn.TouchUpInside += (s, e) =>
            {
                demonstrationView.Hidden = true;
                //NavigationController.PopViewController(false);
            };
            leftMenuBn.TouchUpInside += (s, e) =>
            {
                RootQRViewController.SidebarController.ToggleMenu();
            };
            plusBn.TouchUpInside += PlusBn_TouchUpInside;

            tintBn.TouchUpInside += (s, e) =>
            {
                removeBn.Hidden = true;
                editBn.Hidden = true;
                _showInWebBn.Hidden = true;
                _linkStickerBn.Hidden = true;
                dividerView.Hidden = true;
                _divider2View.Hidden = true;
                _divider3View.Hidden = true;
                tintBn.Hidden = true;
            };

            _showInWebBn.TouchUpInside += ShowInWebClick;

            _linkStickerBn.TouchUpInside += LinkStickerBnTouchUpInside;

            editBn.TouchUpInside += (s, e) =>
            {
                EditViewController.card_id = qrsList[global_tag].id;
                //EditViewController.card_id = reverseList[global_tag].id;
                edit_delete_indicator = Constants.edit;
                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(EditViewController)), true);
                content_offset = scrollView.ContentOffset;
            };

            removeBn.TouchUpInside += RemoveBn_TouchUpInside;
        }




        public static void CallSnackBar(string message)
        {
            var snackbar = new TTGSnackbar(" " + message);
            snackbar.BackgroundColor = UIColor.FromRGB(255, 99, 62);
            snackbar.Show();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            arrowsIV.Hidden = true;
            is_premium = false;
            cards_remaining = 0;
            card_names = null;
            reverseList = null;
            deserialized_cards_list = null;
            qR_Logo_ID_List = null;
            global_tag = 0;
            cards_count = 0;
            try
            {
                foreach (var sub in scrollView.Subviews)
                {
                    sub.RemoveFromSuperview();
                }
            }
            catch { }
            Clear();

            InitElements();

            //clear features
            ExtraEmploymentData = null;
            CompanyLogoInQr = null;
            ExtraPersonData = null;
            ShowWeHaveNotDownloadedCardScrollDraggingInformer = false;
            //if (methods.IsConnected())
            //{
            //    InvokeInBackground(async () =>
            //    {
            //        bool res_sync;
            //        string cards_remainingcontent = "";
            //        try
            //        {
            //            res_sync = await SyncCachedCard();
            //            cards_remainingcontent = await accounts.AccountAuthorize(databaseMethods.GetAccessJwt());
            //        }
            //        catch (Exception ex)
            //        {
            //            if (!methods.IsConnected())
            //                InvokeOnMainThread(() =>
            //                {
            //                    NoConnectionViewController.view_controller_name = GetType().Name;
            //                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
            //                    return;
            //                });
            //            return;
            //        }
            //        if (cards_remainingcontent == Constants.status_code409 || cards_remainingcontent == Constants.status_code401)
            //        {
            //            InvokeOnMainThread(() =>
            //            {
            //                ShowSeveralDevicesRestriction();
            //                return;
            //            });
            //            return;
            //        }
            //        string res_cards_list = null;
            //        try
            //        {
            //            res_cards_list = await cards.CardsListGet(databaseMethods.GetAccessJwt());
            //        }
            //        catch
            //        {
            //            if (!methods.IsConnected())
            //                InvokeOnMainThread(() =>
            //                {
            //                    NoConnectionViewController.view_controller_name = GetType().Name;
            //                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
            //                    return;
            //                });
            //            return;
            //        }
            //        ShowWeHaveNotDownloadedCard = false;
            //        try
            //        {
            //            deserialized_cards_list = JsonConvert.DeserializeObject<List<QRListModel>>(res_cards_list);
            //        }
            //        catch { }
            //        try
            //        {
            //            #region placing primary card to the end of the list (the list will be reversed)
            //            var primary_card_index = deserialized_cards_list.FindIndex(x => x.isPrimary);
            //            var primary_card_item = deserialized_cards_list[primary_card_index];
            //            deserialized_cards_list.RemoveAt(primary_card_index);
            //            deserialized_cards_list.Add(primary_card_item);
            //            #endregion placing primary card to the end of the list (the list will be reversed)
            //        }
            //        catch
            //        { }

            //        var des_auth = JsonConvert.DeserializeObject<AuthorizeRootObject>(cards_remainingcontent);
            //        InvokeOnMainThread(() =>
            //        {
            //            try
            //            {
            //                foreach (var subs in des_auth.subscriptions)
            //                {
            //                    if (subs.limitations != null)
            //                        if (subs.limitations.allowMultiClients)
            //                        {
            //                            is_premium = true;
            //                            break;
            //                        }
            //                }
            //                //if (!is_premium)
            //                foreach (var subscription in des_auth.subscriptions)
            //                {
            //                    if (subscription.limitations?.cardsRemaining == null)
            //                    {
            //                        cards_remaining = 10;
            //                        break;
            //                    }
            //                    else
            //                    {
            //                        if (subscription.limitations != null)
            //                            if (subscription.limitations.cardsRemaining > cards_remaining)
            //                                cards_remaining = subscription.limitations.cardsRemaining.Value;
            //                    }
            //                }
            //                try
            //                {
            //                    NativeMethods.ClearFeatures();
            //                    foreach (var subscription in des_auth.subscriptions)
            //                    {
            //                        if (subscription.features != null)
            //                        {
            //                            foreach (var feature in subscription.features)
            //                            {
            //                                if (String.IsNullOrEmpty(ExtraEmploymentData))
            //                                    if (feature == Constants.ExtraEmploymentData)
            //                                        ExtraEmploymentData = feature;
            //                                if (String.IsNullOrEmpty(CompanyLogoInQr))
            //                                    if (feature == Constants.CompanyLogoInQr)
            //                                        CompanyLogoInQr = feature;
            //                                if (String.IsNullOrEmpty(ExtraPersonData))
            //                                    if (feature == Constants.ExtraPersonData)
            //                                        ExtraPersonData = feature;
            //                            }
            //                        }
            //                    }
            //                }
            //                catch { }
            //            }
            //            catch { }
            //            SideMenuViewController.reloadOption();
            //        });

            //        reverseList = deserialized_cards_list.AsEnumerable().Reverse().ToList();
            //        databaseMethods.InsertLastCloudSync(DateTime.Now);
            //        if (reverseList.Count > 0)
            //        {
            //            InitializeCards();
            //        }
            //        else
            //        {
            //            InvokeOnMainThread(() =>
            //            {
            //                clear_current_card_name_and_pos();
            //                this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
            //            });
            //        }
            //    });
            //}
            //else
            //{
            //    NoConnectionViewController.view_controller_name = GetType().Name;
            //    ShowCachedQRs();
            //    LaunchConnectionWaitingTimer();
            //}
            scrollView.Scrolled += (s, e) =>
            {
                try
                {
                    cards_count = card_names.Count;
                    var X_offs = (int)scrollView.ContentSize.Width / cards_count;
                    index = (int)((scrollView.ContentOffset.X + (View.Frame.Width / 2)) / (X_offs));
                    current_card_name_header = card_names[index];
                    headerLabel.Text = current_card_name_header;
                }
                catch (Exception ex)
                {
                }
            };


            scrollView.ScrollAnimationEnded += (s, e) =>
            {

            };
            scrollView.WillEndDragging += (s, e) =>
            {

            };
            scrollView.DecelerationEnded += (s, e) =>
            {

            };

            RootQRViewController.SidebarController.Sidebar.StateChangeHandler += (s, e) =>
              {
                  if (RootQRViewController.SidebarController.Sidebar.IsOpen)
                      scrollView.UserInteractionEnabled = false;
                  else
                      scrollView.UserInteractionEnabled = true;
              };





            // TODO remove block
            //{
            //    activityIndicator.Hidden = true;
            //    mainTextTV.Hidden = true;
            //    emailLogo.Hidden = true;
            //    scrollView.Hidden = false;
            //    arrowsIV.Hidden = false;
            //}
            InvokeInBackground(async () =>
            {
                if (!await FirstCheck())
                {
                    //StartNoConnection();
                    //Finish();
                    return;
                }

                if (!methods.IsConnected())
                {
                    await ShowCachedQRs();
                    LaunchConnectionWaitingTimer();
                    return;
                }
            });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            {
                try
                {
                    connectionWaitingTimer.Stop();
                    connectionWaitingTimer.Dispose();
                }
                catch { }
            }
        }

        private async Task<bool> FirstCheck()
        {
            var cardscount = databaseMethods.GetCardNames();
            if (cardscount.Count > 0)
            {
                // TODO UNCOMMENT!!!!! TODOTODOTODOTODO 
                InvokeInBackground(async () => await ShowCachedQRs());
                //await ShowCachedQRs();

                var TaskA = new Task(async () =>
                {
                    await DoOnResumeStuffBackground();
                });
                TaskA.Start();
            }
            else
            {
                if (!methods.IsConnected())
                    return false;
                InvokeOnMainThread(async () =>
                {
                    await DoOnResumeStuff();
                });
            }
            return true;
        }

        async Task<bool> DoOnResumeStuffBackground()
        {
            try
            {
                is_premium = false;
                cards_remaining = 0;
                //card_names = null;
                reverseList = null;
                deserialized_cards_list = null;
                global_tag = 0;
                cards_count = 0;

                // TODO
                //await Clear();

                // TODO
                InvokeOnMainThread(() => plusBn.Hidden = true);
                //await InitElements();

                // clear features
                ExtraEmploymentData = null;
                CompanyLogoInQr = null;
                ExtraPersonData = null;
                ShowWeHaveNotDownloadedCardScrollDraggingInformer = false;
                //SetLoadingView();
                if (!methods.IsConnected())
                {
                    InvokeOnMainThread(() => plusBn.Hidden = false);
                    return false;
                }

                // TODO set this here
                var res_sync = await SyncCachedCard(/*false*/);
                InvokeOnMainThread(() =>
                    {
                        mainTextTV.Text = cacheMessage;
                        mainTextTV.SizeToFit();
                        mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height / 3));
                    });// "Получение данных");

                string cards_remainingcontent = null;
                try
                {
                    cards_remainingcontent = await accounts.AccountAuthorize(databaseMethods.GetAccessJwt(), UDID);//"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NDk1NjEyOTcsIkFjY291bnRDbGllbnRUb2tlbiI6IjVQYUw2Sm9XcWdJIiwiQWNjb3VudElEIjoyMCwiQ2xpZW50SUQiOiJiNTA2OTJlNmNkYTdhYTlkIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.UBoRD61YNYjOu60Wu1bpotRAFvxbMaDzt6PCxQrMtKA");
                }
                catch (Exception ex)
                {
                    if (!methods.IsConnected())
                        return false;
                }
                if (String.IsNullOrEmpty(cards_remainingcontent))
                {
                    if (!methods.IsConnected())
                    {
                        // TODO
                        //NoConnectionActivity.activityName = this;
                        //StartActivity(typeof(NoConnectionActivity));
                        //Finish();
                        return false;
                    }
                }
                if (/*res_card_data == Constants.status_code409 ||*/  cards_remainingcontent == Constants.status_code401)
                {
                    InvokeOnMainThread(() => ShowSeveralDevicesRestriction());
                    return false;
                }

                var des_auth = JsonConvert.DeserializeObject<AuthorizeRootObject>(cards_remainingcontent);
                try
                {
                    foreach (var subs in des_auth.subscriptions)
                    {
                        if (subs.limitations != null)
                            if (subs.limitations.allowMultiClients)
                            {
                                is_premium = true;
                                break;
                            }
                    }
                    //if (!is_premium)
                    foreach (var subscription in des_auth.subscriptions)
                    {
                        if (subscription.limitations?.cardsRemaining == null)
                        {
                            cards_remaining = 10;
                            break;
                        }
                        else
                        {
                            if (subscription.limitations != null)
                                if (subscription.limitations.cardsRemaining > cards_remaining)
                                    cards_remaining = subscription.limitations.cardsRemaining.Value;
                        }
                    }
                    try
                    {
                        foreach (var subscription in des_auth.subscriptions)
                        {
                            if (subscription.features != null)
                            {
                                foreach (var feature in subscription.features)
                                {
                                    if (String.IsNullOrEmpty(ExtraEmploymentData))
                                        if (feature == Constants.ExtraEmploymentData)
                                            ExtraEmploymentData = feature;
                                    if (String.IsNullOrEmpty(CompanyLogoInQr))
                                        if (feature == Constants.CompanyLogoInQr)
                                            CompanyLogoInQr = feature;
                                    if (String.IsNullOrEmpty(ExtraPersonData))
                                        if (feature == Constants.ExtraPersonData)
                                            ExtraPersonData = feature;
                                }
                            }
                        }
                    }
                    catch { }
                }
                catch { }

                InvokeOnMainThread(() => plusBn.Hidden = false);

                string res_cards_list = null;
                try
                {
                    res_cards_list = await cards.CardsListGetEtag(databaseMethods.GetAccessJwt(), UDID); //"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NDk1NjEyOTcsIkFjY291bnRDbGllbnRUb2tlbiI6IjVQYUw2Sm9XcWdJIiwiQWNjb3VudElEIjoyMCwiQ2xpZW50SUQiOiJiNTA2OTJlNmNkYTdhYTlkIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.UBoRD61YNYjOu60Wu1bpotRAFvxbMaDzt6PCxQrMtKA");
                    if (res_cards_list == Constants.status_code304)
                    {
                        card_names = databaseMethods.GetCardNames();
                        return false;
                    }
                    else
                    {
                        InvokeOnMainThread(async () =>
                        {
                            activityIndicator.Hidden = false;
                            mainTextTV.Hidden = false;
                            emailLogo.Hidden = false;
                            scrollView.Hidden = true;
                            arrowsIV.Hidden = true;
                            await DoOnResumeStuff();
                        });
                        return true;
                    }
                    //if (!we_here)
                    //return false;
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
                        return false;
                    }
                }
                if (String.IsNullOrEmpty(res_cards_list))
                {
                    if (!methods.IsConnected())
                    {
                        InvokeOnMainThread(() =>
                        {
                            NoConnectionViewController.view_controller_name = GetType().Name;
                            this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                            return;
                        });
                        return false;
                    }
                }

                //ShowWeHaveNotDownloadedCard = false;
                //try
                //{
                //    deserialized_cards_list = JsonConvert.DeserializeObject<List<QRListModel>>(res_cards_list);
                //}
                //catch { }
                //try
                //{
                //    #region placing primary card to the end of the list (the list will be reversed)
                //    var primary_card_index = deserialized_cards_list.FindIndex(x => x.isPrimary);
                //    var primary_card_item = deserialized_cards_list[primary_card_index];
                //    deserialized_cards_list.RemoveAt(primary_card_index);
                //    deserialized_cards_list.Add(primary_card_item);
                //    #endregion placing primary card to the end of the list (the list will be reversed)
                //}
                //catch
                //{ }
                ////SetLoadingView();

                //// TODO crash here
                //reverseList = deserialized_cards_list.AsEnumerable().Reverse().ToList();
                //databaseMethods.InsertLastCloudSync(DateTime.Now);
                //if (reverseList.Count > 0)
                //{
                //    // TODO
                //    await Task.Run(() => InitializeCardsFromBackground());
                //    try
                //    {
                //        var TaskA = new Task(async () => await Cache_cards());
                //        TaskA.Start();
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //}
                //else
                //{
                //    clear_current_card_name_and_pos();
                //    StartActivity(typeof(MyCardActivity));
                //}
                //SetMainView();
            }
            catch (Exception ex)
            {

            }
            return true;
        }

        async Task<bool> DoOnResumeStuff()
        {
            try
            {
                //arrowsIV.Visibility = ViewStates.Gone;
                is_premium = false;
                cards_remaining = 0;
                card_names = null;
                reverseList = null;
                deserialized_cards_list = null;
                global_tag = 0;
                cards_count = 0;

                //TODO
                //await Clear();

                //await InitElements();

                //clear features
                ExtraEmploymentData = null;
                CompanyLogoInQr = null;
                ExtraPersonData = null;
                ShowWeHaveNotDownloadedCardScrollDraggingInformer = false;
                InvokeOnMainThread(() =>
                {
                    mainTextTV.Text = cacheMessage; //"Получение данных";
                    mainTextTV.SizeToFit();
                    mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height / 3));
                });
                var res_sync = await SyncCachedCard();
                //mainTextTV.Text = TranslationHelper.GetString("dataAcquisition", ci);
                string cards_remainingcontent = null;
                try
                {
                    cards_remainingcontent = await accounts.AccountAuthorize(databaseMethods.GetAccessJwt(), UDID);//"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NDk1NjEyOTcsIkFjY291bnRDbGllbnRUb2tlbiI6IjVQYUw2Sm9XcWdJIiwiQWNjb3VudElEIjoyMCwiQ2xpZW50SUQiOiJiNTA2OTJlNmNkYTdhYTlkIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.UBoRD61YNYjOu60Wu1bpotRAFvxbMaDzt6PCxQrMtKA");
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
                        return false;
                    }
                }
                if (String.IsNullOrEmpty(cards_remainingcontent))
                {
                    if (!methods.IsConnected())
                    {
                        InvokeOnMainThread(() =>
                        {
                            NoConnectionViewController.view_controller_name = GetType().Name;
                            this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                            return;
                        });
                        return false;
                    }
                }
                if (/*res_card_data == Constants.status_code409 ||*/ cards_remainingcontent == Constants.status_code401)
                {
                    ShowSeveralDevicesRestriction();
                    return false;
                }
                string res_cards_list = null;
                try
                {
                    res_cards_list = await cards.CardsListGetEtag(databaseMethods.GetAccessJwt(), UDID); //"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NDk1NjEyOTcsIkFjY291bnRDbGllbnRUb2tlbiI6IjVQYUw2Sm9XcWdJIiwiQWNjb3VudElEIjoyMCwiQ2xpZW50SUQiOiJiNTA2OTJlNmNkYTdhYTlkIiwiaXNzIjoibXlxcmNhcmRzLmNvbSIsImF1ZCI6ImRldi1hcGkubXlxcmNhcmRzLmNvbSJ9.UBoRD61YNYjOu60Wu1bpotRAFvxbMaDzt6PCxQrMtKA");
                    if (/*res_card_data == Constants.status_code409 ||*/ res_cards_list == Constants.status_code401)
                    {
                        ShowSeveralDevicesRestriction();
                        return false;
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
                        return false;
                    }
                }
                if (String.IsNullOrEmpty(res_cards_list))
                {
                    if (!methods.IsConnected())
                    {
                        InvokeOnMainThread(() =>
                        {
                            NoConnectionViewController.view_controller_name = GetType().Name;
                            this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                            return;
                        });
                        return false;
                    }
                }

                ShowWeHaveNotDownloadedCard = false;
                try
                {
                    deserialized_cards_list = JsonConvert.DeserializeObject<List<QRListModel>>(res_cards_list);
                }
                catch { }
                try
                {
                    #region placing primary card to the end of the list (the list will be reversed)
                    var primary_card_index = deserialized_cards_list.FindIndex(x => x.isPrimary);
                    var primary_card_item = deserialized_cards_list[primary_card_index];
                    deserialized_cards_list.RemoveAt(primary_card_index);
                    deserialized_cards_list.Add(primary_card_item);
                    #endregion placing primary card to the end of the list (the list will be reversed)
                }
                catch
                { }
                mainTextTV.Text = cacheMessage; //"Получение данных";
                mainTextTV.SizeToFit();
                mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height / 3));
                var des_auth = JsonConvert.DeserializeObject<AuthorizeRootObject>(cards_remainingcontent);
                try
                {
                    foreach (var subs in des_auth.subscriptions)
                    {
                        if (subs.limitations != null)
                            if (subs.limitations.allowMultiClients)
                            {
                                is_premium = true;
                                break;
                            }
                    }
                    //if (!is_premium)
                    foreach (var subscription in des_auth.subscriptions)
                    {
                        if (subscription.limitations?.cardsRemaining == null)
                        {
                            cards_remaining = 10;
                            break;
                        }
                        else
                        {
                            if (subscription.limitations != null)
                                if (subscription.limitations.cardsRemaining > cards_remaining)
                                    cards_remaining = subscription.limitations.cardsRemaining.Value;
                        }
                    }
                    try
                    {
                        foreach (var subscription in des_auth.subscriptions)
                        {
                            if (subscription.features != null)
                            {
                                foreach (var feature in subscription.features)
                                {
                                    if (String.IsNullOrEmpty(ExtraEmploymentData))
                                        if (feature == Constants.ExtraEmploymentData)
                                            ExtraEmploymentData = feature;
                                    if (String.IsNullOrEmpty(CompanyLogoInQr))
                                        if (feature == Constants.CompanyLogoInQr)
                                            CompanyLogoInQr = feature;
                                    if (String.IsNullOrEmpty(ExtraPersonData))
                                        if (feature == Constants.ExtraPersonData)
                                            ExtraPersonData = feature;
                                }
                            }
                        }
                    }
                    catch { }
                }
                catch { }

                reverseList = deserialized_cards_list.AsEnumerable().Reverse().ToList();
                databaseMethods.InsertLastCloudSync(DateTime.Now);
                if (reverseList.Count > 0)
                {
                    mainTextTV.Text = cacheMessage;
                    mainTextTV.SizeToFit();
                    mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height / 3));
                    await Task.Run(() => InitializeCards());
                    //RunOnUiThread(async()=> await Task.Run(() => InitializeCards()));
                    try
                    {
                        var TaskA = new Task(async () => await Cache_cards());
                        TaskA.Start();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    clear_current_card_name_and_pos();
                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
                }
                SetMainView();
            }
            catch (Exception ex)
            {

            }
            return true;
        }
        // TODO
        async Task<bool> Cache_cards()
        {
            //await nativeMethods.RemoveOfflineCache();
            //int index = 0;
            //try
            //{
            //    foreach (var item in QRAdapter.qrsList)
            //    {
            //        try
            //        {
            //            RunOnUiThread(() => SetLoadingView(TranslationHelper.GetString("cachingData", ci)));
            //            if (QRAdapter.qrsList[index].LogoImage != null)
            //                await nativeMethods.CacheQROffline(QRAdapter.qrsList[index].QRImage, QRAdapter.qrsList[index].LogoImage, index);
            //            else
            //            {
            //                try
            //                {
            //                    var companyLodoDefaultDrawable = GetDrawable(Resource.Drawable.company_qr_template);
            //                    await nativeMethods.CacheQROffline(QRAdapter.qrsList[index].QRImage, ((BitmapDrawable)companyLodoDefaultDrawable).Bitmap, index);
            //                }
            //                catch
            //                {
            //                    RunOnUiThread(() => SetMainView());
            //                }
            //            }
            //            index++;
            //        }
            //        catch (Exception ex)
            //        {
            //            RunOnUiThread(() => SetMainView());
            //            return false;
            //        }
            //        RunOnUiThread(() => SetMainView());
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            return true;
        }

        private void SetMainView()
        {
            InvokeOnMainThread(() =>
            {
                demonstrationView.Hidden = true;
                //loadingLL.Visibility = ViewStates.Gone;
                //mainLL.Visibility = ViewStates.Visible;
            });
        }

        private void SetLoadingView(string message)
        {
            InvokeOnMainThread(() =>
            {
                demonstrationView.Hidden = true;
                //loadingLL.Visibility = ViewStates.Visible;
                //mainLL.Visibility = ViewStates.Gone;
                mainTextTV.Text = message;
                mainTextTV.SizeToFit();
                mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height / 3));
            });
        }

        private void LaunchConnectionWaitingTimer()
        {
            connectionWaitingTimer = new System.Timers.Timer();
            connectionWaitingTimer.Interval = 1000;

            connectionWaitingTimer.Elapsed += delegate
            {
                connectionWaitingTimer.Interval = 1000;
                if (methods.IsConnected())
                {
                    InvokeOnMainThread(() =>
                    {
                        content_offset = scrollView.ContentOffset;
                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootQRViewController)), false);
                    });
                    connectionWaitingTimer.Stop();
                    connectionWaitingTimer.Dispose();
                }
            };
            connectionWaitingTimer.Start();
        }

        void RemoveBn_TouchUpInside(object sender, EventArgs e)
        {
            // TODO
            //clear_previous_cahed_qrs();
            //databaseMethods.CleanCardNames();
            RemoveCardProcessViewController.card_id = qrsList[global_tag].id;
            string[] tableItems = new string[] { "Удалить " + qrsList[global_tag].CardName + "?" };
            option = new UIActionSheet(null, null, "Не удалять", null, tableItems);

            option.Clicked += (btn_sender, args) => //Console.WriteLine("{0} Clicked", args.ButtonIndex);
            {
                if (args.ButtonIndex == 1)
                {
                    removeBn.Hidden = true;
                    editBn.Hidden = true;
                    _showInWebBn.Hidden = true;
                    _linkStickerBn.Hidden = true;
                    dividerView.Hidden = true;
                    _divider2View.Hidden = true;
                    _divider3View.Hidden = true;
                    tintBn.Hidden = true;
                    option = null;
                }
                if (args.ButtonIndex == 0)
                {
                    option = null;
                    //set index to 0 because we do not need to display wrong card name after deleting
                    index = 0;
                    edit_delete_indicator = Constants.delete;
                    clear_current_card_name_and_pos();
                    clear_previous_cahed_qrs();
                    databaseMethods.CleanCardNames();
                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(RemoveCardProcessViewController)), false);
                }
            };
            //if (remove_clicked)
            //{
            //remove_clicked = false;
            option.ShowInView(View);
            //}
        }

        private void PlusBn_TouchUpInside(object sender, EventArgs e)
        {
            if (methods.IsConnected())
            {
                if (!is_premium && cards_remaining == 0)
                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(PremiumSplashViewController)), true);
                else if (is_premium && cards_remaining == 0)
                {
                    UIAlertView alert = new UIAlertView()
                    {
                        Title = "Ошибка",
                        Message = "Достигнут лимит визиток для текущей подписки"
                    };
                    alert.AddButton("OK");
                    alert.Show();
                }
                else
                {
                    clear_current_card_name_and_pos();
                    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(CreatingCardViewController)), true);
                }
            }
            else
            {
                CallSnackBar("Необходимо соединение");
                //if (cards_remaining > 0)
                //{
                //    clear_current_card_name_and_pos();
                //    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(CreatingCardViewController)), true);
                //}
                //else
                //{
                //    UIAlertView alert = new UIAlertView()
                //    {
                //        Title = "Нет соединения",
                //        Message = "Необходимо включить интернет для проверки количества оставшихся для создания визиток и перезагрузить страницу"
                //    };
                //    alert.AddButton("OK");
                //    alert.Show();

                //    NoConnectionViewController.view_controller_name = GetType().Name;
                //    //clear_current_card_name_and_pos();
                //    this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), true);
                //}
            }
        }


        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            _linkStickerBn.Hidden = true;
            _showInWebBn.Hidden = true;
            removeBn.Hidden = true;
            editBn.Hidden = true;
            dividerView.Hidden = true;
            _divider2View.Hidden = true;
            _divider3View.Hidden = true;
            tintBn.Hidden = true;
            //white_bg_view.Hidden = true;
            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
            activityIndicator.Frame = new Rectangle((int)(View.Frame.Width / 2 - View.Frame.Width / 20), (int)(View.Frame.Height - View.Frame.Width / 5), (int)(View.Frame.Width / 10), (int)(View.Frame.Width / 10));
            activityIndicator.Hidden = false;
            mainTextTV.Hidden = false;
            mainTextTV.Lines = 4;
            emailLogo.Hidden = false;
            if (deviceModel.Contains("X"))
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10) + 8);
                leftMenuBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 40,
                                                 Convert.ToInt32(View.Frame.Width) / 22 + 20,
                                                 Convert.ToInt32(View.Frame.Width) / 8,
                                                 Convert.ToInt32(View.Frame.Width) / 8);
                plusBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) - (Convert.ToInt32(leftMenuBn.Frame.Width) + (Convert.ToInt32(View.Frame.Width) / 40)),
                                             Convert.ToInt32(View.Frame.Width) / 22 + 20,
                                             Convert.ToInt32(View.Frame.Width) / 8,
                                             Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, (Convert.ToInt32(View.Frame.Width) / 12) + 20, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                leftMenuBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 40,
                                                 Convert.ToInt32(View.Frame.Width) / 22,
                                                 Convert.ToInt32(View.Frame.Width) / 8,
                                                 Convert.ToInt32(View.Frame.Width) / 8);
                plusBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) - (Convert.ToInt32(leftMenuBn.Frame.Width) + (Convert.ToInt32(View.Frame.Width) / 40)),
                                             Convert.ToInt32(View.Frame.Width) / 22,
                                             Convert.ToInt32(View.Frame.Width) / 8,
                                             Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
            }
            plusBn.ImageEdgeInsets = new UIEdgeInsets(plusBn.Frame.Width / 4, plusBn.Frame.Width / 4, plusBn.Frame.Width / 4, plusBn.Frame.Width / 4);
            leftMenuBn.ImageEdgeInsets = new UIEdgeInsets(plusBn.Frame.Width / 3.3F, plusBn.Frame.Width / 4, plusBn.Frame.Width / 3.3F, plusBn.Frame.Width / 4);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            //headerLabel.Text = "Личная";
            emailLogo.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3,
                                            Convert.ToInt32(View.Frame.Width) / 3);
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), 26);
            mainTextTV.TextAlignment = UITextAlignment.Center;
            //var d = cardsLogo.Frame.X;
            mainTextTV.Text = cacheMessage;//"Получение данных";
            mainTextTV.Font = mainTextTV.Font.WithSize(22f);
            mainTextTV.SizeToFit();
            mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height / 3));

            scrollView.Frame = new CGRect(0, headerView.Frame.Height, View.Frame.Width, View.Frame.Height - headerView.Frame.Height);
            arrowsIV.Frame = new CGRect(View.Frame.Width / 7 * 3, View.Frame.Height - View.Frame.Width / 33, View.Frame.Width / 7, View.Frame.Width / 42);
        }

        private void InitializeCards()
        {
            cards_count = deserialized_cards_list.Count;
            var lastCard = deserialized_cards_list[deserialized_cards_list.Count - 1];

            InvokeOnMainThread(() =>
            {
                float margin_left_for_button = (float)(View.Frame.Width / 24);
                int i = 0;
                card_names = new List<string>();
                qrsList = new List<QRListModel>();
                qR_Logo_ID_List = new List<QR_Logo_ID_Model>();

                InvokeOnMainThread(() =>
                {
                    try
                    {
                        foreach (var sub in scrollView?.Subviews)
                        {
                            sub.RemoveFromSuperview();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                });

                count_cached = 0;
                clear_previous_cahed_qrs();
                databaseMethods.CleanCardNames();

                //TODO
                List<UIImage> logosList = new List<UIImage>();

                foreach (var item in reverseList)
                {
                    // TODO
                    //card_names.Add(item.name);

                    activityIndicator.Hidden = false;
                    mainTextTV.Hidden = false;
                    emailLogo.Hidden = false;
                    scrollView.Hidden = true;
                    arrowsIV.Hidden = true;

                    var options = new QrCodeEncodingOptions
                    {
                        DisableECI = true,
                        CharacterSet = "UTF-8",
                        Width = (int)View.Frame.Width,
                        Height = (int)View.Frame.Width,
                        ErrorCorrection = ErrorCorrectionLevel.H
                    };
                    var writer = new BarcodeWriter();
                    writer.Format = BarcodeFormat.QR_CODE;

                    writer.Options = options;
                    var qr = new ZXing.BarcodeWriterSvg();
                    qr.Options = options;
                    qr.Format = ZXing.BarcodeFormat.QR_CODE;
                    UIImage img = null;
                    if (reverseList[i].url != null)
                        img = writer.Write(reverseList[i].url);

                    QR_button = new UIButton();

                    var white_bg_view = new UIView();
                    white_bg_view.BackgroundColor = UIColor.White;
                    var QR_IV = new UIImageView();
                    QR_IV.ContentMode = UIViewContentMode.ScaleAspectFill;
                    var shareBn = new UIButton();
                    var optionBn = new UIButton();
                    //show_in_webBn.SetImage(UIImage.FromBundle("show_in_web.png"), UIControlState.Normal);
                    shareBn.SetTitle("ПОДЕЛИТЬСЯ", UIControlState.Normal);
                    shareBn.SetTitleColor(UIColor.Black, UIControlState.Normal);

                    shareBn.Layer.BorderColor = UIColor.FromRGB(38, 46, 56).CGColor;
                    shareBn.Layer.BorderWidth = 1f;
                    shareBn.TitleEdgeInsets = new UIEdgeInsets(0, 10, 0, 10);
                    shareBn.Font = UIFont.SystemFontOfSize(13);

                    optionBn.Layer.BorderColor = UIColor.FromRGB(38, 46, 56).CGColor;
                    optionBn.Layer.BorderWidth = 1f;
                    optionBn.SetImage(UIImage.FromBundle("optionBn.png"), UIControlState.Normal);

                    CompanyLogoNotPublicLogo logo = null;
                    if (reverseList != null)
                        if (reverseList[i].company != null)
                            logo = reverseList[i].company.logo;
                        else
                            logo = null;
                    UIImageView logoIV = new UIImageView();
                    logoIV.BackgroundColor = UIColor.White;
                    logoIV.ContentMode = UIViewContentMode.ScaleAspectFit;
                    bool isLogoStandard = false;
                    if (!String.IsNullOrEmpty(CompanyLogoInQr))
                    {

                        if (logo != null)
                        {
                            UIImage image_logo = null;
                            var logo_url = reverseList[i].company.logo.url;
                            using (var url = new NSUrl(logo_url))
                            using (var data = NSData.FromUrl(url))
                                try
                                {
                                    image_logo = UIImage.LoadFromData(data);
                                }
                                catch (Exception ex)
                                {
                                    if (ex is ArgumentNullException)
                                    {
                                        NoConnectionViewController.view_controller_name = GetType().Name;
                                        this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(NoConnectionViewController)), false);
                                        return;
                                    }
                                }
                            logoIV.Image = image_logo;
                            qR_Logo_ID_List.Add(new QR_Logo_ID_Model { url_for_qr = reverseList[i].url, logo_image = image_logo, id = i });
                        }
                        else
                        {
                            isLogoStandard = true;
                            logoIV.Image = UIImage.FromBundle("company_qr_template.png");
                        }
                    }
                    else
                    {
                        isLogoStandard = true;
                        logoIV.Image = UIImage.FromBundle("company_qr_template.png");
                    }
                    logosList.Add(logoIV.Image);

                    if (img != null)
                    {
                        QR_IV.AddSubview(logoIV);
                        QR_IV.Image = img;
                    }
                    qrsList.Add(new QRListModel
                    { /*QRImage = x., LogoImage = tempLogoBitmap,*/
                        id = item.id,
                        url = item.url,
                        CardName = item.name,
                        isLogoStandard = isLogoStandard,
                        person = item.person
                    });
                    databaseMethods.InsertCardName(item.name,
                                                   item.id,
                                                   item.url,
                                                   isLogoStandard,
                                                   item.person.firstName,
                                                   item.person.lastName);
                    if (QR_IV.Image != null)
                        cache_card(QR_IV.Image, logoIV.Image);
                    //QR_button.AddSubview(QR_IV);
                    white_bg_view.Frame = new CGRect(margin_left_for_button, View.Frame.Width / 24, View.Frame.Width - (float)(View.Frame.Width / 12), View.Frame.Height - headerView.Frame.Height - View.Frame.Width / 12);
                    white_bg_view.AddSubviews(QR_IV, QR_button, shareBn, optionBn);
                    QR_IV.Frame = new CGRect(white_bg_view.Frame.Width / 8, white_bg_view.Frame.Width / 2.7, (white_bg_view.Frame.Width / 8) * 6, (white_bg_view.Frame.Width / 8) * 6);
                    logoIV.Frame = new CGRect(QR_IV.Frame.Width / 17 * 6, QR_IV.Frame.Width / 17 * 6, QR_IV.Frame.Width / 17 * 5, QR_IV.Frame.Width / 17 * 5);
                    QR_button.Frame = QR_IV.Frame;//new CGRect(white_bg_view.Frame.Width / 6 - QR_IV.Frame.Width / 15, white_bg_view.Frame.Width / 2.7 - QR_IV.Frame.Width / 15, (white_bg_view.Frame.Width / 6) * 4 + QR_IV.Frame.Width / 7.5, (white_bg_view.Frame.Width / 6) * 4 + QR_IV.Frame.Width / 7.5);
                    QR_button.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
                    QR_button.Layer.BorderWidth = 1f;
                    //QR_IV.Frame = new CGRect(0, QR_button.Frame.Y, /*(*/white_bg_view.Frame.Width/* / 6) * 5.5*/, /*(*/white_bg_view.Frame.Width/* / 6) * 5.5*/);
                    shareBn.Tag = i;
                    QR_button.Tag = i;
                    optionBn.Tag = i;
                    QR_IV.Tag = i;
                    QR_IV.UserInteractionEnabled = true;
                    shareBn.Frame = new CGRect(white_bg_view.Frame.Width / 6 - QR_IV.Frame.Width / 15, QR_button.Frame.Y + QR_button.Frame.Height + QR_button.Frame.Height / 5, QR_button.Frame.Width / 7 * 5, QR_button.Frame.Width / 8);
                    optionBn.Frame = new CGRect(shareBn.Frame.X + shareBn.Frame.Width + 10, QR_button.Frame.Y + QR_button.Frame.Height + QR_button.Frame.Height / 5, QR_button.Frame.Width / 7 * 2 - 10, QR_button.Frame.Width / 8);

                    scrollView.AddSubview(white_bg_view);
                    QR_button.TouchUpInside += new EventHandler(this.OnQrClick);
                    shareBn.TouchUpInside += new EventHandler(this.ShareClick);
                    optionBn.TouchUpInside += new EventHandler(this.OptionClick);
                    i++;
                    margin_left_for_button += (float)white_bg_view.Frame.Width + (float)(View.Frame.Width / 48);
                    scrollView.ContentSize = new CGSize(margin_left_for_button, scrollView.Frame.Height);
                    //TODO
                    //qrsList.Add(new QRListModel { /*QRImage = x.,*/ LogoImage = logoIV.Image, id = item.id, url = item.url, CardName = item.CardName });
                }
                // TODO
                int o = 0;
                qrsList?.Clear();
                databaseMethods.GetCardNamesWithUrlAndIdAndPersonData().ForEach(x =>
                {
                    card_names.Add(x.card_name);
                    qrsList.Add(new QRListModel
                    { /*QRImage = x.,*/
                        LogoImage = logosList[o],
                        id = x.card_id,
                        url = x.card_url,
                        CardName = x.card_name,
                        isLogoStandard = x.isLogoStandard,
                        person = new Person { firstName = x.PersonName, lastName = x.PersonSurname }
                    });
                    o++;
                });
                scrollView.ContentOffset = content_offset;
                activityIndicator.Hidden = true;
                mainTextTV.Hidden = true;
                emailLogo.Hidden = true;
                scrollView.Hidden = false;
                arrowsIV.Hidden = true;
                removeBn.Frame = new CGRect(0, View.Frame.Height - View.Frame.Height / 12, View.Frame.Width, View.Frame.Height / 12);
                removeBn.SetTitle("Удалить", UIControlState.Normal);
                dividerView.Frame = new CGRect(0, removeBn.Frame.Y - 1, View.Frame.Width, 1);
                editBn.Frame = new CGRect(0, View.Frame.Height - View.Frame.Height / 6 - 1, View.Frame.Width, View.Frame.Height / 12);
                editBn.SetTitle("Редактировать", UIControlState.Normal);
                _divider2View.Frame = new CGRect(0, editBn.Frame.Y - 1, View.Frame.Width, 1);
                _linkStickerBn.Frame = new CGRect(0, editBn.Frame.Y - editBn.Frame.Height - 1, View.Frame.Width, View.Frame.Height / 12);
                _linkStickerBn.SetTitle("Привязать наклейку с QR", UIControlState.Normal);
                _divider3View.Frame = new CGRect(0, _linkStickerBn.Frame.Y - 1, View.Frame.Width, 1);
                _showInWebBn.Frame = new CGRect(0, _linkStickerBn.Frame.Y - _linkStickerBn.Frame.Height - 1, View.Frame.Width, View.Frame.Height / 12);
                _showInWebBn.SetTitle("Показать в WEB", UIControlState.Normal);
                tintBn.Frame = new CGRect(0, headerView.Frame.Height, View.Frame.Width, View.Frame.Height - headerView.Frame.Height - View.Frame.Height / 12 * 4 - 1);
                tintBn.BackgroundColor = UIColor.Black.ColorWithAlpha((nfloat)0.7);
                headerLabel.Text = card_names[0];
                if (edit_delete_indicator != Constants.delete)
                    try { headerLabel.Text = card_names[index]; } catch { }
                if (!String.IsNullOrEmpty(just_created_card_name))
                    display_just_created_card();

                databaseMethods.InsertEtag(Cards.ETagNew);
                dragging_subs();
            });
        }

        private async Task<bool> SyncCachedCard()
        {
            #region sync cached card
            //check if cached card exists and it must not be edit card
            if (databaseMethods.card_exists() && !databaseMethods.Card_from_edit())
            {
                #region uploading photos
                bool photos_exist = true;
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var cards_cache_dir = Path.Combine(documents, Constants.CardsPersonalImages);
                if (!Directory.Exists(cards_cache_dir))
                    photos_exist = false;
                else
                {
                    photos_exist = false;
                    string[] filenames = Directory.GetFiles(cards_cache_dir);
                    foreach (var img in filenames)
                    {
                        photos_exist = true;
                        break;
                    }
                }
                var documentsLogo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var logo_cache_dir = Path.Combine(documentsLogo, Constants.CardsLogo);
                if (Directory.Exists(logo_cache_dir))
                {
                    string[] filenames = Directory.GetFiles(logo_cache_dir);
                    foreach (var img in filenames)
                    {
                        photos_exist = true;
                        break;
                    }
                }
                int? logo_id = null;
                List<int> attachments_ids_list = new List<int>();
                if (photos_exist)
                {
                    InvokeOnMainThread(() =>
                    {
                        mainTextTV.Text = "Фотографии выгружаются";
                        mainTextTV.SizeToFit();
                        mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height / 3));
                    });
                    AttachmentsUploadModel res_photos = null;
                    try
                    {
                        res_photos = await attachments.UploadIOS(databaseMethods.GetAccessJwt(), UDID);
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
                    if (res_photos != null)
                    {
                        //var deserialized_logo = JsonConvert.DeserializeObject<CompanyLogoModel>(res_photos);
                        logo_id = res_photos.logo_id;
                        attachments_ids_list = res_photos.attachments_ids;
                    }
                }
                #endregion uploading photos
                InvokeOnMainThread(() =>
                {
                    mainTextTV.Text = "Визитка синхронизируется";
                    mainTextTV.SizeToFit();
                    mainTextTV.Frame = new Rectangle(0, (Convert.ToInt32(emailLogo.Frame.Y) + Convert.ToInt32(emailLogo.Frame.Height)) + 40, Convert.ToInt32(View.Frame.Width), Convert.ToInt32(View.Frame.Height / 3));
                });
                string company_card_res = null;
                try
                {
                    if (logo_id != null)
                        company_card_res = await companies.CreateCompanyCard(databaseMethods.GetAccessJwt(), UDID, databaseMethods.GetDataFromCompanyCard(), logo_id);
                    else
                        company_card_res = await companies.CreateCompanyCard(databaseMethods.GetAccessJwt(), UDID, databaseMethods.GetDataFromCompanyCard());
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
                try
                {
                    string user_card_res;
                    CompanyCardResponse deserialized;
                    if (company_card_res != null)
                    {
                        deserialized = JsonConvert.DeserializeObject<CompanyCardResponse>(company_card_res);
                        user_card_res = await cards.CreatePersonalCard(databaseMethods.GetAccessJwt(),
                                                                       databaseMethods.GetDataFromUsersCard(deserialized.id,
                                                                                                            databaseMethods.GetLastSubscription(),
                                                                                                            EditCompanyDataViewControllerNew.position,
                                                                                                            EditCompanyDataViewControllerNew.corporativePhone),
                                                                       attachments_ids_list,
                                                                       UDID);
                    }
                    //if company is null
                    else
                        user_card_res = await cards.CreatePersonalCard(databaseMethods.GetAccessJwt(),
                                                                       databaseMethods.GetDataFromUsersCard(null,
                                                                                                            databaseMethods.GetLastSubscription(),
                                                                                                            EditCompanyDataViewControllerNew.position,
                                                                                                            EditCompanyDataViewControllerNew.corporativePhone),
                                                                       attachments_ids_list,
                                                                       UDID);
                    try
                    {
                        var users_card_des = JsonConvert.DeserializeObject<CompanyCardResponse>(user_card_res);
                        CardsCreatingProcessViewController.personal_card_id = users_card_des.id;
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
                        //InvokeOnMainThread(() =>
                        //{
                        //    var deserialized_error = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(user_card_res);
                        //    UIAlertView alert = new UIAlertView()
                        //    {
                        //        Title = "Ошибка при создании персональной визитки",
                        //        Message = deserialized_error[0].message
                        //    };
                        //    alert.AddButton("OK");
                        //    alert.Show();
                        //    this.NavigationController.PopViewController(true);
                        //});
                    }
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
                    //InvokeOnMainThread(() =>
                    //{
                    //    var deserialized_error = JsonConvert.DeserializeObject<List<CreateCompanyErrorModel>>(company_card_res);
                    //    UIAlertView alert = new UIAlertView()
                    //    {
                    //        Title = "Ошибка при создании корпоративной визитки",
                    //        Message = deserialized_error[0].message
                    //    };
                    //    alert.AddButton("OK");
                    //    alert.Show();
                    //    this.NavigationController.PopViewController(true);
                    //});
                }
                ClearAll(cards_cache_dir, logo_cache_dir);
            }
            #endregion sync cached card
            return true;
        }

        private void ClearAll(string cards_cache_dir, string logo_cache_dir)
        {
            #region clearing tables, variables and photos
            if (Directory.Exists(cards_cache_dir))
                Directory.Delete(cards_cache_dir, true);
            if (Directory.Exists(logo_cache_dir))
                Directory.Delete(logo_cache_dir, true);
            databaseMethods.CleanPersonalNetworksTable();
            databaseMethods.ClearCompanyCardTable();
            databaseMethods.ClearUsersCardTable();
            SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
            SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear();
            SocialNetworkTableViewSource<int, int>._checkedRows.Clear();
            CropCompanyLogoViewController.currentImage = null;
            CropCompanyLogoViewController.cropped_result = null;
            CompanyAddressMapViewController.lat = null;
            CompanyAddressMapViewController.lng = null;
            CompanyAddressMapViewController.company_lat = null;
            CompanyAddressMapViewController.company_lng = null;
            CompanyAddressViewController.FullCompanyAddressStatic = null;
            CompanyAddressViewController.country = null;
            CompanyAddressViewController.region = null;
            CompanyAddressViewController.city = null;
            CompanyAddressViewController.index = null;
            CompanyAddressViewController.notation = null;
            CompanyDataViewControllerNew.companyName = null;
            CompanyDataViewControllerNew.linesOfBusiness = null;
            CompanyDataViewControllerNew.position = null;
            CompanyDataViewControllerNew.foundationYear = null;
            CompanyDataViewControllerNew.clients = null;
            CompanyDataViewControllerNew.companyPhone = null;
            CompanyDataViewControllerNew.corporativePhone = null;
            CompanyDataViewControllerNew.fax = null;
            CompanyDataViewControllerNew.companyEmail = null;
            CompanyDataViewControllerNew.corporativeSite = null;
            PersonalDataViewControllerNew.mySurname = null;
            PersonalDataViewControllerNew.myName = null;
            PersonalDataViewControllerNew.myMiddlename = null;
            PersonalDataViewControllerNew.myPhone = null;
            PersonalDataViewControllerNew.myEmail = null;
            PersonalDataViewControllerNew.myHomePhone = null;
            PersonalDataViewControllerNew.mySite = null;
            PersonalDataViewControllerNew.myDegree = null;
            PersonalDataViewControllerNew.myCardName = null;
            try { PersonalDataViewControllerNew.images_list.Clear(); } catch { }
            PersonalDataViewControllerNew.myBirthdate = null;
            HomeAddressViewController.FullAddressStatic = null;
            HomeAddressViewController.myCountry = null;
            HomeAddressViewController.myRegion = null;
            HomeAddressViewController.myCity = null;
            HomeAddressViewController.myIndex = null;
            HomeAddressViewController.myNotation = null;
            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
            EditCompanyDataViewControllerNew.position = null;
            EditCompanyDataViewControllerNew.logo_id = null;

            HomeAddressViewController.FullAddressTemp = null;
            HomeAddressViewController.myCountryTemp = null;
            HomeAddressViewController.myRegionTemp = null;
            HomeAddressViewController.myCityTemp = null;
            HomeAddressViewController.myIndexTemp = null;
            HomeAddressViewController.myNotationTemp = null;
            CompanyAddressViewController.FullCompanyAddressTemp = null;
            CompanyAddressViewController.countryTemp = null;
            CompanyAddressViewController.regionTemp = null;
            CompanyAddressViewController.cityTemp = null;
            CompanyAddressViewController.indexTemp = null;
            CompanyAddressViewController.notationTemp = null;
            #endregion clearing tables, variables and photos
        }

        private void Clear()
        {
            #region clearing
            try { SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear(); } catch { }
            try { SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl.Clear(); } catch { }
            try { SocialNetworkTableViewSource<int, int>._checkedRows.Clear(); } catch { }
            CropCompanyLogoViewController.currentImage = null;
            CropCompanyLogoViewController.cropped_result = null;
            CompanyAddressMapViewController.lat = null;
            CompanyAddressMapViewController.lng = null;
            CompanyAddressMapViewController.company_lat = null;
            CompanyAddressMapViewController.company_lng = null;
            CompanyAddressViewController.FullCompanyAddressStatic = null;
            CompanyAddressViewController.country = null;
            CompanyAddressViewController.region = null;
            CompanyAddressViewController.city = null;
            CompanyAddressViewController.index = null;
            CompanyAddressViewController.notation = null;
            CompanyDataViewControllerNew.companyName = null;
            CompanyDataViewControllerNew.linesOfBusiness = null;
            CompanyDataViewControllerNew.position = null;
            CompanyDataViewControllerNew.foundationYear = null;
            CompanyDataViewControllerNew.clients = null;
            CompanyDataViewControllerNew.companyPhone = null;
            CompanyDataViewControllerNew.corporativePhone = null;
            CompanyDataViewControllerNew.fax = null;
            CompanyDataViewControllerNew.companyEmail = null;
            CompanyDataViewControllerNew.corporativeSite = null;
            EditPersonalDataViewControllerNew.mySurname = null;
            EditPersonalDataViewControllerNew.myName = null;
            EditPersonalDataViewControllerNew.myMiddlename = null;
            EditPersonalDataViewControllerNew.myPhone = null;
            EditPersonalDataViewControllerNew.myEmail = null;
            EditPersonalDataViewControllerNew.myHomePhone = null;
            EditPersonalDataViewControllerNew.mySite = null;
            EditPersonalDataViewControllerNew.myDegree = null;
            EditPersonalDataViewControllerNew.myCardName = null;
            try { PersonalDataViewControllerNew.images_list.Clear(); } catch { }
            EditPersonalDataViewControllerNew.myBirthDate = null;
            HomeAddressViewController.FullAddressStatic = null;
            HomeAddressViewController.myCountry = null;
            HomeAddressViewController.myRegion = null;
            HomeAddressViewController.myCity = null;
            HomeAddressViewController.myIndex = null;
            HomeAddressViewController.myNotation = null;
            NewCardAddressMapViewController.lat = null;
            NewCardAddressMapViewController.lng = null;
            EditPersonalProcessViewController.company_id = null;
            EditCompanyDataViewControllerNew.position = null;
            EditCompanyDataViewControllerNew.logo_id = null;

            HomeAddressViewController.FullAddressTemp = null;
            HomeAddressViewController.myCountryTemp = null;
            HomeAddressViewController.myRegionTemp = null;
            HomeAddressViewController.myCityTemp = null;
            HomeAddressViewController.myIndexTemp = null;
            HomeAddressViewController.myNotationTemp = null;
            CompanyAddressViewController.FullCompanyAddressTemp = null;
            CompanyAddressViewController.countryTemp = null;
            CompanyAddressViewController.regionTemp = null;
            CompanyAddressViewController.cityTemp = null;
            CompanyAddressViewController.indexTemp = null;
            CompanyAddressViewController.notationTemp = null;
            #endregion clearing
        }

        private void dragging_subs()
        {
            nfloat speed = 0, distance;
            int time = 0;
            nfloat start_scroll_x = 0, end_scroll_x;
            bool is_draging = false;
            scrollView.DraggingStarted += async (s, e) =>
            {
                try
                {
                    is_draging = true;

                    start_scroll_x = scrollView.ContentOffset.X;

                    while (true)
                    {
                        if (is_draging)
                        {
                            await Task.Delay(10);
                            time++;
                        }
                        else
                        {
                            start_scroll_x = 0;
                            distance = 0;
                            time = 0;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            };
            scrollView.DraggingEnded += async (s, e) =>
            {
                try
                {

                    if (scrollView.ContentOffset.X > start_scroll_x)
                        distance = scrollView.ContentOffset.X - start_scroll_x;
                    else
                        distance = start_scroll_x - scrollView.ContentOffset.X;

                    speed = distance / time;
                    var _index = index;
                    int _cards_count = 0;
                    //if (methods.IsConnected())
                    //    _cards_count = cards_count;
                    //else
                    _cards_count = card_names.Count;//cached_files_array.Count() / 2;
                    if (ShowWeHaveNotDownloadedCardScrollDraggingInformer)
                    {
                        _cards_count++;
                        //ShowWeHaveNotDownloadedCardScrollDraggingInformer = false;
                    }
                    var _scrollWidth = scrollView.ContentSize.Width;
                    var current_offset = scrollView.ContentOffset;
                    if (speed < 20)
                    {
                        var offset = new CGPoint(_scrollWidth / _cards_count * _index, 0);
                        scrollView.ContentOffset = current_offset;
                        //Thread.Sleep(250);
                        //scrollView.ContentOffset = current_offset;
                        scrollView.SetContentOffset(offset, true);

                        //{
                        //    while (true)
                        //    {
                        //      if (scrollView.ContentOffset.X >= offset.X)
                        //        {
                        //            break;ƒ√
                        //        }
                        //    await Task.Delay(200);
                        //    }
                        //};
                        //scrollView.ContentOffset = offset;
                    }
                    //else
                    //{

                    //}
                    is_draging = false;
                }
                catch (Exception ex)
                {

                }
            };
        }

        void display_just_created_card()
        {
            var current_card_index = reverseList.FindIndex(x => x.name == just_created_card_name);
            var _scrollWidth = scrollView.ContentSize.Width;
            var offset = new CGPoint(_scrollWidth / cards_count * current_card_index, 0);
            if (offset.X >= 0)
                scrollView.ContentOffset = offset;
            just_created_card_name = null;
        }

        void OnQrClick(object sender, EventArgs e)
        {
            var bn = sender as UIButton;
            var tag = bn.Tag;
            logoImageDemonstr = UIImage.FromBundle("company_qr_template.png"); ;
            //companyLogoIV.Hidden = true;
            foreach (var item in qR_Logo_ID_List)
            {
                if (item.id == Convert.ToInt32(tag))
                {
                    var logo = reverseList[Convert.ToInt32(tag)].company.logo;
                    if (logo != null)
                        logoImageDemonstr = item.logo_image;

                    break;
                }
            }

            var options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = (int)View.Frame.Width,
                Height = (int)View.Frame.Width,
                ErrorCorrection = ErrorCorrectionLevel.H
            };
            var writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;

            writer.Options = options;
            var qr = new ZXing.BarcodeWriterSvg();
            qr.Options = options;
            qr.Format = ZXing.BarcodeFormat.QR_CODE;
            var img = writer.Write(reverseList[Convert.ToInt32(tag)].url);
            qrIV.Image = img;
            //var sdsd= UIApplication.SharedApplication.StatusBarFrame.Height;
            if (logoImageDemonstr != null)
            {
                companyLogoIV.Image = logoImageDemonstr;
                companyLogoIV.Hidden = false;
            }
            demonstrationView.Hidden = false;
            //NavigationController.PushViewController(sb.InstantiateViewController("QR_demonstrationViewController"), false);

        }
        void OnQrOfflineClick(object sender, EventArgs e)
        {
            var bn = sender as UIButton;
            var tag = bn.Tag;
            logoImageDemonstr = UIImage.FromBundle("company_qr_template.png");
            logoImageDemonstr = UIImage.FromFile(cached_files_array[tag + 1].FullName);
            qrIV.Image = UIImage.FromFile(cached_files_array[tag].FullName);
            companyLogoIV.Image = logoImageDemonstr;
            companyLogoIV.Hidden = false;
            demonstrationView.Hidden = false;

        }
        void ShareClick(object sender, EventArgs e)
        {
            var bn = sender as UIButton;
            var tag = bn.Tag;

            if (qrsList?[Convert.ToInt32(tag)]?.url != null)
            {
                var text = FromObject($"C вами делится визиткой {qrsList[Convert.ToInt32(tag)]?.person?.firstName} {qrsList[Convert.ToInt32(tag)]?.person?.lastName}\n\n{qrsList[Convert.ToInt32(tag)].url}\n\nСоздать эл. визитку себе - https://myqrcards.page.link/nM9x");
                var items = new[] { text };
                var activity = new UIActivityViewController(items, null);
                PresentViewController(activity, true, null);
            }
        }

        void LinkStickerBnTouchUpInside(object sender, EventArgs e)
        {
            LinkStickerViewController.CardId = qrsList[global_tag].id;
            this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(LinkStickerViewController)), true);
        }

        void ShowInWebClick(object sender, EventArgs e)
        {
            //var bn = sender as UIButton;
            //var tag = bn.Tag;
            //SocialWebViewController.card_id = reverseList[Convert.ToInt32(tag)].id;
            clear_current_card_name_and_pos();
            //this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(SocialWebViewController)), true);
            if (methods.IsConnected())
            {
                InvokeInBackground(async () =>
                {
                    // TODO
                    //var res_card_data = await cards.CardDataGet(databaseMethods.GetAccessJwt(), qrsList[Convert.ToInt32(tag)].id);
                    //if (res_card_data == Constants.status_code409 || res_card_data == Constants.status_code401)
                    //{
                    //    InvokeOnMainThread(() =>
                    //    {
                    //        ShowSeveralDevicesRestriction();
                    //        return;
                    //    });
                    //    return;
                    //}
                    //var des_card_data = JsonConvert.DeserializeObject<CardsDataModel>(res_card_data);
                    InvokeOnMainThread(() =>
            {
                if (qrsList?[Convert.ToInt32(global_tag)]?.url != null)
                {
                    NSString urlString = new NSString(qrsList[Convert.ToInt32(global_tag)].url);//des_card_data.url);
                    NSUrl myFileUrl = new NSUrl(urlString);
                    UIApplication.SharedApplication.OpenUrl(myFileUrl);
                }
            });
                });
            }
            else
            {
                CallSnackBar("Необходимо соединение");
            }
        }
        void OptionClick(object sender, EventArgs e)
        {
            if (!methods.IsConnected())
            {
                CallSnackBar("Необходимо соединение");
                return;
            }
            var bn = sender as UIButton;
            global_tag = Convert.ToInt32(bn.Tag);
            removeBn.Hidden = false;
            editBn.Hidden = false;
            _showInWebBn.Hidden = false;
            _linkStickerBn.Hidden = false;
            dividerView.Hidden = false;
            _divider2View.Hidden = false;
            _divider3View.Hidden = false;
            tintBn.Hidden = false;

            //foreach (var item in qR_Logo_ID_List)
            //{
            //    CropCompanyLogoViewController.cropped_result = null;
            //    if (item.id == Convert.ToInt32(global_tag))
            //    {
            //        var logo = reverseList[Convert.ToInt32(global_tag)].company.logo;
            //        if (logo != null)
            //            CropCompanyLogoViewController.cropped_result = item.logo_image;

            //        break;
            //    }
            //}
            CropCompanyLogoViewController.cropped_result = null;
            if (!qrsList[global_tag].isLogoStandard)
                CropCompanyLogoViewController.cropped_result = qrsList[global_tag].LogoImage;
        }

        public static void clear_current_card_name_and_pos()
        {
            current_card_name_header = String.Empty;
            content_offset = new CGPoint(0, 0);
        }
        void call_premium_option_menu()
        {
            string[] constraintItems = new string[] { "Подробнее о Premium" };

            if (!databaseMethods.userExists())
                constraintItems = new string[] { "Подробнее о Premium", "Войти в учетную запись" };
            var option_const = new UIActionSheet(null, null, "Отменить", null, constraintItems);
            option_const.Title = "Достигнут лимит визиток для текущей подписки";
            option_const.Clicked += (btn_sender, args) =>
            {
                if (args.ButtonIndex == 0)
                {
                    NavigationController.PushViewController(sb.InstantiateViewController(nameof(PremiumViewController)), true);
                }
                if (!databaseMethods.userExists())
                    if (args.ButtonIndex == 1)
                    {
                        NavigationController.PushViewController(sb.InstantiateViewController(nameof(EmailViewControllerNew)), true);
                    }
            };
            option_const.ShowInView(View);
        }


        //cache cards to display QRs offline
        void cache_card(UIImage qr_img, UIImage logo_img)
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var QRs_cache_dir = Path.Combine(documents, Constants.QRs_cache_dir);
            if (!Directory.Exists(QRs_cache_dir))
                Directory.CreateDirectory(QRs_cache_dir);

            UIImage resultImage;
            NSData image_jpeg = null;
            NSError err = null;

            #region cache QR
            var width_original = qr_img.Size.Width;
            if (width_original > Constants.BitmapSide)
            {
                var width = Constants.BitmapSide;
                var height = Constants.BitmapSide;

                UIGraphics.BeginImageContext(new CGSize(width, height));

                qr_img.Draw(new CGRect(0, 0, width, height));
                resultImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
            }
            else
                resultImage = qr_img;

            image_jpeg = null;
            image_jpeg = resultImage.AsJPEG();
            if (image_jpeg.Length > 8000000)
                image_jpeg = resultImage.AsJPEG(0.8F);

            string filenames = "qr_image" + count_cached.ToString() + ".jpeg";
            var fileName = Path.Combine(QRs_cache_dir, filenames);
            //var img = UIImage.FromFile(fileName);
            err = null;
            image_jpeg.Save(fileName, false, out err);
            #endregion cach QR


            #region cache logo
            var width_original_logo = logo_img.Size.Width;
            var height_original_logo = logo_img.Size.Height;
            if (width_original_logo > Constants.BitmapSide)
            {
                var width = Constants.BitmapSide;
                var height = Constants.BitmapSide;

                nativeMethods.SqueezeImage(ref width, ref height, ref width_original_logo, ref height_original_logo);

                UIGraphics.BeginImageContext(new CGSize(width, height));

                logo_img.Draw(new CGRect(0, 0, width, height));
                resultImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
            }
            else
                resultImage = logo_img;

            image_jpeg = null;
            image_jpeg = resultImage.AsJPEG();
            if (image_jpeg.Length > 8000000)
                image_jpeg = resultImage.AsJPEG(0.8F);

            string filenames_logo = "image_logo" + count_cached.ToString() + ".jpeg";
            var fileName_logo = Path.Combine(QRs_cache_dir, filenames_logo);
            //var img_logo = UIImage.FromFile(fileName_logo);
            err = null;
            image_jpeg.Save(fileName_logo, false, out err);
            #endregion cach logo

            count_cached++;
        }

        private void clear_previous_cahed_qrs()
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var QRs_cache_dir = Path.Combine(docs, Constants.QRs_cache_dir);
            if (Directory.Exists(QRs_cache_dir))
                Directory.Delete(QRs_cache_dir, true);
        }

        private async Task<bool> ShowCachedQRs()
        {
            try
            {
                // TODO
                //qrsList?.Clear();
                qrsList = new List<QRListModel>();
                var listCardObj = databaseMethods.GetCardNamesWithUrlAndIdAndPersonData();

                card_names = new List<string>();
                databaseMethods.GetCardNamesWithUrlAndIdAndPersonData().ForEach(x =>
                {
                    card_names.Add(x.card_name);
                    qrsList.Add(new QRListModel
                    { /*QRImage = x., LogoImage = tempLogoBitmap,*/
                        id = x.card_id,
                        url = x.card_url,
                        CardName = x.card_name,
                        isLogoStandard = x.isLogoStandard,
                        person = new Person { firstName = x.PersonName, lastName = x.PersonSurname }
                    });
                });

                //card_names = databaseMethods.GetCardNames();
                cards_count = card_names.Count();
                if (cards_count == 0)
                {
                    clear_current_card_name_and_pos();
                    InvokeOnMainThread(() => this.NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true));
                    return true;
                }

                InvokeOnMainThread(() =>
                {
                    try
                    {
                        headerLabel.Text = card_names[0];
                    }
                    catch (Exception ex)
                    {

                    }
                });

                // Insert just created card name.
                if (ShowWeHaveNotDownloadedCard)
                    if (!String.IsNullOrEmpty(just_created_card_name))
                    {
                        //int cnt = 0;
                        //var tmp_list_for_offset = new List<string>();
                        //foreach (var name in card_names)
                        //{
                        //    if (cnt == 1)
                        //        tmp_list_for_offset.Add(just_created_card_name);
                        //    tmp_list_for_offset.Add(name);
                        //    cnt++;
                        //}
                        //card_names = tmp_list_for_offset;
                        ShowWeHaveNotDownloadedCardScrollDraggingInformer = true;
                        card_names.Insert(1, just_created_card_name);
                        cards_count++;
                    }
                float margin_left_for_button = 0;
                InvokeOnMainThread(() => margin_left_for_button = (float)(View.Frame.Width / 24));
                int i = 0;
                int counter = 0;
                int iterator = 0;

                count_cached = 0;
                //InvokeOnMainThread(() => plusBn.Hidden = true);

                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var QRs_cache_dir = Path.Combine(documents, Constants.QRs_cache_dir);
                if (!Directory.Exists(QRs_cache_dir))
                    Directory.CreateDirectory(QRs_cache_dir);

                DirectoryInfo info = new DirectoryInfo(QRs_cache_dir);
                cached_files_array = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
                UIImageView QR_IV = new UIImageView();
                UIImageView logoIV;
                UIView white_bg_view;

                //InvokeOnMainThread(() =>
                //{
                //    try
                //    {
                //        foreach (var sub in scrollView.Subviews)
                //        {
                //            sub.RemoveFromSuperview();
                //        }
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //});

                foreach (FileInfo file in cached_files_array)
                {
                    if (file.ToString().Contains("logo"))
                    {

                    }
                    else
                    {
                        InvokeOnMainThread(() =>
                        {
                            QR_button = new UIButton();
                            QR_button.Layer.BorderColor = UIColor.FromRGB(255, 99, 62).CGColor;
                            QR_button.Layer.BorderWidth = 1f;
                            QR_button.Tag = i;
                            QR_IV = new UIImageView();
                            QR_IV.ContentMode = UIViewContentMode.ScaleAspectFill;
                            //QR_IV.AddSubview(logoIV);


                            var shareBn = new UIButton();
                            var optionBn = new UIButton();
                            //show_in_webBn.SetImage(UIImage.FromBundle("show_in_web.png"), UIControlState.Normal);
                            shareBn.SetTitle("ПОДЕЛИТЬСЯ", UIControlState.Normal);
                            shareBn.SetTitleColor(UIColor.Black, UIControlState.Normal);

                            shareBn.Layer.BorderColor = UIColor.FromRGB(38, 46, 56).CGColor;
                            shareBn.Layer.BorderWidth = 1f;
                            shareBn.TitleEdgeInsets = new UIEdgeInsets(0, 10, 0, 10);
                            shareBn.Font = UIFont.SystemFontOfSize(13);

                            optionBn.Layer.BorderColor = UIColor.FromRGB(38, 46, 56).CGColor;
                            optionBn.Layer.BorderWidth = 1f;
                            optionBn.SetImage(UIImage.FromBundle("optionBn.png"), UIControlState.Normal);

                            white_bg_view = new UIView();
                            white_bg_view.BackgroundColor = UIColor.White;
                            white_bg_view.Frame = new CGRect(margin_left_for_button, View.Frame.Width / 24, View.Frame.Width - (float)(View.Frame.Width / 12), View.Frame.Height - headerView.Frame.Height - View.Frame.Width / 12);
                            white_bg_view.AddSubviews(QR_IV, QR_button, shareBn, optionBn);
                            QR_IV.Frame = new CGRect(white_bg_view.Frame.Width / 8, white_bg_view.Frame.Width / 2.7, (white_bg_view.Frame.Width / 8) * 6, (white_bg_view.Frame.Width / 8) * 6);
                            QR_button.Frame = QR_IV.Frame;
                            QR_IV.Tag = i;
                            QR_IV.UserInteractionEnabled = true;
                            scrollView.AddSubview(white_bg_view);

                            logoIV = new UIImageView();
                            logoIV.BackgroundColor = UIColor.White;
                            logoIV.ContentMode = UIViewContentMode.ScaleAspectFit;
                            logoIV.Frame = new CGRect(QR_IV.Frame.Width / 17 * 6, QR_IV.Frame.Width / 17 * 6, QR_IV.Frame.Width / 17 * 5, QR_IV.Frame.Width / 17 * 5);
                            var logoImage = UIImage.FromFile(cached_files_array[counter + 1].FullName);
                            logoIV.Image = logoImage;
                            QR_IV.AddSubview(logoIV);

                            QR_button.TouchUpInside += new EventHandler(this.OnQrOfflineClick);

                            QR_IV.Image = UIImage.FromFile(file.FullName);
                            shareBn.Tag = iterator;
                            optionBn.Tag = iterator;

                            // TODOTODOTODOTODOTODOTODOTODOTODOTODOTODO
                            //qrsList.Add(new QRListModel { });
                            qrsList[iterator].LogoImage = logoImage;
                            iterator++;
                            i = i + 2;
                            counter = counter + 2;
                            margin_left_for_button += (float)white_bg_view.Frame.Width + (float)(View.Frame.Width / 48);
                            // Show template card that have been uploaded but not downloaded to the app.
                            if (ShowWeHaveNotDownloadedCard)
                                margin_left_for_button = ShowCardTemplate(margin_left_for_button);

                            shareBn.Frame = new CGRect(white_bg_view.Frame.Width / 6 - QR_IV.Frame.Width / 15, QR_button.Frame.Y + QR_button.Frame.Height + QR_button.Frame.Height / 5, QR_button.Frame.Width / 7 * 5, QR_button.Frame.Width / 8);
                            optionBn.Frame = new CGRect(shareBn.Frame.X + shareBn.Frame.Width + 10, QR_button.Frame.Y + QR_button.Frame.Height + QR_button.Frame.Height / 5, QR_button.Frame.Width / 7 * 2 - 10, QR_button.Frame.Width / 8);

                            shareBn.TouchUpInside += new EventHandler(this.ShareClick);
                            optionBn.TouchUpInside += new EventHandler(this.OptionClick);

                            scrollView.ContentSize = new CGSize(margin_left_for_button, scrollView.Frame.Height);
                        });
                    }
                }
                //scrollView.ContentOffset = content_offset;
                InvokeOnMainThread(() =>
                {
                    activityIndicator.Hidden = true;
                    mainTextTV.Hidden = true;
                    emailLogo.Hidden = true;
                    scrollView.Hidden = false;
                    arrowsIV.Hidden = true;

                    removeBn.Frame = new CGRect(0, View.Frame.Height - View.Frame.Height / 12, View.Frame.Width, View.Frame.Height / 12);
                    removeBn.SetTitle("Удалить", UIControlState.Normal);
                    dividerView.Frame = new CGRect(0, removeBn.Frame.Y - 1, View.Frame.Width, 1);
                    editBn.Frame = new CGRect(0, View.Frame.Height - View.Frame.Height / 6 - 1, View.Frame.Width, View.Frame.Height / 12);
                    editBn.SetTitle("Редактировать", UIControlState.Normal);
                    _divider2View.Frame = new CGRect(0, editBn.Frame.Y - 1, View.Frame.Width, 1);
                    _linkStickerBn.Frame = new CGRect(0, editBn.Frame.Y - editBn.Frame.Height - 1, View.Frame.Width, View.Frame.Height / 12);
                    _linkStickerBn.SetTitle("Привязать наклейку с QR", UIControlState.Normal);
                    _divider3View.Frame = new CGRect(0, _linkStickerBn.Frame.Y - 1, View.Frame.Width, 1);
                    _showInWebBn.Frame = new CGRect(0, _linkStickerBn.Frame.Y - _linkStickerBn.Frame.Height - 1, View.Frame.Width, View.Frame.Height / 12);
                    _showInWebBn.SetTitle("Показать в WEB", UIControlState.Normal);
                    tintBn.Frame = new CGRect(0, headerView.Frame.Height, View.Frame.Width, View.Frame.Height - headerView.Frame.Height - View.Frame.Height / 12 * 4);
                    tintBn.BackgroundColor = UIColor.Black.ColorWithAlpha((nfloat)0.7);

                    dragging_subs();
                });
            }
            catch (Exception ex)
            {

            }
            return true;
        }

        private float ShowCardTemplate(float margin_left_for_button)
        {
            UIView white_bg_view;
            UIImageView reconnectIV;
            UILabel infoLabel;
            white_bg_view = new UIView();
            reconnectIV = new UIImageView();
            infoLabel = new UILabel();
            white_bg_view.BackgroundColor = UIColor.White;
            white_bg_view.Frame = new CGRect(margin_left_for_button, View.Frame.Width / 24, View.Frame.Width - (float)(View.Frame.Width / 12), View.Frame.Height - headerView.Frame.Height - View.Frame.Width / 12);
            scrollView.AddSubview(white_bg_view);

            reconnectIV.Image = UIImage.FromBundle("no_internet_arrow.png");
            reconnectIV.Frame = new CGRect(white_bg_view.Frame.Width / 7 * 3, white_bg_view.Frame.Height / 2 - white_bg_view.Frame.Width / 7, white_bg_view.Frame.Width / 7, white_bg_view.Frame.Width / 7);
            infoLabel.Frame = new CGRect(white_bg_view.Frame.Width / 10, reconnectIV.Frame.Y + reconnectIV.Frame.Height + reconnectIV.Frame.Height / 2, View.Frame.Width - View.Frame.Width / 5, reconnectIV.Frame.Height * 2.5);
            infoLabel.Text = "Проверьте соединение" + "\r\n" + "с интернетом";
            infoLabel.Lines = 3;
            infoLabel.TextAlignment = UITextAlignment.Center;
            white_bg_view.AddSubviews(reconnectIV, infoLabel);
            ShowWeHaveNotDownloadedCard = false;
            return margin_left_for_button += (float)white_bg_view.Frame.Width + (float)(View.Frame.Width / 48);
        }

        void ShowSeveralDevicesRestriction()
        {
            LogOutClass.log_out();
            MyCardViewController.device_restricted = true;
            NavigationController.PushViewController(sb.InstantiateViewController(nameof(RootMyCardViewController)), true);
        }
    }
}