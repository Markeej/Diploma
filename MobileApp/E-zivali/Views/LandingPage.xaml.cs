using System.Runtime.CompilerServices;

namespace E_zivali.Views;

public partial class LandingPage : ContentPage
{
    public LandingPage()
    {
        InitializeComponent();
    }

    

    private async void OnRegistracijaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegistracijaPage));
    }

    private async void OnPrijavaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PrijavaPage));
    }
}