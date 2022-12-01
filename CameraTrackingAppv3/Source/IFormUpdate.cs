using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    interface IFormUpdate
    {
        UserControl1 UserControl { get; }
        void FormUpdate(ref Mat frame);



    }
}
