using System.Text;
using System.Text.Json;
using E_zivali.Models;
using System.Collections.Generic;

namespace E_zivali.Services;

public class FirebaseAuthService
{
    private readonly string apiKey = "FIREBASE-API-KEY";
    public async Task<FirebaseAuthResponse?> RegistrirajUporabnika(string email, string geslo)
    {
        HttpClient client = new HttpClient(); 

        string url = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" + apiKey;  

        var podatki = new { email = email, password = geslo, returnSecureToken = true };

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json"); 

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina); 

        if (odgovor.IsSuccessStatusCode)
        {
            string odgovorJson = await odgovor.Content.ReadAsStringAsync();
            FirebaseAuthResponse? uporabnik = JsonSerializer.Deserialize<FirebaseAuthResponse>(odgovorJson);

            return uporabnik;
        }
        else
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            await Application.Current.MainPage.DisplayAlert("Firebase napaka", napaka, "OK");
            return null;
        }
    }

    public async Task<FirebaseAuthResponse?> PrijaviUporabnika(string email, string geslo)
    {
        HttpClient client = new HttpClient();
        string url = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" + apiKey;

        Dictionary<string, object> podatki = new Dictionary<string, object>();

        podatki.Add("email", email);
        podatki.Add("password", geslo);
        podatki.Add("returnSecureToken", true);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (odgovor.IsSuccessStatusCode)
        {
            string odgovorJson = await odgovor.Content.ReadAsStringAsync();

            FirebaseAuthResponse? uporabnik = JsonSerializer.Deserialize<FirebaseAuthResponse>(odgovorJson);

            return uporabnik;
        }
        else
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();

            Console.WriteLine(napaka);

            return null;
        }
    }
}