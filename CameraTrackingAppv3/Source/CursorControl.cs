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
        static double[] IncreaseFactor = new double[31];
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
            SetIncreaseFactor(1.5, 2, 2.5);
            dwell_time = new System.Diagnostics.Stopwatch();
        }

        static void SetIncreaseFactor(double factor1,double factor2,double factor3)
        {
            for (int i = 0; i < IncreaseFactor.Length; i++)
                IncreaseFactor[i] = 1;

            for (int i = 3; i < IncreaseFactor.Length; i++)
                IncreaseFactor[i] *= factor1;

            for (int i = 10; i < IncreaseFactor.Length; i++)
                IncreaseFactor[i] *= factor2;

            for (int i = 13; i < IncreaseFactor.Length; i++)
                IncreaseFactor[i] *= factor3;
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



            var k = 0.8f * speed  + 4f;
      //      var k_x = Math.Min(k,cursor_magnification.Item0);
      //      var k_y = Math.Min(k,cursor_magnification.Item1);

      //      var ik = Math.Min((int)speed, 30);
         //   dx.Item0 *= IncreaseFactor[ik];//k_x;
        //    dx *= IncreaseFactor[ik];//k_x;
          //  dx.Item1 *= //k_y;
            dx *= Math.Min(k,14) * velo_mag;

           // Utils.WriteLine("speed:" + k.ToString());
           /* if(speed <= 0.5)
            {
                dx.Item0 = 0;
                dx.Item1 = 0;
            }*/

            return dx;
        }

        static public void MoveCursor(Vec2d sensor_center,Vec2d sensor_velocity)
        {

            speed = Utils.GetDistanceSquared(sensor_center, pre_valid_point);//
           // speed = Utils.GetDistanceSquared(sensor_velocity.Item0, sensor_velocity.Item1);
            // speed = Math.Sqrt(speed);
           // Utils.WriteLine("valied:" + Utils.GetDistanceSquared(sensor_center, pre_valid_point).ToString());
          //  Utils.WriteLine("velici:" + speed.ToString());


          //  Utils.WriteLine("speed:" + speed.ToString());
            if (f_range_of_motion)// && speed >=1)
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

                var range_point = new Vec2d(axisX * cursor_magnification.Item0 + Utils.ScreenWidthHalf,
                    axisY * cursor_magnification.Item1 + Utils.ScreenHeightHalf
                    );

                var range_dx = range_point - location;
                //   location.Item0 = axisX * cursor_magnification.Item0 + Utils.ScreenWidthHalf;
                //   location.Item1 = axisY * cursor_magnification.Item1 + Utils.ScreenHeightHalf;
                if (speed > move_threshold)
                {
                    pre_valid_point = sensor_center;
                    var dx = cvtSensor2Delta(sensor_velocity);

                 //   location.Item0 += dx.Item0;
                //    location.Item1 += dx.Item1;

                    var n_range_dx = Utils.NormalizVec2d(range_dx);
                    var n_dx = Utils.NormalizVec2d(dx);

                    var n_range_inner_dx = n_dx * Math.Max(0,range_dx.Item0 * n_dx.Item0 + range_dx.Item1 * n_dx.Item1);

                    //var cos = Math.Max(0,n_range_dx.Item0 * n_dx.Item0 + n_range_dx.Item1 * n_dx.Item1-0.5);


                    var range_sp = Math.Sqrt(speed) / 10;
                    //Utils.WriteLine("sp:" + sp.ToString());
                    //range_sp = Utils.Grap(0, range_sp - 0.1 / velo_mag, 1);// * cos; //Math.Min(sp, 1);
                    range_sp = Utils.Grap(0, range_sp - 0.1/velo_mag, 1);// * cos; //Math.Min(sp, 1);
                                                          //   location = range_point * sp + location * (1 - sp);
                   // range_sp = 0.2;
                 //   location = range_point * range_sp + location * (1 - range_sp);

                    location += n_range_inner_dx * range_sp + dx * (1 - range_sp);
                }


            //    location.Item0 =  * range_point.Item0 + 0.5 * location.Item0;
             //   location.Item1 = 0.5 * range_point.Item1 + 0.5 * location.Item1;

            }
            else
            {
                if (speed <= move_threshold)
                    return;

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


            cursor_magnification.Item0 = Utils.ScreenWidth / 
              Math.Sqrt( Math.Min(Utils.GetDistanceSquared(range[0], range[1]), Utils.GetDistanceSquared(range[2], range[3])));

            cursor_magnification.Item1 = Utils.ScreenHeight /
              Math.Sqrt( Math.Min(Utils.GetDistanceSquared(range[0], range[3]), Utils.GetDistanceSquared(range[1], range[2])));

            var k = Math.Max(cursor_magnification.Item0,cursor_magnification.Item1);
           // cursor_magnification = new Vec2d(k, k);
            Utils.WriteLine("k:" + k.ToString());
            velo_mag = k / 15;
            move_threshold = 0.4 / (velo_mag * velo_mag);
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
