using E_zivali.Services;
using E_zivali.Models;

namespace E_zivali.Views;

public partial class DokumentiZivaliPage : ContentPage
{
    private Zival zival;

    public DokumentiZivaliPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = zival.Ime;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        List<Dokument> dokumenti = await firestore.PridobiDokumente(zival.Id, UserSession.IdToken);

        DokumentiCollectionView.ItemsSource = dokumenti;
    }

    private async void DodajDokumentKlik(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(new DodajDokumentPage(zival));
    }

    private async void OdpriDokumentKlik(
        object sender,
        EventArgs e)
    {
        Button button = (Button)sender;

        Dokument? dokument = button.CommandParameter as Dokument;

        if (dokument == null)
            return;

        if (!File.Exists(dokument.LokalnaPot))
        {
            await DisplayAlert("Napaka", "Datoteka ne obstaja.", "OK");

            return;
        }

        await Launcher.Default.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(dokument.LokalnaPot) });

    }

}