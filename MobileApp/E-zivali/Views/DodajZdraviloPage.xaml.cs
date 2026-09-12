using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class DodajZdraviloPage : ContentPage
{
    private Zival zival;
    private Zdravilo? zdraviloZaUrejanje;

    public DodajZdraviloPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;
    }
    public DodajZdraviloPage(
    Zival zival,
    Zdravilo zdravilo)
    {
        InitializeComponent();

        this.zival = zival;
        zdraviloZaUrejanje = zdravilo;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;

        NazivVnos.Text = zdravilo.Naziv;

        OdmerekVnos.Text = zdravilo.Odmerek;

        PogostostVnos.Text = zdravilo.Pogostost;

        DatumZacetkaVnos.Date = zdravilo.DatumZacetka;

        if (zdravilo.DatumKonca.HasValue)
        {
            DatumKoncaVnos.Date = zdravilo.DatumKonca.Value;
        }

        VeterinarVnos.Text = zdravilo.Veterinar;

        OpombeVnos.Text = zdravilo.Opombe;
    }


    private async void ShraniZdraviloKlik(
        object sender,
        EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NazivVnos.Text))
        {
            await DisplayAlert("Napaka", "Vnesite naziv zdravila.", "OK");

            return;
        }

        Zdravilo zdravilo = new Zdravilo();

        zdravilo.ZivalId = zival.Id;

        zdravilo.Naziv = NazivVnos.Text.Trim();

        zdravilo.Odmerek = OdmerekVnos.Text?.Trim() ?? "";

        zdravilo.Pogostost = PogostostVnos.Text?.Trim() ?? "";

        zdravilo.DatumZacetka = DatumZacetkaVnos.Date;

        zdravilo.DatumKonca = DatumKoncaVnos.Date;

        zdravilo.Veterinar = VeterinarVnos.Text?.Trim() ?? "";

        zdravilo.Opombe = OpombeVnos.Text?.Trim() ?? "";


        FirebaseFirestoreService firestore = new FirebaseFirestoreService();


        bool uspesno;

        if (zdraviloZaUrejanje == null)
        {
            uspesno = await firestore.ShraniZdravilo(zdravilo, UserSession.IdToken);
        }
        else
        {
            zdravilo.Id = zdraviloZaUrejanje.Id;

            uspesno = await firestore.PosodobiZdravilo(zdravilo, UserSession.IdToken);
        }
    }
}