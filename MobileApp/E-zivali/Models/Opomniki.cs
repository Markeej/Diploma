using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_zivali.Models;

public class Opomnik
{
    public string Id { get; set; } = "";

    public string ImeZivali { get; set; } = "";
    public string ZivalId { get; set; } = "";

    public string Naziv { get; set; } = "";

    public string Opis { get; set; } = "";

    public DateTime DatumCas { get; set; }

    public string Tip { get; set; } = "";
}