using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.SignalTrace
{
    public interface ITracer
    {
        String SignalName          { get; }
        IEnumerable<Double> Signal { get; }
        bool Enabled { get; set; }

        void AddValue(double value);
        void AddValue(IEnumerable<short> value);
        void AddValue(IEnumerable<int> value);
        void AddValue(IEnumerable<double> value);

        void Clear();
    }
}
