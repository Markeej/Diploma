using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class DodajDokumentPage : ContentPage
{
    private Zival zival;

    private FileResult? izbranaDatoteka;


    public DodajDokumentPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;
    }


    private async void IzberiDatotekoKlik(
        object sender,
        EventArgs e)
    {
        izbranaDatoteka = await FilePicker.Default.PickAsync();

        if (izbranaDatoteka != null)
        {
            IzbranaDatotekaLabel.Text = izbranaDatoteka.FileName;
        }
    }


    private async void ShraniDokumentKlik(
        object sender,
        EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NazivVnos.Text))
        {
            await DisplayAlert("Napaka", "Vnesite naziv dokumenta.", "OK");

            return;
        }

        if (VrstaVnos.SelectedItem == null)
        {
            await DisplayAlert("Napaka", "Izberite vrsto dokumenta.", "OK");

            return;
        }

        if (izbranaDatoteka == null)
        {
            await DisplayAlert("Napaka", "Izberite datoteko.", "OK");

            return;
        }


        string mapa = Path.Combine(FileSystem.AppDataDirectory, "documents", zival.Id);
        Directory.CreateDirectory(mapa);

        string ciljnaPot = Path.Combine(mapa, izbranaDatoteka.FileName);

        using Stream vhod = await izbranaDatoteka.OpenReadAsync();

        using FileStream izhod = File.Create(ciljnaPot);

        await vhod.CopyToAsync(izhod);


        Dokument dokument = new Dokument();

        dokument.ZivalId = zival.Id;
        dokument.Naziv = NazivVnos.Text.Trim();
        dokument.Vrsta = VrstaVnos.SelectedItem.ToString() ?? "";
        dokument.Datum = DatumVnos.Date;
        dokument.ImeDatoteke = izbranaDatoteka.FileName;
        dokument.LokalnaPot = ciljnaPot;
        dokument.Opombe = OpombeVnos.Text?.Trim() ?? "";


        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        bool uspesno = await firestore.ShraniDokument(dokument, UserSession.IdToken);


        if (uspesno)
        {
            await DisplayAlert("Uspešno", "Dokument je bil shranjen.", "OK");

            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Napaka", "Dokumenta ni bilo mogoče shraniti.", "OK");
        }
    }
}