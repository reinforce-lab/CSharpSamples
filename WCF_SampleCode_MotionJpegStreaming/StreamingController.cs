using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using System.ServiceModel.Web;

namespace com.ReinforceLab.mjpeg_server
{
    class StreamingController : IStreamingController, IDisposable
    {
        #region Variables
        readonly IImageSource _imageSource;
        readonly IMjpegStreamingService _service;        
        readonly WebServiceHost _host;

        int _frameRate;
        System.Timers.Timer _timer;
        #endregion

        #region Constructor
        public StreamingController(Uri baseAddress)
        {   
            // setting up data source and web service
            _service = new MjpegStreamingService(this);
            _imageSource = new JpegImageSource();
            
            // setting up a web service point
            _host = new WebServiceHost(_service, baseAddress);
            var bnd = new WebHttpBinding();            
            // -important- to stream image, "streamed" transfermode is required.
            bnd.TransferMode = TransferMode.Streamed;
            // streaming session time is restricted to 30-sec in this demonstration code
            bnd.SendTimeout = new TimeSpan(0, 0, 30);
            _host.AddServiceEndpoint(typeof(IMjpegStreamingService), bnd, baseAddress);

            // start streaming
            setFrameRate(10);
            startTimer();
            _host.Open();
        }
        #endregion

        #region Private methods
        void stopTimer()
        {
            if(null != _timer)
            {
                _timer.Dispose();
                _timer = null;
            }
        }        
        void startTimer()
        {
            stopTimer();

            _timer = new System.Timers.Timer(1000.0 / (double)_frameRate);
            _timer.AutoReset = true;
            _timer.Elapsed +=new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            _timer.Start();
        }        
        void restartTimer()
        {
            startTimer();
        }        
        void setFrameRate(int frameRate) 
        {            
            if(_frameRate == frameRate) return;
            _frameRate = Math.Min(15, Math.Max(0, frameRate));
        }        
        #endregion

        #region Event handlers
        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("StreamingController._timer_Elapsed()");

            // check streaming buffer
            if(_service.StreamingBufferStalled) return;

            // get camaera image
            var img = _imageSource.GetImageAsJpeg(_imageSource.Frame);
            if(null == img)  return;

            // stream it
            _service.AddImage(img);
        }
        #endregion
        
        #region IStreamingController メンバ        
        public int FrameRate
        {
            get{return _frameRate;}
            set{setFrameRate(value);}
        }
        public System.Drawing.RectangleF Frame
        {
            get {return _imageSource.Frame; }            
        }
        public System.Drawing.RectangleF RegionOfInterest
        {
            get { return _imageSource.Frame; }
            set { }
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            stopTimer();
            _host.Close();
        }
        #endregion
    }
}
