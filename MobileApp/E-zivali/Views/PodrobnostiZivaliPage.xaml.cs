using E_zivali.Models;
using Microsoft.Maui;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class PodrobnostiZivaliPage : ContentPage
{
    private Zival zival;

    public PodrobnostiZivaliPage(Zival zival)
    {
        InitializeComponent();
        this.zival = zival;
        PrikaziPodatke();
    }
    private void PrikaziPodatke()
    {
        ImeLabel.Text = zival.Ime;
        VrstaLabel.Text = "Vrsta: " + zival.Vrsta;
        PasmaLabel.Text = "Pasma: " + zival.Pasma;
        SpolLabel.Text = "Spol: " + zival.Spol;
        DatumRojstvaLabel.Text = "Datum rojstva: " + zival.DatumRojstva.ToString("dd.MM.yyyy");
        MikrocipLabel.Text = "Mikročip: " + zival.Mikrocip;
        PotniListLabel.Text = "Potni list: " + zival.PotniList;
    }

    private void UrediPodatkeKlik(object sender, EventArgs e)
    {
        ImeVnos.Text = zival.Ime;
        PasmaVnos.Text = zival.Pasma;
        MikrocipVnos.Text = zival.Mikrocip;
        PotniListVnos.Text = zival.PotniList;
        DatumRojstvaVnos.Date = zival.DatumRojstva;

        VrstaVnos.SelectedItem = zival.Vrsta;
        SpolVnos.SelectedItem = zival.Spol;

        PrikazPodatkov.IsVisible = false;
        UrejanjePodatkov.IsVisible = true;
    }

    private void PrekliciUrejanjeKlik(object sender, EventArgs e)
    {
        UrejanjePodatkov.IsVisible = false;
        PrikazPodatkov.IsVisible = true;
    }

    private async void ShraniSpremembeKlik(object sender, EventArgs e)
    {
        if (ImeVnos.Text == null || ImeVnos.Text == "")
        {
            await DisplayAlert("Napaka", "Ime živali ne sme biti prazno.", "OK");
            return;
        }

        zival.Ime = ImeVnos.Text.Trim();
        zival.Vrsta = VrstaVnos.SelectedItem?.ToString() ?? zival.Vrsta;
        zival.Pasma = PasmaVnos.Text?.Trim() ?? "";
        zival.Spol = SpolVnos.SelectedItem?.ToString() ?? zival.Spol;
        zival.DatumRojstva = DatumRojstvaVnos.Date;
        zival.Mikrocip = MikrocipVnos.Text?.Trim() ?? "";
        zival.PotniList = PotniListVnos.Text?.Trim() ?? "";

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        bool uspesno = await firestore.PosodobiZival(zival, UserSession.IdToken);

        if (uspesno)
        {
            PrikaziPodatke();
            UrejanjePodatkov.IsVisible = false;
            PrikazPodatkov.IsVisible = true;

            await DisplayAlert("Uspešno", "Podatki živali so bili posodobljeni.", "OK");
        }
        else
        {
            await DisplayAlert("Napaka", "Podatkov ni bilo mogoče posodobiti.", "OK");
        }
    }


}