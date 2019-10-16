using System;
using System.Collections.Generic;
using System.Linq;
using CardsIOS.Cells;
using Foundation;
using UIKit;

namespace CardsIOS.TableViewSources
{
    public class MyCardsListTableViewSource/*<TGroup, TItem>*/ : UITableViewSource
    {
        protected string CellIdentifier { get; }
        protected string HeaderIdentifier { get; }
        protected UITableView TableView { get; }
        private List<int>/*IEnumerable<IGrouping<TGroup, TItem>>*/ _items;
        public /*IEnumerable<IGrouping<TGroup, TItem>>*/List<int> Items
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
        public MyCardsListTableViewSource(UITableView tableView)
        {
            tableView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            TableView = tableView;
            var cellType = typeof(MyCardsListTableViewCell);
            CellIdentifier = cellType.Name;
            TableView.RegisterClassForCellReuse(cellType, CellIdentifier);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (MyCardsListTableViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);// as MyCardsListTableViewCell;
            cell.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            cell.Bind(Items.ElementAt(indexPath.Row));
            cell.TextLabel.TextColor = UIColor.Green;
            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var row = indexPath.Item;

            //new CreatingCardViewController(intPtr).set_visibility();
            CreatingCardViewController.show_loader(CreatingCardViewController.datalist[(int)row].id);
            TableView.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Right);
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            cell.BackgroundColor = UIColor.FromRGB(36, 43, 52);
        }

        public override nint NumberOfSections(UITableView tableView) => 1;//Items?.Count() ?? 0;
        public override nint RowsInSection(UITableView tableview, nint section) //=> CreatingCardViewController.datalist.Count;//Items?.ElementAt((int)section).Count() ?? 0;
        {
            return CreatingCardViewController.datalist.Count;
        }
    }
}
