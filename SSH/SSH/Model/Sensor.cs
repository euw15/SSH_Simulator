using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSH.Model
{
    class Sensor
    {
        public bool activado    { get; set; }
        public int id           { get; set; }
        public ZonaSensor zona         { get; set; }

        public Sensor()
        {
            zona = ZonaSensor.NoZona;
        }
    }
}
