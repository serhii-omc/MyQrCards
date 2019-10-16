using System;
using System.Drawing;
using CardsIOS.TableViewSources;
using CardsPCL;
using CardsPCL.CommonMethods;
using UIKit;
using CardsIOS.NativeClasses;

namespace CardsIOS.Cells
{
    public class SocialNetworkTableViewCell : UITableViewCell
    {
        bool _isChecked;
        protected UILabel NameNetworkLabel { get; }
        protected UIImageView CheckImageView { get; }
        protected UIImageView LogoImageView { get; }
        protected UIImageView GoForwardImageView { get; }
        protected UIView DividerBottomView { get; }
        protected UIView DividerTopView { get; }
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                if (_isChecked)
                    CheckImageView.Hidden = false;
                else
                    CheckImageView.Hidden = true;

            }
        }

        public SocialNetworkTableViewCell(IntPtr handle) : base(handle)
        {
            NameNetworkLabel = new UILabel
            {
                Lines = 0,
                TextAlignment = UITextAlignment.Justified,
                LineBreakMode = UILineBreakMode.WordWrap
            };
            LogoImageView = new UIImageView { };
            CheckImageView = new UIImageView
            {
                Image = UIImage.FromBundle("check_mark.png"),
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
            CheckImageView.Frame = new Rectangle((int)(SocialNetworkViewController.viewWidth - SocialNetworkViewController.viewWidth / 4.2),
                                                (int)SocialNetworkViewController.cellHeight / 5 * 2,
                                                 (int)(SocialNetworkViewController.cellHeight / 3.3),
                                                (int)SocialNetworkViewController.cellHeight / 5);

            LogoImageView.Frame = new Rectangle(0,
                                                0,
                                                (int)SocialNetworkViewController.cellHeight,
                                                (int)SocialNetworkViewController.cellHeight);

            NameNetworkLabel.Frame = new Rectangle((int)(LogoImageView.Frame.X + LogoImageView.Frame.Width),
                                                   0,
                                                   (int)SocialNetworkViewController.viewWidth,
                                                   (int)SocialNetworkViewController.cellHeight);

            GoForwardImageView.Frame = new Rectangle((int)(SocialNetworkViewController.viewWidth - SocialNetworkViewController.viewWidth / 13),
                                                (int)SocialNetworkViewController.cellHeight / 5 * 2,
                                                (int)SocialNetworkViewController.cellHeight / 10,
                                                (int)SocialNetworkViewController.cellHeight / 5);
            DividerBottomView.Frame = new CoreGraphics.CGRect(0, SocialNetworkViewController.cellHeight - 1, SocialNetworkViewController.viewWidth, 0.5);
            DividerTopView.Frame = new CoreGraphics.CGRect(0, 0, SocialNetworkViewController.viewWidth, 0.5);

            ContentView.AddSubviews(NameNetworkLabel, LogoImageView, CheckImageView, GoForwardImageView, DividerBottomView, DividerTopView);
        }

        public void Bind(object item)
        {
            var index = SocialNetworkData.SampleData().FindIndex(x => x.Id == Convert.ToInt32(item));
            if (index > 0)
                DividerTopView.Hidden = true;
            else
                DividerTopView.Hidden = false;
            NameNetworkLabel.Text = SocialNetworkData.SampleData()[index].NameNetworkLabel;
            NameNetworkLabel.Font = UIFont.FromName(Constants.fira_sans, 18f);
            NameNetworkLabel.TextColor = UIColor.White;
            LogoImageView.Image = SocialNetworkData.SampleData()[index].Logo;
        }
    }
}
