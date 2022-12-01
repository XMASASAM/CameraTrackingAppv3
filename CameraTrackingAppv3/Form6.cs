﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    public partial class Form6 : Form
    {
        Form3 form3;
        Form1 form1;
        public Form6(Form3 form3,Form1 form1)
        {
            InitializeComponent();
            this.form3 = form3;
            this.form1 = form1;

            Utils.Temp_Config = new SettingsConfig(Utils.Config);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4(form3,form1);
            form4.Show();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            form1.FormStart(ref Utils.Temp_Config);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Utils.Config.Set(Utils.Temp_Config);

            Utils.Config.Save();

        }
    }
}