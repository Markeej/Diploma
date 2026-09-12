using E_zivali.Models;
using E_zivali.Services;
using Plugin.LocalNotification;

namespace E_zivali.Views;

public partial class DodajOpomnikPage : ContentPage
{
    private Zival zival;
    private Opomnik? opomnikZaUrejanje;

    public DodajOpomnikPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;
    }
    public DodajOpomnikPage(
    Zival zival,
    Opomnik opomnik)
    {
        InitializeComponent();

        this.zival = zival;
        opomnikZaUrejanje = opomnik;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;

        NazivVnos.Text = opomnik.Naziv;

        TipVnos.SelectedItem = opomnik.Tip;

        DatumVnos.Date = opomnik.DatumCas.Date;

        CasVnos.Time = opomnik.DatumCas.TimeOfDay;

        OpisVnos.Text = opomnik.Opis;
    }

    private async void ShraniOpomnikKlik(
        object sender,
        EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NazivVnos.Text))
        {
            await DisplayAlert("Napaka", "Vnesite naziv opomnika.", "OK");
            return;
        }

        if (TipVnos.SelectedItem == null)
        {
            await DisplayAlert("Napaka", "Izberite tip opomnika.", "OK");
            return;
        }

        DateTime datumCas = DatumVnos.Date.Add(CasVnos.Time);

        if (datumCas <= DateTime.Now)
        {
            await DisplayAlert("Napaka", "Datum in čas opomnika morata biti v prihodnosti.", "OK");
            return;
        }

        Opomnik opomnik = new Opomnik();

        opomnik.ZivalId = zival.Id;

        opomnik.Naziv = NazivVnos.Text.Trim();

        opomnik.Tip = TipVnos.SelectedItem.ToString() ?? "";

        opomnik.DatumCas = datumCas;

        opomnik.Opis = OpisVnos.Text?.Trim() ?? "";

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        bool uspesno;

        if (opomnikZaUrejanje == null)
        {
            uspesno = await firestore.ShraniOpomnik(opomnik, UserSession.IdToken);
        }
        else
        {
            opomnik.Id = opomnikZaUrejanje.Id;

            uspesno = await firestore.PosodobiOpomnik(opomnik, UserSession.IdToken);
        }

        if (uspesno)
        {
            bool dovoljeno = await LocalNotificationCenter.Current.AreNotificationsEnabled();

            if (!dovoljeno)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            var obvestilo = new NotificationRequest { NotificationId = Random.Shared.Next(1, 100000), Title = opomnik.Naziv, Description = opomnik.Opis, Schedule = new NotificationRequestSchedule { NotifyTime = opomnik.DatumCas } };

            await LocalNotificationCenter.Current.Show(obvestilo);

            await DisplayAlert("Uspešno", "Opomnik je shranjen.", "OK");

            await Navigation.PopAsync();
        }
    }
}