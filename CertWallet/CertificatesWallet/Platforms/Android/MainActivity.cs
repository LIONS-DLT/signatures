using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Nfc;
using Android.OS;
using Plugin.NFC;

namespace CertificatesWallet;


[IntentFilter(new[] { NfcAdapter.ActionTechDiscovered }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = "text/plain")]
[IntentFilter(new[] { NfcAdapter.ActionNdefDiscovered }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = "application/example")]
[IntentFilter(new[] { NfcAdapter.ActionTagDiscovered }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = "text/plain")]
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity/*, NfcAdapter.ICreateNdefMessageCallback*/
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        CrossNFC.Init(this);

        OnNewIntent(Intent);
    }
    protected override void OnResume()
    {
        base.OnResume();

        // Plugin NFC: Restart NFC listening on resume (needed for Android 10+) 
        CrossNFC.OnResume();
    }
    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);


        // Plugin NFC: Tag Discovery Interception
        CrossNFC.OnNewIntent(intent);

        var data = intent.DataString;

        if (intent.Action != Intent.ActionView) return;
        if (string.IsNullOrWhiteSpace(data)) return;

        //if (data.StartsWith(IdentityInterface.URLSCHEME + ":"))
        //{
        //    MauiProgram.OnDataReceived(data);
        //}
    }

}
