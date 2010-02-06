using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.mjpeg_server
{
    public interface IImageSource
    {
        RectangleF Frame { get; }
        Byte[] GetImageAsJpeg(RectangleF roi);
    }
}
