using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class DodajZivalPage : ContentPage
{
	public DodajZivalPage()
	{
		InitializeComponent();
	}
    private void NaprejNaKorak2(object sender, EventArgs e)
    {
        Korak1.IsVisible = false;
        Korak2.IsVisible = true;

        KorakLabel.Text = "Korak 2 od 3";
        KorakProgress.Progress = 0.66;

    }

    private void NazajNaKorak1(object sender, EventArgs e)
    {
        Korak2.IsVisible = false;
        Korak1.IsVisible = true;

        KorakLabel.Text = "Korak 1 od 3";
        KorakProgress.Progress = 0.33;

    }

    private void NaprejNaKorak3(object sender, EventArgs e)
    {
        Korak2.IsVisible = false;
        Korak3.IsVisible = true;

        KorakLabel.Text = "Korak 3 od 3";
        KorakProgress.Progress = 1.0;

        PregledIme.Text = "Ime: " + ImeZivaliVnos.Text;
        PregledVrsta.Text = "Vrsta: " + VrstaZivaliVnos.SelectedItem;
        PregledPasma.Text = "Pasma: " + PasmaVnos.Text;
        PregledSpol.Text = "Spol: " + SpolVnos.SelectedItem;
        PregledDatum.Text = "Datum rojstva: " + DatumRojstvaVnos.Date.ToString("dd.MM.yyyy");
        PregledMikrocip.Text = "Mikročip: " + MikrocipVnos.Text;
        PregledPotniList.Text = "Potni list: " + PotniListVnos.Text;

    }

    private void NazajNaKorak2(object sender, EventArgs e)
    {
        Korak3.IsVisible = false;
        Korak2.IsVisible = true;

        KorakLabel.Text = "Korak 2 od 3";
        KorakProgress.Progress = 0.66;

    }

    private async void ShraniZival(object sender, EventArgs e)
    {
        if (VrstaZivaliVnos.SelectedItem == null)
        {
            await DisplayAlert("Napaka", "Izberite vrsto živali.", "OK");
            return;
        }

        if (SpolVnos.SelectedItem == null)
        {
            await DisplayAlert("Napaka", "Izberite spol živali.", "OK");
            return;
        }

        Zival zival = new Zival();

        zival.Ime = ImeZivaliVnos.Text.Trim();
        zival.Vrsta = VrstaZivaliVnos.SelectedItem.ToString();
        zival.Pasma = PasmaVnos.Text?.Trim() ?? "";
        zival.Spol = SpolVnos.SelectedItem.ToString();
        zival.DatumRojstva = DatumRojstvaVnos.Date;
        zival.Mikrocip = MikrocipVnos.Text?.Trim() ?? "";
        zival.PotniList = PotniListVnos.Text?.Trim() ?? "";

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        string? animalId = await firestore.ShraniZival(zival,UserSession.Uid, UserSession.IdToken);

        if (animalId == null)
        {
            await DisplayAlert("Napaka", "Živali ni bilo mogoče shraniti.", "OK");
            return;
        }

        bool dostopShranjeno = await firestore.ShraniDostop(UserSession.Uid, animalId, UserSession.IdToken);

        if (!dostopShranjeno)
        {
            await DisplayAlert("Napaka", "Žival je bila shranjena, povezave z uporabnikom pa ni bilo mogoče ustvariti.", "OK");

            return;
        }

        await DisplayAlert("Uspešno", "Žival je bila shranjena in povezana z uporabnikom.", "OK");
        await Shell.Current.GoToAsync(nameof(MojeZivaliPage));

    }
}