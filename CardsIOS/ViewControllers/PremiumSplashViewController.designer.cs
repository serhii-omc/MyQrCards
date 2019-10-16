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
    [Register ("PremiumSplashViewController")]
    partial class PremiumSplashViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton backBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView cardsLogo { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton detailsBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel infoLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel mainTextTV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton thanksBn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (backBn != null) {
                backBn.Dispose ();
                backBn = null;
            }

            if (cardsLogo != null) {
                cardsLogo.Dispose ();
                cardsLogo = null;
            }

            if (detailsBn != null) {
                detailsBn.Dispose ();
                detailsBn = null;
            }

            if (infoLabel != null) {
                infoLabel.Dispose ();
                infoLabel = null;
            }

            if (mainTextTV != null) {
                mainTextTV.Dispose ();
                mainTextTV = null;
            }

            if (thanksBn != null) {
                thanksBn.Dispose ();
                thanksBn = null;
            }
        }
    }
}