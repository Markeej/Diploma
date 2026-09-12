using E_zivali.Models;
using E_zivali.Services;

namespace E_zivali.Views;

public partial class DeljenjeZivaliPage : ContentPage
{
    private Zival zival;

    public DeljenjeZivaliPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = "Žival: " + zival.Ime;
        NaloziShareCode();
    }

    private async void NaloziShareCode()
    {
        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        string? shareCode = await firestore.PridobiMojShareCode(UserSession.Uid, UserSession.IdToken);

        if (shareCode != null)
        {
            MojaShareCodeLabel.Text = shareCode;
        }
        else
        {
            MojaShareCodeLabel.Text = "Koda ni na voljo";
        }
    }

    private async void KopirajShareCodeKlik(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(MojaShareCodeLabel.Text) && MojaShareCodeLabel.Text != "Koda ni na voljo")
        {
            await Clipboard.Default.SetTextAsync(MojaShareCodeLabel.Text);

            await DisplayAlert("Kopirano", "Koda za deljenje je bila kopirana.", "OK");
        }
    }
    private async void DeliDostopKlik(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ShareCodeVnos.Text))
        {
            await DisplayAlert("Napaka", "Vnesite kodo uporabnika za deljenje.", "OK");
            return;
        }

        string shareCode = ShareCodeVnos.Text.Trim().ToUpper();

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        string? userId = await firestore.PridobiUidPoShareCode(shareCode, UserSession.IdToken);

        if (userId == null)
        {
            await DisplayAlert("Napaka", "Uporabnik s to kodo ne obstaja.", "OK");
            return;
        }

        if (userId == UserSession.Uid)
        {
            await DisplayAlert("Napaka", "Dostopa ne morete deliti sami s seboj.", "OK");
            return;
        }

        bool zeImaDostop = await firestore.PreveriDostop(userId, zival.Id, UserSession.IdToken);

        if (zeImaDostop)
        {
            await DisplayAlert("Obvestilo", "Ta uporabnik že ima dostop do živali.", "OK");
            return;
        }

        bool uspesno = await firestore.ShraniDeljenDostop(userId, zival.Id, UserSession.IdToken);

        if (uspesno)
        {
            await DisplayAlert("Uspešno", "Dostop je bil deljen z uporabnikom.", "OK");
            ShareCodeVnos.Text = "";
        }
        else
        {
            await DisplayAlert("Napaka", "Dostopa ni bilo mogoče deliti.", "OK");
        }
    }
}