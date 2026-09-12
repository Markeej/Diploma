using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_zivali.Models
{
    public class Pregled
    {
        public string Id { get; set; } = "";

        public string ZivalId { get; set; } = "";

        public DateTime DatumPregleda { get; set; }

        public string Razlog { get; set; } = "";

        public string Diagnoza { get; set; } = "";

        public string Veterinar { get; set; } = "";

        public string Opombe { get; set; } = "";
    }
}
