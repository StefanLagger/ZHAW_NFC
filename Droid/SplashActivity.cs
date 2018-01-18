using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content;

namespace NfcScan.Droid
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/AppTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnResume()
        {
            base.OnResume();

            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}
