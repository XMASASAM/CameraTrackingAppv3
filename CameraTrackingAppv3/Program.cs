using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Utils.SavePath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + Application.ProductName+"Masao";//"\\CameraTrackingMouseMasao";
            Utils.UserName = Environment.UserName;

            Application.Run(new Form3());
        }
    }
}
