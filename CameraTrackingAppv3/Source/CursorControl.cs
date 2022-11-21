using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    static class CursorControl
    {
        static bool f_first = true;
      //  static bool f_first_2 = true;
        static Vec2d pre_dx;
     //   static Vec2d pre_sensor_point;
        static float[] IncreaseFactor = new float[30];
        static Vec2d location;
        static double speed;
        static Point mouse_vel;
        static Point pre_mouse_point;
        static bool f_stop;
        static System.Diagnostics.Stopwatch dwell_time;
        static CursorControl()
        {
            float factor1 = 1.5f;
            float factor2 = 2f;
            float factor3 = 2.5f;
            for (int i = 0; i < IncreaseFactor.Length; i++)
                IncreaseFactor[i] = 1;

            for (int i = 5; i < IncreaseFactor.Length; i++)
                IncreaseFactor[i] *= factor1;

            for (int i = 10; i < IncreaseFactor.Length; i++)
                IncreaseFactor[i] *= factor2;

            for (int i = 13; i < IncreaseFactor.Length; i++)
                IncreaseFactor[i] *= factor3;

            dwell_time = new System.Diagnostics.Stopwatch();
        }

        static public Vec2d cvtSensor2Delta(Vec2d dx)
        {
            //dx.Item0 = (int)(pre_dx.X * 0.5 + dx.X * 0.5);
            //dx.Item1 = (int)(pre_dx.Y * 0.5 + dx.Y * 0.5);
            dx = pre_dx * 0.4 + dx * 0.6;

            pre_dx = dx;


            speed = Utils.GetDistanceSquared(dx.Item0, dx.Item1);

            var k = Math.Min(0.8f * speed + 4f,14);

            dx *= k;

           // Utils.WriteLine("speed:" + k.ToString());
            if(speed <= 0.5)
            {
                dx.Item0 = 0;
                dx.Item1 = 0;
            }

            return dx;
        }

        static public void MoveCursor(Vec2d sensor_velocity)
        {
            var dx = cvtSensor2Delta(sensor_velocity);

      //      pre_sensor_point = sensor_point;

            location.Item0 += dx.Item0;
            location.Item1 += dx.Item1;
            location.Item0 = Utils.Grap(0,location.Item0,Utils.ScreenWidth);
            location.Item1 = Utils.Grap(0,location.Item1,Utils.ScreenHeight);
            //Math.Min(Math.Max(location.Item0,),);
            //MouseControl.Move((int)dx.Item0,- (int)dx.Item1);
            MouseControl.SetLocation((int)location.Item0, (int)location.Item1);
        }

        static public void Init()
        {
            f_first = true;
            f_stop = false;
          //  f_first_2 = true;
        }

        static public void Update(bool error, Vec2d vel)
        {
            if (error) return;

            if (f_first)
            {
                pre_dx = vel;
                location.Item0 = MouseControl.GetLocation.X;
                location.Item1 = MouseControl.GetLocation.Y;
                pre_mouse_point = MouseControl.GetLocation;
                f_first = false;
            }

            MoveCursor(vel);


            mouse_vel = MouseControl.GetLocation - pre_mouse_point;

            pre_mouse_point = MouseControl.GetLocation;

            bool f_pre_stop = f_stop;

            f_stop = (mouse_vel.X == 0 && mouse_vel.Y == 0);


            if (f_stop)
            {
                if(!f_pre_stop)
                    dwell_time.Start();

                if (dwell_time.ElapsedMilliseconds > 500)
                {
                    dwell_time.Reset();
                    MouseControl.Click(MouseState.LeftClick);
                }
            }
            else
            {
                if (f_pre_stop)
                    dwell_time.Reset();
            }


        }

    }
}
