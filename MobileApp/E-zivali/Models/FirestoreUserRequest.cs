using System.Text.Json.Serialization;

namespace E_zivali.Models;

public class FirestoreUserRequest
{
    [JsonPropertyName("fields")]
    public FirestoreUserFields Fields { get; set; } = new();
}

public class FirestoreUserFields
{
    [JsonPropertyName("ime")]
    public FirestoreStringValue Ime { get; set; } = new();

}

public class FirestoreStringValue
{
    [JsonPropertyName("stringValue")]
    public string StringValue { get; set; } = "";
}