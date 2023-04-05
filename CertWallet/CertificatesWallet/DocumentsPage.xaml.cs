using System.Security.Cryptography.X509Certificates;

namespace CertificatesWallet;

public partial class DocumentsPage : ContentPage
{
    private List<Button> buttons = new List<Button>();

    public DocumentsPage()
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
            if (file.EndsWith(".cjws"))
            {
                string filepath = file;
                CJWSHeaderInfo info = CJWSHeaderInfo.FromString(File.ReadAllText(filepath).Split('.')[0]);

                var btn = new Button()
                {
                    Text = string.Format("{0}\n{1}", info.DisplayText, info.ContentType),
                    HorizontalOptions = LayoutOptions.Fill,
                    Style = App.FindResource("EntryButton") as Style
                };
                btn.Clicked += (object sender, EventArgs e) =>
                {
                    OnDocumentClicked(filepath);
                };
                layout.Add(btn);
            }
        }
    }

    private void OnDocumentClicked(string file)
    {
        App.Current.MainPage.Navigation.PushAsync(new DocPage(file));
    }
}