namespace E_zivali.Models;

public class Cepljenje
{
    public string Id { get; set; } = "";

    public string ZivalId { get; set; } = "";

    public string Naziv { get; set; } = "";

    public DateTime DatumCepljenja { get; set; }

    public DateTime? NaslednjeCepljenje { get; set; }

    public string Veterinar { get; set; } = "";

    public string Opombe { get; set; } = "";
}