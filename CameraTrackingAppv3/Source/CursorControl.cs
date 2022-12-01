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

        static Vec2d location;
        static double speed;
        static Point mouse_vel;
        static Point pre_mouse_point;
        static bool f_cursor_stay;
        static System.Diagnostics.Stopwatch dwell_time;
        static bool f_range_of_motion = false;
        static Vec2d[] range_of_motion;
        static Vec2d cursor_magnification = new Vec2d(14,14);
        static Vec2d[] cursor_normal_axis = new Vec2d[2];
        static double velo_mag = 1;
        static double move_threshold = 0.5;
        static Vec2d pre_valid_point;

        static  double screen_motion_scale_x;
        static double screen_motion_scale_y;

        static  double screen_motion_scale_min;
        static  double screen_motion_scale_max;

        public static bool IsStay { get { return f_cursor_stay; } }

        public static bool IsDwell { get; private set; }
        public static bool IsDwellImpulse { get; private set; }

        public static Vec2d[] RangeOfMotionNormalAxis { get { return cursor_normal_axis; } }
        public static Vec2d RangeOfMotionCenterPoint { get; private set; }

        public static Vec2d[] RangeOfMotion { get { return range_of_motion; } }

        public static bool IsRangeOfMotion { get { return f_range_of_motion; } set { f_range_of_motion = value; } }

        public static void SettingMode()
        {
            move_threshold = 0.5;
            IsRangeOfMotion = false;
            MouseControl.IsControl = false;

        }

        static CursorControl()
        {
            dwell_time = new System.Diagnostics.Stopwatch();
        }


        static public void Init()
        {
            f_first = true;
            f_cursor_stay = false;
        }


        static public Vec2d cvtSensor2Delta(Vec2d dx)
        {
            dx = pre_dx * 0.4 + dx * 0.6;

            pre_dx = dx;

            var k = 0.8f * speed  + 4f;


            dx *= Math.Min(k,14) * velo_mag;

            if (Math.Abs(dx.Item0) < 1) dx.Item0 = 0;
            if (Math.Abs(dx.Item1) < 1) dx.Item1 = 0;

            return dx;
        }

        static public void MoveCursor(Vec2d sensor_center,Vec2d sensor_velocity)
        {

            speed = Utils.GetDistanceSquared(sensor_center, pre_valid_point);
            bool f_more_threshold = speed > move_threshold;
            if (speed > move_threshold)
            {
                pre_valid_point = sensor_center;
            }


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

                var range_point = new Vec2d(axisX * cursor_magnification.Item0 + Utils.AllScreenWidthHalf,
                    axisY * cursor_magnification.Item1 + Utils.AllScreenHeightHalf
                    );

                var range_dx = range_point - location;


                if (speed > move_threshold)
                {
                    var dx = cvtSensor2Delta(sensor_velocity);


                    //   var n_dx = Utils.NormalizVec2d(dx,out var distance);
                    Vec2d n_dx_s;
                    if (dx.Item0 == 0 && dx.Item1 == 0)
                        n_dx_s = new Vec2d(0,0);
                    else
                        n_dx_s = dx / Utils.GetDistanceSquared(dx.Item0,dx.Item1);

                //    var n_range_inner_dx = n_dx * Math.Max(0, range_dx.Item0 * n_dx.Item0 + range_dx.Item1 * n_dx.Item1);
                
                    //    var n_range_inner = Math.Max(0, range_dx.Item0 * n_dx.Item0 + range_dx.Item1 * n_dx.Item1);

                    var n_range_inner_s = Math.Max(0, range_dx.Item0 * n_dx_s.Item0 + range_dx.Item1 * n_dx_s.Item1);

                    var range_sp = speed / 100;

                    range_sp = Utils.Grap(0, range_sp - 0.1 / (velo_mag * velo_mag ), 1);


                  //  var k = Math.Max( n_range_inner * range_sp + distance * (1 - range_sp),1);
                    var k = Math.Max(n_range_inner_s * range_sp + (1 - range_sp),1);

                    //   var add_vel = n_range_inner_dx * range_sp + dx * (1 - range_sp);
                    //  var add_vel = k * n_dx;//n_range_inner_dx * range_sp + dx * (1 - range_sp);

                    // n_range_inner * range_sp * n_dx +  distance * (1 - range_sp) * n_dx;
                    //(n_range_inner * range_sp + distance * (1 - range_sp)) * n_dx;

                    //add_vel.Item0 = Math.Max(add_vel.Item0, dx.Item0);
                    //add_vel.Item1 = Math.Max(add_vel.Item1, dx.Item1);

                    location += dx * k;//add_vel;//n_range_inner_dx * range_sp + dx * (1 - range_sp);
                }


            }
            else
            {
                if (speed <= move_threshold)
                    return;

                var dx = cvtSensor2Delta(sensor_velocity);

                location.Item0 += dx.Item0;
                location.Item1 += dx.Item1;

            }

            location.Item0 = Utils.Grap(0, location.Item0, Utils.AllScreenWidth);
            location.Item1 = Utils.Grap(0, location.Item1, Utils.AllScreenHeight);
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
                pre_valid_point = center;
                f_first = false;
            }

            MoveCursor(center,vel);

            Point now_location = new Point((int)location.Item0, (int)location.Item1);
            mouse_vel = now_location - pre_mouse_point;//MouseControl.GetLocation - pre_mouse_point;

            pre_mouse_point = now_location;//MouseControl.GetLocation;

            bool f_pre_stay = f_cursor_stay;

            f_cursor_stay = (mouse_vel.X == 0 && mouse_vel.Y == 0);

            IsDwellImpulse = false;

            if (f_cursor_stay)
            {
                if(!f_pre_stay)
                    dwell_time.Start();

                if (dwell_time.ElapsedMilliseconds > 500)
                {
                    IsDwell = true;
                    IsDwellImpulse = true;

                    dwell_time.Reset();
                    //otosozai.com　様 se_saa06 を使用
                    var player = new System.Media.SoundPlayer(Properties.Resources.se_saa06);
                    player.Play();

                    //  Properties.Resources.se
                    //  MouseControl.Click(MouseState.LeftClick);
                }
            }
            else
            {
                IsDwell = false;
                if (f_pre_stay)
                    dwell_time.Reset();
            }


        }

        static public void SetRangeOfMotion(Vec2d[] range)
        {
            range_of_motion = range;

            RangeOfMotionCenterPoint = (range[0] + range[1] + range[2] + range[3]) * 0.25;


            var axis_x = (range[1] + range[2]) * 0.5 - (range[0] + range[3]) * 0.5;
            //var axis_y = (range[0] + range[1]) * 0.5 - (range[3] + range[2]) * 0.5;
            var axis_y = new Vec2d(axis_x.Item1,- axis_x.Item0);//(range[3] + range[2]) * 0.5 - (range[0] + range[1]) * 0.5;



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


            cursor_magnification.Item0 = Utils.AllScreenWidth /
            Math.Sqrt(Math.Min(Utils.GetDistanceSquared(range[0], range[1]), Utils.GetDistanceSquared(range[2], range[3])));

            cursor_magnification.Item1 = Utils.AllScreenHeight/
            Math.Sqrt(Math.Min(Utils.GetDistanceSquared(range[0], range[3]), Utils.GetDistanceSquared(range[1], range[2])));

            screen_motion_scale_x = cursor_magnification.Item0;
            screen_motion_scale_y = cursor_magnification.Item1;

            screen_motion_scale_max = Math.Max(cursor_magnification.Item0,cursor_magnification.Item1);
            screen_motion_scale_min = Math.Min(cursor_magnification.Item0,cursor_magnification.Item1);

            var k = screen_motion_scale_max;
           // cursor_magnification = new Vec2d(k, k);
            Utils.WriteLine("k:" + k.ToString());
            velo_mag = k / 20;
            move_threshold = 2 / (velo_mag * velo_mag);
           // k /= 20;
           // 1.5, 2, 2.5
           // SetIncreaseFactor(1.5*k, 2*k, 2.5*k);
        }

        public static void DisplayRangeOfMotion(ref Mat frame,Vec2d[] ps=null,Vec2d[] ax=null)
        {

           // Vec2d[] ax;//CursorControl.RangeOfMotionNormalAxis;
            Point cp;// = Utils.cvtVec2d2Point(CursorControl.RangeOfMotionCenterPoint);
            if (ps == null || ax==null)
            {
                ps = CursorControl.RangeOfMotion;
                ax = CursorControl.RangeOfMotionNormalAxis;
                cp = Utils.cvtVec2d2Point(CursorControl.RangeOfMotionCenterPoint);
            }
            else
            {
                cp = Utils.cvtVec2d2Point((ps[0] + ps[1] + ps[2] + ps[3]) * 0.25);
            }

            for (int i = 0; i < ps.Length; i++)
            {
                frame.Circle((int)ps[i].Item0, (int)ps[i].Item1, 4, Scalar.Yellow, 4);
            }

            

            var a_x = Utils.cvtVec2d2Point(ax[0] * 100);
            var a_y = Utils.cvtVec2d2Point(ax[1] * 100);
            

            frame.Line(cp - a_x, cp, Scalar.Yellow, 4);
            frame.Line(cp + a_x, cp, Scalar.Yellow, 4);

            frame.Line(cp - a_y, cp, Scalar.Yellow, 4);
            frame.Line(cp + a_y, cp, Scalar.Yellow, 4);

            frame.Circle(cp, 5, Scalar.Yellow, 4);

        }

    }
}
