using System;
using CoreNFC;
using Foundation;
using UIKit;

namespace NfcScan.iOS
{
    public partial class ViewController : UIViewController, INFCNdefReaderSessionDelegate
    {
        public ViewController(IntPtr handle) : base(handle) {}

        NFCNdefReaderSession session;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            scanButton.TouchUpInside += OnScanButtonClicked;

            CheckNfcAvailability();
        }

        private void CheckNfcAvailability()
        {
            // Check if device supports NFC reading
            if (NFCNdefReaderSession.ReadingAvailable)
            {
                LogMessage("NFC Reading is available.");
            }
            else
            {
                LogMessage("NFC Reading not available.");
                scanButton.Enabled = false;
            }
        }

        private void OnScanButtonClicked(object sender, EventArgs e)
        {
            LogMessage("Scanning startet.");

            // Initialize NDEF reading session and set delegate to this ViewController.
            session = new NFCNdefReaderSession(this, null, true);
            session.AlertMessage = "Approach NFC Tag to top of phone";

            session.BeginSession();
        }

        public void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            LogMessage("NFC Tag detected!");

            // Check if there are any NDEF messages
            if (messages.Length == 0)
            {
                LogMessage("No NDEF messages found!");
                return;
            }

            // Find first record
            var records = messages[0].Records;
            if (records.Length == 0)
            {
                LogMessage("No Records found!");
                return;
            }

            var record = records[0];

            // Extract Uri from record
            var text = (string)new NSString(record.Payload, NSStringEncoding.UTF8);
            var url = "http://" + text.Substring(1);

            LogMessage("NDEF formatted Tag found.");
            LogMessage("Url: " + url);

            //BeginInvokeOnMainThread(() =>
            //{
            //    UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
            //});
        }

        public void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            var readerError = (NFCReaderError)(long)error.Code;

            // Actually not an Error, we found a valid NDEF Tag
            if (readerError == NFCReaderError.ReaderSessionInvalidationErrorFirstNDEFTagRead)
            {
                LogMessage("Session finished.");
            }
            // When usser pressed Cancel button.
            else if (readerError == NFCReaderError.ReaderSessionInvalidationErrorUserCanceled)
            {
                LogMessage("Session cancelled");
            }
            // Show all other errors
            else
            {
                LogMessage("Scanning failed.");
                LogMessage("Error: " + readerError);
            }
        }

        private void LogMessage(string message)
        {
            BeginInvokeOnMainThread(() =>
            {
                statusTextView.Text += message + "\n";
            });
        }
    }
}
