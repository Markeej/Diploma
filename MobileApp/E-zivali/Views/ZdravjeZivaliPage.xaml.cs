using E_zivali.Models;
namespace E_zivali.Views;

public partial class ZdravjeZivaliPage : ContentPage
{
    private Zival zival;

    public ZdravjeZivaliPage(Zival zival)
    {
        InitializeComponent();
        this.zival = zival;
    }

    private async void OnCepljenjaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CepljenjaPage(zival));
    }
    private async void OnPreglediClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PreglediPage(zival));
    }

    private async void OnZdravilaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ZdravilaPage(zival));
    }
}