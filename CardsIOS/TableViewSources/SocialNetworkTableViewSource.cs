using System;
using System.Collections.Generic;
using System.Linq;
using CardsIOS.Cells;
using CardsPCL.CommonMethods;
using CardsPCL.Models;
using Foundation;
using UIKit;
using CardsIOS.NativeClasses;

namespace CardsIOS.TableViewSources
{
    public class SocialNetworkTableViewSource<TGroup, TItem> : UITableViewSource
    {
        public static IDictionary<NSIndexPath, string> _checkedRows = new Dictionary<NSIndexPath, string>();
        UIStoryboard storyboard = UIStoryboard.FromName("Main", NSBundle.MainBundle);
        private NSIndexPath _indexPathOfSelectedRow;
        private IEnumerable<IGrouping<TGroup, TItem>> _items;
        UINavigationController navigationController;
        //temporary list to store selected indexes
        public static List<int> selectedIndexes = new List<int>();
        public static List<SocialNetworkModel> socialNetworkListWithMyUrl = new List<SocialNetworkModel>();
        List<int> selected_indexes_from_sampleData = new List<int>();
        public static int currentIndex;
        public IEnumerable<IGrouping<TGroup, TItem>> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                TableView.ReloadData();
            }
        }

        protected string CellIdentifier { get; }
        protected string HeaderIdentifier { get; }
        public static UITableView TableView { get; set; }

        public SocialNetworkTableViewSource(UITableView tableView, UINavigationController navigationController)
        {
            this.navigationController = navigationController;
            tableView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            TableView = tableView;
            var cellType = typeof(SocialNetworkTableViewCell);
            CellIdentifier = cellType.Name;
            TableView.RegisterClassForCellReuse(cellType, CellIdentifier);
            //var fdf = TableView.IndexPathsForVisibleRows;
            selected_indexes_from_sampleData.Clear();
            int i = 0;
            //foreach(var item in SocialNetworkData.SampleData())
            //{
            //    //if(item.Id==)
            //    foreach(var item_ in selectedIndexes)
            //    {
            //        if (item.Id == item_)
            //            selected_indexes_from_sampleData.Add(i);
            //    }
            //    i++;
            //}
            selected_indexes_from_sampleData = selectedIndexes;
            //_checkedRows = new Dictionary<NSIndexPath, string>();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (SocialNetworkTableViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);// as SocialNetworkTableViewCell;
            cell.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            cell.Bind(Items.ElementAt(indexPath.Section).ElementAt(indexPath.Row));

            var row = indexPath.Row;
            try
            {
                if (selected_indexes_from_sampleData.Contains(row))
                    _checkedRows.Add(indexPath, string.Empty);
            }
            catch { }
            if (_checkedRows != null)
                cell.IsChecked = _checkedRows.ContainsKey(indexPath);
            cell.TextLabel.TextColor = UIColor.Green;
            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            currentIndex = indexPath.Row;
            try
            {
                //if (_checkedRows.ContainsKey(indexPath))
                //{
                //    var removeValue = selectedIndexes.First(x => x == indexPath.Row);
                //    selectedIndexes.Remove(removeValue);
                //    _checkedRows.Remove(indexPath);
                //}
                //else
                //{
                //    _checkedRows.Add(indexPath, string.Empty);
                //    selectedIndexes.Add(indexPath.Row);
                //}
                _indexPathOfSelectedRow = indexPath;
            }
            catch { }
            TableView.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Right);

            var id = SocialNetworkData.SampleData()[indexPath.Row].Id;
            bool link_exists_in_users_social_list = false;
            int i = 0;
            foreach (var item in socialNetworkListWithMyUrl)
            {
                if (item.SocialNetworkID == id)
                {
                    WebViewSocialToChooseViewController.urlString = socialNetworkListWithMyUrl[i].ContactUrl;
                    link_exists_in_users_social_list = true;
                    break;
                }
                i++;
            }
            if (!link_exists_in_users_social_list)
            {
                WebViewSocialToChooseViewController.urlString = null;
            }
            WebViewSocialToChooseViewController.urlRoot = SocialNetworkData.SampleData()[indexPath.Row].ContactUrl;
            WebViewSocialToChooseViewController.headerValue = SocialNetworkData.SampleData()[indexPath.Row].NameNetworkLabel;

            navigationController.PushViewController(storyboard.InstantiateViewController(nameof(WebViewSocialToChooseViewController)), true);
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            cell.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            if (_indexPathOfSelectedRow == indexPath)
            {
                _indexPathOfSelectedRow = null;
            }
        }

        public override nint NumberOfSections(UITableView tableView) => Items?.Count() ?? 0;
        public override nint RowsInSection(UITableView tableview, nint section) => Items?.ElementAt((int)section).Count() ?? 0;
    }
}
