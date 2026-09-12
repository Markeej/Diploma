using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class DodajCepljenjePage : ContentPage
{
    private Zival zival;
    private Cepljenje? cepljenjeZaUrejanje;

    public DodajCepljenjePage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;
    }

    private async void ShraniCepljenjeKlik(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NazivVnos.Text))
        {
            await DisplayAlert("Napaka", "Vnesite naziv cepljenja.", "OK");
            return;
        }

        Cepljenje cepljenje = new Cepljenje();
        cepljenje.ZivalId = zival.Id;
        cepljenje.Naziv = NazivVnos.Text.Trim();
        cepljenje.DatumCepljenja = DatumCepljenjaVnos.Date;
        cepljenje.NaslednjeCepljenje = NaslednjeCepljenjeVnos.Date;
        cepljenje.Veterinar = VeterinarVnos.Text?.Trim() ?? "";
        cepljenje.Opombe = OpombeVnos.Text?.Trim() ?? "";

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        bool uspesno;

        if (cepljenjeZaUrejanje == null)
        {
            uspesno = await firestore.ShraniCepljenje(cepljenje, UserSession.IdToken);
        }
        else
        {
            cepljenje.Id = cepljenjeZaUrejanje.Id;
            uspesno = await firestore.PosodobiCepljenje(cepljenje, UserSession.IdToken);
        }
    }
    public DodajCepljenjePage(Zival zival, Cepljenje cepljenje)
    {
        InitializeComponent();

        this.zival = zival;
        cepljenjeZaUrejanje = cepljenje;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;

        NazivVnos.Text = cepljenje.Naziv;

        DatumCepljenjaVnos.Date = cepljenje.DatumCepljenja;

        if (cepljenje.NaslednjeCepljenje.HasValue)
        {
            NaslednjeCepljenjeVnos.Date = cepljenje.NaslednjeCepljenje.Value;
        }

        VeterinarVnos.Text = cepljenje.Veterinar;

        OpombeVnos.Text = cepljenje.Opombe;
    }
}