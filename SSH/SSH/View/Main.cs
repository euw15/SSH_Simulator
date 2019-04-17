using SSH.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSH
{
    public partial class Main : Form
    {

        ControladorSSH mMainController = null;
        public Main()
        {
            InitializeComponent();

            mMainController                     = new ControladorSSH();
            mMainController.LblBateria          = LblBateria;
            mMainController.LblComando          = LblCommando;
            mMainController.LblError            = LblError;
            mMainController.LblModo0            = LblModo0;
            mMainController.LblModo1            = LblModo1;
            mMainController.LblActionRequired  = LblActionRequired;
            mMainController.PicArmado           = PictureArmado;
            mMainController.PicBateria          = PictureBattery;
            mMainController.IniciarInterfaz();


        }

        private void button1_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("1");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("2");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("3");
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("4");
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("5");
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("6");
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("7");
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("8");
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("9");
        }

        private void ButtonAsterisco_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("*");
        }

        private void Button0_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("0");
        }

        private void ButtonNumeral_Click(object sender, EventArgs e)
        {
            mMainController.AddSymbolToCommandLabel("#");
        }

        private void ButtonEsc_Click(object sender, EventArgs e)
        {
            mMainController.DeleteLastSymbolToCommandLabel();
        }

        private void ButtonEnter_Click(object sender, EventArgs e)
        {
            mMainController.EjecutarComando();
        }

        private void ButtonPanic_Click(object sender, EventArgs e)
        {
            mMainController.ActivarAlarmaPanico();
        }

        private void ButtonBomberos_Click(object sender, EventArgs e)
        {
            mMainController.ActivarAlarmaBomberos();
        }

        
    }
}
