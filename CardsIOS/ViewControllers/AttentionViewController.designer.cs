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
    [Register ("AttentionViewController")]
    partial class AttentionViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton acceptBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView activityIndicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton backBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton cancelBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView cardsLogo { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel infoLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel mainTextTV { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (acceptBn != null) {
                acceptBn.Dispose ();
                acceptBn = null;
            }

            if (activityIndicator != null) {
                activityIndicator.Dispose ();
                activityIndicator = null;
            }

            if (backBn != null) {
                backBn.Dispose ();
                backBn = null;
            }

            if (cancelBn != null) {
                cancelBn.Dispose ();
                cancelBn = null;
            }

            if (cardsLogo != null) {
                cardsLogo.Dispose ();
                cardsLogo = null;
            }

            if (infoLabel != null) {
                infoLabel.Dispose ();
                infoLabel = null;
            }

            if (mainTextTV != null) {
                mainTextTV.Dispose ();
                mainTextTV = null;
            }
        }
    }
}