namespace CertificatesWallet;

public partial class SelectCertModal : ContentPage
{
    Action<string> _callback;
    public SelectCertModal(string title, Action<string> callback)
    {
        _callback = callback;
        InitializeComponent();
        label.Text = title;

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
                    App.Current.MainPage.Navigation.PopModalAsync();
                    _callback(filepath);
                };
                layout.Add(btn);
            }
        }
    }


    private void OnCancelClick(object sender, EventArgs e)
    {
        App.Current.MainPage.Navigation.PopModalAsync();
        _callback(null);
    }
}