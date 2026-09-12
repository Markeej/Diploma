using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class ZdravilaPage : ContentPage
{
    private Zival zival;

    public ZdravilaPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = zival.Ime;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await NaloziZdravila();
    }


    private async Task NaloziZdravila()
    {
        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        List<Zdravilo> zdravila = await firestore.PridobiZdravila(zival.Id, UserSession.IdToken);

        ZdravilaCollectionView.ItemsSource = zdravila;
    }


    private async void DodajZdraviloKlik(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DodajZdraviloPage(zival));
    }
    private async void IzbrisiZdraviloKlik(
    object sender,
    EventArgs e)
    {
        Button button = (Button)sender;

        Zdravilo? zdravilo = button.CommandParameter as Zdravilo;

        if (zdravilo == null)
            return;

        bool potrdi = await DisplayAlert("Izbris", "Ali �elite izbrisati zdravilo?", "Da", "Ne");

        if (!potrdi)
            return;

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        if (await firestore.IzbrisiZdravilo(zdravilo.Id, UserSession.IdToken))
        {
            await NaloziZdravila();
        }
    }
    private async void UrediZdraviloKlik(
   object sender,
   EventArgs e)
    {
        Button button = (Button)sender;

        Zdravilo? zdravilo = button.CommandParameter as Zdravilo;

        if (zdravilo == null)
            return;

        await Navigation.PushAsync(new DodajZdraviloPage(zival, zdravilo));
    }

}