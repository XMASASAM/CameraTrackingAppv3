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
        string message;
        Point point;
        Image image;
        bool bottom = false;
        public Form2(string title,string message,Image image,Point point)
        {
            this.title = title;
            this.message = message;
            this.image = image;
            this.point = point;
            InitializeComponent();
            ControlBox = false;
            
        }

        public Form2(string title, string message, Image image, Point point,bool bottom)
        {
            this.title = title;
            this.message = message;
            this.image = image;
            this.point = point;
            InitializeComponent();
            ControlBox = false;
            this.bottom = bottom;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text = title;
            
            label1.Text = message;
            pictureBox1.Image = image;

            if (bottom)
            {
                this.Location = new Point(
                    point.X - (Size.Width>>1),
                    point.Y - (Size.Height));
            }
            else
            {
                this.Location = new Point(
                    point.X - (Size.Width >> 1),
                    point.Y - (Size.Height >> 1));//center_point + new Point(Size.Width>>1 , Size.Height>>1);
            }

            
        }
    }
}
