using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;

namespace CertificatesWallet;

public partial class CertificatesPage : ContentPage
{
    private List<Button> buttons = new List<Button>();

    public CertificatesPage()
    {
        InitializeComponent();
        loadList();
    }

    private void loadList()
    {
        foreach (Button button in buttons)
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

    private void OnCertClicked(string file)
    {
        App.Current.MainPage.Navigation.PushModalAsync(new PasswordModal((string pswd) =>
        {
            if (string.IsNullOrEmpty(pswd))
                return;

            X509Certificate2 cert = new X509Certificate2(file, pswd);

            App.Current.MainPage.Navigation.PushAsync(new CertPage(file, cert));
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