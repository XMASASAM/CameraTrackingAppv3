using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    interface IDrawFrameForm
    {

        void DrawFrame(ref Mat frame);
    }
}
