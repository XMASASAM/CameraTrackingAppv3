using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    static class CursorControl
    {
        static bool f_first = true;
        static bool f_first_2 = true;
        static OpenCvSharp.Point pre_dx;
        static OpenCvSharp.Point pre_sensor_point;
        static public OpenCvSharp.Point cvtSensor2Delta(OpenCvSharp.Point dx)
        {
            if (f_first)
            {
                pre_dx = dx;
                f_first = false;
            }

            dx = pre_dx * 0.5 + dx * 0.5;

            pre_dx = dx;

            var speed = Utils.GetDistanceSquared(dx.X, dx.Y);
          /*  if(speed <= 1)
            {
                dx.X = 0;
                dx.Y = 0;
            }*/

            return dx;
        }

        static public void MoveCursor(OpenCvSharp.Point sensor_point)
        {
            if (f_first_2)
            {
                pre_sensor_point = sensor_point;
                f_first_2 = false;
            }

            var dx = cvtSensor2Delta(sensor_point - pre_sensor_point);

            pre_sensor_point = sensor_point;

            MouseControl.Move(dx.X*4, dx.Y*4);

        }



    }
}
