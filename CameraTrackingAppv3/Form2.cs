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
        Point start_point;
        public Form2(string title,Point point)
        {
            this.title = title;
            start_point = point;
            InitializeComponent();
            ControlBox = false;
            
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text = title;
            this.Location = start_point;
        }
    }
}
