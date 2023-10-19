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
    }


    private void OnScanClick(object sender, EventArgs e)
    {
        App.Current.MainPage.Navigation.PushModalAsync(new ScanQrModal((string url) =>
        {
            if (string.IsNullOrEmpty(url))
                return;

            if (url.StartsWith("certwallet:"))
                onUrlScanned_certwallet(url);
            else if(url.StartsWith("cjws:"))
                onUrlScanned_cjws(url);
        }));
     }

    private void onUrlScanned_cjws(string url)
    {
        string[] urlParameters = url.Substring(5).Split('/');

        if (urlParameters[0] == "retrieve")
        {
            string retrieveUrl = Uri.UnescapeDataString(urlParameters[1]);
            string retrieveKey = Uri.UnescapeDataString(urlParameters[2]);
            byte[] data = DataExchangeService.RetrieveMessage(retrieveUrl);
            data = CryptHelper.DecryptAES(data, retrieveKey);

            string path = Path.Combine(FileSystem.Current.AppDataDirectory, DateTime.Now.Ticks.ToString() + ".cjws");
            File.WriteAllBytes(path, data);
            OnClickDocuments(null, null);
        }
        else if(urlParameters[0] == "send")
        {
            string sendUrl = Uri.UnescapeDataString(urlParameters[1]);
            string sendKey = Uri.UnescapeDataString(urlParameters[2]);

            App.Current.MainPage.Navigation.PushModalAsync(new SelectDocModal("select document...", (string file) =>
            {
                byte[] data = File.ReadAllBytes(file);
                data = CryptHelper.EncryptAES(data, sendKey);

                DataExchangeService.UploadMessageAsync(sendUrl, data);
            }));
        }
        else if (urlParameters[0] == "document")
        {
            string cjwsString = urlParameters[1];

            string path = Path.Combine(FileSystem.Current.AppDataDirectory, DateTime.Now.Ticks.ToString() + ".cjws");
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(cjwsString));
            OnClickDocuments(null, null);
        }
    }

    private void onUrlScanned_certwallet(string url)
    {
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
    }

    private void OnClickCertificates(object sender, EventArgs e)
    {
        App.Current.MainPage.Navigation.PushAsync(new CertificatesPage());
    }
    private void OnClickDocuments(object sender, EventArgs e)
    {
        App.Current.MainPage.Navigation.PushAsync(new DocumentsPage());
    }


}

