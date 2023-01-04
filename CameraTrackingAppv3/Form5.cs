using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
namespace CameraTrackingAppv3
{
    public partial class Form5 : Form
    {
        MouseState current_state = MouseState.LeftClick;
      //  bool visible;

        public Form5(bool visible)
        {
            InitializeComponent();
            ControlBox = false;
           // this.visible = visible;

        }
        private 
            void Form5_Shown(object sender, EventArgs e)
        {
            //Visible = visible;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            current_state = MouseState.LeftClick;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            current_state = MouseState.RightClick;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            current_state = MouseState.LeftDoubleClick;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            current_state = MouseState.LeftDrag;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            current_state = MouseState.ScrollUp;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            current_state = MouseState.ScrollDown;
        }

        new public void Update()
        {
            //label1.Text = "OnMouse:" + MouseControl.IsCursorOnForm.ToString();

            if (CursorControl.IsDwellImpulse)
            {
                    MouseControl.Click(current_state);

            }

            
        }

        private void Form5_MouseLeave(object sender, EventArgs e)
        {
            MouseControl.IsCursorOnForm = false;
          //  Utils.WriteLine("離れましたましたよ!!!!!!");

        }

        private void Form5_MouseEnter(object sender, EventArgs e)
        {
            MouseControl.IsCursorOnForm = true;
          //  Utils.WriteLine("Enter来ましたよ!!!!!!");
        }


        private void button7_Click(object sender, EventArgs e)
        {
            var users = Main.GetConnect().GetRecodeUsers();
            if (users.Count == 2)
            {
                var macs = Utils.GetAllPhysicalAddress();
                var index = 0;
                if (macs.Contains(users[index].MACAddress))
                    index = 1;


                if (Main.GetConnect().SendActiveSignal(index))
                {
                    Utils.MainForm.WaitCursor(true);
                }

            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            current_state = MouseState.RightDoubleClick;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Utils.MainForm.WindowState = FormWindowState.Normal;
            Utils.MainForm.Focus();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            current_state = MouseState.RightDrag;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            current_state = MouseState.MiddleClick;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            current_state = MouseState.MiddleDoubleClick;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            current_state = MouseState.MiddleDrag;
        }
        
        private void button14_Click(object sender, EventArgs e)
        {
            ScreenKeyBoard.Switch(!ScreenKeyBoard.DetectRunOSK());
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Location = new Point(Location.X, 0);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Location = new Point(Location.X, Utils.PrimaryScreenHeight - Size.Height);
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            Utils.WriteLine("MouseClick_button");
        }

        private void button1_MouseCaptureChanged(object sender, EventArgs e)
        {
            Utils.WriteLine("MouseClick_capture_change");

        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            Utils.WriteLine("MouseClick_MOuseUp");

        }

        private void button1_DragEnter(object sender, DragEventArgs e)
        {
            Utils.WriteLine("MouseClick_DragEnter");

            MouseControl.IsCursorOnForm = true;
        }

        private void button1_DragLeave(object sender, EventArgs e)
        {
            Utils.WriteLine("MouseClick_DragLeave");

            MouseControl.IsCursorOnForm = false;

        }

        private void Form5_DragEnter(object sender, DragEventArgs e)
        {
            Utils.WriteLine("DragEnter!!!");
        }

        private void Form5_DragOver(object sender, DragEventArgs e)
        {
            Utils.WriteLine("DragOver!!!");

        }

        private void button1_DragOver(object sender, DragEventArgs e)
        {

        }
    }
}
