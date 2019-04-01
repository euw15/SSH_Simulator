using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSH.Model
{
    enum ZonaSensor
    {
        Zona0,
        Zona1,
        NoZona
    }
    

    class Sensores
    {
        public List<Sensor> mSensores { get; }
        public Sensores()
        {
            mSensores = new List<Sensor>();
        }
        public void CrearSensores()
        {

        }

        public bool LeerEstadoSensores()
        {
            mSensores.Clear();
            using (var reader = new StreamReader(@".//EstadoSensores.csv"))
            {
               
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if(values.Length == 2)
                    {
                        Sensor nuevoSensor = new Sensor();
                        int id      = 0;
                        int estado  = 0;

                        var sensorIDEsNumero = int.TryParse(values[0], out id);
                        var sersorEstadoEsNumero = int.TryParse(values[1], out estado);

                        if(sensorIDEsNumero  && sersorEstadoEsNumero)
                        {
                            nuevoSensor.id = id;
                            nuevoSensor.activado = Convert.ToBoolean(estado);

                            mSensores.Add(nuevoSensor);
                        }
                    }
                    
                }
            }
            return true;
        }
    }
}
