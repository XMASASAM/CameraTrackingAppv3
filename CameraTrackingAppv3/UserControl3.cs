using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
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
    public partial class UserControl3 : UserControl
    {
        ManagementEventWatcher insertWatcher;
        ManagementEventWatcher removeWatcher;
        List<string> camera_name = new List<string>();
        List<string> camera_id = new List<string>();
        string condidate_camera_id = "";
      //  int selected_index = -1;
        string selected_id = "";
        string selected_name = "";
     //   int active_index = -1;
        string active_camera_id = "";
        UserControl1 userControl1;
        bool f_start_up = false;
        VideoCapture temp_capture;
        VideoCapture capture;

        public UserControl3()
        {
            InitializeComponent();
        }

        public string ActiveCameraID()
        {
            return active_camera_id;
        }

        public VideoCapture ActiveVideoCapture()
        {
            return capture;
        }
        

        public void Init(SettingsConfig config,bool have_active=true)
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();


          //  if (have_active)
         //   {
                capture = config.VideoCapture;
            if (capture != null)
                active_camera_id = config.Property.CameraID;
        //    }
        //    else
         //   {
         //       StartUp(config.Property.CameraID);
          //  }

            if (!active_camera_id.Equals(""))
            {
                StartUp(active_camera_id);
            }
            else
            {
                StartUp(config.Property.CameraID);
               // LoadDeviceList();
            }

            /*   var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPSignedDriver WHERE DeviceID LIKE 'USB%' AND DeviceClass = 'Camera'");

               foreach (var device in searcher.Get().Cast<ManagementObject>())//.OrderBy(n => n["PDO"]))
               {
                   Utils.WriteLine("-----");
                   Utils.WriteLine(device.GetPropertyValue("FriendlyName") as string);
                   Utils.WriteLine(device.GetPropertyValue("DeviceClass") as string);
                   Utils.WriteLine(device.GetPropertyValue("DeviceID") as string);
                   Utils.WriteLine(device.GetPropertyValue("PDO") as string);
                   Utils.WriteLine("-----");
               }*/

        }

        public void End()
        {
            insertWatcher.Stop();
            removeWatcher.Stop();
            insertWatcher.Dispose();
            removeWatcher.Dispose();
        }

        public void SetUserControl1(UserControl1 userControl1)
        {
            this.userControl1 = userControl1;
        }

        public void StartUp(string camera_id)
        {
            condidate_camera_id = camera_id;
            LoadDeviceList();

            // comboBox1.SelectedIndex = camera_id.IndexOf(condidate_camera_id);
            button1_Click(null, null);
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

        void GetAllConnectedCameras(out List<string> cameraName, out List<string> cameraID)
        {
            cameraName = new List<string>();
            cameraID = new List<string>();
            
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    cameraName.Add(device["Caption"].ToString());
                    cameraID.Add(device["DeviceID"].ToString());
                }
            }

        }

        void LoadDeviceList()
        {
            comboBox1.Items.Clear();

            GetAllConnectedCameras(out camera_name , out camera_id);

            foreach (var i in camera_name)
                comboBox1.Items.Add(i);


            comboBox1.SelectedIndex = camera_id.IndexOf(condidate_camera_id);

            /* if(camera_name.Count > 0 && comboBox1.SelectedIndex < 0)
             {
                 comboBox1.SelectedIndex = 0;
                 selected_camera_id = camera_id[0];
             }*/
        /*    Utils.WriteLine("===================");
           for(int i = 0; i < camera_name.Count; i++)
            {
                Utils.WriteLine("CameraName: " + camera_name[i]);
                Utils.WriteLine("CameraID  : " + camera_id[i]);
                Utils.WriteLine("===================");

            }
            Utils.WriteLine("CondidateID: " + condidate_camera_id);
            Utils.WriteLine("ActiveID: " + active_camera_id);
            Utils.WriteLine("ConfigID: " + Utils.Config.Property.CameraID);*/

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < camera_id.Count)
                condidate_camera_id = camera_id[comboBox1.SelectedIndex];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //  selected_index = comboBox1.SelectedIndex;
            selected_id = condidate_camera_id;
            if (selected_id.Equals(""))
            {
                if (camera_id.Count > 0)
                {
                    //selected_index = 0;
                    selected_id = camera_id[0];
                    condidate_camera_id = camera_id[0];
                }
                else
                {
                    Utils.Alert_Note("起動できるカメラがありません");
                    return;
                }
            }

            

            if (f_start_up || active_camera_id.Equals(selected_id)) return;

            var selected_index = camera_id.IndexOf(selected_id);
            if (selected_index == -1) return;

            f_start_up = true;

            selected_name = camera_name[selected_index];

            var thread = new Thread(new ThreadStart(StartUpCamera));
            thread.Start();

            if (userControl1 != null)
            {
                userControl1.VisibleFPS(false);
                userControl1.DisplayClear();
            }
        }



        void StartUpCamera()
        {
            var p = this.Location;
            var cp = userControl1.GetPictureBoxCenterPoint();
            p.X += userControl1.Location.X + cp.X;
            p.Y += userControl1.Location.Y + cp.Y;

            Invoke(new Utils.InvokeLoadAlert(Utils.ShowLoadAlert), "カメラを起動", "ロード中...", Properties.Resources.icon_loader_c_ww_01_s1, p, false);
            //Invoke(new Utils.InvokeVoid(userControl1.DisplayClear));

            Main.ReSetVideoCapture();

           /* if (capture != null && capture.IsEnabledDispose)
            {
                lock (capture)
                {
                    capture.Release();
                    capture.Dispose();

                }
            }*/
          // new VideoCapture(0,VideoCaptureAPIs.)
            string id = selected_id;//camera_id[(int)index];

            var index = camera_id.IndexOf(id);
            var ok = index != -1;
            if (ok)
                temp_capture = new VideoCapture(index);
              //  capture = new VideoCapture(index);
           // new VideoCapture()
            
            if (!ok || !temp_capture.IsOpened())
            {
                Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), "カメラを起動");
                Utils.Alert_Error("カメラを開けませんでした");
                f_start_up = false;

                return;
            }




            Utils.CameraWidth = temp_capture.FrameWidth;
            Utils.CameraHeight = temp_capture.FrameHeight;




            Utils.WriteLine("確認プロセス完了");
            //   Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), "カメラを起動");
            Invoke(new Utils.InvokeVoid(DecideActiveCamera));


        }
        
        void DecideActiveCamera()
        {
            Utils.WriteLine("DecideActiveCamera");

            active_camera_id = selected_id;

            
            Main.SetUpVideoCapture(temp_capture, userControl1.PictureWidth, userControl1.PictureHeight);

            if (capture != null && capture.IsEnabledDispose)
            {
                lock (capture)
                {
                    capture.Release();
                    capture.Dispose();

                }
            }
            capture = temp_capture;

            f_start_up = false;

            userControl1.SetCameraName(selected_name);
            userControl1.VisibleCameraName(true);
            userControl1.VisibleFPS(true);

            Utils.CloseLoadAlert("カメラを起動");

          /*  if (f_decide_skip)
            {
                button2_Click(null, EventArgs.Empty);

            }*/

        }

    }
}
