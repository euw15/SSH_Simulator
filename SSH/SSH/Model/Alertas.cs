using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSH.Model
{
    class Alertas
    {
        public int    mCodigoDeArmado { set; get; }
        public string mNumeroTelefono { set; get; }
        public string mNumeroUsuario  { set; get; }

        public Alertas()
        {
            mNumeroTelefono = "22550505";
            mCodigoDeArmado = 123;
        }
    }
}
