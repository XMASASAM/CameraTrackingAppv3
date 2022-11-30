using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
namespace CameraTrackingAppv3
{
    class DisplayInformation
    {
        public DisplayInformation(Form form)
        {
            try
            {
                GetPrimaryDisplayInformation();
                System.Diagnostics.Debug.WriteLine("");
                GetFormDisplayInformation(form);
                System.Diagnostics.Debug.WriteLine("");
                GetAllDisplayInformation();
            }
            catch
            {
            }

        }

        /// <summary>
        /// プライマリディスプレイの情報取得
        /// </summary>
        private void GetPrimaryDisplayInformation()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("●プライマリディスプレイの情報取得");
                System.Diagnostics.Debug.WriteLine("デバイス名 : " + System.Windows.Forms.Screen.PrimaryScreen.DeviceName);
                System.Diagnostics.Debug.WriteLine("ディスプレイの位置 : X=" + System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + " - Y=" + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y);
                System.Diagnostics.Debug.WriteLine("ディスプレイのサイズ : 幅=" + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width + " - 高さ=" + System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
                System.Diagnostics.Debug.WriteLine("ディスプレイの作業領域の位置 : X" + System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.X + " - Y=" + System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Y);
                System.Diagnostics.Debug.WriteLine("ディスプレイの作業領域のサイズ : 幅" + System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width + " - 高さ=" + System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height);
            }
            catch
            {
            }
        }

        /// <summary>
        /// フォームがあるディスプレイの情報取得
        /// </summary>
        private void GetFormDisplayInformation(Form form)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("●フォームがあるディスプレイの情報取得");
                System.Diagnostics.Debug.WriteLine("デバイス名 : " + System.Windows.Forms.Screen.FromControl(form).DeviceName);
                System.Diagnostics.Debug.WriteLine("ディスプレイの位置 : X=" + System.Windows.Forms.Screen.FromControl(form).Bounds.X + " - Y=" + System.Windows.Forms.Screen.FromControl(form).Bounds.Y);
                System.Diagnostics.Debug.WriteLine("ディスプレイのサイズ : 幅=" + System.Windows.Forms.Screen.FromControl(form).Bounds.Width + " - 高さ=" + System.Windows.Forms.Screen.FromControl(form).Bounds.Height);
                System.Diagnostics.Debug.WriteLine("ディスプレイの作業領域の位置 : X" + System.Windows.Forms.Screen.FromControl(form).WorkingArea.X + " - Y=" + System.Windows.Forms.Screen.FromControl(form).WorkingArea.Y);
                System.Diagnostics.Debug.WriteLine("ディスプレイの作業領域のサイズ : 幅" + System.Windows.Forms.Screen.FromControl(form).WorkingArea.Width + " - 高さ=" + System.Windows.Forms.Screen.FromControl(form).WorkingArea.Height);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 全てのディスプレイの情報取得
        /// </summary>
        private void GetAllDisplayInformation()
        {
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;
            try
            {
                System.Diagnostics.Debug.WriteLine("●全てのディスプレイの情報取得");
                foreach (System.Windows.Forms.Screen screen_data in System.Windows.Forms.Screen.AllScreens)
                {
                    System.Diagnostics.Debug.WriteLine("デバイス名 : " + screen_data.DeviceName);
                    System.Diagnostics.Debug.WriteLine("ディスプレイの位置 : X=" + screen_data.Bounds.X + " - Y=" + screen_data.Bounds.Y);
                    System.Diagnostics.Debug.WriteLine("ディスプレイのサイズ : 幅=" + screen_data.Bounds.Width + " - 高さ=" + screen_data.Bounds.Height);
                    System.Diagnostics.Debug.WriteLine("ディスプレイの作業領域の位置 : X" + screen_data.WorkingArea.X + " - Y=" + screen_data.WorkingArea.Y);
                    System.Diagnostics.Debug.WriteLine("ディスプレイの作業領域のサイズ : 幅" + screen_data.WorkingArea.Width + " - 高さ=" + screen_data.WorkingArea.Height);
                    System.Diagnostics.Debug.WriteLine("-----");
                    top = Math.Min(screen_data.Bounds.Top,top);
                    bottom = Math.Max(screen_data.Bounds.Bottom,bottom);
                    left = Math.Min(screen_data.Bounds.Left, left);
                    right = Math.Max(screen_data.Bounds.Right, right);
                }
                System.Diagnostics.Debug.WriteLine("top:" + top + " bottom:" + bottom + " left:" + left + " right:" + right);
            }
            catch
            {
            }
        }
    }
}
