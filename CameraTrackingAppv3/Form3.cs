using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    public partial class Form3 : Form
    {
        Form1 form1;
        int pre_form_height;
        public Form3(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;

        }
        private void Form3_Load(object sender, EventArgs e)
        {
            form1.SetCameraOutPut(pictureBox1.CreateGraphics());
            form1.SetResizeParams(pictureBox1.Width, pictureBox1.Height);
            pre_form_height = this.Size.Height;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (form1.IsCameraVisible)
            {
                this.Size = new Size(this.Size.Width,pre_form_height - pictureBox1.Height - 5);
                form1.IsCameraVisible = false;
                pictureBox1.Visible = false;
                button2.Text = "カメラ表示";
            }
            else
            {
                this.Size = new Size(this.Size.Width,pre_form_height);
                form1.IsCameraVisible = true;
                pictureBox1.Visible = true;
                button2.Text = "カメラ非表示";
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            form1.BeginControl();
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            var r = MessageBox.Show("このソフトウェアを終了します", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if(r == DialogResult.OK)
            {
                form1.Close();
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void DisplayFPS(int fps)
        {
            label1.Text = "FPS:" + fps.ToString();
        }

    }
}
