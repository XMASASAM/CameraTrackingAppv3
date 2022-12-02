using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
using System.Management;
using System.Threading;
namespace CameraTrackingAppv3
{
    public partial class Form1 : Form,IFormUpdate
    {
        VideoCapture capture;
        ManagementEventWatcher insertWatcher;
        ManagementEventWatcher removeWatcher;
        List<string> deviceID = new List<string>();
        bool f_start_up = false;
        bool f_first = false;

        delegate void ControlFormDelegate();
        string active_camera_id = "";
        public UserControl1 UserControl { get { return userControl11; } }
        public string GetActiveCameraID { get { return active_camera_id; } }

        bool f_jump2form3 = false;
        bool f_decide_skip = false;


        SettingsConfig config;

        public Form1(ref SettingsConfig config,bool jump2Form3,bool decide_skip)
        {
            InitializeComponent();
            this.config = config;


            if (config.VideoCapture != null)
            {
                capture = this.config.VideoCapture;
                active_camera_id = this.config.Property.CameraID;
                Main.SetUpVideoCapture(capture, UserControl.Width, UserControl.Height);

            }

            f_jump2form3 = jump2Form3;
            f_decide_skip = decide_skip;
            f_first = !this.config.Property.CameraID.Equals("");
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();


        }


        private void Form1_Load(object sender, EventArgs e)
        {

            LoadDeviceList();
            new DisplayInformation(this);


            Main.ChangeDisplayCameraForm(this);
            if (config.Property.CameraID.Equals(""))
            {
                userControl11.VisibleCameraName(false);
                userControl11.VisibleFPS(false);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var selected_index = comboBox1.SelectedIndex;
            Utils.WriteLine(comboBox1.SelectedIndex.ToString());
            if (selected_index < 0)
            {
                Utils.Alert_Note("起動できるカメラがありません");
                return;
            }

            //if (f_start_up || (!f_first && active_camera_id.Equals(deviceID[comboBox1.SelectedIndex])))
            if (f_start_up || (active_camera_id.Equals(deviceID[comboBox1.SelectedIndex])))
                return;
            
            f_start_up = true;
            var thread = new Thread(new ParameterizedThreadStart(ComformCamera));
            thread.Start(selected_index);
            userControl11.VisibleFPS(false);

        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {

            Utils.WriteLine("USB insert");
            Invoke(new Utils.InvokeVoid(LoadDeviceList));
            
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            Utils.WriteLine("USB remove");
            Invoke(new Utils.InvokeVoid(LoadDeviceList));
        }

        void GetAllConnectedCameras(out List<string> cameraName,out List<string> cameraID)
        {
            cameraName = new List<string>();
            cameraID = new List<string>();
            deviceID.Clear();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    cameraName.Add(device["Caption"].ToString());
                    cameraID.Add(device["DeviceID"].ToString());
                }
            }

        }


        void ComformCamera(object index)
        {
            var p = this.Location;
            var cp = userControl11.GetPictureBoxCenterPoint();
            p.X += userControl11.Location.X + cp.X;
            p.Y += userControl11.Location.Y + cp.Y;

            Invoke(new Utils.InvokeLoadAlert(Utils.ShowLoadAlert), "カメラを起動", "ロード中...", Properties.Resources.icon_loader_c_ww_01_s1,p,false);
            Invoke(new Utils.InvokeVoid(userControl11.DisplayClear));

            Main.ReSetVideoCapture();

            if (capture != null && capture.IsEnabledDispose)
            {
                lock (capture)
                {
                    capture.Release();
                    capture.Dispose();

                }
            }

            string id = deviceID[(int)index];

                capture = new VideoCapture((int)index);
         //   }

            if (!capture.IsOpened())
            {
                Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), "カメラを起動");
                Utils.Alert_Error("カメラを開けませんでした");
                return;
            }

            

            Utils.CameraWidth = capture.FrameWidth;
            Utils.CameraHeight = capture.FrameHeight;


            
            
            Utils.WriteLine("確認プロセス完了");
         //   Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), "カメラを起動");
            Invoke(new Utils.InvokeInt(DisplayActiveCamera),(int)index);


        }
        void LoadDeviceList()
        {
            comboBox1.Items.Clear();

            GetAllConnectedCameras(out var cameralist,out deviceID);



            foreach (var i in cameralist)
                comboBox1.Items.Add(i);

            if (comboBox1.Items.Count >= 1 && comboBox1.SelectedIndex < 0)
                comboBox1.SelectedIndex = 0;


            var find_id = active_camera_id;
            if (f_first) find_id = config.Property.CameraID;


            if (!find_id.Equals(""))
            {
                for (int i = 0; i < deviceID.Count; i++)
                    if (deviceID[i].Equals(find_id))
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
            }

            if (f_first)
            {
                if (comboBox1.SelectedIndex >= 0)
                {
                    this.button1_Click(null, EventArgs.Empty);
                }
                f_first = false;
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            insertWatcher.Dispose();
            removeWatcher.Dispose();
            Dispose();
        }


        void DisplayActiveCamera(int index)
        {
            active_camera_id = deviceID[index];


            Main.SetUpVideoCapture(capture, userControl11.PictureWidth, userControl11.PictureHeight);

            userControl11.SetCameraName((string)comboBox1.Items[index]);
            userControl11.VisibleCameraName(true);
            userControl11.VisibleFPS(true);

            f_start_up = false;
            Utils.CloseLoadAlert("カメラを起動");

            if (f_decide_skip)
            {
                button2_Click(null, EventArgs.Empty);

            }

        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            Main.Update();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(capture ==null || !capture .IsOpened())
            {
                Utils.Alert_Note("カメラを起動してください");
                return;
            }


            config.Property.CameraID = active_camera_id;
            config.VideoCapture = capture ;

            if (f_jump2form3)
            {
                if(!f_decide_skip)
                    config.Save();
                Utils.MainForm.FormStart(ref config);
            }
            else
            {
                var form = new Form4(ref config, true);
                form.Show();
            }
            insertWatcher.Stop();
            removeWatcher.Stop();
           // this.Visible = false;
            Close();
            
        }


        public void FormUpdate(ref Mat frame)
        {
            Main.DisplayCamera(frame);
        }

    }
}
