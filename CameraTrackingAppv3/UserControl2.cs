using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    public partial class UserControl2 : UserControl
    {
        //   List<RecodeUser> users;
        int hashcode = 0;
        public UserControl2()
        {
            InitializeComponent();
            listView1.Items.Clear();

            var a = new string[] { "なし","なし","なし","なし" };
            listView1.Items.Add(new ListViewItem(a));
            
        }

        public void SetRecodes(List<RecodeUser> users)
        {
            var t_hash = users.GetHashCode();
            if (hashcode == t_hash) return;

            hashcode = t_hash;
            listView1.Items.Clear();

            var my_ip = Utils.GetIPv4Address();

            if (users.Count == 0)
            {
                var a = new string[] { "なし", "なし", "なし", "なし" };
                listView1.Items.Add(new ListViewItem(a));
                return;
            }


            foreach(var i in users)
            {
                var a = new string[] {i.UserName,i.MachineName,i.IPAddress,i.MACAddress};

                if (my_ip.Contains(i.IPAddress))
                {
                    a[0] = "(自分)" + a[0];
                }

                listView1.Items.Add(new ListViewItem(a));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(Main.GetConnect()!=null)
            SetRecodes(Main.GetConnect().GetRecodeUsers());
        }

        private void UserControl2_Load(object sender, EventArgs e)
        {

        }
    }
}
