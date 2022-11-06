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
    public partial class Form1 : Form
    {
        VideoCapture capture;
        Graphics graphics;
        ManagementEventWatcher insertWatcher;
        ManagementEventWatcher removeWatcher;
        List<string> deviceID = new List<string>();
        Vec2i comform_picture_offset = new Vec2i(0,0);
        Vec2i comform_picture_size =  new Vec2i(0,0);
        double scale;
        bool f_set_up = false;
        bool f_start_up = false;
        bool f_camera_visible = true;
        bool f_control_active = false;
        bool f_infrared_mode = true;

        delegate void ControlFormDelegate();
        string active_camera_id = "";
        Mat camera_frame = new Mat();
        Tracker mouse_tracker;
        public Form1()
        {
            InitializeComponent();
            //TODO:aaaaa
            LoadDeviceList();
            label1.Visible = false;
            SetCameraOutPut(pictureBox1.CreateGraphics());

            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
            mouse_tracker = new Tracker();
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

            if (f_start_up || active_camera_id.Equals(deviceID[comboBox1.SelectedIndex]))
                return;
            f_start_up = true;
            var thread = new Thread(new ParameterizedThreadStart(ComformCamera));
            thread.Start(selected_index);

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
            p.X += pictureBox1.Location.X + (int)(pictureBox1.Width * 0.2);
            p.Y += pictureBox1.Location.Y + (int)(pictureBox1.Height * 0.4);
            Invoke(new Utils.InvokeLoadAlert(Utils.ShowLoadAlert),"カメラを起動",p);
            graphics.Clear(BackColor);
            f_set_up = false;

            if (capture != null)
            {
                lock (capture)
                {
                    capture.Release();
                    capture.Dispose();
                    capture = new VideoCapture((int)index);
                }
            }
            else
            {
                capture = new VideoCapture((int)index);
            }

            if (!capture.IsOpened())
            {
                Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), "カメラを起動");
                Utils.Alert_Error("カメラを開けませんでした");
                return;
            }


            SetResizeParams(pictureBox1.Width, pictureBox1.Height);


            f_set_up = true;
            f_start_up = false;
            Utils.WriteLine("確認プロセス完了");
            Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), "カメラを起動");
            Invoke(new Utils.InvokeInt(DisplayActiveCamera),(int)index);


        }
        void LoadDeviceList()
        {

        /*    string pre_id = null;
            if (comboBox1.Items.Count > 0)
            {
                pre_id = deviceID[comboBox1.SelectedIndex];
            }*/


            comboBox1.Items.Clear();

            GetAllConnectedCameras(out var cameralist,out deviceID);

            foreach (var i in cameralist)
                comboBox1.Items.Add(i);

            if (comboBox1.Items.Count >= 1 && comboBox1.SelectedIndex < 0)
                comboBox1.SelectedIndex = 0;


            if (!active_camera_id.Equals(""))
            {
                for (int i = 0; i < deviceID.Count; i++)
                    if (deviceID[i].Equals(active_camera_id))
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            insertWatcher.Dispose();
            removeWatcher.Dispose();
            if (capture != null)
            {
                capture.Release();
                capture.Dispose();
            }
        }


        void DisplayActiveCamera(int index)
        {
            label1.Text = "起動中:" + comboBox1.Items[index];
            label1.Visible = true;
            active_camera_id = deviceID[index];
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!f_set_up)
            {
                label1.Visible = false;
                return;
            }


            if (camera_frame != null)
            {
                camera_frame.Dispose();
                camera_frame = new Mat();
            }

            if (capture.Read(camera_frame))
            {
                f_infrared_mode = DetectColorORGray(camera_frame);

                if (f_control_active)
                {
                    mouse_tracker.Update(f_infrared_mode,camera_frame);
                }
                if (f_camera_visible)
                    using (Bitmap bitmap = BitmapConverter.ToBitmap(camera_frame))
                    using (var resize_bitmap = new Bitmap(bitmap, comform_picture_size[0], comform_picture_size[1]))
                        graphics.DrawImage(resize_bitmap, comform_picture_offset[0], comform_picture_offset[1], comform_picture_size[0], comform_picture_size[1]);
            }
            else
            {
                f_set_up = false;
                graphics.Clear(BackColor);
                capture.Release();
                capture.Dispose();
                capture = null;
                Utils.Alert_Error("カメラからの画像読み取りができませんでした");
            }
            

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(capture==null || !capture.IsOpened())
            {
                Utils.Alert_Note("カメラを起動してください");
                return;
            }

            var form3 = new Form3(this);
            form3.Show();
            insertWatcher.Stop();
            removeWatcher.Stop();
            this.Visible = false;
        }

        public void SetCameraOutPut(Graphics graphics)
        {
            this.graphics = graphics;
        }

        public void SetResizeParams(int out_width,int out_height)
        {
            int w = capture.FrameWidth;
            int h = capture.FrameHeight;
            Utils.ZoomFitSize(w, h, out_width,out_height, out int ox, out int oy, out int rw, out int rh, out double scale);
            comform_picture_offset[0] = ox;
            comform_picture_offset[1] = oy;
            comform_picture_size[0] = rw;
            comform_picture_size[1] = rh;
            this.scale = scale;
        }

        public bool IsCameraVisible { get { return f_camera_visible; }
            set
            {
                f_camera_visible = value;
            }
        }

        public bool IsInfraredMode { get { return f_infrared_mode; } }

        public Mat GetCameraFrame()
        {
            return camera_frame;
        }

        public void BeginControl()
        {
            f_control_active = true;
        }

        public void StopControl()
        {
            f_control_active = false;
        }

        public bool DetectColorORGray(Mat frame)
        {
            using (var resize = frame.Resize(new OpenCvSharp.Size(10, 10)))
            {
                var bgr = resize.Mean();
                if (Math.Abs(bgr[0] - bgr[1]) < 0.1 && Math.Abs(bgr[2] - bgr[1]) < 0.1)
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
}
