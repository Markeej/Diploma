using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_zivali.Models
{
    public class Lokacija
    {
        public string Id { get; set; } = "";
        public string GpsNapravaId { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Cas { get; set; }
    }
}
