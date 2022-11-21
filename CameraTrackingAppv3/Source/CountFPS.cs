
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    class CountFPS
    {
        Stopwatch stopwatch;
        int count = 0;
        int fps = 0;
        
        public CountFPS()
        {
            this.stopwatch = new Stopwatch();
            stopwatch.Start();
        }
        
        public CountFPS(Stopwatch stopwatch)
        {
            this.stopwatch = stopwatch;
        }


        public void Update()
        {
            if (stopwatch.ElapsedMilliseconds <= 1000)
            {
                count++;
            }
            else
            {
                fps = count;
                count = 0;
                stopwatch.Restart();
            }

        }

        public void SetTextFPS(Control control)
        {
            control.Text = Convert.ToString(fps) + "[FPS]";
        }

        public int Get { get { return fps; } }
    }
}
