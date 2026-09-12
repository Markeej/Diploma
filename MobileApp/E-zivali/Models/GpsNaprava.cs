using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_zivali.Models
{
    public class GpsNaprava
    {
        public string Id { get; set; } = "";
        public string ZivalId { get; set; } = "";
        public string Naziv { get; set; } = "";
        public string DeviceId { get; set; } = "";
        public bool Aktivna { get; set; }
    }
}
