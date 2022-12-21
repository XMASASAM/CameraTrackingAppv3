using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
namespace CameraTrackingAppv3
{
    public partial class Form6 : Form
    {
        // Form3 form3;
        // Form1 form1;

        public Form6()//Form3 form3,Form1 form1)
        {
            InitializeComponent();
            //this.form3 = form3;
            // this.form1 = form1;

            Utils.Temp_Config = new SettingsConfig(Utils.Config);


        }

        private void Form6_Load(object sender, EventArgs e)
        {
            userControl51.SetValue(Utils.Config.Property.MoveMag);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4(ref Utils.Temp_Config, false);
            form4.Show();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form form = new Form1(ref Utils.Temp_Config, true, false);
            form.Show();
            // form1.FormStart(ref Utils.Temp_Config);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Utils.Config.Set(Utils.Temp_Config);

            Utils.Config.Save();
            SettingsConfig.Adapt(Utils.Config);
            Utils.Config.Property.MoveMag = userControl51.GetOutPut();
            Utils.Config.Property.ThresholdMag = userControl52.GetOutPut();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Utils.WriteLine("ConnectStart!!!!!!!");
            Main.GetConnect().Start();
            Utils.WriteLine("StartBroadcast!!!!!!!");
            Main.GetConnect().BroadcastConnectSignal();

            Thread thread = new Thread(TEMP01);
            thread.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Utils.WriteLine("ConnectStart2!!!!!!!");
            Main.GetConnect().Start();
        }

        void TEMP01()
        {
            Utils.WriteLine("StartWait500!!!!!!!");
            Thread.Sleep(500);
            Utils.WriteLine("StartSequenceLoad!!!!!!!");
            Main.GetConnect().SequenceLoadSignal();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Utils.MainForm.WaitCursor(true);
        }

    }
    
}
