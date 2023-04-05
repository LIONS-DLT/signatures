using Plugin.NFC;

namespace CertificatesWallet;

public partial class NfcModal : ContentPage
{
    bool isWrite = false;

    private Action<string> _readCallback;

    public NfcModal(Action<string> callback)
    {
        isWrite = false;
        _readCallback = callback;
        InitializeComponent();

        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
        
        if (DeviceInfo.Platform != DevicePlatform.iOS)
            CrossNFC.Current.StartListening();
    }

    string _content;
    Action<bool> _writecallback;

    public NfcModal(string content, Action<bool> callback)
    {
        isWrite = true;
        _content = content;
        _writecallback = callback;
        InitializeComponent();

        CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
        CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;

        if (DeviceInfo.Platform != DevicePlatform.iOS)
            CrossNFC.Current.StartListening();
        CrossNFC.Current.StartPublishing(false);
    }


    private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        if (!isWrite)
            return;

        if (format)
        {
            Dispatcher.Dispatch(() =>
            {
                CrossNFC.Current.ClearMessage(tagInfo);
                DisplayAlert("Cleared.", "Tag formatted.", "OK");
            });
        }
        else
        {
            var record = new NFCNdefRecord
            {
                TypeFormat = NFCNdefTypeFormat.WellKnown,
                MimeType = "text/blanc",
                Payload = NFCUtils.EncodeToByteArray(_content)
            };
            tagInfo.Records = new NFCNdefRecord[] { record };
            tagInfo.IsWritable = true;
            
            Dispatcher.Dispatch(() =>
            {
                try
                {
                    CrossNFC.Current.PublishMessage(tagInfo, false);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error.", ex.Message, "OK");
                }
            });
        }
    }

    private void Current_OnMessagePublished(ITagInfo tagInfo)
    {
        if (!isWrite)
            return;
        // done.
        DisplayAlert("Completed.", "The key has been successfully written to your card.", "OK");
        close(true);
    }


    private void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        if (isWrite)
            return;
        Dispatcher.Dispatch(() =>
        {
            if (tagInfo.Records != null && tagInfo.Records.Length > 0)
                close(tagInfo.Records.Length.ToString());
            else
                close(null);
        });
    }

    private void CancelButton_Clicked(object sender, EventArgs e)
    {
        if (isWrite)
            close(false);
        else
            close(null);
    }

    void close(bool success)
    {
        if (DeviceInfo.Platform != DevicePlatform.iOS)
            CrossNFC.Current.StopListening();

        CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
        CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;

        App.Current.MainPage.Navigation.PopModalAsync();

        _writecallback(success);
    }

    private void close(string key)
    {
        if (DeviceInfo.Platform != DevicePlatform.iOS)
            CrossNFC.Current.StopListening();
        CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;

        App.Current.MainPage.Navigation.PopModalAsync();

        _readCallback(key);
    }
}