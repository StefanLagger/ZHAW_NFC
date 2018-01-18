using System;
using Android.Content;
using Android.Nfc;


namespace NfcScan.Droid
{
    public class NfcAdapterStateMonitor : BroadcastReceiver
    {
        private INfcAdapterStateChangeReceiver receiver;

        public NfcAdapterStateMonitor(INfcAdapterStateChangeReceiver receiver)
        {
            this.receiver = receiver;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == NfcAdapter.ActionAdapterStateChanged)
            {
                int adapterState = intent.GetIntExtra(NfcAdapter.ExtraAdapterState, NfcAdapter.StateOff);

                if (adapterState == NfcAdapter.StateOn || adapterState == NfcAdapter.StateOff)
                {
                    receiver.OnNfcAdapterStateChanged();
                }
            }
        }
    }
}
