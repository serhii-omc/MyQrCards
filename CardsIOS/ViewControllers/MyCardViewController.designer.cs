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
    [Register ("MyCardViewController")]
    partial class MyCardViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView backgroundIV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView cardsLogo { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton createBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton enterBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel headerLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView headerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel infoLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton leftMenuBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel mainTextLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton plusBn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (backgroundIV != null) {
                backgroundIV.Dispose ();
                backgroundIV = null;
            }

            if (cardsLogo != null) {
                cardsLogo.Dispose ();
                cardsLogo = null;
            }

            if (createBn != null) {
                createBn.Dispose ();
                createBn = null;
            }

            if (enterBn != null) {
                enterBn.Dispose ();
                enterBn = null;
            }

            if (headerLabel != null) {
                headerLabel.Dispose ();
                headerLabel = null;
            }

            if (headerView != null) {
                headerView.Dispose ();
                headerView = null;
            }

            if (infoLabel != null) {
                infoLabel.Dispose ();
                infoLabel = null;
            }

            if (leftMenuBn != null) {
                leftMenuBn.Dispose ();
                leftMenuBn = null;
            }

            if (mainTextLabel != null) {
                mainTextLabel.Dispose ();
                mainTextLabel = null;
            }

            if (plusBn != null) {
                plusBn.Dispose ();
                plusBn = null;
            }
        }
    }
}