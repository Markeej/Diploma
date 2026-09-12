using E_zivali.Services;
namespace E_zivali.Views;

public partial class DomovPage : ContentPage
{
    private string uid;
    private string idToken;
    public DomovPage(string uid, string idToken)
    {
        InitializeComponent();

        this.uid = uid;
        this.idToken = idToken;
    }

    private async void OnMojeZivali(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MojeZivaliPage));
    }
    private async void OnOpomnikiClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OpomnikiPage));
    }
}