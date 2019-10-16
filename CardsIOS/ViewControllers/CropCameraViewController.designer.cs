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
    [Register ("CropCameraViewController")]
    partial class CropCameraViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton backBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView cadreIV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton cancelBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView footerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel headerLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView headerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView imageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton readyBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton retakeBn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (backBn != null) {
                backBn.Dispose ();
                backBn = null;
            }

            if (cadreIV != null) {
                cadreIV.Dispose ();
                cadreIV = null;
            }

            if (cancelBn != null) {
                cancelBn.Dispose ();
                cancelBn = null;
            }

            if (footerView != null) {
                footerView.Dispose ();
                footerView = null;
            }

            if (headerLabel != null) {
                headerLabel.Dispose ();
                headerLabel = null;
            }

            if (headerView != null) {
                headerView.Dispose ();
                headerView = null;
            }

            if (imageView != null) {
                imageView.Dispose ();
                imageView = null;
            }

            if (readyBn != null) {
                readyBn.Dispose ();
                readyBn = null;
            }

            if (retakeBn != null) {
                retakeBn.Dispose ();
                retakeBn = null;
            }
        }
    }
}