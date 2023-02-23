using System.Buffers.Text;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CertificatesWallet;

public partial class MainPage : ContentPage
{
	private List<Button> buttons = new List<Button>();

	public MainPage()
	{
		InitializeComponent();
        string directory = FileSystem.Current.AppDataDirectory;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
		loadList();
    }

	private void loadList()
	{
		foreach(Button button in buttons)
			layout.Remove(button);
		buttons.Clear();
        
        foreach (var file in Directory.GetFiles(FileSystem.Current.AppDataDirectory))
        {
            if (file.EndsWith(".p12") || file.EndsWith(".pfx"))
            {
                string filepath = file;
                string filename = Path.GetFileName(filepath);
                
                var btn = new Button()
                {
                    Text = filename,
                    HorizontalOptions = LayoutOptions.Fill,
                    Style = App.FindResource("EntryButton") as Style
                };
                btn.Clicked += (object sender, EventArgs e) =>
                {
                    OnCertClicked(filepath);
                };
                layout.Add(btn);
            }
        }
    }

	private async void OnAddClick(object sender, EventArgs e)
	{
        var certFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.pkcs12" } }, // UTType values
                    { DevicePlatform.Android, new[] { "application/x-pkcs12" } }, // MIME type
                    { DevicePlatform.WinUI, new[] { ".p12", ".pfx" } }, // file extension
                    { DevicePlatform.Tizen, new[] { "*/*" } },
                    { DevicePlatform.macOS, new[] { "p12", "pfx" } }, // UTType values
                });

        PickOptions options = new()
        {
            PickerTitle = "Please select a certificate file",
            FileTypes = certFileType,
        };

        var result = await FilePicker.Default.PickAsync(options);
        if (result != null)
        {
            string filename = Path.GetFileName(result.FullPath);
            string filenpath = result.FullPath;

            if (filename.EndsWith(".p12", StringComparison.OrdinalIgnoreCase) ||
                filename.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(3000);
                await App.Current.MainPage.Navigation.PushModalAsync(new PasswordModal((string pswd) =>
                {
                    if (string.IsNullOrEmpty(pswd))
                        return;

                    try
                    {
                        X509Certificate2 cert = new X509Certificate2(filenpath, pswd);

                        File.Copy(filenpath, Path.Combine(FileSystem.Current.AppDataDirectory, filename), true);
                        loadList();
                    }
                    catch
                    {
                        DisplayAlert("Invalid", "Password or certificate is invalid or not supportet on this platform.", "OK");
                    }

                }));
            }
        }
    }

    private void OnScanClick(object sender, EventArgs e)
    {
        App.Current.MainPage.Navigation.PushModalAsync(new ScanQrModal((string url) =>
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith("certwallet:"))
                return;

            SignatureRequestJson request = JsonObjects.DeserializeRequest(url);


            App.Current.MainPage.Navigation.PushModalAsync(new SelectCertModal(request.Message, (string file) =>
            {
                if (string.IsNullOrEmpty(file))
                    return;

                App.Current.MainPage.Navigation.PushModalAsync(new PasswordModal((string password) =>
                {
                    if (string.IsNullOrEmpty(password))
                        return;

                    X509Certificate2 cert = new X509Certificate2(file, password);
                    SignatureResponseJson response = new SignatureResponseJson();

                    response.SessionId = request.SessionId;
                    response.Certificate = cert.Export(X509ContentType.Cert);


                    ECDsa key_ecdsa = cert.GetECDsaPrivateKey();
                    RSA key_rsa = cert.GetRSAPrivateKey();
#if !IOS && !MACCATALYST
                    DSA key_dsa = cert.GetDSAPrivateKey();
#endif
                    byte[] signature = new byte[0];

                    if (key_rsa != null)
                    {
                        signature = key_rsa.SignData(request.DataToSign, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                    }
                    else if (key_ecdsa != null)
                    {
                        signature = key_ecdsa.SignData(request.DataToSign, HashAlgorithmName.SHA512);
                    }
#if !IOS && !MACCATALYST
                    else if (key_dsa != null)
                    {
                        signature = key_dsa.SignData(request.DataToSign, HashAlgorithmName.SHA512);
                    }
#endif

                    response.Signature = signature;

                    string responseJson = JsonObjects.SerializeResponse(response);

                    using (var client = new HttpClient())
                    {
                        StringContent content = new StringContent(responseJson, Encoding.UTF8, "application/json");
                        client.PostAsync(request.EndpointUrl, content).Wait();
                    }
                }));


            }));
        }));
     }

    private void OnCertClicked(string file)
    {
        App.Current.MainPage.Navigation.PushModalAsync(new PasswordModal((string pswd) =>
        {
            if (string.IsNullOrEmpty(pswd))
                return;

            X509Certificate2 cert = new X509Certificate2(file, pswd);

            App.Current.MainPage.Navigation.PushAsync(new CertPage(cert));
        }));
    }

    private void OnCreateClick(object sender, EventArgs e)
    {
        App.Current.MainPage.Navigation.PushModalAsync(new CreateModal(() =>
        {
            loadList();
        }));
    }
}

