using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    public partial class Form2 : Form
    {
        string title;
        Point center_point;
        public Form2(string title,Point point)
        {
            this.title = title;
            center_point = point;
            InitializeComponent();
            ControlBox = false;
            
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text = title;
            this.Location = new Point(
                center_point.X - (Size.Width >> 1),
                center_point.Y - (Size.Height >> 1));//center_point + new Point(Size.Width>>1 , Size.Height>>1);
        }
    }
}
