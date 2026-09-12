using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class CepljenjaPage : ContentPage
{
    private Zival zival;

    public CepljenjaPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = zival.Ime;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await NaloziCepljenja();
    }

    private async Task NaloziCepljenja()
    {
        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        List<Cepljenje> cepljenja = await firestore.PridobiCepljenja(zival.Id, UserSession.IdToken);

        CepljenjaCollectionView.ItemsSource = cepljenja;
    }

    private async void DodajCepljenjeKlik(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(new DodajCepljenjePage(zival));
    }
    private async void IzbrisiCepljenjeKlik(
    object sender,
    EventArgs e)
    {
        Button button = (Button)sender;

        Cepljenje? cepljenje = button.CommandParameter as Cepljenje;

        if (cepljenje == null)
            return;

        bool potrdi = await DisplayAlert("Izbris", "Ali �elite izbrisati cepljenje?", "Da", "Ne");

        if (!potrdi)
            return;

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        bool uspesno = await firestore.IzbrisiCepljenje(cepljenje.Id, UserSession.IdToken);

        if (uspesno)
        {
            await NaloziCepljenja();
        }
    }
    private async void UrediCepljenjeKlik(
    object sender,
    EventArgs e)
    {
        Button button = (Button)sender;

        Cepljenje? cepljenje = button.CommandParameter as Cepljenje;

        if (cepljenje == null)
            return;

        await Navigation.PushAsync(new DodajCepljenjePage(zival, cepljenje));
    }
}
