using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.ReinforceLab.SignalTrace;

namespace com.ReinforceLab.AudioModem
{
    /// <summary>
    /// BPSK バイトデータ伝送
    ///   ボーレート: 2400， 4800， 9600 のいずれか
    ///   変調方式: BPSK
    ///   通信路符号化: NRZI
    ///   サンプリングレート: 4倍オーバサンプリング (ボーレトに対して)    
    ///   ビットオーダー: LSb-first (先頭は最下位ビット) 
    ///   同期方式: キャラクタ同期(0x7e)， エスケープキャラクタは(0x7d)
    ///   
    /// キャリア検出: Carrier未検出条件は、10秒以上の通信がないこと。
    /// レイテンシ: 規定せず。(タイマ割り込み周期は100msecをデフォルト値に。レイテンシ=送信データ時間 (16バイトで14msec@9600bps)+ (送受信遅れ(OpenAL)+タイマー(100msec) ~ 200msec程度)
    /// </summary>
    public class BPSKModem : IModem
    {
        #region Variables
        const int _samplePerCycle = 4;
        const int _sampleDuration = 100;  // OpenAL buffer read timer period
        const int _bufferLength   = 1024; // 

        const Byte _SyncChar   = 0x7e;
        const Byte _EscapeChar = 0x7d;
        const int _TimerPeriod = 100; // timer period in ms

        readonly IAudioSocket      _socket;
        readonly BPSKDemodulator   _demodulator;
        readonly BPSKModulator     _modulator;
        readonly ClockDataRecovery _dataRecovery;

        readonly int _baudRate;
        readonly int _samplingRate;

        System.Threading.Timer _timer;

        ITracer _audioSignal;
        ITracer _demodSignal;
        #endregion

        #region Properties
        public int BaudRate {get{return _baudRate;}}
        #endregion

        #region Constructor
        public BPSKModem(int baudRate)
        {
            // check baud rate, サンプリングレート範囲 8k ~ 44k sps よって、2k ~ 11k bps -> 2400から9600bpsまで対応
            if (baudRate < 1200 || baudRate > 9600)
            {
                throw new ArgumentOutOfRangeException("baud rate", baudRate, String.Format("baud rate should be within 2400 to 9600 bps."));
            }
            _baudRate = baudRate;
            
            // setting sampling period to the signal tracer
            com.ReinforceLab.SignalTrace.TracerFactory.SamplingPeriod = 1.0 / _baudRate / _samplePerCycle;            
            
            _samplingRate = (_baudRate * _samplePerCycle);
            _socket       = new OpenALSocket(_samplingRate, (_samplingRate * _sampleDuration) * 4 / 1000);
            _demodulator  = new BPSKDemodulator();
            _modulator    = new BPSKModulator();
            _dataRecovery = new ClockDataRecovery(_SyncChar);

            _audioSignal = SignalTrace.TracerFactory.CreateRecorder("Input audio");
            _demodSignal = SignalTrace.TracerFactory.CreateRecorder("Demod sig");
        }
        #endregion

        #region Private methods
        void _timerWorker(object state)
        {
            if (!_socket.IsRunning)
                return;    
            
            // receive data
            short[] buffer = _socket.Read();
            while (buffer.Length > 0)
            {
                var demod = _demodulator.Demodulate(buffer);
                _dataRecovery.Recovery(demod);

                // debug
                _audioSignal.AddValue(buffer);
                _demodSignal.AddValue(demod);

                buffer = _socket.Read();
            }            
        }
        #endregion

        #region IModem メンバ
        public void Start()
        {
            _socket.Start();

            // staring worker thread to send/receive data
            if (null == _timer)
            {                
                var span = TimeSpan.FromMilliseconds(_sampleDuration);
                _timer = new System.Threading.Timer(new System.Threading.TimerCallback(_timerWorker), null, span, span);
            }
        }
        public void Stop()
        {
            if (null != _timer)
            {
                _timer.Dispose();
                _timer = null;
            }
            _socket.Stop();
        }
        public void Write(IEnumerable<byte> data)
        {
            // modulation signal
            List<byte> packet = new List<byte>();
            for(int i = 0; i < 8; i++)
                packet.Add(_SyncChar);

            foreach (var b in data)
            {
                if (b == _SyncChar)
                    packet.Add(_EscapeChar);
                packet.Add(b);
            }
            var modsig = _modulator.Modulate(packet.ToArray());
            var modsig_2 = new List<short>();
            modsig_2.AddRange(modsig);
            for (int i = 0; i < 10; i++)
                modsig_2.Add(0);
            _socket.Write(modsig_2.ToArray());            
        }
        public byte[] Read()
        {
            Byte[] rcv_data = _dataRecovery.Read();
            if(null == rcv_data || rcv_data.Length <= 0)
                return new byte[]{};
            
            int index = 0;
            List<Byte> data = new List<byte>();
            // skip proceeding sync chars
            for (; index < rcv_data.Length && rcv_data[index] == _SyncChar; index++) ;
            for (; index < rcv_data.Length; index++ )
            {
                if (rcv_data[index] == _EscapeChar)
                    index++; // FIX INVALID ARRAY INDEX ACCESS MAY OCCUR
                data.Add(rcv_data[index]);
            }
            return data.ToArray();
        }
        #endregion        
    }
}
