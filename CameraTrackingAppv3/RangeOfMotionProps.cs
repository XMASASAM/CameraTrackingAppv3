using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    [Serializable()]
    public struct RangeOfMotionProps
    {
        public Vec2d[] NormalAxi;
        public Vec2d[] Points;

        public Vec2d CenterPoint;

        public RangeOfMotionProps(Vec2d[] ps)
        {
            Points = ps;
            CenterPoint = (ps[0] + ps[1] + ps[2] + ps[3]) * 0.25;
            NormalAxi = CursorControl.GetCursorNormalAxis(ps);
        }



        public void Rotation(double sin , double cos , Vec2d center)
        {
            CenterPoint = new Vec2d(0, 0);
            for(int i=0;i<4; i++)
            {
                var p = Points[i] - center;
                Points[i].Item0 = p.Item0 * cos - p.Item1 * sin + center.Item0;
                Points[i].Item1 = p.Item0 * sin + p.Item1 * cos + center.Item1;
                
                CenterPoint += Points[i];
            }
            CenterPoint *= .25;

            NormalAxi = CursorControl.GetCursorNormalAxis(Points);

        }


        public override bool Equals(object obj)
        {
            RangeOfMotionProps p = (RangeOfMotionProps)obj;

            if (!p.CenterPoint.Equals(CenterPoint)) return false;

            if (NormalAxi == null ^ p.NormalAxi == null) return false;

            if (Points == null ^ p.Points == null) return false;

            if (NormalAxi == null) return true;
            if (Points == null) return true;

            for (int i = 0; i < NormalAxi.Length; i++)
                if (!NormalAxi[i].Equals(p.NormalAxi[i])) return false;

            for (int i = 0; i < Points.Length; i++)
                if (!Points[i].Equals(p.Points[i])) return false;


            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
