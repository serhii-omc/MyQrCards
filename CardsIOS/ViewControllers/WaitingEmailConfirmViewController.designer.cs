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
    [Register ("WaitingEmailConfirmViewController")]
    partial class WaitingEmailConfirmViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView activityIndicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton backBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel emailLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView emailLogo { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView headerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel infoLabel1 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel infoLabel2 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel mainTextTV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton resendBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel timer_main_label { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel timer_valueLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView timerBgIV { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (activityIndicator != null) {
                activityIndicator.Dispose ();
                activityIndicator = null;
            }

            if (backBn != null) {
                backBn.Dispose ();
                backBn = null;
            }

            if (emailLabel != null) {
                emailLabel.Dispose ();
                emailLabel = null;
            }

            if (emailLogo != null) {
                emailLogo.Dispose ();
                emailLogo = null;
            }

            if (headerView != null) {
                headerView.Dispose ();
                headerView = null;
            }

            if (infoLabel1 != null) {
                infoLabel1.Dispose ();
                infoLabel1 = null;
            }

            if (infoLabel2 != null) {
                infoLabel2.Dispose ();
                infoLabel2 = null;
            }

            if (mainTextTV != null) {
                mainTextTV.Dispose ();
                mainTextTV = null;
            }

            if (resendBn != null) {
                resendBn.Dispose ();
                resendBn = null;
            }

            if (timer_main_label != null) {
                timer_main_label.Dispose ();
                timer_main_label = null;
            }

            if (timer_valueLabel != null) {
                timer_valueLabel.Dispose ();
                timer_valueLabel = null;
            }

            if (timerBgIV != null) {
                timerBgIV.Dispose ();
                timerBgIV = null;
            }
        }
    }
}