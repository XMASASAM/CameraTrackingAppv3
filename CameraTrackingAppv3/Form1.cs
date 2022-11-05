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
        delegate void ControlFormDelegate();
        string active_camera_id = "";
        public Form1()
        {
            InitializeComponent();
            //TODO:aaaaa
            LoadDeviceList();
            label1.Visible = false;
            graphics = pictureBox1.CreateGraphics();

            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var selected_index = listBox1.SelectedIndex;
            Utils.WriteLine(listBox1.SelectedIndex.ToString());
            if (selected_index < 0)
            {
                Utils.Alert_Note("起動できるカメラがありません");
                return;
            }

            if (f_start_up || active_camera_id.Equals(deviceID[listBox1.SelectedIndex]))
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

            if(capture != null)
            {
                capture.Release();
                capture.Dispose();
            }
            capture = new VideoCapture((int)index);

            if (!capture.IsOpened())
            {
                Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), "カメラを起動");
                Utils.Alert_Error("カメラを開けませんでした");
                return;
            }


            int w = capture.FrameWidth;
            int h = capture.FrameHeight;
            Utils.ZoomFitSize(w, h, pictureBox1.Width, pictureBox1.Height, out int ox, out int oy, out int rw, out int rh, out double scale);
            comform_picture_offset[0] = ox;
            comform_picture_offset[1] = oy;
            comform_picture_size[0] = rw;
            comform_picture_size[1] = rh;
            this.scale = scale;
            f_set_up = true;
            f_start_up = false;
            Utils.WriteLine("確認プロセス完了");
            Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), "カメラを起動");
            Invoke(new Utils.InvokeInt(DisplayActiveCamera),(int)index);


        }
        void LoadDeviceList()
        {

        /*    string pre_id = null;
            if (listBox1.Items.Count > 0)
            {
                pre_id = deviceID[listBox1.SelectedIndex];
            }*/


            listBox1.Items.Clear();

            GetAllConnectedCameras(out var cameralist,out deviceID);

            foreach (var i in cameralist)
                listBox1.Items.Add(i);

            if (listBox1.Items.Count >= 1 && listBox1.SelectedIndex < 0)
                listBox1.SelectedIndex = 0;


            if (!active_camera_id.Equals(""))
            {
                for (int i = 0; i < deviceID.Count; i++)
                    if (deviceID[i].Equals(active_camera_id))
                    {
                        listBox1.SelectedIndex = i;
                        break;
                    }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            insertWatcher.Dispose();
            removeWatcher.Dispose();
            capture.Release();
            capture.Dispose();
        }


        void DisplayActiveCamera(int index)
        {
            label1.Text = "起動中:" + listBox1.Items[index];
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

           
            using (Mat mat = new Mat())
            {
                if (capture.Read(mat))
                {
                    using (Bitmap bitmap = BitmapConverter.ToBitmap(mat))
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

        }
    }
}
