using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.ReinforceLab.SignalTrace;

namespace com.ReinforceLab.AudioModem
{
    /// <summary>
    /// 復調クラス
    ///     動作：復調、入力信号と同じサンプリングレートの復調信号を生成
    ///     動作条件：オーバサンプル4でのみ    
    /// </summary>
    class BPSKDemodulator
    {
        #region Variables        
        readonly int[] _delayLine;
        const int _sliceLevel = 200000000;

        int _phase;        
        int _lpfmod;
        #endregion

        #region Constructor
        public BPSKDemodulator()
        {            
            _delayLine = new int[4];
            _phase = 0;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// 信号波形の復号処理
        /// </summary>
        /// <param name="waveform"></param>
        public int[] Demodulate(short[] waveform)
        {
            var demodSig = new int[waveform.Length];
            for(int index=0; index <waveform.Length; index++)
            {
                var sig = waveform[index];
                // demodulation using delayed line
                int demod = sig * _delayLine[_phase];
                _delayLine[_phase] = sig;
                // 2-omegalow pass filter
                int lpfsig = (_lpfmod + demod) / 2;
                _lpfmod = demod;
                
                // set value
                demodSig[index] = (lpfsig > _sliceLevel) ? 1: 0;

                // next phase
                _phase = (_phase + 1) % 4;
            }

            return demodSig;
        }
        #endregion
    }
}
