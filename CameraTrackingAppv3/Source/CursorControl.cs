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
        static Vec2d pre_dx;
        static Vec2d pre_sensor_point;
        static float[] IncreaseFactor = new float[30];
        static Vec2d location;
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

        }

        static public Vec2d cvtSensor2Delta(Vec2d dx)
        {
            if (f_first)
            {
                pre_dx = dx;
                f_first = false;
            }
            
            //dx.Item0 = (int)(pre_dx.X * 0.5 + dx.X * 0.5);
            //dx.Item1 = (int)(pre_dx.Y * 0.5 + dx.Y * 0.5);
            dx = pre_dx * 0.4 + dx * 0.6;

            pre_dx = dx;


            var speed = Utils.GetDistanceSquared(dx.Item0, dx.Item1);

            var k = Math.Min(0.8f * speed + 4f,14);

            dx *= k;

            Utils.WriteLine("speed:" + k.ToString());
            if(speed <= 0.5)
            {
                dx.Item0 = 0;
                dx.Item1 = 0;
            }

            return dx;
        }

        static public void MoveCursor(Vec2d sensor_point)
        {
            if (f_first_2)
            {
                pre_sensor_point = sensor_point;
                f_first_2 = false;
                location.Item0 = MouseControl.GetLocation.X;
                location.Item1 = MouseControl.GetLocation.Y;
            }

            var dx = cvtSensor2Delta(sensor_point - pre_sensor_point);

            pre_sensor_point = sensor_point;

            location += dx;

            //MouseControl.Move((int)dx.Item0,- (int)dx.Item1);
            MouseControl.SetLocation((int)location.Item0, -(int)location.Item1);
        }



    }
}
