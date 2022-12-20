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
        bool visible;

        public Form5(bool visible)
        {
            InitializeComponent();
            ControlBox = false;
            this.visible = visible;

        }
        private 
            void Form5_Shown(object sender, EventArgs e)
        {
            Visible = visible;
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
        }

        private void Form5_MouseEnter(object sender, EventArgs e)
        {
            MouseControl.IsCursorOnForm = true;
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

        }
    }
}
