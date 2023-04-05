using System.Text;

namespace CertificatesWallet;

public partial class DocPage : ContentPage
{
    string _file;
	public DocPage(string file)
	{
        _file = file;
		InitializeComponent();

        string[] cjws = File.ReadAllText(file).Split('.');
        CJWSHeaderInfo info = CJWSHeaderInfo.FromString(cjws[0]);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(info.DisplayText);
        sb.AppendLine(info.ContentType);

        sb.AppendLine();

        try
        {
            byte[] data = CJWSHeaderInfo.DecodeUrlBase64(cjws[1]);
            sb.AppendLine(Encoding.UTF8.GetString(data));
        }
        catch
        {
            sb.AppendLine(cjws[1]);
        }


        infoLabel.Text = sb.ToString();

    }

    private void OnDeleteClick(object sender, EventArgs e)
    {
        File.Delete(_file);
        App.Current.MainPage.Navigation.PopToRootAsync();
    }
}