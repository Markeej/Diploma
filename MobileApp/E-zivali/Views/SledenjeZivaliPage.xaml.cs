using E_zivali.Services;
using E_zivali.Models;
using System.Globalization;

namespace E_zivali.Views;

public partial class SledenjeZivaliPage : ContentPage
{
    private Zival zival;

    private bool osvezevanjeAktivno = false;

    private bool zemljevidNalozen = false;


    public SledenjeZivaliPage(Zival zival)
    {
        InitializeComponent();

        this.zival = zival;

        ImeZivaliLabel.Text = zival.Ime;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        osvezevanjeAktivno = true;

        await PreveriGpsNapravo();

        _ = OsvezevanjeLokacije();
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        osvezevanjeAktivno = false;
    }


    private async Task OsvezevanjeLokacije()
    {
        while (osvezevanjeAktivno)
        {
            await Task.Delay(10000);

            if (!osvezevanjeAktivno)
            {
                break;
            }

            await PreveriGpsNapravo();
        }
    }


    private async void PoveziNapravoKlik(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DeviceIdVnos.Text))
        {
            await DisplayAlert("Napaka", "Vnesite ID naprave.", "OK");

            return;
        }

        string deviceId = DeviceIdVnos.Text.Trim();

        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        bool uspesno = await firestore.PoveziGpsNapravo(deviceId, zival.Id, UserSession.Uid, UserSession.IdToken);

        if (uspesno)
        {
            await DisplayAlert("Uspešno", "GPS naprava je bila povezana z živaljo.", "OK");

            StatusNapraveLabel.Text = "Povezana naprava: " + deviceId;

            PovezavaNapraveLayout.IsVisible = false;

            await PreveriGpsNapravo();
        }
        else
        {
            await DisplayAlert("Napaka", "Ta GPS naprava že obstaja ali povezava ni uspela.", "OK");
        }
    }


    private async Task PreveriGpsNapravo()
    {
        FirebaseFirestoreService firestore = new FirebaseFirestoreService();

        string? deviceId = await firestore.PridobiGpsNapravo(zival.Id, UserSession.IdToken);

        if (deviceId != null)
        {
            StatusNapraveLabel.Text = "Povezana naprava: " + deviceId;

            PovezavaNapraveLayout.IsVisible = false;

            Lokacija? lokacija = await firestore.PridobiZadnjoLokacijo(deviceId, UserSession.IdToken);

            if (lokacija != null)
            {
                LokacijaLayout.IsVisible = true;

                LatitudeLabel.Text = "Latitude: " + lokacija.Latitude;

                LongitudeLabel.Text = "Longitude: " + lokacija.Longitude;

                CasLokacijeLabel.Text = "Posodobljeno: " + lokacija.Cas.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");


                string lat = lokacija.Latitude.ToString(CultureInfo.InvariantCulture);

                string lon = lokacija.Longitude.ToString(CultureInfo.InvariantCulture);


                if (!zemljevidNalozen)
                {
                    string imeZivali = zival.Ime.Replace("'", "\\'");

                    string html = $@"
                                <html>

                                <head>

                                <meta name='viewport'
                                      content='width=device-width, initial-scale=1.0'>

                                <link
                                    rel='stylesheet'
                                    href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>

                                <script
                                    src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'>
                                </script>

                                <style>

                                html, body, #map {{
                                    height: 100%;
                                    margin: 0;
                                    padding: 0;
                                }}

                                </style>

                                </head>


                                <body>

                                <div id='map'></div>


                                <script>

                                var map =
                                    L.map('map').setView(
                                        [{lat}, {lon}],
                                        16
                                    );

                                L.tileLayer(
                                    'https://tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png',
                                    {{
                                        maxZoom: 19,
                                        attribution: '&copy; OpenStreetMap contributors'
                                    }}
                                ).addTo(map);


                                var marker =
                                    L.marker(
                                        [{lat}, {lon}]
                                    )
                                    .addTo(map)
                                    .bindPopup('{imeZivali}')
                                    .openPopup();


                                function posodobiLokacijo(lat, lon)
                                {{
                                    marker.setLatLng([lat, lon]);

                                    map.panTo([lat, lon]);
                                }}

                                </script>


                                </body>

                                </html>";


                    MapWebView.Source = new HtmlWebViewSource { Html = html };

                    ZemljevidLayout.IsVisible = true;

                    zemljevidNalozen = true;
                }
                else
                {
                    await MapWebView.EvaluateJavaScriptAsync($"posodobiLokacijo({lat}, {lon});");
                }
            }
            else
            {
                LokacijaLayout.IsVisible = false;

                ZemljevidLayout.IsVisible = false;
            }
        }
        else
        {
            StatusNapraveLabel.Text = "Sledilna naprava še ni povezana.";

            PovezavaNapraveLayout.IsVisible = true;

            LokacijaLayout.IsVisible = false;

            ZemljevidLayout.IsVisible = false;
        }
    }
}
