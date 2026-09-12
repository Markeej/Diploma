using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_zivali.Models;

public class Zdravilo
{
    public string Id { get; set; } = "";

    public string ZivalId { get; set; } = "";

    public string Naziv { get; set; } = "";

    public string Odmerek { get; set; } = "";

    public string Pogostost { get; set; } = "";

    public DateTime DatumZacetka { get; set; }

    public DateTime? DatumKonca { get; set; }

    public string Veterinar { get; set; } = "";

    public string Opombe { get; set; } = "";
}
