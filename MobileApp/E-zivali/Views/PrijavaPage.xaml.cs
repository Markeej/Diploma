using E_zivali.Views;
using E_zivali.Models;
using E_zivali.Services;


namespace E_zivali.Views;

public partial class PrijavaPage : ContentPage
{
    public PrijavaPage()
    {
        InitializeComponent(); 
    }
        private async void PrijaviSe(object sender, EventArgs e)
    {
        if (EmailVnos.Text == null || EmailVnos.Text == "")
        {
            await DisplayAlert("Napaka", "Vnesite e-postni naslov.", "OK");
            return;
        }

        if (GesloVnos.Text == null || GesloVnos.Text == "")
        {
            await DisplayAlert("Napaka", "Vnesite geslo.", "OK");
            return;
        }

        string email = EmailVnos.Text.Trim();
        string geslo = GesloVnos.Text;

        FirebaseAuthService firebase = new FirebaseAuthService();

        FirebaseAuthResponse? uporabnik = await firebase.PrijaviUporabnika(email, geslo);

        if (uporabnik != null)
        {
            UserSession.Uid = uporabnik.LocalId;
            UserSession.IdToken = uporabnik.IdToken;
            await Navigation.PushAsync(new DomovPage(uporabnik.LocalId, uporabnik.IdToken));
        }
        else
        {
            await DisplayAlert("Napaka", "Napačen e-postni naslov ali geslo.", "OK");
        }

    }
}
