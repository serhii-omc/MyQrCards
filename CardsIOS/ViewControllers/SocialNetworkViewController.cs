using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using CardsPCL.Database;
using CardsPCL.Models;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UIKit;
using CardsIOS.Models;
using CardsIOS.NativeClasses;

namespace CardsIOS
{
    public partial class SocialNetworkViewController : UIViewController
    {
        public SocialNetworkViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public static string came_from;
        public static nfloat cellHeight, viewWidth;
        List<CardsIOS.NativeClasses.SocialNetworkData> datalist;
        DatabaseMethodsIOS databaseMethods = new DatabaseMethodsIOS();
       
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();

            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(true);

            var source = new SocialNetworkTableViewSource<int, int>(tableView, this.NavigationController);
            var items = new List<int>();
            datalist = SocialNetworkData.SampleData();

            for (int i = 0; i < datalist.Count; i++)
            {
                items.Add(datalist[i].Id);
            }

            source.Items = items.GroupBy(item => 10 * ((item + 9) / 10));

            tableView.RowHeight = cellHeight;
            tableView.Source = source;
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;

            new AppDelegate().disableAllOrientation = false;

            cellHeight = View.Frame.Height / 12;
            viewWidth = View.Frame.Width;
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            var deviceModel = Xamarin.iOS.DeviceHardware.Model;
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
            headerLabel.Text = "Социальные сети";

            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);

            tableView.Frame = new Rectangle(0, (int)(headerView.Frame.Height), (int)(View.Frame.Width), (int)(View.Frame.Height - headerView.Frame.Height));
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (came_from == Constants.personal)
            {
                databaseMethods.CleanPersonalNetworksTable();
                //if(SocialNetworkTableViewSource<int, int>.selectedIndexes !=null)
                //{
                //    int i = 0;
                //    foreach (var item in SocialNetworkData.SampleData())
                //    {
                //        //if(item.Id==)
                //        foreach (var item_ in SocialNetworkTableViewSource<int, int>.selectedIndexes)
                //        {
                //            if (item.Id == item_)
                //                //selected_indexes_from_sampleData.Add(i);
                //            {
                //                databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = datalist[i].Id, ContactUrl = datalist[i].ContactUrl });
                //            }

                //        }
                //        i++;
                //    }
                //}
                try
                {
                    databaseMethods.CleanPersonalNetworksTable();
                    foreach (var item/*index*/ in SocialNetworkTableViewSource<int, int>.socialNetworkListWithMyUrl)//.selectedIndexes)
                    {
                        databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = item.SocialNetworkID, ContactUrl = item.ContactUrl });
                        //databaseMethods.InsertPersonalNetwork(new SocialNetworkModel { SocialNetworkID = datalist[index].Id, ContactUrl = datalist[index].ContactUrl });
                    }
                }
                catch { }
                //var netwListRes = databaseMethods.GetPersonalNetworkList();
            }

            //SocialNetworkTableViewSource<int, int>.selectedIndexes.Clear();
        }
    }
}