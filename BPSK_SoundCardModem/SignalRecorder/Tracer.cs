using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.SignalTrace
{
    class Tracer : ITracer
    {
        #region Variables
        readonly String signalName;
        readonly List<double> signal;
        
        bool _enabled;
        #endregion
        
        #region Constructor
        public Tracer(String sigName)
        {
            signalName = sigName;
            signal = new List<double>();
            
            _enabled = false;
        }
        #endregion

        #region IRecorder メンバ        
        public string SignalName {get { return signalName; }}
        public IEnumerable<double> Signal {get{return signal;}}
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public void AddValue(double value)
        {
            if (_enabled)
            {
                signal.Add(value);
            }            
        }
        public void AddValue(IEnumerable<short> values)
        {
            if (_enabled)
            {
                foreach (var val in values)
                    signal.Add((double)val);
            }
        }
        public void AddValue(IEnumerable<int> values)
        {
            if (_enabled)
            {
                foreach (var val in values)
                    signal.Add((double)val);
            }
        }
        public void AddValue(IEnumerable<double> values)
        {
            if (_enabled)
            {
                signal.AddRange(values);
            }
        }
        public void Clear()
        {
            signal.Clear();
        }
        #endregion

    }
}
