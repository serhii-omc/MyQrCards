using System;
using System.Linq;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CoreGraphics;
using UIKit;

namespace CardsIOS
{
    public partial class NoConnectionViewController : UIViewController
    {
        UIStoryboard sb = UIStoryboard.FromName("Main", null);
        System.Timers.Timer connectionWaitingTimer;
        Methods methods = new Methods();
        DatabaseMethodsIOS databaseMethodsIOS = new DatabaseMethodsIOS();

        public static string view_controller_name;
        public NoConnectionViewController(IntPtr handle) : base(handle)
        {
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            InitElements();
            reconnectBn.TouchUpInside += Reload;
            exitBn.TouchUpInside += ExitBn_TouchUpInside;

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            LaunchConnectionWaitingTimer();
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

        private void InitElements()
        {
            reconnectBn.Frame = new CGRect(View.Frame.Width / 7 * 3, View.Frame.Height / 2 - View.Frame.Width / 7, View.Frame.Width / 7, View.Frame.Width / 7);
            infoLabel.Frame = new CGRect(View.Frame.Width / 10, reconnectBn.Frame.Y + reconnectBn.Frame.Height + reconnectBn.Frame.Height / 2, View.Frame.Width - View.Frame.Width / 5, reconnectBn.Frame.Height * 2.5);
            infoLabel.Text = "Необходимо соединение." + "\r\n" + "Включите интернет";// и нажмите кнопку перезагрузки";
            exitBn.Frame = new CGRect(View.Frame.Width / 15,
                                         infoLabel.Frame.Y + infoLabel.Frame.Height + 30,
                                         View.Frame.Width - View.Frame.Width / 15 * 2,
                                         View.Frame.Height / 12);
        }

        void ExitBn_TouchUpInside(object sender, EventArgs e)
        {
            var option_back = UIAlertController.Create("Выйти без сохранения данных?",
                                    null,
                                    UIAlertControllerStyle.ActionSheet);

            option_back.AddAction(UIAlertAction.Create("Подтвердить", UIAlertActionStyle.Default, (action) =>
            {
                UIViewController vc;
                if (databaseMethodsIOS.userExists() && databaseMethodsIOS.GetCardNames()?.Count > 0)
                    vc = sb.InstantiateViewController(nameof(RootQRViewController));
                else
                    vc = sb.InstantiateViewController(nameof(RootMyCardViewController));
                this.NavigationController.PushViewController(vc, true);

                // Remove previous view controllers from stack
                //var vc_list = this.NavigationController.ViewControllers.ToList();
                //int vc_count = vc_list.Count;
                //for (int i = 0; i < vc_count - 2; i++)
                //{
                //    vc_list?.RemoveAt(0);
                //}
                //NavigationController.ViewControllers = vc_list?.ToArray();
            }));
            option_back.AddAction(UIAlertAction.Create("Отмена", UIAlertActionStyle.Cancel, null));
            this.PresentViewController(option_back, true, null);
        }

        void Reload(object sender, EventArgs e)
        {
            if (view_controller_name == nameof(QRViewController))
                view_controller_name = nameof(RootQRViewController);
            if (view_controller_name == nameof(MyCardViewController))
                view_controller_name = nameof(RootMyCardViewController);
            InvokeOnMainThread(() => this.NavigationController.PushViewController(sb.InstantiateViewController(view_controller_name), true));
            var vc_list = this.NavigationController.ViewControllers.ToList();
            try { vc_list.RemoveAt(vc_list.Count - 2); } catch { }
            try { vc_list.RemoveAt(vc_list.Count - 2); } catch { }
            this.NavigationController.ViewControllers = vc_list.ToArray();
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
                    InvokeOnMainThread(() => Reload(null, null));
                    connectionWaitingTimer.Stop();
                    connectionWaitingTimer.Dispose();
                }
            };
            connectionWaitingTimer.Start();
        }
    }
}