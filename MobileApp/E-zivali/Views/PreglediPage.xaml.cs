using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class PreglediPage : ContentPage
{
    private Zival zival;

    public PreglediPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = zival.Ime;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await NaloziPreglede();
    }


    private async Task NaloziPreglede()
    {
        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        List<Pregled> pregledi = await firestore.PridobiPreglede(zival.Id, UserSession.IdToken);

        PreglediCollectionView.ItemsSource = pregledi;
    }


    private async void DodajPregledKlik(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(new DodajPregledPage(zival));
    }
    private async void IzbrisiPregledKlik(
    object sender,
    EventArgs e)
    {
        Button button = (Button)sender;

        Pregled? pregled = button.CommandParameter as Pregled;

        if (pregled == null)
            return;

        bool potrdi = await DisplayAlert("Izbris", "Ali �elite izbrisati pregled?", "Da", "Ne");

        if (!potrdi)
            return;

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        if (await firestore.IzbrisiPregled(pregled.Id, UserSession.IdToken))
        {
            await NaloziPreglede();
        }
    }
    private async void UrediPregledKlik(
    object sender,
    EventArgs e)
    {
        Button button = (Button)sender;

        Pregled? pregled = button.CommandParameter as Pregled;

        if (pregled == null)
            return;

        await Navigation.PushAsync(new DodajPregledPage(zival, pregled));
    }
}