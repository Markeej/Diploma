# Diploma

Projekt je bil izdelan v okviru diplomskega dela in predstavlja informacijski sistem za upravljanje podatkov o hišnih živalih z mobilno aplikacijo ter prototipom GPS sledilne naprave.

Sistem sestavljajo:

- mobilna aplikacija, izdelana v .NET MAUI,
- Firebase Authentication,
- Cloud Firestore,
- ASP.NET Core API za sprejem lokacij sledilne naprave,
- sledilna naprava,
- Firestore Security Rules.

Za zagon projekta je potrebno nastaviti naslednje vrednosti.

## Mobilna aplikacija

V `FirebaseAuthService.cs` nastavi:

```csharp
private readonly string apiKey = "FIREBASE-API-KEY";
```
V FirebaseFirestoreService.cs nastavi:
```csharp
private readonly string projectId = "FIREBASE-PROJECT-ID";
```
API

Ustvari appsettings.json:
```csharp
{
  "TrackerSecrets": {
    "DEVICE-ID": "DEVICE-SECRET"
  }
}
```
Iz Firebase Console prenesi Service Account JSON in ga shrani kot:

API/E_zivali.api/Secrets/firebase-service-account.json

V Program.cs nastavi:
```csharp
FirestoreDb firestore = FirestoreDb.Create("FIREBASE-PROJECT-ID");
```
ESP32

V kodi sledilne naprave nastavi:
```csharp
const char* API_URL = "API-URL";
const char* DEVICE_ID = "DEVICE-ID";
const char* DEVICE_SECRET = "DEVICE-SECRET";
```
DEVICE_SECRET mora biti enak vrednosti v appsettings.json.

Firestore Security Rules

Pravila iz Firebase/firestore.rules kopiraj v Firebase Console pod:

Firestore Database → Rules

in jih objavi.

Opomba

Za pravilno delovanje projekta je treba v Firebase projektu omogočiti storitvi Cloud Firestore in Firebase Authentication. V Firebase Authentication je treba kot način prijave omogočiti Email/Password. 
