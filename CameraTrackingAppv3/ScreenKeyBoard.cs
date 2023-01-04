using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
namespace CameraTrackingAppv3
{
    static class ScreenKeyBoard
    {
        const string title = "キーボード";
        static bool f_active = false;
        public static void Switch(bool on)
        {
       //     if (f_active) return;
            Thread thread;
            if (on)
                thread = new Thread(new ThreadStart(On));
            else
                thread = new Thread(new ThreadStart(Off));
            thread.Start();
        }

        public static void On()
        {
        //    f_active = true;
          //  Utils.MainForm.Invoke(new Utils.InvokeLoadAlert(Utils.ShowLoadAlert),title, "起動中...", Properties.Resources.icon_loader_c_ww_01_s1, MouseControl.GetLocation, false);
            bool f = RunCommand("/c osk.exe", out _, out var error);
         //   Utils.MainForm.Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), title);
            if (!f)
                Utils.Alert_Error(error);
      //      f_active = false;

        }
        public static void Off()
        {
         //   f_active = true;
        //    Utils.MainForm.Invoke(new Utils.InvokeLoadAlert(Utils.ShowLoadAlert), title, "停止中...", Properties.Resources.icon_loader_c_ww_01_s1, MouseControl.GetLocation, false);
            bool f = RunCommand("/c wmic process where \"name = 'osk.exe'\" delete", out _, out var error);
         //   Utils.MainForm.Invoke(new Utils.InvokeString(Utils.CloseLoadAlert), title);
            if (!f)
                Utils.Alert_Error(error);
        //    f_active = false;
        }

        public static bool DetectRunOSK()
        {
            if (RunCommand("/c tasklist | find /c \"osk.exe\"", out var output, out var error))
            {
                return !(Int32.Parse(output) == 0);
            }
            else
            {
                Utils.Alert_Error(error);
                return false;
            }
        }

        static bool RunCommand(string command, out string output, out string error,bool wait = true)
        {
            //   var command = "/c tasklist | find /c \"osk.exe\"";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", command);
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;

            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            // コマンド実行
            Process process = Process.Start(processStartInfo);


            // コマンド終了の待ち合わせ
            int ans = 0;
            output = "";
            error = "";
            if (wait)
            {
                process.WaitForExit();
                ans = process.ExitCode;
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();
            }

            process.Close();
            return ans == 0 || error.Equals("");
        }
    }
}
