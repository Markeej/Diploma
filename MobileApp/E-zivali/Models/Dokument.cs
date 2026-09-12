using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_zivali.Models;

public class Dokument
{
    public string Id { get; set; } = "";

    public string ZivalId { get; set; } = "";

    public string Naziv { get; set; } = "";

    public string Vrsta { get; set; } = "";

    public DateTime Datum { get; set; }

    public string ImeDatoteke { get; set; } = "";

    public string LokalnaPot { get; set; } = "";

    public string Opombe { get; set; } = "";
}
