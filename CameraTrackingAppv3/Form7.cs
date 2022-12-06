using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    public partial class Form7 : Form
    {

        public bool IsResume { get; private set; }
        Point start_point;
        public Form7(Point start_point)
        {
            InitializeComponent();
            this.start_point = start_point;
        }

        public void SecondRemining(int second)
        {
            label1.Text = "残り" + second.ToString() + "秒";
        }


        private void button1_Click(object sender, EventArgs e)
        {
            IsResume = true;
        }

        private void Form7_MouseLeave(object sender, EventArgs e)
        {
            MouseControl.IsCursorOnForm = false;
        }

        private void Form7_MouseEnter(object sender, EventArgs e)
        {
            Utils.WriteLine("enter!!343434");
            MouseControl.IsCursorOnForm = true;
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            Location = start_point;
        }
    }
}
