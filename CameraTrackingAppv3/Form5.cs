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


        public Form5()
        {
            InitializeComponent();
            ControlBox = false;

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
            current_state = MouseState.DoubleClick;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            current_state = MouseState.Drag;
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
    }
}
