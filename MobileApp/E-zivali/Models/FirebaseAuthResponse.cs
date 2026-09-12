using System.Text.Json.Serialization;

namespace E_zivali.Models;

public class FirebaseAuthResponse
{
    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = ""; 

    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = "";

}