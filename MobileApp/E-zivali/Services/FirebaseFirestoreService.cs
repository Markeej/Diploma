using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using E_zivali.Models;
using System.Collections.Generic;
using System.Linq;

namespace E_zivali.Services;

public class FirebaseFirestoreService
{
    private readonly string projectId = "FIREBASE-PROJECT-ID";

    public async Task<bool> ShraniUporabnika(string uid, string ime, string priimek, string email, string shareCode, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/users/" + uid;

        Dictionary<string, object> fields = new Dictionary<string, object>();

                fields.Add("firstName", new Dictionary<string, string>
            {
                { "stringValue", ime }
            });

                fields.Add("lastName", new Dictionary<string, string>
            {
                { "stringValue", priimek }
            });

                fields.Add("email", new Dictionary<string, string>
            {
                { "stringValue", email }
            });

                fields.Add("shareCode", new Dictionary<string, string>
            {
                { "stringValue", shareCode }
            });

        Dictionary<string, object> podatki = new Dictionary<string, object>();
        podatki.Add("fields", fields);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PatchAsync(url, vsebina);

        if (odgovor.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();

            await Application.Current.MainPage.DisplayAlert("Firestore napaka", napaka, "OK");

            return false;
        }
    }

    public async Task<string?> ShraniZival(Zival zival, string userId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/animals";

        Dictionary<string, object> fields = new Dictionary<string, object>();

                fields.Add("name", new Dictionary<string, string>
            {
                { "stringValue", zival.Ime }
            });

                fields.Add("species", new Dictionary<string, string>
            {
                { "stringValue", zival.Vrsta }
            });

                fields.Add("breed", new Dictionary<string, string>
            {
                { "stringValue", zival.Pasma }
            });

                fields.Add("sex", new Dictionary<string, string>
            {
                { "stringValue", zival.Spol }
            });

                fields.Add("dateOfBirth", new Dictionary<string, string>
            {
                { "timestampValue", zival.DatumRojstva.ToUniversalTime().ToString("o") }
            });

                fields.Add("microchip", new Dictionary<string, string>
            {
                { "stringValue", zival.Mikrocip }
            });

                fields.Add("passportNumber", new Dictionary<string, string>
            {
                { "stringValue", zival.PotniList }
            });

                fields.Add("ownerId", new Dictionary<string, string>
            {
                { "stringValue", userId }
            });

        Dictionary<string, object> podatki = new Dictionary<string, object>();
        podatki.Add("fields", fields);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (odgovor.IsSuccessStatusCode)
        {
            string odgovorJson =  await odgovor.Content.ReadAsStringAsync();

            JsonDocument dokument = JsonDocument.Parse(odgovorJson);

            string celotnoIme = dokument.RootElement.GetProperty("name").GetString() ?? "";

            string animalId = celotnoIme.Split('/').Last();
            return animalId;
        }
        else
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();

            Console.WriteLine(napaka);

            return null;
        }
    }
    public async Task<bool> ShraniDostop(string userId, string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string accessId = userId + "_" + animalId;

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/access" + "?documentId=" + Uri.EscapeDataString(accessId);

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("userId", new Dictionary<string, string>
            {
                { "stringValue", userId }
            });

                fields.Add("animalId", new Dictionary<string, string>
            {
                { "stringValue", animalId }
            });

                fields.Add("role", new Dictionary<string, string>
            {
                { "stringValue", "owner" }
            });

        Dictionary<string, object> podatki = new Dictionary<string, object>();
        podatki.Add("fields", fields);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (odgovor.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();

            Console.WriteLine(napaka);

            return false;
        }
    }

    public async Task<List<Zival>> PridobiZivaliUporabnika(string userId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        List<Zival> zivali = new List<Zival>();

        string queryUrl = "https://firestore.googleapis.com/v1/projects/" + projectId +  "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = "access"
                }
            },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = "userId"
                        },
                        op = "EQUAL",
                        value = new
                        {
                            stringValue = userId
                        }
                    }
                }
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent queryContent = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage accessOdgovor = await client.PostAsync(queryUrl, queryContent);

        if (!accessOdgovor.IsSuccessStatusCode)
        {
            string napaka = await accessOdgovor.Content.ReadAsStringAsync();
            Console.WriteLine(napaka);

            return zivali;
        }

        string accessJson = await accessOdgovor.Content.ReadAsStringAsync();

        JsonDocument accessDokument =  JsonDocument.Parse(accessJson);

        foreach (JsonElement rezultat in accessDokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty("document", out JsonElement access))
            {
                continue;
            }

            JsonElement fields = access.GetProperty("fields");

            string animalId = fields.GetProperty("animalId").GetProperty("stringValue").GetString() ?? "";

            if (string.IsNullOrWhiteSpace(animalId))
            {
                continue;
            }

            string animalUrl = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/animals/" + animalId;

            HttpResponseMessage animalOdgovor = await client.GetAsync(animalUrl);

            if (!animalOdgovor.IsSuccessStatusCode)
            {
                string napaka = await animalOdgovor.Content.ReadAsStringAsync();

                Console.WriteLine(napaka);

                continue;
            }

            string animalJson = await animalOdgovor.Content.ReadAsStringAsync();

            JsonDocument animalDokument = JsonDocument.Parse(animalJson);

            JsonElement animalFields = animalDokument.RootElement.GetProperty("fields");

            Zival zival = new Zival();

            zival.Id = animalId;

            zival.Ime = animalFields.GetProperty("name").GetProperty("stringValue").GetString() ?? "";

            zival.Vrsta = animalFields.GetProperty("species").GetProperty("stringValue").GetString() ?? "";

            zival.Pasma = animalFields.GetProperty("breed").GetProperty("stringValue").GetString() ?? "";

            zival.Spol = animalFields.GetProperty("sex").GetProperty("stringValue").GetString() ?? "";

            zival.Mikrocip = animalFields.GetProperty("microchip").GetProperty("stringValue").GetString() ?? "";

            zival.PotniList = animalFields.GetProperty("passportNumber").GetProperty("stringValue").GetString() ?? "";

            string datum = animalFields.GetProperty("dateOfBirth").GetProperty("timestampValue").GetString() ?? "";

            if (DateTime.TryParse(datum, out DateTime datumRojstva))
            {
                zival.DatumRojstva = datumRojstva;
            }

            zivali.Add(zival);
        }

        return zivali;
    }

    public async Task<bool> PosodobiZival(Zival zival, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/animals/" + zival.Id;

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("name", new Dictionary<string, string> { { "stringValue", zival.Ime } });

                        fields.Add("species", new Dictionary<string, string> { { "stringValue", zival.Vrsta } });

                        fields.Add("breed", new Dictionary<string, string> { { "stringValue", zival.Pasma } });

                        fields.Add("sex", new Dictionary<string, string> { { "stringValue", zival.Spol } });

                        fields.Add("dateOfBirth", new Dictionary<string, string> { { "timestampValue", zival.DatumRojstva.ToUniversalTime().ToString("o") } });

                        fields.Add("microchip", new Dictionary<string, string> { { "stringValue", zival.Mikrocip } });

                        fields.Add("passportNumber", new Dictionary<string, string> { { "stringValue", zival.PotniList } });

        Dictionary<string, object> podatki = new Dictionary<string, object>();
        podatki.Add("fields", fields);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PatchAsync(url, vsebina);

        return odgovor.IsSuccessStatusCode;
    }

    public async Task<bool> PoveziGpsNapravo(string deviceId, string animalId, string userId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/gpsDevices/" + deviceId;

        HttpResponseMessage preveriOdgovor = await client.GetAsync(url);

        if (preveriOdgovor.IsSuccessStatusCode)
        {
            return false;
        }

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", animalId } });

                    fields.Add("userId", new Dictionary<string, string> { { "stringValue", userId } });

                    fields.Add("active", new Dictionary<string, bool> { { "booleanValue", true } });

        Dictionary<string, object> podatki = new Dictionary<string, object>();

        podatki.Add("fields", fields);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PatchAsync(url, vsebina);

        return odgovor.IsSuccessStatusCode;

    }
    public async Task<string?> PridobiGpsNapravo(string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = "gpsDevices"
                }
            },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = "animalId"
                        },
                        op = "EQUAL",
                        value = new
                        {
                            stringValue = animalId
                        }
                    }
                },
                limit = 1
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent vsebina = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (!odgovor.IsSuccessStatusCode)
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            Console.WriteLine(napaka);
            return null;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        foreach (JsonElement rezultat in dokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty("document", out JsonElement device))
            {
                continue;
            }

            string name = device.GetProperty("name").GetString() ?? "";

            string deviceId = name.Split('/').Last();

            return deviceId;
        }

        return null;
    }

    public async Task<Lokacija?> PridobiZadnjoLokacijo(string deviceId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = "locations"
                }
            },

                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = "deviceId"
                        },
                        op = "EQUAL",
                        value = new
                        {
                            stringValue = deviceId
                        }
                    }
                },

                orderBy = new[]
                {
                new
                {
                    field = new
                    {
                        fieldPath = "timestamp"
                    },
                    direction = "DESCENDING"
                }
            },

                limit = 1
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent vsebina = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (!odgovor.IsSuccessStatusCode)
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            Console.WriteLine(napaka);
            return null;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        foreach (JsonElement rezultat in dokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty("document", out JsonElement document))
            {
                continue;
            }

            JsonElement fields = document.GetProperty("fields");

            double latitude = fields.GetProperty("latitude").GetProperty("doubleValue").GetDouble();

            double longitude = fields.GetProperty("longitude").GetProperty("doubleValue").GetDouble();

            string casString = fields.GetProperty("timestamp").GetProperty("timestampValue").GetString() ?? "";

            DateTime cas = DateTime.Parse(casString);

            return new Lokacija
            {
                GpsNapravaId = deviceId,
                Latitude = latitude,
                Longitude = longitude,
                Cas = cas
            };
        }

        return null;
    }

    public async Task<bool> ShraniCepljenje( Cepljenje cepljenje, string idToken)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/vaccinations";
        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", cepljenje.ZivalId } });

        fields.Add("name", new Dictionary<string, string> { { "stringValue", cepljenje.Naziv } });

        fields.Add("vaccinationDate", new Dictionary<string, string> { { "timestampValue", cepljenje.DatumCepljenja.ToUniversalTime().ToString("o") } });

        if (cepljenje.NaslednjeCepljenje.HasValue)
        {
            fields.Add("nextVaccinationDate", new Dictionary<string, string> { { "timestampValue", cepljenje.NaslednjeCepljenje.Value.ToUniversalTime().ToString("o") } });
        }

        fields.Add("veterinarian", new Dictionary<string, string> { { "stringValue", cepljenje.Veterinar } });

        fields.Add("notes", new Dictionary<string, string> { { "stringValue", cepljenje.Opombe } });

        Dictionary<string, object> podatki = new Dictionary<string, object>();

        podatki.Add("fields", fields);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (odgovor.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            Console.WriteLine("Napaka pri shranjevanju cepljenja: " + napaka);
            return false;
        }
    }

    public async Task<List<Cepljenje>> PridobiCepljenja(string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        List<Cepljenje> cepljenja = new List<Cepljenje>();

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = "vaccinations"
                }
            },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = "animalId"
                        },
                        op = "EQUAL",
                        value = new
                        {
                            stringValue = animalId
                        }
                    }
                }
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent vsebina = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (!odgovor.IsSuccessStatusCode)
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            Console.WriteLine("Napaka pri pridobivanju cepljenj: " + napaka);
            return cepljenja;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        foreach (JsonElement rezultat in dokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty("document", out JsonElement document))
            {
                continue;
            }

            JsonElement fields = document.GetProperty("fields");

            Cepljenje cepljenje = new Cepljenje();

            string celotnoIme = document.GetProperty("name").GetString() ?? "";

            cepljenje.Id = celotnoIme.Split('/').Last();

            cepljenje.ZivalId = fields.GetProperty("animalId").GetProperty("stringValue").GetString() ?? "";

            cepljenje.Naziv = fields.GetProperty("name").GetProperty("stringValue").GetString() ?? "";

            string datum = fields.GetProperty("vaccinationDate").GetProperty("timestampValue").GetString() ?? "";

            if (DateTime.TryParse(datum, out DateTime datumCepljenja))
            {
                cepljenje.DatumCepljenja = datumCepljenja;
            }

            if (fields.TryGetProperty("nextVaccinationDate", out JsonElement naslednjeField))
            {
                string naslednje = naslednjeField.GetProperty("timestampValue").GetString() ?? "";

                if (DateTime.TryParse(naslednje, out DateTime naslednjeCepljenje))
                {
                    cepljenje.NaslednjeCepljenje = naslednjeCepljenje;
                }
            }

            if (fields.TryGetProperty("veterinarian", out JsonElement veterinarField))
            {
                cepljenje.Veterinar = veterinarField.GetProperty("stringValue").GetString() ?? "";
            }

            if (fields.TryGetProperty("notes", out JsonElement opombeField))
            {
                cepljenje.Opombe = opombeField.GetProperty("stringValue").GetString() ?? "";
            }

            cepljenja.Add(cepljenje);
        }

        return cepljenja.OrderByDescending(c => c.DatumCepljenja).ToList();
    }

    public async Task<bool> ShraniPregled(Pregled pregled, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/examinations";


        Dictionary<string, object> fields = new Dictionary<string, object>();


        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", pregled.ZivalId } });


        fields.Add("examinationDate", new Dictionary<string, string> { { "timestampValue", pregled.DatumPregleda.ToUniversalTime().ToString("o") } });


        fields.Add("reason", new Dictionary<string, string> { { "stringValue", pregled.Razlog } });


        fields.Add("diagnosis", new Dictionary<string, string> { { "stringValue", pregled.Diagnoza } });


        fields.Add("veterinarian", new Dictionary<string, string> { { "stringValue", pregled.Veterinar } });


        fields.Add("notes", new Dictionary<string, string> { { "stringValue", pregled.Opombe } });


        Dictionary<string, object> podatki = new Dictionary<string, object>();

        podatki.Add("fields", fields);


        string json = JsonSerializer.Serialize(podatki);


        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");


        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);


        if (odgovor.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();

            Console.WriteLine("Napaka pri shranjevanju pregleda: " + napaka);

            return false;
        }
    }

    public async Task<List<Pregled>> PridobiPreglede(string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        List<Pregled> pregledi = new List<Pregled>();

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = "examinations"
                }
            },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = "animalId"
                        },
                        op = "EQUAL",
                        value = new
                        {
                            stringValue = animalId
                        }
                    }
                }
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent vsebina = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (!odgovor.IsSuccessStatusCode)
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            Console.WriteLine("Napaka pri pridobivanju pregledov: " + napaka);
            return pregledi;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        foreach (JsonElement rezultat in dokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty("document", out JsonElement document))
            {
                continue;
            }

            JsonElement fields = document.GetProperty("fields");

            Pregled pregled = new Pregled();

            string celotnoIme = document.GetProperty("name").GetString() ?? "";

            pregled.Id = celotnoIme.Split('/').Last();

            pregled.ZivalId = fields.GetProperty("animalId").GetProperty("stringValue").GetString() ?? "";

            string datum = fields.GetProperty("examinationDate").GetProperty("timestampValue").GetString() ?? "";

            if (DateTime.TryParse(datum, out DateTime datumPregleda))
            {
                pregled.DatumPregleda = datumPregleda;
            }

            pregled.Razlog = fields.GetProperty("reason").GetProperty("stringValue").GetString() ?? "";

            if (fields.TryGetProperty("diagnosis", out JsonElement diagnozaField))
            {
                pregled.Diagnoza = diagnozaField.GetProperty("stringValue").GetString() ?? "";
            }

            if (fields.TryGetProperty("veterinarian", out JsonElement veterinarField))
            {
                pregled.Veterinar = veterinarField.GetProperty("stringValue").GetString() ?? "";
            }

            if (fields.TryGetProperty("notes", out JsonElement opombeField))
            {
                pregled.Opombe = opombeField.GetProperty("stringValue").GetString() ?? "";
            }

            pregledi.Add(pregled);
        }

        return pregledi.OrderByDescending(p => p.DatumPregleda).ToList();
    }
    public async Task<bool> ShraniZdravilo(Zdravilo zdravilo, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/medications";


        Dictionary<string, object> fields = new Dictionary<string, object>();


        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", zdravilo.ZivalId } });


        fields.Add("name", new Dictionary<string, string> { { "stringValue", zdravilo.Naziv } });


        fields.Add("dosage", new Dictionary<string, string> { { "stringValue", zdravilo.Odmerek } });


        fields.Add("frequency", new Dictionary<string, string> { { "stringValue", zdravilo.Pogostost } });


        fields.Add("startDate", new Dictionary<string, string> { { "timestampValue", zdravilo.DatumZacetka.ToUniversalTime().ToString("o") } });


        if (zdravilo.DatumKonca.HasValue)
        {
            fields.Add("endDate", new Dictionary<string, string> { { "timestampValue", zdravilo.DatumKonca.Value.ToUniversalTime().ToString("o") } });
        }


        fields.Add("veterinarian", new Dictionary<string, string> { { "stringValue", zdravilo.Veterinar } });


        fields.Add("notes", new Dictionary<string, string> { { "stringValue", zdravilo.Opombe } });


        Dictionary<string, object> podatki = new Dictionary<string, object>();

        podatki.Add("fields", fields);


        string json = JsonSerializer.Serialize(podatki);


        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");


        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);


        if (odgovor.IsSuccessStatusCode)
        {
            return true;
        }

        string napaka = await odgovor.Content.ReadAsStringAsync();

        Console.WriteLine("Napaka pri shranjevanju zdravila: " + napaka);

        return false;
    }
    public async Task<List<Zdravilo>> PridobiZdravila(string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        List<Zdravilo> zdravila = new List<Zdravilo>();

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = "medications"
                }
            },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = "animalId"
                        },
                        op = "EQUAL",
                        value = new
                        {
                            stringValue = animalId
                        }
                    }
                }
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent vsebina = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (!odgovor.IsSuccessStatusCode)
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            Console.WriteLine("Napaka pri pridobivanju zdravil: " + napaka);
            return zdravila;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        foreach (JsonElement rezultat in dokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty("document", out JsonElement document))
            {
                continue;
            }

            JsonElement fields = document.GetProperty("fields");

            Zdravilo zdravilo = new Zdravilo();

            string celotnoIme = document.GetProperty("name").GetString() ?? "";

            zdravilo.Id = celotnoIme.Split('/').Last();

            zdravilo.ZivalId = fields.GetProperty("animalId").GetProperty("stringValue").GetString() ?? "";

            zdravilo.Naziv = fields.GetProperty("name").GetProperty("stringValue").GetString() ?? "";

            if (fields.TryGetProperty("dosage", out JsonElement odmerekField))
            {
                zdravilo.Odmerek = odmerekField.GetProperty("stringValue").GetString() ?? "";
            }

            if (fields.TryGetProperty("frequency", out JsonElement pogostostField))
            {
                zdravilo.Pogostost = pogostostField.GetProperty("stringValue").GetString() ?? "";
            }

            string datumZacetka = fields.GetProperty("startDate").GetProperty("timestampValue").GetString() ?? "";

            if (DateTime.TryParse(datumZacetka, out DateTime zacetek))
            {
                zdravilo.DatumZacetka = zacetek;
            }

            if (fields.TryGetProperty("endDate", out JsonElement konecField))
            {
                string datumKonca = konecField.GetProperty("timestampValue").GetString() ?? "";

                if (DateTime.TryParse(datumKonca, out DateTime konec))
                {
                    zdravilo.DatumKonca = konec;
                }
            }

            if (fields.TryGetProperty("veterinarian", out JsonElement veterinarField))
            {
                zdravilo.Veterinar = veterinarField.GetProperty("stringValue").GetString() ?? "";
            }

            if (fields.TryGetProperty("notes", out JsonElement opombeField))
            {
                zdravilo.Opombe = opombeField.GetProperty("stringValue").GetString() ?? "";
            }

            zdravila.Add(zdravilo);
        }

        return zdravila.OrderByDescending(z => z.DatumZacetka).ToList();
    }
    public async Task<bool> ShraniDokument( Dokument dokument, string idToken)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/documents";
        Dictionary<string, object> fields = new Dictionary<string, object>();


        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", dokument.ZivalId } });

        fields.Add("name", new Dictionary<string, string> { { "stringValue", dokument.Naziv } });

        fields.Add("type", new Dictionary<string, string> { { "stringValue", dokument.Vrsta } });

        fields.Add("date", new Dictionary<string, string> { { "timestampValue", dokument.Datum.ToUniversalTime().ToString("o") } });

        fields.Add("fileName", new Dictionary<string, string> { { "stringValue", dokument.ImeDatoteke } });


        fields.Add("localPath", new Dictionary<string, string> { { "stringValue", dokument.LokalnaPot } });


        fields.Add("notes", new Dictionary<string, string> { { "stringValue", dokument.Opombe } });


        Dictionary<string, object> podatki = new Dictionary<string, object>();

        podatki.Add("fields", fields);


        string json = JsonSerializer.Serialize(podatki);


        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");


        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (odgovor.IsSuccessStatusCode)
        {
            return true;
        }

        string napaka = await odgovor.Content.ReadAsStringAsync();

        Console.WriteLine("Napaka pri shranjevanju dokumenta: " + napaka);

        return false;
    }
    public async Task<List<Dokument>> PridobiDokumente(string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        List<Dokument> dokumenti = new List<Dokument>();

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = "documents"
                }
            },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = "animalId"
                        },
                        op = "EQUAL",
                        value = new
                        {
                            stringValue = animalId
                        }
                    }
                }
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent vsebina = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (!odgovor.IsSuccessStatusCode)
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            Console.WriteLine("Napaka pri pridobivanju dokumentov: " + napaka);
            return dokumenti;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        foreach (JsonElement rezultat in dokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty("document", out JsonElement document))
            {
                continue;
            }

            JsonElement fields = document.GetProperty("fields");

            Dokument novDokument = new Dokument();

            string celotnoIme = document.GetProperty("name").GetString() ?? "";

            novDokument.Id = celotnoIme.Split('/').Last();

            novDokument.ZivalId = fields.GetProperty("animalId").GetProperty("stringValue").GetString() ?? "";

            novDokument.Naziv = fields.GetProperty("name").GetProperty("stringValue").GetString() ?? "";

            novDokument.Vrsta = fields.GetProperty("type").GetProperty("stringValue").GetString() ?? "";

            string datum = fields.GetProperty("date").GetProperty("timestampValue").GetString() ?? "";

            if (DateTime.TryParse(datum, out DateTime datumDokumenta))
            {
                novDokument.Datum = datumDokumenta;
            }

            if (fields.TryGetProperty("fileName", out JsonElement imeField))
            {
                novDokument.ImeDatoteke = imeField.GetProperty("stringValue").GetString() ?? "";
            }

            if (fields.TryGetProperty("localPath", out JsonElement potField))
            {
                novDokument.LokalnaPot = potField.GetProperty("stringValue").GetString() ?? "";
            }

            if (fields.TryGetProperty("notes", out JsonElement opombeField))
            {
                novDokument.Opombe = opombeField.GetProperty("stringValue").GetString() ?? "";
            }

            dokumenti.Add(novDokument);
        }

        return dokumenti.OrderByDescending(d => d.Datum).ToList();
    }
    public async Task<bool> ShraniOpomnik(Opomnik opomnik, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/reminders";


        Dictionary<string, object> fields = new Dictionary<string, object>();


        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", opomnik.ZivalId } });


        fields.Add("name", new Dictionary<string, string> { { "stringValue", opomnik.Naziv } });


        fields.Add("type", new Dictionary<string, string> { { "stringValue", opomnik.Tip } });


        fields.Add("dateTime", new Dictionary<string, string> { { "timestampValue", opomnik.DatumCas.ToUniversalTime().ToString("o") } });


        fields.Add("description", new Dictionary<string, string> { { "stringValue", opomnik.Opis } });


        Dictionary<string, object> podatki = new Dictionary<string, object>();

        podatki.Add("fields", fields);


        string json = JsonSerializer.Serialize(podatki);


        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");


        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);


        if (odgovor.IsSuccessStatusCode)
        {
            return true;
        }


        string napaka = await odgovor.Content.ReadAsStringAsync();

        Console.WriteLine("Napaka pri shranjevanju opomnika: " + napaka);

        return false;
    }
    public async Task<List<Opomnik>> PridobiOpomnike(string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        List<Opomnik> opomniki = new List<Opomnik>();

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = "reminders"
                }
            },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = "animalId"
                        },
                        op = "EQUAL",
                        value = new
                        {
                            stringValue = animalId
                        }
                    }
                }
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent vsebina = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (!odgovor.IsSuccessStatusCode)
        {
            string napaka = await odgovor.Content.ReadAsStringAsync();
            Console.WriteLine("Napaka pri pridobivanju opomnikov: " + napaka);
            return opomniki;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        foreach (JsonElement rezultat in dokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty("document", out JsonElement document))
            {
                continue;
            }

            JsonElement fields = document.GetProperty("fields");

            Opomnik opomnik = new Opomnik();

            string celotnoIme = document.GetProperty("name").GetString() ?? "";

            opomnik.Id = celotnoIme.Split('/').Last();

            opomnik.ZivalId = fields.GetProperty("animalId").GetProperty("stringValue").GetString() ?? "";

            opomnik.Naziv = fields.GetProperty("name").GetProperty("stringValue").GetString() ?? "";

            opomnik.Tip = fields.GetProperty("type").GetProperty("stringValue").GetString() ?? "";

            string datumCas = fields.GetProperty("dateTime").GetProperty("timestampValue").GetString() ?? "";

            if (DateTime.TryParse(datumCas, out DateTime cas))
            {
                opomnik.DatumCas = cas;
            }

            if (fields.TryGetProperty("description", out JsonElement opisField))
            {
                opomnik.Opis = opisField.GetProperty("stringValue").GetString() ?? "";
            }

            opomniki.Add(opomnik);
        }

        return opomniki.OrderBy(o => o.DatumCas).ToList();
    }
    public async Task<List<Opomnik>> PridobiVseOpomnikeUporabnika(string userId, string idToken)
    {
        List<Opomnik> vsiOpomniki = new List<Opomnik>();

        List<Zival> zivali = await PridobiZivaliUporabnika(userId, idToken);

        foreach (Zival zival in zivali)
        {
            List<Opomnik> opomniki = await PridobiOpomnike(zival.Id, idToken);

            foreach (Opomnik opomnik in opomniki)
            {
                opomnik.ImeZivali = zival.Ime;

                vsiOpomniki.Add(opomnik);
            }
        }

        return vsiOpomniki.OrderBy(o => o.DatumCas).ToList();
    }

    public async Task<string?> PridobiUidPoShareCode(string shareCode, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/shareCodes/" + Uri.EscapeDataString(shareCode.Trim().ToUpper());

        HttpResponseMessage odgovor = await client.GetAsync(url);

        if (!odgovor.IsSuccessStatusCode)
        {
            return null;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        JsonElement fields = dokument.RootElement.GetProperty("fields");

        string userId = fields.GetProperty("userId").GetProperty("stringValue").GetString() ?? "";

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return userId;
    }
    public async Task<bool> PreveriDostop(string userId, string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string accessId = userId + "_" + animalId;

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/access/" + Uri.EscapeDataString(accessId);

        HttpResponseMessage odgovor = await client.GetAsync(url);

        return odgovor.IsSuccessStatusCode;
    }
    public async Task<bool> ShraniDeljenDostop(string userId, string animalId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string accessId = userId + "_" + animalId;

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/access" + "?documentId=" + Uri.EscapeDataString(accessId);

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("userId", new Dictionary<string, string>
    {
        { "stringValue", userId }
    });

        fields.Add("animalId", new Dictionary<string, string>
    {
        { "stringValue", animalId }
    });

        fields.Add("role", new Dictionary<string, string>
    {
        { "stringValue", "member" }
    });

        Dictionary<string, object> podatki = new Dictionary<string, object>();
        podatki.Add("fields", fields);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (odgovor.IsSuccessStatusCode)
        {
            return true;
        }

        string napaka = await odgovor.Content.ReadAsStringAsync();
        Console.WriteLine(napaka);

        return false;
    }
    public Task<bool> IzbrisiCepljenje(string id, string idToken)
    {
        return IzbrisiDokument("vaccinations", id, idToken);
    }

    public Task<bool> IzbrisiPregled(string id, string idToken)
    {
        return IzbrisiDokument("examinations", id, idToken);
    }

    public Task<bool> IzbrisiZdravilo(string id, string idToken)
    {
        return IzbrisiDokument("medications", id, idToken);
    }

    public Task<bool> IzbrisiOpomnik(string id, string idToken)
    {
        return IzbrisiDokument("reminders", id, idToken);
    }
    private async Task<bool> IzbrisiDokument(string collection, string documentId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/" + collection + "/" + documentId;

        HttpResponseMessage odgovor = await client.DeleteAsync(url);

        return odgovor.IsSuccessStatusCode;
    }



    public async Task<bool> PosodobiCepljenje(Cepljenje cepljenje, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/vaccinations/" + cepljenje.Id;

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", cepljenje.ZivalId } });

        fields.Add("name", new Dictionary<string, string> { { "stringValue", cepljenje.Naziv } });

        fields.Add("vaccinationDate", new Dictionary<string, string> { { "timestampValue", cepljenje.DatumCepljenja.ToUniversalTime().ToString("o") } });

        if (cepljenje.NaslednjeCepljenje.HasValue)
        {
            fields.Add("nextVaccinationDate", new Dictionary<string, string> { { "timestampValue", cepljenje.NaslednjeCepljenje.Value.ToUniversalTime().ToString("o") } });
        }

        fields.Add("veterinarian", new Dictionary<string, string> { { "stringValue", cepljenje.Veterinar } });

        fields.Add("notes", new Dictionary<string, string> { { "stringValue", cepljenje.Opombe } });

        string json = JsonSerializer.Serialize(new Dictionary<string, object> { { "fields", fields } });

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PatchAsync(url, vsebina);

        return odgovor.IsSuccessStatusCode;
    }

    public async Task<bool> PosodobiOpomnik(Opomnik opomnik, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/reminders/" + opomnik.Id;

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", opomnik.ZivalId } });

        fields.Add("name", new Dictionary<string, string> { { "stringValue", opomnik.Naziv } });

        fields.Add("type", new Dictionary<string, string> { { "stringValue", opomnik.Tip } });

        fields.Add("dateTime", new Dictionary<string, string> { { "timestampValue", opomnik.DatumCas.ToUniversalTime().ToString("o") } });

        fields.Add("description", new Dictionary<string, string> { { "stringValue", opomnik.Opis } });

        string json = JsonSerializer.Serialize(new Dictionary<string, object> { { "fields", fields } });

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PatchAsync(url, vsebina);

        return odgovor.IsSuccessStatusCode;
    }
    public async Task<bool> PosodobiPregled(Pregled pregled, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/examinations/" + pregled.Id;

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", pregled.ZivalId } });

        fields.Add("examinationDate", new Dictionary<string, string> { { "timestampValue", pregled.DatumPregleda.ToUniversalTime().ToString("o") } });

        fields.Add("reason", new Dictionary<string, string> { { "stringValue", pregled.Razlog } });

        fields.Add("diagnosis", new Dictionary<string, string> { { "stringValue", pregled.Diagnoza } });

        fields.Add("veterinarian", new Dictionary<string, string> { { "stringValue", pregled.Veterinar } });

        fields.Add("notes", new Dictionary<string, string> { { "stringValue", pregled.Opombe } });

        string json = JsonSerializer.Serialize(new Dictionary<string, object> { { "fields", fields } });

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PatchAsync(url, vsebina);

        return odgovor.IsSuccessStatusCode;
    }
    public async Task<bool> PosodobiZdravilo(Zdravilo zdravilo, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/medications/" + zdravilo.Id;

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("animalId", new Dictionary<string, string> { { "stringValue", zdravilo.ZivalId } });

        fields.Add("name", new Dictionary<string, string> { { "stringValue", zdravilo.Naziv } });

        fields.Add("dosage", new Dictionary<string, string> { { "stringValue", zdravilo.Odmerek } });

        fields.Add("frequency", new Dictionary<string, string> { { "stringValue", zdravilo.Pogostost } });

        fields.Add("startDate", new Dictionary<string, string> { { "timestampValue", zdravilo.DatumZacetka.ToUniversalTime().ToString("o") } });

        if (zdravilo.DatumKonca.HasValue)
        {
            fields.Add("endDate", new Dictionary<string, string> { { "timestampValue", zdravilo.DatumKonca.Value.ToUniversalTime().ToString("o") } });
        }

        fields.Add("veterinarian", new Dictionary<string, string> { { "stringValue", zdravilo.Veterinar } });

        fields.Add("notes", new Dictionary<string, string> { { "stringValue", zdravilo.Opombe } });

        string json = JsonSerializer.Serialize(new Dictionary<string, object> { { "fields", fields } });

        StringContent vsebina = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PatchAsync(url, vsebina);

        return odgovor.IsSuccessStatusCode;
    }
    public async Task<bool> ShraniShareCode(string shareCode, string userId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", idToken);

        string url =
            "https://firestore.googleapis.com/v1/projects/" +
            projectId +
            "/databases/(default)/documents/shareCodes?documentId=" +
            Uri.EscapeDataString(shareCode);

        Dictionary<string, object> fields = new Dictionary<string, object>();

        fields.Add("userId", new Dictionary<string, string>
    {
        { "stringValue", userId }
    });

        Dictionary<string, object> podatki = new Dictionary<string, object>();
        podatki.Add("fields", fields);

        string json = JsonSerializer.Serialize(podatki);

        StringContent vsebina =
            new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor =
            await client.PostAsync(url, vsebina);

        return odgovor.IsSuccessStatusCode;
    }

    public async Task<string?> PridobiMojShareCode(string userId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", idToken);

        string url =
            "https://firestore.googleapis.com/v1/projects/" +
            projectId +
            "/databases/(default)/documents/users/" +
            userId;

        HttpResponseMessage odgovor = await client.GetAsync(url);

        if (!odgovor.IsSuccessStatusCode)
        {
            return null;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        JsonElement fields =
            dokument.RootElement.GetProperty("fields");

        if (!fields.TryGetProperty("shareCode", out JsonElement shareCodeField))
        {
            return null;
        }

        return shareCodeField
            .GetProperty("stringValue")
            .GetString();
    }

    public string GenerirajShareCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Random random = new Random();

        string code = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray()
        );
        return "PET-" + code;
    }

    public async Task<bool> JeLastnikZivali(string animalId, string userId, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/animals/" + Uri.EscapeDataString(animalId);

        HttpResponseMessage odgovor = await client.GetAsync(url);

        if (!odgovor.IsSuccessStatusCode)
        {
            return false;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        JsonElement fields = dokument.RootElement.GetProperty("fields");

        string ownerId =
            fields.GetProperty("ownerId")
                  .GetProperty("stringValue")
                  .GetString() ?? "";

        return ownerId == userId;
    }

    private async Task<bool> IzbrisiDokumentePoPolju(string collection, string fieldName, string fieldValue, string idToken)
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        string url = "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents:runQuery";

        var queryBody = new
        {
            structuredQuery = new
            {
                from = new[]
                {
                new
                {
                    collectionId = collection
                }
            },

                where = new
                {
                    fieldFilter = new
                    {
                        field = new
                        {
                            fieldPath = fieldName
                        },

                        op = "EQUAL",

                        value = new
                        {
                            stringValue = fieldValue
                        }
                    }
                }
            }
        };

        string queryJson = JsonSerializer.Serialize(queryBody);

        StringContent vsebina = new StringContent(queryJson, Encoding.UTF8, "application/json");

        HttpResponseMessage odgovor = await client.PostAsync(url, vsebina);

        if (!odgovor.IsSuccessStatusCode)
        {
            return false;
        }

        string json = await odgovor.Content.ReadAsStringAsync();

        JsonDocument dokument = JsonDocument.Parse(json);

        foreach (JsonElement rezultat in dokument.RootElement.EnumerateArray())
        {
            if (!rezultat.TryGetProperty(
                    "document",
                    out JsonElement firestoreDokument))
            {
                continue;
            }

            string celotnoIme = firestoreDokument.GetProperty("name").GetString() ?? "";

            string documentId = celotnoIme.Split('/').Last();

            bool izbrisano = await IzbrisiDokument(collection, documentId, idToken);

            if (!izbrisano)
            {
                return false;
            }
        }

        return true;
    }
    public async Task<bool> IzbrisiZivalInPodatke(string animalId, string userId, string idToken)
    {
        try
        {
            
            bool jeLastnik = await JeLastnikZivali(animalId, userId, idToken);

            if (!jeLastnik)
            {
                return false;
            }
            
            string? deviceId = await PridobiGpsNapravo(animalId, idToken);

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                bool lokacijeIzbrisane = await IzbrisiDokumentePoPolju("locations", "deviceId", deviceId, idToken);

                if (!lokacijeIzbrisane)
                {
                    return false;
                }
            }

       
            if (!await IzbrisiDokumentePoPolju("vaccinations", "animalId", animalId, idToken))
            {
                return false;
            }

            if (!await IzbrisiDokumentePoPolju( "examinations","animalId", animalId, idToken))
            {
                return false;
            }

            if (!await IzbrisiDokumentePoPolju("medications", "animalId", animalId, idToken))
            {
                return false;
            }

            if (!await IzbrisiDokumentePoPolju( "documents","animalId", animalId, idToken))
            {
                return false;
            }

            if (!await IzbrisiDokumentePoPolju("reminders", "animalId", animalId, idToken))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                bool napravaIzbrisana = await IzbrisiDokument("gpsDevices", deviceId, idToken);

                if (!napravaIzbrisana)
                {
                    return false;
                }
            }

           
            if (!await IzbrisiDokumentePoPolju("access", "animalId", animalId, idToken))
            {
                return false;
            }

            
            return await IzbrisiDokument("animals", animalId, idToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine( "Napaka pri izbrisu živali: " + ex.Message);
            return false;
        }
    }
}


