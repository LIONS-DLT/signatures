using System.Security.Cryptography.X509Certificates;

namespace CertificatesWallet;

public partial class CertPage : ContentPage
{
	public CertPage(X509Certificate2 cert)
	{
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
}