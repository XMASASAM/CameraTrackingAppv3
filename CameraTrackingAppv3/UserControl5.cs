using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    public partial class UserControl5 : UserControl
    {
        int max_value, min_value, div=1 , decimal_place=0 , default_value;

        [EditorBrowsable]
        [Description("設定する名前の値")]
        [Category("動作")]
        public string Prop_Name { get { return label1.Text; } set { label1.Text = value; } }

        [EditorBrowsable]
        [Description("値の最大値")]
        [Category("動作")]
        public int MaxValue { get { return trackBar1.Maximum; } set { trackBar1.Maximum = value; } }

        [EditorBrowsable]
        [Description("値の最小値")]
        [Category("動作")]
        public int MinValue { get { return trackBar1.Minimum; } set { trackBar1.Minimum = value; } }


        [EditorBrowsable]
        [Description("割って表示するので、その割る分母")]
        [Category("動作")]
        public int Divide{get { return trackBar1.TickFrequency; }set { trackBar1.TickFrequency = value; }}


        [EditorBrowsable]
        [Description("四捨五入する際の小数第何位")]
        [Category("動作")]
        public int Decimal_Place { get { return decimal_place; } set { decimal_place = value; } }


        [EditorBrowsable]
        [Description("最初の値")]
        [Category("動作")]
        public int DefaultValue { get { return trackBar1.Value; } set { trackBar1.Value = value; } }


        private void button2_Click(object sender, EventArgs e)
        {
            trackBar1.Value++;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            output = trackBar1.Value / (double)trackBar1.TickFrequency;
            label2.Text = Math.Round(output, decimal_place).ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            trackBar1.Value--;
        }

        double output;
        private void UserControl5_Load(object sender, EventArgs e)
        {
            output = trackBar1.Value / (double)trackBar1.TickFrequency;
            label2.Text = Math.Round(output, decimal_place).ToString();
            //  label1.Text = name;
            //  trackBar1.Minimum = min_value;
            //  trackBar1.Maximum = max_value;
            //  trackBar1.Value = default_value;
        }

        public UserControl5()
        {
            InitializeComponent();
            trackBar1.TickFrequency = 1;
            trackBar1.Maximum = 1000;//int.MaxValue;
        }

     /*   public UserControl5(string name ,int default_value , int max_value,int min_value , int div = 1,int decimal_place = 0)
        {
            InitializeComponent();
            //this.name = name;
            this.max_value = max_value;
            this.min_value = min_value;
            this.div = div;
            this.decimal_place = decimal_place;
            this.default_value = default_value;
            
        }*/
        public double GetOutPut()
        {
            return output;
        }

        public void SetValue(double value)
        {
            DefaultValue = (int)(value * Divide);
        }

    }
}
