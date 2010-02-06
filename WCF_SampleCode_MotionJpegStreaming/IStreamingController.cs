using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.mjpeg_server
{
    public interface IStreamingController
    {
        int FrameRate { get; set; }
        RectangleF Frame { get; }
        RectangleF RegionOfInterest { get; set; }
    }
}
