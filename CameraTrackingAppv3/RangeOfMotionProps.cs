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

     /*   public override bool Equals(object obj)
        {
            RangeOfMotionProps p = (RangeOfMotionProps)obj;

            for (int i = 0; i < NormalAxi.Length; i++)
                if (NormalAxi[i].Equals(p.NormalAxi[i])) return false;

           

            return base.Equals(obj);
        }*/


    }
}
