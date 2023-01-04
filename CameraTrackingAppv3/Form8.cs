using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    public partial class Form8 : Form
    {
        SettingsConfig config;
        Form6 form6;
        public Form8(Form6 form, SettingsConfig config)
        {
            InitializeComponent();
            this.config = config;
            form6 = form;
        }

        private void Form8_Load(object sender, EventArgs e)
        {
            textBox1.Text = config.Property.PortNumber.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            form6.SetPortNumber(Int32.Parse(textBox1.Text));
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
