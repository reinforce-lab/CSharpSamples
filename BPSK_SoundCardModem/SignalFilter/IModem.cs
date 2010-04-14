using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.AudioModem
{
    public interface IModem
    {
        int BaudRate { get; }

        void Start();
        void Stop();

        void Write(IEnumerable<byte> data);
        Byte[] Read();
    }
}
