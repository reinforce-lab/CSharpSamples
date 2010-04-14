using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace com.ReinforceLab.AudioModem
{
    /// <summary>
    /// オーディオ物理インタフェース。
    /// 録音バッファ    
    ///   周期的に読み出すこと。放置すればバッファオーバフロー。使わないときはStop()。
    ///   読み出しバッファは、Read()して直ちに使用すること。バッファは内部で再利用。Read()の都度値が変わる。
    /// </summary>
    public class OpenALSocket :  IAudioSocket
    {
        #region Variables
        readonly int _samplingRate;        
        readonly short[] _readBuffer;
        readonly short[] _emptyBuffer;

        AudioContext _audioContext;
        AudioCapture _audioCapture;
        int _audioSource;
        #endregion

        #region Property
        public bool IsRunning   { get { return (null != _audioContext); } }
        public int  SamplingRate { get { return _samplingRate; } }
        public int  SendBufferQueueLength
        {
            get {
                if (!IsRunning) return 0;

                int send_queue_len = 0;
                AL.GetSource(_audioSource, ALGetSourcei.BuffersQueued, out send_queue_len);
                return send_queue_len;
            }
        }
        #endregion

        #region Constructor
        public OpenALSocket(int sampling_rate, int readBufferSize)
        {
            _readBuffer  = new short[readBufferSize];
            _emptyBuffer = new short[] { };

            _samplingRate = sampling_rate;                        
        }
        #endregion

        #region Private methods
        void clearBuffer(int buff)
        {
            int[] buffers;
            if (buff == 0)
            {
                int buffer_processed;
                AL.GetSource(_audioSource, ALGetSourcei.BuffersProcessed, out buffer_processed);
                if (buffer_processed == 0)
                    return;
                buffers = AL.SourceUnqueueBuffers(_audioSource, buffer_processed);
            }
            else
            {
                buffers = AL.SourceUnqueueBuffers(_audioSource, buff);
            }
            AL.DeleteBuffers(buffers);
        }
        #endregion

        #region Public methods
        public void Start()
        {
            if (null != _audioContext)
                return;

            _audioContext = new AudioContext();
            AL.Listener(ALListenerf.Gain, 1.0f);
            _audioSource  = AL.GenSource();
            _audioCapture = new AudioCapture(String.Empty, _samplingRate, OpenTK.Audio.OpenAL.ALFormat.Mono16, _readBuffer.Length);

            _audioCapture.Start();
        }
        public void Stop()
        {
            if (null == _audioContext)
                return;

            if (null != _audioCapture)
            {
                _audioCapture.Stop();
                _audioCapture.Dispose();
                _audioCapture = null;
            }

            if (null != _audioContext)
            {
                int r;
                AL.GetSource(_audioSource, ALGetSourcei.BuffersQueued, out r);
                clearBuffer(r);

                AL.DeleteSource(_audioSource);

                _audioContext.Dispose();
                _audioContext = null;
            }
        }
        public void Write(short[] buffer)
        {
            if (null == _audioContext)
                return;

            clearBuffer(0);

            int buf = AL.GenBuffer();
            
            System.Diagnostics.Debug.WriteLine("OpenAL: write buffer.");

            AL.BufferData(buf, ALFormat.Mono16, buffer, buffer.Length * OpenTK.BlittableValueType.StrideOf(buffer), _samplingRate);
            AL.SourceQueueBuffer(_audioSource, buf);

            if (AL.GetSourceState(_audioSource) != ALSourceState.Playing)
                AL.SourcePlay(_audioSource);

            //clearBuffer(0);
        }
        public short[] Read()
        {
            if (null == _audioCapture)
                return _emptyBuffer;

            // does capturing buffer have enough data?
            int available_samples = _audioCapture.AvailableSamples;
            if (available_samples < _readBuffer.Length)
                return _emptyBuffer;
            
            _audioCapture.ReadSamples(_readBuffer, _readBuffer.Length);
            return _readBuffer;
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            Stop();
        }
        #endregion

        #region INotifyPropertyChanged メンバ
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void invokePropertyChanged(String propName)
        {
            if (null != PropertyChanged)
                PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}
