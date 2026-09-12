using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class MojeZivaliPage : ContentPage
{
    public MojeZivaliPage()
    {
        InitializeComponent();

        NaloziZivali();
    }

    private async void OnDodajClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DodajZivalPage));
    }
    private async void ZivalKlik(object sender, TappedEventArgs e)
    {
        Zival? zival = e.Parameter as Zival;

        if (zival == null)
        {
            return;
        }

        await Navigation.PushAsync(new ZivalPage(zival));
    }
    private async void NaloziZivali()
    {
        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        List<Zival> zivali = await firestore.PridobiZivaliUporabnika(UserSession.Uid, UserSession.IdToken);

        ZivaliCollectionView.ItemsSource = zivali;
    }


}