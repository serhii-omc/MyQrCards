using System;

using UIKit;
using AVFoundation;
using Foundation;
using CoreGraphics;
using CoreAnimation;
using System.Drawing;
using System.Threading;
using CardsPCL;

namespace CardsIOS
{
    public partial class CameraController : UIViewController, IAVCapturePhotoCaptureDelegate//, IRotateAndScale
    {
        protected CameraController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        private AVCaptureSession captureSession;
        private AVCaptureVideoPreviewLayer previewLayer;
        private AVCapturePhotoOutput captureOutput;
        AVCaptureDevice captureDevice;
        private AVCaptureFlashMode flashMode = AVCaptureFlashMode.Auto;

        public static string came_from { get; set; }
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitElements();
            InitDevice();

            this.captureButton.TouchUpInside += (sender, e) =>
            {
                Thread.Sleep(400);
                try
                {
                    var _input = new AVCaptureDeviceInput(captureDevice, out NSError errw);
                    _input.Dispose();
                    this.HandleCapture();
                }
                catch (Exception ex)
                {
                    allowAndBack();
                }
            };

            rotateBn.TouchUpInside += (sender, e) =>
            {
                try
                {
                    HandleRotateCamera();
                }
                catch (Exception ex)
                {
                    allowAndBack();
                }
            };

            cancelBn.TouchUpInside += (sender, e) =>
            {
                this.NavigationController.PopViewController(true);
            };

            flashOptionView.Hidden = true;

            flashButton.TouchUpInside += (sender, e) =>
            {
                UpdateFlashView();
            };

            autoFlashButton.TouchUpInside += (sender, e) =>
            {
                flashMode = AVCaptureFlashMode.Auto;
                UpdateFlashView();

                autoFlashButton.SetTitleColor(UIColor.Yellow, UIControlState.Normal);
                onFlashButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                offFlashButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            };
            autoFlashButton.SetTitleColor(UIColor.Yellow, UIControlState.Normal);

            onFlashButton.TouchUpInside += (sender, e) =>
            {
                if (flashMode != AVCaptureFlashMode.On)
                {
                    flashMode = AVCaptureFlashMode.On;
                    onFlashButton.SetTitleColor(UIColor.Yellow, UIControlState.Normal);
                }
                else
                {
                    flashMode = AVCaptureFlashMode.Auto;
                    onFlashButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                }
                autoFlashButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                offFlashButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                UpdateFlashView();
            };

            offFlashButton.TouchUpInside += (sender, e) =>
            {
                if (flashMode != AVCaptureFlashMode.Off)
                {
                    flashMode = AVCaptureFlashMode.Off;
                    offFlashButton.SetTitleColor(UIColor.Yellow, UIControlState.Normal);
                }
                else
                {
                    flashMode = AVCaptureFlashMode.Auto;
                    offFlashButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                }
                autoFlashButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                onFlashButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                UpdateFlashView();
            };

            DetectRotation();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            AppDelegate.Instance();//.IsLockOrientation = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            AppDelegate.Instance();//.IsLockOrientation = false;
        }

        private void InitDevice()
        {
            captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video);

            AVCaptureDeviceInput input;
            try
            {
                input = new AVCaptureDeviceInput(captureDevice, out NSError err);

                if (err == null)
                {
                    captureSession = new AVCaptureSession();
                    captureSession.AddInput(input);

                    previewLayer = new AVCaptureVideoPreviewLayer(captureSession)
                    {
                        VideoGravity = AVLayerVideoGravity.ResizeAspectFill,
                        Frame = previewView.Layer.Bounds
                    };
                    previewView.Layer.AddSublayer(previewLayer);

                    captureOutput = new AVCapturePhotoOutput
                    {
                        IsHighResolutionCaptureEnabled = true
                    };
                    captureSession.AddOutput(captureOutput);
                    captureSession.StartRunning();
                }
            }
            catch (Exception ex)
            {
                allowAndBack();
            }
        }

        private void InitElements()
        {
            // Enable back navigation using swipe.
            NavigationController.InteractivePopGestureRecognizer.Delegate = null;
            this.NavigationController.NavigationBar.Hidden = true;

            flashOptionView.Frame = new CGRect(View.Frame.Width / 5, UIApplication.SharedApplication.StatusBarFrame.Height, View.Frame.Width / 5 * 3, View.Frame.Width / 10);
            flashButton.Frame = new CGRect(15, UIApplication.SharedApplication.StatusBarFrame.Height, flashOptionView.Frame.Height / 4 * 2.7, flashOptionView.Frame.Height / 4 * 2.7);

            View.BackgroundColor = UIColor.FromRGB(36, 43, 52);
            autoFlashButton.Frame = new Rectangle(0, 0, (int)flashOptionView.Frame.Width / 3, (int)flashOptionView.Frame.Height);
            onFlashButton.Frame = new Rectangle((int)(autoFlashButton.Frame.X + autoFlashButton.Frame.Width), 0, (int)flashOptionView.Frame.Width / 3, (int)flashOptionView.Frame.Height);
            offFlashButton.Frame = new Rectangle((int)(onFlashButton.Frame.X + onFlashButton.Frame.Width), 0, (int)flashOptionView.Frame.Width / 3, (int)flashOptionView.Frame.Height);


            captureButton.Frame = new Rectangle((int)((View.Frame.Width / 5) * 2), (int)(View.Frame.Height - View.Frame.Width / 5 - 10), (int)(View.Frame.Width / 5), (int)(View.Frame.Width / 5));
            cancelBn.Frame = new Rectangle((int)(View.Frame.Width / 18), (int)(captureButton.Frame.Y + captureButton.Frame.Height / 5 * 2), (int)(View.Frame.Width / 4), (int)(captureButton.Frame.Height / 5));
            rotateBn.Frame = new Rectangle((int)(View.Frame.Width - cancelBn.Frame.Width - View.Frame.Width / 18), (int)(captureButton.Frame.Y + captureButton.Frame.Height / 5 * 2), (int)(View.Frame.Width / 4), (int)(captureButton.Frame.Height / 5));

            cancelBn.SetTitle("Отменить", UIControlState.Normal);
            rotateBn.SetTitle("Сменить", UIControlState.Normal);
            cancelBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            rotateBn.Font = UIFont.FromName(Constants.fira_sans, 17f);
            autoFlashButton.Font = UIFont.FromName(Constants.fira_sans, 17f);
            onFlashButton.Font = UIFont.FromName(Constants.fira_sans, 17f);
            offFlashButton.Font = UIFont.FromName(Constants.fira_sans, 17f);
            this.captureButton.Layer.CornerRadius = this.captureButton.Frame.Width / 2;
        }

        private void UpdateFlashView()
        {
            var state = flashOptionView.Hidden;
            flashOptionView.Hidden = state ? flashOptionView.Hidden = false : flashOptionView.Hidden = true;
        }

       

        //public override bool PrefersStatusBarHidden()
        //{
        //    return true;
        //}
        void allowAndBack()
        {
            UIAlertView alert = new UIAlertView()
            {
                Title = "Ошибка",
                Message = "Необходимо разрешить приложению доступ к камере."
            };
            alert.AddButton("OK");
            alert.Show();
            this.NavigationController.PopViewController(true);
        }
        private void HandleCapture()
        {
            if (captureOutput == null) return;

            var photoSettings = AVCapturePhotoSettings.Create();
            photoSettings.IsAutoStillImageStabilizationEnabled = true;
            photoSettings.IsHighResolutionPhotoEnabled = true;
            photoSettings.FlashMode = flashMode;

            captureOutput.CapturePhoto(photoSettings, this);
        }

        private void DetectRotation()
        {
            UIDevice.Notifications.ObserveOrientationDidChange((sender, args) =>
            {
                UIView.Animate(0.3, () =>
                {
                    switch (UIDevice.CurrentDevice.Orientation)
                    {
                        case UIDeviceOrientation.Portrait:
                            //rotateCameraButton.Transform = CGAffineTransform.MakeRotation(0);
                            //flashButton.Transform = CGAffineTransform.MakeRotation(0);
                            break;
                        case UIDeviceOrientation.LandscapeRight:
                            //rotateCameraButton.Transform = CGAffineTransform.MakeRotation(-(float)Math.PI / 2);
                            //flashButton.Transform = CGAffineTransform.MakeRotation(-(float)Math.PI / 2);
                            break;
                        case UIDeviceOrientation.LandscapeLeft:
                            //rotateCameraButton.Transform = CGAffineTransform.MakeRotation((float)Math.PI / 2);
                            //flashButton.Transform = CGAffineTransform.MakeRotation((float)Math.PI / 2);
                            break;
                        case UIDeviceOrientation.PortraitUpsideDown:
                            //rotateCameraButton.Transform = CGAffineTransform.MakeRotation(-(float)Math.PI);
                            //flashButton.Transform = CGAffineTransform.MakeRotation(-(float)Math.PI);
                            break;
                    }
                });
            });
        }

        private void HandleRotateCamera()
        {
            captureSession.BeginConfiguration();

            var currentCameraInput = captureSession.Inputs[0];
            captureSession.RemoveInput(currentCameraInput);

            AVCaptureDevice camera;
            AVCaptureDeviceInput input = (AVCaptureDeviceInput)currentCameraInput;
            if (input.Device.Position == AVCaptureDevicePosition.Back)
            {
                camera = CameraWithPosition(AVCaptureDevicePosition.Front);
            }
            else
            {
                camera = CameraWithPosition(AVCaptureDevicePosition.Back);
            }

            var videoInput = new AVCaptureDeviceInput(camera, out NSError err);
            if (err == null)
                captureSession.AddInput(videoInput);

            captureSession.CommitConfiguration();

            AddFlipAnimation();
        }

        private void AddFlipAnimation()
        {
            //captureButton.Enabled = false;
            //UIView.Transition(previewView, 0.5, UIViewAnimationOptions.TransitionFlipFromLeft | UIViewAnimationOptions.AllowAnimatedContent, null, () => {
            //    captureButton.Enabled = true;
            //});
        }

        private AVCaptureDevice CameraWithPosition(AVCaptureDevicePosition pos)
        {
            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            foreach (var dev in devices)
            {
                if (dev.Position == pos)
                {
                    return dev;
                }
            }
            return null;
        }

        [Export("captureOutput:didFinishProcessingPhotoSampleBuffer:previewPhotoSampleBuffer:resolvedSettings:bracketSettings:error:")]
        public void DidFinishProcessingPhoto(AVCapturePhotoOutput captureOutput, CoreMedia.CMSampleBuffer photoSampleBuffer, CoreMedia.CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
        {
            var imageData = AVCapturePhotoOutput.GetJpegPhotoDataRepresentation(photoSampleBuffer, previewPhotoSampleBuffer);
            if (imageData != null)
            {
                var capturedImage = new UIImage(imageData);


                var storyboard = UIStoryboard.FromName("Main", NSBundle.MainBundle);
                UIViewController vc = new UIViewController();
                if (came_from == Constants.personal)
                {
                    CropCameraViewController.currentImage = capturedImage;
                    vc = storyboard.InstantiateViewController(nameof(CropCameraViewController));
                }
                else if (came_from == Constants.company_logo)
                {
                    CropCompanyLogoViewController.currentImage = capturedImage;
                    vc = storyboard.InstantiateViewController(nameof(CropCompanyLogoViewController));
                }

                this.NavigationController.PushViewController(vc, true);
            }
        }

        public void DidSelectedImage(UIImage image)
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName("GetImageNotification", image);
            this.NavigationController.PopViewController(true);
        }
    }
}
