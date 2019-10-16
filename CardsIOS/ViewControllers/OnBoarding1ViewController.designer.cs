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
    [Register ("OnBoarding1ViewController")]
    partial class OnBoarding1ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel accountExistsLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView accountView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView backgroundIV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView cardsLogo { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton enterBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel infoLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel mainTextTV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton nextBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton skipBn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (accountExistsLabel != null) {
                accountExistsLabel.Dispose ();
                accountExistsLabel = null;
            }

            if (accountView != null) {
                accountView.Dispose ();
                accountView = null;
            }

            if (backgroundIV != null) {
                backgroundIV.Dispose ();
                backgroundIV = null;
            }

            if (cardsLogo != null) {
                cardsLogo.Dispose ();
                cardsLogo = null;
            }

            if (enterBn != null) {
                enterBn.Dispose ();
                enterBn = null;
            }

            if (infoLabel != null) {
                infoLabel.Dispose ();
                infoLabel = null;
            }

            if (mainTextTV != null) {
                mainTextTV.Dispose ();
                mainTextTV = null;
            }

            if (nextBn != null) {
                nextBn.Dispose ();
                nextBn = null;
            }

            if (skipBn != null) {
                skipBn.Dispose ();
                skipBn = null;
            }
        }
    }
}