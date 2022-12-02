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


        static Vec2d cursor_magnification = new Vec2d(14,14);


        static double velo_mag = 1;
        static double move_threshold = 0.5;
        static Vec2d pre_valid_point;

        static  double screen_motion_scale_x;
        static double screen_motion_scale_y;

        static  double screen_motion_scale_min;
        static  double screen_motion_scale_max;

        static RangeOfMotionProps range_of_motion;

        public static bool IsStay { get { return f_cursor_stay; } }

        public static bool IsDwell { get; private set; }
        public static bool IsDwellImpulse { get; private set; }



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

        static public void Init(SettingsConfig config)
        {
            Init();

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
                
                var Xx = range_of_motion.NormalAxi[0].Item0;
                var Xy = range_of_motion.NormalAxi[0].Item1;
                var Yx = range_of_motion.NormalAxi[1].Item0;
                var Yy = range_of_motion.NormalAxi[1].Item1;

                var sx = sensor_center.Item0 - range_of_motion.CenterPoint.Item0;
                var sy = sensor_center.Item1 - range_of_motion.CenterPoint.Item1;

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


                }
            }
            else
            {
                IsDwell = false;
                if (f_pre_stay)
                    dwell_time.Reset();
            }


        }

        static public void SetRangeOfMotion(RangeOfMotionProps prop)
        {

            range_of_motion = prop;


            var range = range_of_motion.Points;

            cursor_magnification.Item0 = Utils.AllScreenWidth /
            Math.Sqrt(Math.Min(Utils.GetDistanceSquared(range[0], range[1]), Utils.GetDistanceSquared(range[2], range[3])));

            cursor_magnification.Item1 = Utils.AllScreenHeight/
            Math.Sqrt(Math.Min(Utils.GetDistanceSquared(range[0], range[3]), Utils.GetDistanceSquared(range[1], range[2])));

            screen_motion_scale_x = cursor_magnification.Item0;
            screen_motion_scale_y = cursor_magnification.Item1;

            screen_motion_scale_max = Math.Max(cursor_magnification.Item0,cursor_magnification.Item1);
            screen_motion_scale_min = Math.Min(cursor_magnification.Item0,cursor_magnification.Item1);

            var k = screen_motion_scale_max;


            Utils.WriteLine("k:" + k.ToString());
            velo_mag = k / 20;
            move_threshold = 2 / (velo_mag * velo_mag);


        }

        static public Vec2d[] GetCursorNormalAxis(Vec2d[] range)
        {
            Vec2d[] ans = new Vec2d[2];
            var axis_x = (range[1] + range[2]) * 0.5 - (range[0] + range[3]) * 0.5;
            var axis_y = new Vec2d(axis_x.Item1, -axis_x.Item0);
            var dis_x = Math.Sqrt(Utils.GetDistanceSquared(axis_x.Item0, axis_x.Item1));
            var dis_y = Math.Sqrt(Utils.GetDistanceSquared(axis_y.Item0, axis_y.Item1));

            if (dis_x == 0) dis_x += 0.0001;
            if (dis_y == 0) dis_y += 0.0001;

            ans[0] = axis_x / dis_x;
            ans[1] = axis_y / dis_y;

            if (ans[0].Item0 == 0) ans[0].Item0 += 0.0001;
            if (ans[0].Item1 == 0) ans[0].Item1 += 0.0001;
            if (ans[1].Item0 == 0) ans[1].Item0 += 0.0001;
            if (ans[1].Item1 == 0) ans[1].Item1 += 0.0001;

            return ans;
        }

        public static void DisplayRangeOfMotion(ref Mat frame,RangeOfMotionProps prop)//Vec2d[] ps=null,Vec2d[] ax=null)
        {

            var ps = prop.Points;
            for (int i = 0; i < ps.Length; i++)
            {
                frame.Circle((int)ps[i].Item0, (int)ps[i].Item1, 4, Scalar.Yellow, 4);
            }

            var ax = prop.NormalAxi;

            var a_x = Utils.cvtVec2d2Point(ax[0] * 100);
            var a_y = Utils.cvtVec2d2Point(ax[1] * 100);


            var cp = Utils.cvtVec2d2Point(prop.CenterPoint);

            frame.Line(cp - a_x, cp, Scalar.Yellow, 4);
            frame.Line(cp + a_x, cp, Scalar.Yellow, 4);

            frame.Line(cp - a_y, cp, Scalar.Yellow, 4);
            frame.Line(cp + a_y, cp, Scalar.Yellow, 4);

            frame.Circle(cp, 5, Scalar.Yellow, 4);

        }

    }
}
