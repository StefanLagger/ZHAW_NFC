// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace NfcScan.iOS
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        UIKit.UIButton Button { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton scanButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView statusTextView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (scanButton != null) {
                scanButton.Dispose ();
                scanButton = null;
            }

            if (statusTextView != null) {
                statusTextView.Dispose ();
                statusTextView = null;
            }
        }
    }
}