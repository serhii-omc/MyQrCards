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
    [Register ("PremiumViewController")]
    partial class PremiumViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView activityIndicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton backBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel headerLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView headerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton month_bn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView month_subscriptionTV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView premium_advantagesTV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView premium_detailsTV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton privacyPolicyBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView ratesTV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView subscriptionTillTV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton termsOfUseBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton year_bn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView year_subscriptionTV { get; set; }

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

            if (headerLabel != null) {
                headerLabel.Dispose ();
                headerLabel = null;
            }

            if (headerView != null) {
                headerView.Dispose ();
                headerView = null;
            }

            if (month_bn != null) {
                month_bn.Dispose ();
                month_bn = null;
            }

            if (month_subscriptionTV != null) {
                month_subscriptionTV.Dispose ();
                month_subscriptionTV = null;
            }

            if (premium_advantagesTV != null) {
                premium_advantagesTV.Dispose ();
                premium_advantagesTV = null;
            }

            if (premium_detailsTV != null) {
                premium_detailsTV.Dispose ();
                premium_detailsTV = null;
            }

            if (privacyPolicyBn != null) {
                privacyPolicyBn.Dispose ();
                privacyPolicyBn = null;
            }

            if (ratesTV != null) {
                ratesTV.Dispose ();
                ratesTV = null;
            }

            if (subscriptionTillTV != null) {
                subscriptionTillTV.Dispose ();
                subscriptionTillTV = null;
            }

            if (termsOfUseBn != null) {
                termsOfUseBn.Dispose ();
                termsOfUseBn = null;
            }

            if (year_bn != null) {
                year_bn.Dispose ();
                year_bn = null;
            }

            if (year_subscriptionTV != null) {
                year_subscriptionTV.Dispose ();
                year_subscriptionTV = null;
            }
        }
    }
}