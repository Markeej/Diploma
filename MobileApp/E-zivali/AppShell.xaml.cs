using E_zivali.Views;

namespace E_zivali
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(RegistracijaPage), typeof(RegistracijaPage));
            Routing.RegisterRoute(nameof(PrijavaPage), typeof(PrijavaPage));
            Routing.RegisterRoute(nameof(DomovPage), typeof(DomovPage));
            Routing.RegisterRoute(nameof(MojeZivaliPage), typeof(MojeZivaliPage));
            Routing.RegisterRoute(nameof(DodajZivalPage), typeof(DodajZivalPage));
            Routing.RegisterRoute(nameof(ZivalPage), typeof(ZivalPage));
            Routing.RegisterRoute(nameof(PodrobnostiZivaliPage), typeof(PodrobnostiZivaliPage));
            Routing.RegisterRoute(nameof(OpomnikiZivaliPage), typeof(OpomnikiZivaliPage));
            Routing.RegisterRoute(nameof(ZdravjeZivaliPage), typeof(ZdravjeZivaliPage));
            Routing.RegisterRoute(nameof(SledenjeZivaliPage), typeof(SledenjeZivaliPage));
            Routing.RegisterRoute(nameof(DeljenjeZivaliPage), typeof(DeljenjeZivaliPage));
            Routing.RegisterRoute(nameof(SledenjeZivaliPage), typeof(SledenjeZivaliPage));
            Routing.RegisterRoute(nameof(DokumentiZivaliPage), typeof(DokumentiZivaliPage));
            Routing.RegisterRoute(nameof(OpomnikiPage), typeof(OpomnikiPage));

        }
    }
}
