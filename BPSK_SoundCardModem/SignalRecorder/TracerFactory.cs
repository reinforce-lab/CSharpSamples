using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.SignalTrace
{
    public static class TracerFactory
    {
        static List<ITracer> _recorders = new List<ITracer>();
        static double samplePeriod = 1.0;

        public static double SamplingPeriod { get { return samplePeriod; } set { if (value > 0) samplePeriod = value; } }
        public static ITracer[] Recorders { get { return _recorders.ToArray(); } }

        public static ITracer CreateRecorder(String signalName)
        {
            var recorder = new Tracer(signalName);
            _recorders.Add(recorder);
            return recorder;
        }

        /// <summary>
        /// Clearing signal records
        /// </summary>
        public static void Clear()
        {
            foreach (var recorder in _recorders)
            {
                recorder.Clear();   
            }            
        }

        /// <summary>
        /// Start recording
        /// </summary>
        public static void StartRecording()
        {
            foreach (var recorder in _recorders)
            {
                recorder.Enabled = true;
            }
        }
        
        /// <summary>
        /// Stop recording
        /// </summary>
        public static void StopRecording()
        {
            foreach (var recorder in _recorders)
            {
                recorder.Enabled = false;
            }
        }

        public static void DumpSpiceTr0(String filePath)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);

            var formatter = new SpiceTr0Formatter();
            formatter.Dump(writer, samplePeriod, Recorders);

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
