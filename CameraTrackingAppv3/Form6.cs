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
        Form8 form8;

        public Form6()//Form3 form3,Form1 form1)
        {
            InitializeComponent();
            //this.form3 = form3;
            // this.form1 = form1;

            Utils.Temp_Config = new SettingsConfig(Utils.Config);


        }

        private void Form6_Load(object sender, EventArgs e)
        {
            SetPortNumber(Utils.Config.Property.PortNumber);
            userControl51.SetValue(Utils.Config.Property.MoveMag);
            userControl52.SetValue(Utils.Config.Property.ThresholdMag);
            userControl53.SetValue(Utils.Config.Property.AxisXMag);
            userControl54.SetValue(Utils.Config.Property.AxisYMag);
            userControl55.SetValue(Utils.Config.Property.ClickInterval);
            userControl56.SetValue(Utils.Config.Property.DoubleClickInterval);
            userControl57.SetValue(Utils.Config.Property.InfraredFirstThreshold);
            userControl58.SetValue(Utils.Config.Property.InfraredFirstErodeIteration);
            userControl59.SetValue(Utils.Config.Property.InfraredTrackErodeIteration);


            userControl31.SetUserControl1(Main.GetUserControl1);
            userControl31.Init(Utils.Config);
         //   userControl31.StartUp(Utils.Temp_Config.Property.CameraID);

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
            SetUserControl5Parameter();

            Utils.Config.Set(Utils.Temp_Config);
            Utils.Config.Save();
            SettingsConfig.Adapt(Utils.Config);

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

        private void button6_Click(object sender, EventArgs e)
        {
            if (form8 != null && !form8.IsDisposed)
            {
                form8.Focus();
            }
            else
            {
                form8 = new Form8(this, Utils.Temp_Config);
                form8.Show();
            }
           // MessageBox.Show("ポート番号を変更します。")
        }

        public void SetPortNumber(int port_number)
        {
            Utils.Temp_Config.Property.PortNumber = port_number;
            label1.Text = "ポート番号:" + port_number.ToString();
            Main.GetConnect().Init(port_number);
        }

        private void Form6_FormClosing(object sender, FormClosingEventArgs e)
        {
            SetUserControl5Parameter();

            if (!Utils.Config.Property.Equals(Utils.Temp_Config.Property))
            {
                var r = MessageBox.Show("設定に変更があります。変更を適応させずに設定画面を終了します", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (r == DialogResult.OK)
                {
                    SettingsConfig.Adapt(Utils.Config);
                }
                else
                {
                    e.Cancel = true;
                }

            }

            if (form8 != null && !form8.IsDisposed)
            {
                form8.Close();
            }

            userControl31.End();

        }

        void SetUserControl5Parameter()
        {
            Utils.Temp_Config.Property.MoveMag = userControl51.GetOutPut();
            Utils.Temp_Config.Property.ThresholdMag = userControl52.GetOutPut();
            Utils.Temp_Config.Property.AxisXMag = userControl53.GetOutPut();
            Utils.Temp_Config.Property.AxisYMag = userControl54.GetOutPut();
            Utils.Temp_Config.VideoCapture = userControl31.ActiveVideoCapture();
            Utils.Temp_Config.Property.CameraID = userControl31.ActiveCameraID();
            Utils.Temp_Config.Property.ClickInterval = userControl55.GetOutPut();
            Utils.Temp_Config.Property.DoubleClickInterval = userControl56.GetOutPut();
            Utils.Temp_Config.Property.InfraredFirstThreshold = (int)userControl57.GetOutPut();
            Utils.Temp_Config.Property.InfraredFirstErodeIteration = (int)userControl58.GetOutPut();
            Utils.Temp_Config.Property.InfraredTrackErodeIteration = (int)userControl59.GetOutPut();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage5_Click(object sender, EventArgs e)
        {

        }
    }
    
}
