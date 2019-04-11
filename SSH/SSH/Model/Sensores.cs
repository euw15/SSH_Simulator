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
           // mSensores.Clear();
            using (var reader = new StreamReader(@".//EstadoSensores.csv"))
            {
               
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if(values.Length == 2)
                    {
                      
                        int id      = 0;
                        int estado  = 0;

                        var sensorIDEsNumero     = int.TryParse(values[0], out id);
                        var sersorEstadoEsNumero = int.TryParse(values[1], out estado);

                        if(sensorIDEsNumero  && sersorEstadoEsNumero)
                        {
                            Sensor sensorFound = mSensores.Find(x => x.id == id);
                            if(sensorFound != null)
                            {
                                sensorFound.activado = Convert.ToBoolean(estado);
                            }
                            else
                            {
                                Sensor newSensor = new Sensor();
                                newSensor.id = id;
                                newSensor.activado = Convert.ToBoolean(estado);
                                mSensores.Add(newSensor);
                            }
                           
                        }
                    }
                    
                }
            }
            return true;
        }
    }
}
