using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_zivali.Models
{
    public class Zival
    {
        public string Id { get; set; } = "";
        public string Ime { get; set; } = "";
        public string Vrsta { get; set; } = "";
        public string Pasma { get; set; } = "";
        public string Spol { get; set; } = "";
        public DateTime DatumRojstva { get; set; }
        public string Mikrocip { get; set; } = "";
        public string PotniList { get; set; } = "";
    }
}
