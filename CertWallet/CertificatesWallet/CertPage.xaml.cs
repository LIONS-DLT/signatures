using System.Security.Cryptography.X509Certificates;

namespace CertificatesWallet;

public partial class CertPage : ContentPage
{
	string _file;

	public CertPage(string file, X509Certificate2 cert)
	{
		_file = file;
		InitializeComponent();

		infoLabel.Text = string.Format(@"Issuer: {0}

Subject: {1}

Not before: {2}
Not after: {3}

Thumbprint: {4}",

            cert.Issuer, 
			cert.Subject, 
			cert.NotBefore.ToShortDateString(), 
			cert.NotAfter.ToShortDateString(),
			cert.Thumbprint);
	}


    private void OnDeleteClick(object sender, EventArgs e)
    {
        File.Delete(_file);
        App.Current.MainPage.Navigation.PopToRootAsync();
    }
}