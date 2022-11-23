using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    static class CursorControl
    {
        static bool f_first = true;
      //  static bool f_control_mouse = true;
        static Vec2d pre_dx;
        static float[] IncreaseFactor = new float[30];
        static Vec2d location;
        static double speed;
        static Point mouse_vel;
        static Point pre_mouse_point;
        static bool f_cursor_stay;
        static System.Diagnostics.Stopwatch dwell_time;
        static bool f_range_of_motion = false;
        static Vec2d[] range_of_motion;
        static Vec2d cursor_magnification;
        static Vec2d[] cursor_normal_axis = new Vec2d[2];
        public static bool IsStay { get { return f_cursor_stay; } }
        public static bool IsStayImpulse { get; private set; }

        public static Vec2d[] RangeOfMotionNormalAxis { get { return cursor_normal_axis; } }
        public static Vec2d RangeOfMotionCenterPoint { get; private set; }

        public static Vec2d[] RangeOfMotion { get { return range_of_motion; } }

        public static bool IsRangeOfMotion { get { return f_range_of_motion; } set { f_range_of_motion = value; } }

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

        static public void Init()
        {
            f_first = true;
            f_cursor_stay = false;
            //  f_first_2 = true;
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

        static public void MoveCursor(Vec2d sensor_center,Vec2d sensor_velocity)
        {
            if (f_range_of_motion)
            {
                var Xx = cursor_normal_axis[0].Item0;
                var Xy = cursor_normal_axis[0].Item1;
                var Yx = cursor_normal_axis[1].Item0; 
                var Yy = cursor_normal_axis[1].Item1;

                var sx = sensor_center.Item0 - RangeOfMotionCenterPoint.Item0;
                var sy = sensor_center.Item1 - RangeOfMotionCenterPoint.Item1;

                var lowb = Yy * Xx - Yx * Xy;
                if (lowb == .0)
                    lowb = .0001;
                var lowa = Xx + Xy;
                if (lowa == .0)
                    lowa = .0001;

                var upb = Xx * sy - sx * Xy;

                var axisY = upb / lowb;

                var upa = sx + sy - axisY * (Yx + Yy);

                var axisX = upa / lowa;

                location.Item0 = axisX * cursor_magnification.Item0 + Utils.ScreenWidthHalf;
                location.Item1 = axisY * cursor_magnification.Item1 + Utils.ScreenHeightHalf;
                
            }
            else
            {
                var dx = cvtSensor2Delta(sensor_velocity);

                location.Item0 += dx.Item0;
                location.Item1 += dx.Item1;

            }

            location.Item0 = Utils.Grap(0, location.Item0, Utils.ScreenWidth);
            location.Item1 = Utils.Grap(0, location.Item1, Utils.ScreenHeight);
            MouseControl.SetLocation((int)location.Item0, (int)location.Item1);
        }



        static public void Update(bool error,Vec2d center, Vec2d vel)
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

            MoveCursor(center,vel);

            Point now_location = new Point((int)location.Item0, (int)location.Item1);
            mouse_vel = now_location - pre_mouse_point;//MouseControl.GetLocation - pre_mouse_point;

            pre_mouse_point = now_location;//MouseControl.GetLocation;

            bool f_pre_stay = f_cursor_stay;

            f_cursor_stay = (mouse_vel.X == 0 && mouse_vel.Y == 0);

            IsStayImpulse = false;

            if (f_cursor_stay)
            {
                if(!f_pre_stay)
                    dwell_time.Start();

                if (dwell_time.ElapsedMilliseconds > 500)
                {
                    IsStayImpulse = true;
                    dwell_time.Reset();

                    MouseControl.Click(MouseState.LeftClick);
                }
            }
            else
            {
                if (f_pre_stay)
                    dwell_time.Reset();
            }


        }

        static public void SetRangeOfMotion(Vec2d[] range)
        {
            range_of_motion = range;

            RangeOfMotionCenterPoint = (range[0] + range[1] + range[2] + range[3]) * 0.25;


            var axis_x = (range[1] + range[2]) * 0.5 - (range[0] + range[3]) * 0.5;
            var axis_y = (range[0] + range[1]) * 0.5 - (range[3] + range[2]) * 0.5;

            var dis_x = Math.Sqrt(Utils.GetDistanceSquared(axis_x.Item0, axis_x.Item1));
            var dis_y = Math.Sqrt(Utils.GetDistanceSquared(axis_y.Item0, axis_y.Item1));

            if (dis_x == 0) dis_x += 0.0001;
            if (dis_y == 0) dis_y += 0.0001;

            cursor_normal_axis[0] = axis_x / dis_x;
            cursor_normal_axis[1] = axis_y / dis_y;

            if (cursor_normal_axis[0].Item0 == 0) cursor_normal_axis[0].Item0 += 0.0001;
            if (cursor_normal_axis[0].Item1 == 0) cursor_normal_axis[0].Item1 += 0.0001;
            if (cursor_normal_axis[1].Item0 == 0) cursor_normal_axis[1].Item0 += 0.0001;
            if (cursor_normal_axis[1].Item1 == 0) cursor_normal_axis[1].Item1 += 0.0001;


            cursor_magnification.Item0 = Utils.ScreenWidth / 
              Math.Sqrt( Math.Min(Utils.GetDistanceSquared(range[0], range[1]), Utils.GetDistanceSquared(range[2], range[3])));

            cursor_magnification.Item1 = Utils.ScreenHeight /
              Math.Sqrt( Math.Min(Utils.GetDistanceSquared(range[0], range[3]), Utils.GetDistanceSquared(range[1], range[2])));



        }

    }
}
