using CardsPCL;
using CoreAnimation;
using CoreGraphics;
using System;
using System.Drawing;
using System.Linq;
using UIKit;

namespace CardsIOS
{
    public partial class CropCompanyLogoViewController : UIViewController
    {
        public static UIImage currentImage { get; set; }
        public static UIImage cropped_result { get; set; }
        private RectangleF originalImageFrame = RectangleF.Empty;
        nfloat x, y, w, h;
        UIColor maskColor = UIColor.Black.ColorWithAlpha((nfloat)0.7);
        UILabel hint_label;
        public static string came_from { get; set; }
        public static string came_from_gallery_or_camera { get; set; }
        UIPinchGestureRecognizer pinchGesture;
        public CropCompanyLogoViewController(IntPtr handle) : base(handle)
        {
        }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();

            var circleLayer = new CAShapeLayer();
            var circlePath = UIBezierPath.FromRect(cadreIV.Frame);
            circlePath.UsesEvenOddFillRule = true;
            circleLayer.Path = circlePath.CGPath;
            var maskPath = UIBezierPath.FromRect(new CGRect(0, (int)headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2, View.Frame.Width, (int)(View.Frame.Height - (headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2 + View.Frame.Width / 8))));
            maskPath.AppendPath(circlePath);
            maskPath.UsesEvenOddFillRule = true;
            fillLayer.Path = maskPath.CGPath;
            fillLayer.FillRule = CAShapeLayer.FillRuleEvenOdd;
            fillLayer.FillColor = maskColor.CGColor;

            View.Layer.AddSublayer(fillLayer);

            nfloat width_height_ratio;
            int image_height, image_width;
            if (imageView.Image.Size.Height > imageView.Image.Size.Width)
            {
                width_height_ratio = imageView.Image.Size.Height / imageView.Image.Size.Width;
                image_width = (int)View.Frame.Width;
                image_height = (int)(image_width * width_height_ratio);
                imageView.Frame = new Rectangle(0, (int)(headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2), image_width, image_height);
            }
            else if (imageView.Image.Size.Height < imageView.Image.Size.Width)
            {
                width_height_ratio = imageView.Image.Size.Width / imageView.Image.Size.Height;
                image_width = (int)View.Frame.Width;
                image_height = (int)(image_width / width_height_ratio);
                imageView.Frame = new Rectangle(0, (int)(headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2), image_width, image_height);
            }
            else if (imageView.Image.Size.Height == imageView.Image.Size.Width)
            {
                image_width = (int)View.Frame.Width;
                image_height = image_width;
                imageView.Frame = new Rectangle(0, (int)(headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2), image_width, image_height);
            }

            backBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PopViewController(true);
            };
            pinchGesture = new UIPinchGestureRecognizer(() =>
            {
                if (pinchGesture.State == UIGestureRecognizerState.Began || pinchGesture.State == UIGestureRecognizerState.Changed)
                {
                    if (cadreIV.Frame.X > 0 && cadreIV.Frame.Y > (int)(headerView.Frame.Y + headerView.Frame.Height) && cadreIV.Frame.X + cadreIV.Frame.Width < View.Frame.Width && cadreIV.Frame.Y + cadreIV.Frame.Height < (imageView.Frame.Height + imageView.Frame.Y))
                    {
                        pinchGesture.View.Transform *= CGAffineTransform.MakeScale(pinchGesture.Scale, pinchGesture.Scale);
                        pinchGesture.Scale = 1;

                        circleLayer = new CAShapeLayer();
                        circlePath = UIBezierPath.FromRect(cadreIV.Frame);
                        circlePath.UsesEvenOddFillRule = true;
                        circleLayer.Path = circlePath.CGPath;
                        maskPath = UIBezierPath.FromRect(new CGRect(0, (int)headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2, View.Frame.Width, (int)(View.Frame.Height - (headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2 + View.Frame.Width / 8))));
                        maskPath.AppendPath(circlePath);
                        maskPath.UsesEvenOddFillRule = true;
                        fillLayer.Path = maskPath.CGPath;
                        fillLayer.FillRule = CAShapeLayer.FillRuleEvenOdd;

                        View.Layer.AddSublayer(fillLayer);
                    }
                    else
                    {
                        if (pinchGesture.Scale < 1)
                        {
                            pinchGesture.View.Transform *= CGAffineTransform.MakeScale(pinchGesture.Scale, pinchGesture.Scale);
                            pinchGesture.Scale = 1;

                            circleLayer = new CAShapeLayer();
                            circlePath = UIBezierPath.FromRect(cadreIV.Frame);
                            circlePath.UsesEvenOddFillRule = true;
                            circleLayer.Path = circlePath.CGPath;
                            maskPath = UIBezierPath.FromRect(new CGRect(0, (int)headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2, View.Frame.Width, (int)(View.Frame.Height - (headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2 + View.Frame.Width / 8))));
                            maskPath.AppendPath(circlePath);
                            maskPath.UsesEvenOddFillRule = true;
                            fillLayer.Path = maskPath.CGPath;
                            fillLayer.FillRule = CAShapeLayer.FillRuleEvenOdd;

                            View.Layer.AddSublayer(fillLayer);
                        }
                    }
                }
            });
            cadreIV.AddGestureRecognizer(pinchGesture);
            UIStoryboard sb = UIStoryboard.FromName("Main", null);
            //CropGalleryViewController.currentImage = current_imgBn.CurrentImage;
            var vc = sb.InstantiateViewController(nameof(CompanyDataViewControllerNew));
            if (came_from == "edit")
                vc = sb.InstantiateViewController(nameof(EditCompanyDataViewControllerNew));
            readyBn.TouchUpInside += (s, e) =>
            {
                x = cadreIV.Frame.X;
                y = cadreIV.Frame.Y;
                w = cadreIV.Frame.Width;
                h = cadreIV.Frame.Height;

                var wwww1 = (float)(imageView.Image.Size.Width / View.Frame.Width);
                var hhhh1 = (float)(imageView.Image.Size.Height / imageView.Frame.Height);
                var ppp = (float)(cadreIV.Frame.X * wwww1);
                var lll = (float)(cadreIV.Frame.Y * hhhh1 - imageView.Frame.Y * wwww1);
                var www = (float)(cadreIV.Frame.Width * wwww1);
                var hhh = (float)(cadreIV.Frame.Height * hhhh1);

                cropped_result = CenterCrop(imageView.Image, new RectangleF(ppp, lll, www, hhh));

                this.NavigationController.PushViewController(vc, true);
                var vc_list = this.NavigationController.ViewControllers.ToList();
                vc_list.RemoveAt(vc_list.Count - 2);
                vc_list.RemoveAt(vc_list.Count - 2);
                if (came_from_gallery_or_camera == Constants.camera)
                    vc_list.RemoveAt(vc_list.Count - 2);
                this.NavigationController.ViewControllers = vc_list.ToArray();
            };
            cancelBn.TouchUpInside += (s, e) =>
            {
                this.NavigationController.PushViewController(vc, true);
                //this.NavigationController.PopViewController(true);
            };
            remove_imageBn.TouchUpInside += (s, e) =>
            {
                EditCompanyDataViewControllerNew.logo_id = null;
                cropped_result = null;
                currentImage = null;
                this.NavigationController.PushViewController(vc, true);
                var vc_list = this.NavigationController.ViewControllers.ToList();
                vc_list.RemoveAt(vc_list.Count - 2);
                vc_list.RemoveAt(vc_list.Count - 2);
                if (came_from_gallery_or_camera == Constants.camera)
                    vc_list.RemoveAt(vc_list.Count - 2);
                this.NavigationController.ViewControllers = vc_list.ToArray();
            };
            readyBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            cancelBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            remove_imageBn.Font = UIFont.FromName(Constants.fira_sans, 17f);

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
                remove_imageBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), Convert.ToInt32(View.Frame.Width) / 12 + 20, Convert.ToInt32(View.Frame.Width) / 4, Convert.ToInt32(View.Frame.Width) / 19);
            }
            else
            {
                headerView.Frame = new Rectangle(0, 0, Convert.ToInt32(View.Frame.Width), (Convert.ToInt32(View.Frame.Height) / 10));
                backBn.Frame = new Rectangle(0, Convert.ToInt32(View.Frame.Width) / 20, Convert.ToInt32(View.Frame.Width) / 8, Convert.ToInt32(View.Frame.Width) / 8);
                headerLabel.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width) / 5, Convert.ToInt32(View.Frame.Width) / 12, (Convert.ToInt32(View.Frame.Width) / 5) * 3, Convert.ToInt32(View.Frame.Width) / 18);
                remove_imageBn.Frame = new Rectangle(Convert.ToInt32(View.Frame.Width - View.Frame.Width / 4), Convert.ToInt32(View.Frame.Width) / 12, Convert.ToInt32(View.Frame.Width) / 4, Convert.ToInt32(View.Frame.Width) / 19);
            }
            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerView.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            headerLabel.Text = "Лого";
            remove_imageBn.SetTitle("Удалить", UIControlState.Normal);
            backBn.ImageEdgeInsets = new UIEdgeInsets(backBn.Frame.Height / 3.5F, backBn.Frame.Width / 2.35F, backBn.Frame.Height / 3.5F, backBn.Frame.Width / 3);
            imageView.Image = currentImage;
            imageView.Frame = new Rectangle(0, (int)(headerView.Frame.Y + headerView.Frame.Height), (int)(View.Frame.Width), (int)((View.Frame.Width / 4) * 3));
            footerView.Frame = new Rectangle(0, (int)(View.Frame.Height - View.Frame.Width / 8), (int)(View.Frame.Width), (int)(View.Frame.Height / 8));
            cancelBn.Frame = new Rectangle(0, 0, (int)(View.Frame.Width / 3), (int)(View.Frame.Width / 8));
            cancelBn.SetTitle("Отмена", UIControlState.Normal);
            readyBn.Frame = new Rectangle((int)(View.Frame.Width - View.Frame.Width / 3), 0, (int)(View.Frame.Width / 3), (int)(View.Frame.Width / 8));
            readyBn.SetTitle("Готово", UIControlState.Normal);
            cadreIV.Frame = new Rectangle(Convert.ToInt32(View.Frame.X) + 15, (int)(headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2) + 5, Convert.ToInt32(imageView.Frame.Width / 1.5), Convert.ToInt32(imageView.Frame.Width / 1.5));
            // Save initial state
            originalImageFrame = (System.Drawing.RectangleF)cadreIV.Frame;
            cadreIV.UserInteractionEnabled = true;
            cadreIV.MultipleTouchEnabled = true;
            //WireUpTapGestureRecognizer();
            WireUpDragGestureRecognizer();

            hint_label = new UILabel
            {
                Text = "Сдвиг и масштаб",
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.FromRGB(255, 99, 62),
            };

            hint_label.Frame = new CGRect(0, headerView.Frame.Height, View.Frame.Width, headerView.Frame.Height / 3);
            View.AddSubview(hint_label);
        }

        public UIImage CenterCrop(UIImage image, CGRect rect)
        {
            // Use smallest side length as crop square length
            double squareLength = Math.Min(image.Size.Width, image.Size.Height);

            //This Rect defines the coordinates to be used for the crop
            var croppedRect = rect;

            // Center-Crop the image
            UIGraphics.BeginImageContextWithOptions(croppedRect.Size, false, image.CurrentScale);
            image.Draw(new CGPoint(-croppedRect.X, -croppedRect.Y));
            UIImage croppedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return croppedImage;
        }

        private void WireUpDragGestureRecognizer()
        {
            // Create a new tap gesture
            UIPanGestureRecognizer gesture = new UIPanGestureRecognizer();

            // Wire up the event handler (have to use a selector)
            gesture.AddTarget(() => HandleDrag(gesture));  // to be defined

            // Add the gesture recognizer to the view
            cadreIV.AddGestureRecognizer(gesture);
        }

        public void Draw()
        {
            //get graphics context
            using (CGContext g = UIGraphics.GetCurrentContext())
            {
                CGRect rect = new CGRect(x, y, w, h);

                g.AddEllipseInRect(rect);
                g.Clip();
                g.ClearRect(rect);
            }
        }
        CAShapeLayer fillLayer = new CAShapeLayer();
        private void HandleDrag(UIPanGestureRecognizer recognizer)
        {
            // If it's just began, cache the location of the image
            if (recognizer.State == UIGestureRecognizerState.Began)
            {
                originalImageFrame = (System.Drawing.RectangleF)cadreIV.Frame;
            }

            // Move the image if the gesture is valid
            if (recognizer.State != (UIGestureRecognizerState.Cancelled | UIGestureRecognizerState.Failed
                | UIGestureRecognizerState.Possible))
            {
                // Move the image by adding the offset to the object's frame
                PointF offset = (System.Drawing.PointF)recognizer.TranslationInView(cadreIV);
                RectangleF newFrame = originalImageFrame;

                var offsetX = offset.X;
                newFrame.Offset(offset.X, offset.Y);
                if (newFrame.X > 0 && newFrame.Y > (int)(headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2) && newFrame.X + cadreIV.Frame.Width < View.Frame.Width && newFrame.Y + cadreIV.Frame.Height < (imageView.Frame.Height + imageView.Frame.Y))
                {
                    cadreIV.Frame = newFrame;

                    var circleLayer = new CAShapeLayer();
                    var circlePath = UIBezierPath.FromRect(cadreIV.Frame);
                    circlePath.UsesEvenOddFillRule = true;
                    circleLayer.Path = circlePath.CGPath;
                    var maskPath = UIBezierPath.FromRect(new CGRect(0, (int)headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2, View.Frame.Width, (int)(View.Frame.Height - (headerView.Frame.Y + headerView.Frame.Height + headerView.Frame.Height / 2 + View.Frame.Width / 8))));
                    maskPath.AppendPath(circlePath);
                    maskPath.UsesEvenOddFillRule = true;
                    fillLayer.Path = maskPath.CGPath;
                    fillLayer.FillRule = CAShapeLayer.FillRuleEvenOdd;

                    View.Layer.AddSublayer(fillLayer);
                }
            }
        }
    }
}