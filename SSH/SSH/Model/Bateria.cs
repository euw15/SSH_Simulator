using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSH.Model
{
    class Bateria
    {
        public bool isBatteryLow()
        {
            using (var reader = new StreamReader(@".//EstadoBateria.csv"))
            {

                while (!reader.EndOfStream)
                {
                    var line               = reader.ReadLine();
                    int porcentajeBateria  = 0;
                    var porcentajeEsNumero = int.TryParse(line, out porcentajeBateria);

                    if (porcentajeEsNumero)
                    {
                        if (porcentajeBateria < 20)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }
    }
}
