using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class DodajPregledPage : ContentPage
{
    private Zival zival;
    private Pregled? pregledZaUrejanje;


    public DodajPregledPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;
    }
    public DodajPregledPage(
    Zival zival,
    Pregled pregled)
    {
        InitializeComponent();

        this.zival = zival;
        pregledZaUrejanje = pregled;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;

        DatumPregledaVnos.Date = pregled.DatumPregleda;

        RazlogVnos.Text = pregled.Razlog;

        DiagnozaVnos.Text = pregled.Diagnoza;

        VeterinarVnos.Text = pregled.Veterinar;

        OpombeVnos.Text = pregled.Opombe;
    }


    private async void ShraniPregledKlik(
        object sender,
        EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RazlogVnos.Text))
        {
            await DisplayAlert("Napaka", "Vnesite razlog obiska.", "OK");

            return;
        }


        Pregled pregled = new Pregled();

        pregled.ZivalId = zival.Id;

        pregled.DatumPregleda = DatumPregledaVnos.Date;

        pregled.Razlog = RazlogVnos.Text.Trim();

        pregled.Diagnoza = DiagnozaVnos.Text?.Trim() ?? "";

        pregled.Veterinar = VeterinarVnos.Text?.Trim() ?? "";

        pregled.Opombe = OpombeVnos.Text?.Trim() ?? "";


        FirebaseFirestoreService firestore = new FirebaseFirestoreService();


        bool uspesno;

        if (pregledZaUrejanje == null)
        {
            uspesno = await firestore.ShraniPregled(pregled, UserSession.IdToken);
        }
        else
        {
            pregled.Id = pregledZaUrejanje.Id;

            uspesno = await firestore.PosodobiPregled(pregled, UserSession.IdToken);
        }
    }
}