using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace CertificatesWallet;

public partial class CreateModal : ContentPage
{
    private Action _callback;

	public CreateModal(Action callback)
	{
        _callback = callback;
		InitializeComponent();
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameEntry.Text))
        {
            DisplayAlert("ERROR", "Name required.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(filenameEntry.Text))
        {
            DisplayAlert("ERROR", "Filename required.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(passwordEntry.Text))
        {
            DisplayAlert("ERROR", "Password required.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(daysEntry.Text))
        {
            DisplayAlert("ERROR", "Days valid required.", "OK");
            return;
        }


        string subject = "CN=" + nameEntry.Text;
        if (!string.IsNullOrEmpty(organizationEntry.Text))
            subject += ",O=" + organizationEntry.Text;
        if (!string.IsNullOrEmpty(orgUnitEntry.Text))
            subject += ",OU=" + orgUnitEntry.Text;
        if (!string.IsNullOrEmpty(localityEntry.Text))
            subject += ",L=" + localityEntry.Text;
        if (!string.IsNullOrEmpty(stateEntry.Text))
            subject += ",S=" + stateEntry.Text;
        if (!string.IsNullOrEmpty(countryEntry.Text))
            subject += ",C=" + countryEntry.Text;

        int days = int.Parse(daysEntry.Text);


        var rsa = RSA.Create(4096);
        var certRequest = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        //var subjectAlternativeNames = new SubjectAlternativeNameBuilder();
        //subjectAlternativeNames.AddDnsName("test");
        //certRequest.CertificateExtensions.Add(subjectAlternativeNames.Build());

        var certificate = certRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(365));

        var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);


        // Export the certificate
        var exportData = certificate.Export(X509ContentType.Pfx, passwordEntry.Text);

        string pfxFile = Path.Combine(FileSystem.Current.AppDataDirectory, filenameEntry.Text + ".pfx");
        File.WriteAllBytes(pfxFile, exportData);

        App.Current.MainPage.Navigation.PopModalAsync();
        _callback();
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        App.Current.MainPage.Navigation.PopModalAsync();
    }
}