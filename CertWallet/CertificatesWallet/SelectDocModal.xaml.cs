namespace CertificatesWallet;

public partial class SelectDocModal : ContentPage
{
    Action<string> _callback;
    public SelectDocModal(string title, Action<string> callback)
    {
        _callback = callback;
        InitializeComponent();
        label.Text = title;

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
                    App.Current.MainPage.Navigation.PopModalAsync();
                    _callback(filepath);
                };
                layout.Insert(layout.IndexOf(btnCancel), btn);
            }
        }
    }


    private void OnCancelClick(object sender, EventArgs e)
    {
        App.Current.MainPage.Navigation.PopModalAsync();
        _callback(null);
    }
}