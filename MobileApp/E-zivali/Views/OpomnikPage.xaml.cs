using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class OpomnikiPage : ContentPage
{
    public OpomnikiPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await NaloziOpomnike();
    }

    private async Task NaloziOpomnike()
    {
        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        List<Opomnik> opomniki = await firestore.PridobiVseOpomnikeUporabnika(UserSession.Uid, UserSession.IdToken);

        OpomnikiCollectionView.ItemsSource = opomniki;
    }

    private async void IzbrisiOpomnikKlik(
        object sender,
        EventArgs e)
    {
        Button button = (Button)sender;

        Opomnik? opomnik = button.CommandParameter as Opomnik;

        if (opomnik == null)
            return;

        bool potrdi = await DisplayAlert("Izbris", "Ali želite izbrisati opomnik?", "Da", "Ne");

        if (!potrdi)
            return;

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        bool uspesno = await firestore.IzbrisiOpomnik(opomnik.Id, UserSession.IdToken);

        if (uspesno)
        {
            await NaloziOpomnike();
        }
        else
        {
            await DisplayAlert("Napaka", "Opomnika ni bilo mogoče izbrisati.", "OK");
        }
    }
}