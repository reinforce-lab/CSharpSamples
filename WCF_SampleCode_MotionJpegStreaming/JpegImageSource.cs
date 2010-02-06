using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.mjpeg_server
{
    class JpegImageSource : IImageSource
    {
        #region Variables
        int _index;
        List<Byte[]> _contents;        
        RectangleF _frame;
        #endregion

        #region Constructor
        public JpegImageSource()
        { 
            _index = 0;
            _contents = new List<byte[]>();
            _frame = new RectangleF(0, 0, Properties.Resources.pic_1.Width, Properties.Resources.pic_1.Height);

            var bmps = new System.Drawing.Bitmap[] {                     
                Properties.Resources.pic_1,                    
                Properties.Resources.pic_2,                    
                Properties.Resources.pic_3,                    
                Properties.Resources.pic_4
            };
            for(int i=0; i < bmps.Length; i++)
            {
                var tmpFile = Path.GetTempFileName();
                bmps[i].Save(tmpFile, System.Drawing.Imaging.ImageFormat.Jpeg);
                _contents.Add(File.ReadAllBytes(tmpFile));
            }
        }
        #endregion

        #region IImageSource メンバ
        public RectangleF Frame
        {
            get { return _frame; }
        }
        public byte[] GetImageAsJpeg(RectangleF roi)
        {
            var buf = _contents[_index];            
            _index = (_index + 1) % _contents.Count;

            return buf;
        }
        #endregion

    }
}
