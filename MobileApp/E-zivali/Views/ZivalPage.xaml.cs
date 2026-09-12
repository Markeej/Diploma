using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class ZivalPage : ContentPage
{
    private Zival zival;
	public ZivalPage(Zival zival)
	{
		InitializeComponent();
        this.zival = zival;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        FirebaseFirestoreService firestore =
            new FirebaseFirestoreService();

        bool jeLastnik = await firestore.JeLastnikZivali(
            zival.Id,
            UserSession.Uid,
            UserSession.IdToken);

        IzbrisiZivalButton.IsVisible = jeLastnik;
    }

    private async void OnIzbrisiZivalClicked(object sender, EventArgs e)
    {
        bool potrdi = await DisplayAlert("Izbris živali", "Ali res želite izbrisati žival in vse povezane podatke? Tega dejanja ni mogoče razveljaviti.", "Izbriši", "Prekliči");

        if (!potrdi)
            return;

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        bool uspesno = await firestore.IzbrisiZivalInPodatke(zival.Id, UserSession.Uid, UserSession.IdToken);

        if (!uspesno)
        {
            await DisplayAlert("Napaka", "Živali ni bilo mogoče izbrisati.", "OK");

            return;
        }

        string mapaDokumentov = Path.Combine(FileSystem.AppDataDirectory, "documents", zival.Id);

        if (Directory.Exists(mapaDokumentov))
        {
            Directory.Delete(mapaDokumentov, true);
        }

        await DisplayAlert("Uspešno", "Žival in vsi povezani podatki so bili izbrisani.", "OK");

        await Navigation.PopAsync();
    }
    private async void OnPodatkiClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PodrobnostiZivaliPage(zival));
    }
    private async void OnDeljenjeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DeljenjeZivaliPage(zival));
    }
    private async void OnSledenjeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SledenjeZivaliPage(zival));
    }
    private async void OnDokumentiClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DokumentiZivaliPage(zival));
    }
    private async void OnZdravjeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ZdravjeZivaliPage(zival));
    }
    private async void OnOpomnikiClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OpomnikiZivaliPage(zival));
    }
}

