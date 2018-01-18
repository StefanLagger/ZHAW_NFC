using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Nfc;
using Android.Content;
using Android.Content.PM;
using Android.Views;

namespace NfcScan.Droid
{
    [Activity(
        Label = "Nfc Scan",
        Icon = "@mipmap/icon",
        Theme = "@style/AppTheme",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask
    )]
    [IntentFilter(
        new[] { NfcAdapter.ActionNdefDiscovered },
        DataSchemes = new[] { "http", "https" },
        Categories = new[] { Intent.CategoryDefault }
    )]
    public class MainActivity : Activity, INfcAdapterStateChangeReceiver
    {
        private static string InForgoundExtra = "ch.nfcscan.IN_FOREGROUND";

        private Button scanButton;
        private TextView statusTextView;
        private ProgressBar progressBar;

        private bool scanning = false;

        private NfcAdapter nfcAdapter;
        private NfcAdapterStateMonitor adapterStateMonitor;

        #region Android Activity Lifecycle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Find views and set references
            scanButton = FindViewById<Button>(Resource.Id.scanButton);
            statusTextView = FindViewById<TextView>(Resource.Id.statusTextView);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            scanButton.Click += OnScanButtonClicked;

            InitNfc();

            // App was started from discovered NFC Tag
            if (Intent.Action == NfcAdapter.ActionNdefDiscovered)
            {
                ReadNfcTag(Intent);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Dispatch all found NFC Tags to this App when active.
            if (nfcAdapter != null)
            {
                EnableForegroundDispatch();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Reenable global dispatch when App is in background.
            if (nfcAdapter != null)
            {
                nfcAdapter.DisableForegroundDispatch(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unregister adapter state monitor when quitted.
            if (adapterStateMonitor != null)
            {
                UnregisterReceiver(adapterStateMonitor);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // Handles found NFC Tags when app is running
            if (intent.Action == NfcAdapter.ActionNdefDiscovered)
            {
                bool IsForeground = intent.GetBooleanExtra(InForgoundExtra, false);

                if (IsForeground && scanning || !IsForeground)
                {
                    ReadNfcTag(intent);
                }

                StopScanning();
            }
        }

        #endregion

        private void InitNfc()
        {
            var nfcManager = (NfcManager)GetSystemService(NfcService);
            nfcAdapter = nfcManager.DefaultAdapter;

            if (nfcAdapter == null)
            {
                scanButton.Enabled = false;
                LogMessage("It seems like this device does not have an NFC Adapter.");
            }
            else
            {
                ShowNfcAvailability();

                // Register Broadcast Intent listener when NFC Adapter state changes
                adapterStateMonitor = new NfcAdapterStateMonitor(this);
                RegisterReceiver(adapterStateMonitor, new IntentFilter(NfcAdapter.ActionAdapterStateChanged));
            }
        }

        private void ShowNfcAvailability()
        {
            if (nfcAdapter.IsEnabled)
            {
                scanButton.Enabled = true;
                LogMessage("NFC is enabled. Ready to scan!");
            }
            else
            {
                scanButton.Enabled = false;
                LogMessage("NFC is not enabled, please go to settings and enable it.");
            }
        }

        private void EnableForegroundDispatch()
        {
            // Prepare pending Intent to call us back
            var intent = new Intent(this, typeof(MainActivity));
            intent.PutExtra(InForgoundExtra, true);
            var nfcTagFoundIntent = PendingIntent.GetActivity(this, 0, intent, 0);

            // Filter for NDEF messages with Uris
            var intentFilter = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
            intentFilter.AddDataScheme("http");
            intentFilter.AddDataScheme("https");
            IntentFilter[] intentFilters = { intentFilter };

            // Activate exclusive scanning for our App
            try
            {
                nfcAdapter.EnableForegroundDispatch(this, nfcTagFoundIntent, intentFilters, null);
            }
            catch (Exception e)
            {
                LogMessage("Failed to enable foreground dispatch:\n" + e.Message);
            }
        }

        private void OnScanButtonClicked(object sender, System.EventArgs e)
        {
            // Toggle scanning mode on and off
            if (scanning)
            {
                StopScanning();
                LogMessage("Scanning was stopped");
            }
            else
            {
                StartScanning();
                LogMessage("Started scanning for NFC Tag...");
            }
        }

        private void StartScanning()
        {
            if (!scanning)
            {
                scanButton.Text = "Stop scanning";
                progressBar.Visibility = ViewStates.Visible;
                scanning = true;
            }
        }

        private void StopScanning()
        {
            if (scanning)
            {
                scanButton.Text = "Start scanning";
                progressBar.Visibility = ViewStates.Invisible;
                scanning = false;
            }
        }

        public void OnNfcAdapterStateChanged()
        {
            ShowNfcAvailability();
        }

        private void ReadNfcTag(Intent intent)
        {
            // Find first NDEF message
            var parcellableNdefMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);

            NdefMessage ndefMessage = null;
            if (parcellableNdefMessages != null && parcellableNdefMessages.Length > 0)
            {
                ndefMessage = parcellableNdefMessages[0] as NdefMessage;
                LogMessage("NDEF formatted NFC Tag discovered.");
            }
            else
            {
                LogMessage("Error: Tag is not NDEF formatted. This is unexpected.");
                return;
            }

            // Find first record in NDEF message
            NdefRecord ndefRecord = null;
            var ndefRecords = ndefMessage.GetRecords();
            if (ndefRecords != null && ndefRecords.Length > 0)
            {
                LogMessage("Using first NDEF record.");
                ndefRecord = ndefRecords[0];
            }
            else
            {
                LogMessage("Error: No records found in NDEF Message.");
                return;
            }

            // Log Uri to output
            LogMessage(String.Format("TAG Uri: {0}\n", ndefRecord.ToUri()));


            //var openUriIntent = new Intent(Android.Content.Intent.ActionView, ndefRecord.ToUri());
            //StartActivity(openUriIntent);
        }

        private void LogMessage(string message)
        {
            RunOnUiThread(() =>
            {
                statusTextView.Text += message + "\n";
            });
        }
    }
}

