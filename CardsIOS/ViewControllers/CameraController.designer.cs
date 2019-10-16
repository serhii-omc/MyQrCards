// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CardsIOS
{
    [Register ("CameraController")]
    partial class CameraController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton autoFlashButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton cancelBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton captureButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton flashButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView flashOptionView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton offFlashButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton onFlashButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView previewView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton rotateBn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (autoFlashButton != null) {
                autoFlashButton.Dispose ();
                autoFlashButton = null;
            }

            if (cancelBn != null) {
                cancelBn.Dispose ();
                cancelBn = null;
            }

            if (captureButton != null) {
                captureButton.Dispose ();
                captureButton = null;
            }

            if (flashButton != null) {
                flashButton.Dispose ();
                flashButton = null;
            }

            if (flashOptionView != null) {
                flashOptionView.Dispose ();
                flashOptionView = null;
            }

            if (offFlashButton != null) {
                offFlashButton.Dispose ();
                offFlashButton = null;
            }

            if (onFlashButton != null) {
                onFlashButton.Dispose ();
                onFlashButton = null;
            }

            if (previewView != null) {
                previewView.Dispose ();
                previewView = null;
            }

            if (rotateBn != null) {
                rotateBn.Dispose ();
                rotateBn = null;
            }
        }
    }
}