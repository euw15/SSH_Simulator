using SSH.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSH.Controller
{
    enum Comandos
    {
        ArmarModo0,
        ArmarModo1,
        Desarmar,
        EditarNumeroUsuario,
        EditarNumeroAgencia,
        AsociarZonas,
        EditarNumerosArmado
    }

    enum EnterCmdState
    {
        primeraLlamada,
        segundaLlamada,
        terceraLlamada
    }

    enum ArmadoSistema
    {
        Modo0,
        Modo1,
        Desarmado
    }

    class ControladorSSH
    {

        public Label LblError           { get; set; }
        public Label LblBateria         { get; set; }
        public Label LblModo1           { get; set; }
        public Label LblModo0           { get; set; }
        public Label LblComando         { get; set; }
        public Label LblActionRequeried { get; set; }
        public PictureBox PicArmado     { get; set; }
        public PictureBox PicBateria    { get; set; }

        private Dictionary<string, Comandos>    mComandoEnumDict       = null;
        private Dictionary<Comandos, Delegate>  mDelegateComandos      = null;

        EnterCmdState commandCallState;
        Comandos mPreviuosCommand;
        int mZonaIngresada;

        Alertas  mAlertasModel;
        Sensores mSensoresModel;
        Bateria  mBateria;

        ArmadoSistema mArmadoSistema;

        private Timer mRefreshSensorsTimer  = null;
        private Timer mCheckSensorState     = null;
        private Timer mCheckPrincipalSensor = null;

        System.Media.SoundPlayer mAlarmSound = new System.Media.SoundPlayer(@".//alert.wav");
        System.Media.SoundPlayer mSoftAlarmSound = new System.Media.SoundPlayer(@".//alert.wav");

        bool PrincipalSensorOn = false;
        int timeCount;

        public ControladorSSH()
        {
            mComandoEnumDict = new Dictionary<string, Comandos>();
            mComandoEnumDict.Add("*", Comandos.ArmarModo0);
            mComandoEnumDict.Add("#", Comandos.ArmarModo1);
            mComandoEnumDict.Add("103", Comandos.EditarNumeroUsuario);
            mComandoEnumDict.Add("104", Comandos.EditarNumeroAgencia);
            mComandoEnumDict.Add("105", Comandos.AsociarZonas);
            mComandoEnumDict.Add("106", Comandos.EditarNumerosArmado);

            mDelegateComandos = new Dictionary<Comandos, Delegate>();
            mDelegateComandos.Add(Comandos.ArmarModo0,          new Func<bool>(this.ArmarModo0));
            mDelegateComandos.Add(Comandos.ArmarModo1,          new Func<bool>(this.ArmarModo1));
            mDelegateComandos.Add(Comandos.Desarmar,            new Func<bool>(this.Desarmar));
            mDelegateComandos.Add(Comandos.EditarNumeroUsuario, new Func<bool>(this.EditarNumeroUsuario));
            mDelegateComandos.Add(Comandos.EditarNumeroAgencia, new Func<bool>(this.EditarNumeroAgencia));
            mDelegateComandos.Add(Comandos.AsociarZonas,        new Func<bool>(this.AsociarZonas));
            mDelegateComandos.Add(Comandos.EditarNumerosArmado, new Func<bool>(this.EditarNumerosArmado));

            mAlertasModel    = new Alertas();
            mSensoresModel   = new Sensores();
            mBateria         = new Bateria();
            commandCallState = EnterCmdState.primeraLlamada;
            mArmadoSistema   = ArmadoSistema.Desarmado;
            
            mRefreshSensorsTimer = new Timer();
            mRefreshSensorsTimer.Tick += new EventHandler(RefreshSensors);
            mRefreshSensorsTimer.Tick += new EventHandler(RegreshBattery);
            mRefreshSensorsTimer.Interval = 1000;
            mRefreshSensorsTimer.Start();

            mCheckSensorState = new Timer();
            mCheckSensorState.Tick += new EventHandler(CheckSensorStatus);
            mCheckSensorState.Interval = 2000;
            mCheckSensorState.Start();

            mCheckPrincipalSensor = new Timer();
            mCheckPrincipalSensor.Tick += new EventHandler(CheckPrincipalSensor);
            mCheckPrincipalSensor.Interval = 1000;
            mCheckPrincipalSensor.Start();

        }

        public void IniciarInterfaz()
        {
            LblComando.Text     = "";
            LblModo0.Visible    = false;
            LblModo1.Visible    = false;
            LblBateria.Visible  = false;
            LblError.Visible    = false;
            LblActionRequeried.Visible  = false;
        }

        public void EjecutarComando()
        {
            //PARA ARMAR 
            if (commandCallState == EnterCmdState.primeraLlamada && (LblComando.Text.Contains("*") || LblComando.Text.Contains("#")))
            {
                string comandoIngresado = LblComando.Text;
                if (LblComando.Text.Contains("*"))
                {
                    mPreviuosCommand = Comandos.ArmarModo0;
                    mDelegateComandos[mPreviuosCommand].DynamicInvoke();
                }
                else if (LblComando.Text.Contains("#"))
                {
                    mPreviuosCommand = Comandos.ArmarModo1;
                    mDelegateComandos[mPreviuosCommand].DynamicInvoke();
                }
            }
            //LA PRIMERA VEZ QUE APRETA ENTER, ACEPTA EL COMANDO Y SE ALISTA PARA RECIBIR EL VALOR
            //CUALQUIER COMANDO QUE NO SEA ARMAR O ASOCIAR ZONA
            else if (commandCallState == EnterCmdState.primeraLlamada)
            {
                string comandoIngresado = LblComando.Text;
                Comandos actualComando;
                int numeroDesarmado;
                var isNumeric = int.TryParse(comandoIngresado, out numeroDesarmado);
                if (isNumeric)
                {
                    if(numeroDesarmado == mAlertasModel.mCodigoDeArmado)
                    {
                        Desarmar();
                    }
                }
                else if (mComandoEnumDict.ContainsKey(comandoIngresado))
                {
                    actualComando               = mComandoEnumDict[comandoIngresado];
                    mPreviuosCommand            = actualComando;
                    LblError.Visible            = false;
                    commandCallState            = EnterCmdState.segundaLlamada;
                    LblComando.Text             = "";
                    LblActionRequeried.Visible  = true;
                }
                else
                {
                    LblError.Visible    = true;
                }
            }
            //ASOCIAR
            else if (commandCallState == EnterCmdState.segundaLlamada && mPreviuosCommand == Comandos.AsociarZonas)
            {
                string zonaIngresadaCmd = LblComando.Text;
                if(zonaIngresadaCmd.Equals("0") || zonaIngresadaCmd.Equals("1"))
                {
                    mZonaIngresada = Convert.ToInt32(zonaIngresadaCmd);
                    commandCallState = EnterCmdState.terceraLlamada;
                    LblComando.Text = "";
                }
                else
                {
                    LblError.Visible = true;
                }
            }
            //TODOS LOS COMANDOS PASAN POR ACA EN EL SEGUNDO O TERCER VEZ QUE APRETAN ENTER MENOS ARMAR
            else
            {
                mDelegateComandos[mPreviuosCommand].DynamicInvoke();
            }
            
                
        }
        public void AddSymbolToCommandLabel(string letter)
        {
            string mComandoIngresado = LblComando.Text;

            if (mComandoIngresado.Length > 15)
            {
                return;
            }

            mComandoIngresado += letter;
            LblComando.Text = mComandoIngresado;
        }

        public void DeleteLastSymbolToCommandLabel()
        {
            if(LblError.Visible == true)
            {
                LblError.Visible = false;
                LblActionRequeried.Visible = false;
                commandCallState = EnterCmdState.primeraLlamada;
            }
           
            string mComandoIngresado = LblComando.Text;
            if (mComandoIngresado.Length <= 0)
            {
                return;
            }

            mComandoIngresado = mComandoIngresado.Remove(mComandoIngresado.Length - 1);

            
            LblComando.Text = mComandoIngresado;
        }

        public bool ArmarModo0()
        {
            string comandoIngresado = LblComando.Text;
            var charsToRemove = new string[] { "*", "#" };
            foreach (var c in charsToRemove)
            {
                comandoIngresado = comandoIngresado.Replace(c, string.Empty);
            }

            int numeroArmado;
            var isNumeric = int.TryParse(comandoIngresado, out numeroArmado);
            if (isNumeric)
            {
                if (mAlertasModel.mCodigoDeArmado == numeroArmado)
                {
                    mArmadoSistema = ArmadoSistema.Modo0;
                    LblModo1.Visible = false;
                    LblModo0.Visible = true;
                    PicArmado.Image = global::SSH.Properties.Resources.iconfinder_ledgreen_1784;
                }
                else
                {
                    LblError.Visible = true;
                }
            }
            else
            {
                LblError.Visible = true;
            }
            CleanCommandView();
            return true;
        }

        public bool ArmarModo1()
        {
            string comandoIngresado = LblComando.Text;
            var charsToRemove = new string[] { "*", "#" };
            foreach (var c in charsToRemove)
            {
                comandoIngresado = comandoIngresado.Replace(c, string.Empty);
            }

            int numeroArmado;
            var isNumeric = int.TryParse(comandoIngresado, out numeroArmado);
            if (isNumeric)
            {
                if (mAlertasModel.mCodigoDeArmado == numeroArmado)
                {
                    mArmadoSistema = ArmadoSistema.Modo1;
                    LblModo1.Visible = true;
                    LblModo0.Visible = false;
                    PicArmado.Image = global::SSH.Properties.Resources.iconfinder_ledgreen_1784;
                }
                else
                {
                    LblError.Visible = true;
                }
            }
            else
            {
                LblError.Visible = true;
            }
            CleanCommandView();
            return true;
        }

        public bool Desarmar()
        {
            string mValorIngresado = LblComando.Text;
            int numeroArmado;
            var isNumeric = int.TryParse(mValorIngresado, out numeroArmado);
            if (isNumeric)
            {
                if(numeroArmado == mAlertasModel.mCodigoDeArmado)
                {
                    mArmadoSistema = ArmadoSistema.Desarmado;
                    LblModo1.Visible = false;
                    LblModo0.Visible = false;
                    PicArmado.Image = global::SSH.Properties.Resources.iconfinder_ledorange_1787;
                    mAlarmSound.Stop();
                    mCheckSensorState.Start();
                    PrincipalSensorOn = false;
                    timeCount = 0;
                    mSoftAlarmSound.Stop();
                }
                else
                {
                    LblError.Visible = true;
                }
            }
            else
            {
                LblError.Visible = true;
            }

            CleanCommandView();
            return true;
        }

        public bool EditarNumeroUsuario()
        {
            string mValorIngresado = LblComando.Text;
            if (!string.IsNullOrEmpty(mValorIngresado))
            {
                mAlertasModel.mNumeroUsuario = mValorIngresado;
                MessageBox.Show("Numero usuario: " + mAlertasModel.mCodigoDeArmado);
            }
            else
            {
                CleanCommandView();
                LblError.Visible = false;
            }

            CleanCommandView();
            return true;
        }

        public bool EditarNumeroAgencia()
        {
            string mValorIngresado = LblComando.Text;
            if (!string.IsNullOrEmpty(mValorIngresado))
            {
                mAlertasModel.mNumeroTelefono = mValorIngresado;
            }
            else
            {
                LblError.Visible = false;
            }
            
            CleanCommandView();
            return true;
        }

        public bool AsociarZonas()
        {
            string idSensorIngresadoStr = LblComando.Text;
            if (!string.IsNullOrEmpty(idSensorIngresadoStr))
            {
                int idSensorIngresado;
                var isNumeric = int.TryParse(idSensorIngresadoStr, out idSensorIngresado);
                if (isNumeric)
                {
                    var sensor = mSensoresModel.mSensores.Find(x => x.id == idSensorIngresado);
                    if (mZonaIngresada == 0)
                    {
                        sensor.zona = ZonaSensor.Zona0;
                    }
                    else if(mZonaIngresada == 1)
                    {
                        sensor.zona = ZonaSensor.Zona1;
                    }
                }
                else
                {
                    LblError.Visible = true;
                }
            }
            CleanCommandView();
            return true;
        }

        public bool EditarNumerosArmado()
        {
            string mValorIngresado = LblComando.Text;
            if (!string.IsNullOrEmpty(mValorIngresado))
            {
                int mNuevoCodigoDeArmado;
                var isNumeric = int.TryParse(mValorIngresado, out mNuevoCodigoDeArmado);
                if (isNumeric)
                {
                    mAlertasModel.mCodigoDeArmado = mNuevoCodigoDeArmado;
                    MessageBox.Show("Codigo Armado: " + mAlertasModel.mCodigoDeArmado);
                }
                else
                {
                    LblError.Visible = true;
                }
            }
            else
            {
                LblError.Visible = true;
            }
            
           
            CleanCommandView();
            return true;
        }

        public void ActivarAlarmaBomberos()
        {
            mAlarmSound.PlayLooping();
            MessageBox.Show("Llamando a el numero de agencia: " + mAlertasModel.mNumeroTelefono + " por alarma de incendio");
           // LblActionRequeried.Text = "Llamando a el numero de agencia: " + mAlertasModel.mNumeroTelefono + "\r\npor alarma de incendio";
           // LblActionRequeried.Visible = true;
        }

        public void ActivarAlarmaPanico()
        {
            mAlarmSound.PlayLooping();
            MessageBox.Show("Llamando a el numero de agencia: " + mAlertasModel.mNumeroTelefono + " por alarma de panico");
        }

        private void CleanCommandView()
        {
            LblActionRequeried.Visible = false;
            LblComando.Text = "";
            commandCallState = EnterCmdState.primeraLlamada;
        }

        private void RefreshSensors(object sender, EventArgs e)
        {
           mSensoresModel.LeerEstadoSensores();
        }

        private void RegreshBattery(object sender, EventArgs e)
        {
            if (mBateria.isBatteryLow())
            {
                LblBateria.Visible = true;
                PicBateria.Image = global::SSH.Properties.Resources.iconfinder_ledgreen_1784;
            }
            else
            {
                LblBateria.Visible = false;
                PicBateria.Image = global::SSH.Properties.Resources.iconfinder_ledorange_1787;
            }
        }

        private void CheckSensorStatus(object sender, EventArgs e)
        {
            if(mArmadoSistema == ArmadoSistema.Desarmado)
            {
                return;
            }
            else if (mArmadoSistema == ArmadoSistema.Modo0)
            {
                //DETERMINA SI HAY UN SENSOR ACTIVADO EN LA ZONA 0
                var sensoresActivos = mSensoresModel.mSensores.FindAll(x => x.activado == true  && x.id != 1);
                
                if (sensoresActivos.Count > 0)
                {
                    
                    //ARMA LA LISTA DE SENSORES ACTIVADOS EN UN STRING PARA EL MENSAJE DE LLAMDA
                    string stringSensores = "";
                    foreach (var sensor in sensoresActivos)
                    {
                        stringSensores += sensor.id;
                        stringSensores += ",";
                    }
                    stringSensores = stringSensores.Remove(stringSensores.Length-1);

                    //ACTIVA MENSAJE DE ALARMA Y SONIDO
                    mCheckSensorState.Stop();
                    mAlarmSound.PlayLooping();
                    MessageBox.Show("Llamando a el numero de agencia: " + mAlertasModel.mNumeroTelefono + " por alarma del sensor " + stringSensores +
                       "\r\n con el numero de usuario " + mAlertasModel.mNumeroUsuario);

                }
            }
            else if (mArmadoSistema == ArmadoSistema.Modo1)
            {
                //DETERMINA SI HAY UN SENSOR ACTIVADO EN LA ZONA 1
                var sensoresActivos = mSensoresModel.mSensores.FindAll(x => x.activado == true && x.zona == ZonaSensor.Zona1);

                if(sensoresActivos.Count > 0)
                {
                    //ARMA LA LISTA DE SENSORES ACTIVADOS EN UN STRING PARA EL MENSAJE DE LLAMDA
                    string stringSensores = "";
                    foreach (var sensor in sensoresActivos)
                    {
                        stringSensores += sensor.id;
                        stringSensores += ",";
                    }
                    stringSensores = stringSensores.Remove(stringSensores.Length-1);

                    //ACTIVA MENSAJE DE ALARMA Y SONIDO
                    mCheckSensorState.Stop();
                    mAlarmSound.PlayLooping();
                    MessageBox.Show("Llamando a el numero de agencia: " + mAlertasModel.mNumeroTelefono + " por alarma del sensor " + stringSensores + 
                        "\r\n con el numero de usuario " + mAlertasModel.mNumeroUsuario);
                    
                }
                
            }
        }

        private void CheckPrincipalSensor(object sender, EventArgs e)
        {
            if (PrincipalSensorOn && mArmadoSistema == ArmadoSistema.Modo0 && timeCount <= 0)
            {
                mSoftAlarmSound.PlayLooping();
                timeCount++;
            }
            else if(timeCount < 30 && timeCount > 0 && mArmadoSistema == ArmadoSistema.Modo0)
            {
                timeCount++;
            }
            else if(timeCount >= 30 && mArmadoSistema == ArmadoSistema.Modo0)
            {
                mAlarmSound.PlayLooping();
                MessageBox.Show("Llamando a el numero de agencia: " + mAlertasModel.mNumeroTelefono + " por alarma del sensor 1"  +
                    "\r\n con el numero de usuario " + mAlertasModel.mNumeroUsuario);
                PrincipalSensorOn = false;
                timeCount = 0;
            }
            else
            {
                //DETERMINA SI HAY UN SENSOR ACTIVADO EN LA ZONA 0
                var sensoresActivos = mSensoresModel.mSensores.FindAll(x => x.activado == true && x.zona == ZonaSensor.Zona0 && x.id == 1);
                if (sensoresActivos.Count > 0)
                {
                    PrincipalSensorOn = true;
                }
            }
        }
    }

}
