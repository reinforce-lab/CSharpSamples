using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.AudioModem
{
    /// <summary>
    /// 変調クラス        
    ///     動作：変調、バイトデータによる変調データ生成。バイト単位のデータ生成に特化。同期、エラー検出はない。
    ///     動作条件：オーバサンプル4でのみ
    ///     LSb-first
    ///     位相： 1ならそのまま、0なら反転 （検波すれば＋振幅が1、ー振幅が0に対応する、正論理）
    /// </summary>
    class BPSKModulator
    {
        #region Variables            
        const int _samplePerCycle = 4;
        #endregion
            
        #region Construcotr            
        public BPSKModulator()
        {            
        }            
        #endregion
            
        #region Public methods            
        public short[] Modulate(byte [] data)
        {
            // allocate buffer
            var buf = new short[_samplePerCycle * data.Length * (8 + 2)];
            Array.Clear(buf, 0, buf.Length);
            // fill data
            int ptr = 0;
            bool phase = true;
            for (int index = 0; index < data.Length; index++)
            {
                int b = (data[index] << 1) | 0x01;
                for (int i = 0; i < (8 + 2); i++)
                {
                    phase = ((0x01 & b) == 0) ? ! phase : phase; //0なら反転
                    buf[ptr + 1] = phase ? (short)(short.MaxValue -10) : (short)(short.MinValue + 10);
                    buf[ptr + 3] = phase ? (short)(short.MinValue +10) : (short)(short.MaxValue -10);

                    ptr += 4;
                    b = (b >> 1);
                }
            }

            return buf;
        }
        #endregion
    }
}
