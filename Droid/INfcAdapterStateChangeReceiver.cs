using System;

namespace NfcScan.Droid
{
    public interface INfcAdapterStateChangeReceiver
    {
        void OnNfcAdapterStateChanged();
    }
}
