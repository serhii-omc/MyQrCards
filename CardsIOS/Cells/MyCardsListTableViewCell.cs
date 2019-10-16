using System;
using System.Collections.Generic;
using System.Drawing;
using CardsPCL;
using CardsPCL.CommonMethods;
using UIKit;

namespace CardsIOS.Cells
{
    public class MyCardsListTableViewCell : UITableViewCell
    {
        protected UILabel CardNameLabel { get; }
        protected UIImageView GoForwardImageView { get; }
        protected UIView DividerBottomView { get; }
        protected UIView DividerTopView { get; }
        public MyCardsListTableViewCell(IntPtr handle) : base(handle)
        {
            CardNameLabel = new UILabel
            {
                Lines = 0,
                TextAlignment = UITextAlignment.Justified,
                LineBreakMode = UILineBreakMode.WordWrap
            };

            GoForwardImageView = new UIImageView
            {
                Image = UIImage.FromBundle("go_forward.png"),
            };
            DividerBottomView = new UIView
            {
                BackgroundColor = UIColor.FromRGB(16, 28, 33)
            };
            DividerTopView = new UIView
            {
                BackgroundColor = UIColor.FromRGB(16, 28, 33)
            };

            BackgroundColor = UIColor.Clear;
            //ContentView.BackgroundColor = UIColor.FromRGB(36,43,52);


            CardNameLabel.Frame = new Rectangle((int)(CreatingCardViewController.viewWidth / 10),
                                                   0,
                                                   (int)(CreatingCardViewController.viewWidth - CreatingCardViewController.viewWidth / 13 - CreatingCardViewController.viewWidth / 10),
                                                   (int)CreatingCardViewController.cellHeight);

            GoForwardImageView.Frame = new Rectangle((int)(CreatingCardViewController.viewWidth - CreatingCardViewController.viewWidth / 13),
                                                (int)CreatingCardViewController.cellHeight / 5 * 2,
                                                (int)CreatingCardViewController.cellHeight / 10,
                                                (int)CreatingCardViewController.cellHeight / 5);
            DividerBottomView.Frame = new CoreGraphics.CGRect(0, CreatingCardViewController.cellHeight - 1, CreatingCardViewController.viewWidth, 0.5);
            DividerTopView.Frame = new CoreGraphics.CGRect(0, 0, CreatingCardViewController.viewWidth, 0.5);

            ContentView.AddSubviews(CardNameLabel, GoForwardImageView, DividerBottomView, DividerTopView);
        }

        public void Bind(object item)
        {
            //var index = SocialNetworkData.SampleData().FindIndex(x => x.Id == Convert.ToInt32(item));
            var index = CreatingCardViewController.datalist.FindIndex(x => x.id == Convert.ToInt32(item));
            if (index > 0)
                DividerTopView.Hidden = true;
            else
                DividerTopView.Hidden = false;
            //CardNameLabel.Text = SocialNetworkData.SampleData()[index].NameNetworkLabel;
            CardNameLabel.Text = CreatingCardViewController.datalist[index].name;
            CardNameLabel.Font = UIFont.FromName(Constants.fira_sans, 18f);
            CardNameLabel.TextColor = UIColor.White;
        }
    }
}
