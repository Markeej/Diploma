using E_zivali.Models;
using E_zivali.Services;
using System.Threading.Tasks;

namespace E_zivali.Views;

public partial class RegistracijaPage : ContentPage
{
    public RegistracijaPage()
    {
        InitializeComponent();
    }

    private async void UstvariRegistracijo(object sender, EventArgs e)
    {
        if (ImeVnos.Text == null || ImeVnos.Text == "")
        {
            await DisplayAlert("Napaka", "Vnesite ime.", "OK");
            return;
        }

        if (PriimekVnos.Text == null || PriimekVnos.Text == "")
        {
            await DisplayAlert("Napaka", "Vnesite priimek.", "OK");
            return;
        }

        if (EmailVnos.Text == null || EmailVnos.Text == "")
        {
            await DisplayAlert("Napaka", "Vnesite email.", "OK");
            return;
        }

        if (!EmailVnos.Text.Contains("@"))
        {
            await DisplayAlert("Napaka", "E-poštni naslov mora vsebovati @.", "OK");
            return;
        }

        if (GesloVnos.Text == null || GesloVnos.Text == "")
        {
            await DisplayAlert("Napaka", "Vnesite geslo.", "OK");
            return;
        }

        if (PonovniVnos.Text == null || PonovniVnos.Text == "")
        {
            await DisplayAlert("Napaka", "Ponovno vnesite geslo.", "OK");
            return;
        }

        if (PonovniVnos.Text != GesloVnos.Text)
        {
            await DisplayAlert("Napaka", "Gesli se ne ujemata.", "OK");
            return;
        }

        string ime = ImeVnos.Text.Trim();
        string priimek = PriimekVnos.Text.Trim();
        string email = EmailVnos.Text.Trim();
        string geslo = GesloVnos.Text;

        FirebaseAuthService firebase = new FirebaseAuthService();

        FirebaseAuthResponse? uporabnik = await firebase.RegistrirajUporabnika(email, geslo);

        if (uporabnik != null)
        {
            UserSession.Uid = uporabnik.LocalId;
            UserSession.IdToken = uporabnik.IdToken;

            FirebaseFirestoreService firestore = new FirebaseFirestoreService();

            string shareCode = firestore.GenerirajShareCode();

            bool shranjeno = await firestore.ShraniUporabnika(
                uporabnik.LocalId,
                ime,
                priimek,
                email,
                shareCode,
                uporabnik.IdToken
            );

            if (!shranjeno)
            {
                await DisplayAlert("Napaka", "Račun je bil ustvarjen, podatkov pa ni bilo mogoče shraniti.", "OK");
                return;
            }

            bool shareCodeShranjena = await firestore.ShraniShareCode(
                shareCode,
                uporabnik.LocalId,
                uporabnik.IdToken
            );

            if (!shareCodeShranjena)
            {
                await DisplayAlert("Napaka", "Uporabnik je bil ustvarjen, kode za deljenje pa ni bilo mogoče shraniti.", "OK");
                return;
            }

            await DisplayAlert("Uspešno", "Uporabnik je bil registriran in shranjen.", "OK");
        }
        else
        {
            await DisplayAlert("Napaka", "Registracija ni uspela!", "OK");
        }
    }
}