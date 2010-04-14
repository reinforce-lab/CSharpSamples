using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.AudioModem
{
    /// <summary>
    /// Audio interface
    /// </summary>
    public interface IAudioSocket : IDisposable
    {
        /// <summary>
        /// Audio interface running flag
        /// </summary>
        bool IsRunning { get;}
        
        /// <summary>
        /// Sampling rate (Sample/sec)
        /// </summary>        
        int SamplingRate { get;}
        
        /// <summary>
        /// Start writing to / reading from the audio interface
        /// </summary>
        void Start();

        /// <summary>
        /// stop accessing to the audio interface
        /// </summary>
        void Stop();

        /// <summary>
        /// output audio waveforms
        /// </summary>        
        void Write(short[] buffer);
        
        /// <summary>
        /// getting sampled buffer data
        /// </summary>        
        short[] Read();
    }
}
