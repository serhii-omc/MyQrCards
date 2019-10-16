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
    [Register ("CloudSyncPremiumViewController")]
    partial class CloudSyncPremiumViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton backBn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView bgIV { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel headerLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView headerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lastSyncLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lastSyncValueLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView timerSyncBgIV { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (backBn != null) {
                backBn.Dispose ();
                backBn = null;
            }

            if (bgIV != null) {
                bgIV.Dispose ();
                bgIV = null;
            }

            if (headerLabel != null) {
                headerLabel.Dispose ();
                headerLabel = null;
            }

            if (headerView != null) {
                headerView.Dispose ();
                headerView = null;
            }

            if (lastSyncLabel != null) {
                lastSyncLabel.Dispose ();
                lastSyncLabel = null;
            }

            if (lastSyncValueLabel != null) {
                lastSyncValueLabel.Dispose ();
                lastSyncValueLabel = null;
            }

            if (timerSyncBgIV != null) {
                timerSyncBgIV.Dispose ();
                timerSyncBgIV = null;
            }
        }
    }
}