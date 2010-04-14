using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.AudioModem
{
    /// <summary>
    /// ベースライン信号から、バイトデータを再生
    ///     LSb-first
    ///     オーバサンプリング 4
    ///     同期キャラクタ 0x7e 
    ///     同期キャラクタも、読み出し対象
    ///     連続する0x7e検出により、同期。一旦同期した後は、キャリアが連続する限り、データを受信。
    ///     1ビット（4サイクル)以上のキャリア信号断絶により、キャリ断と判断。
    ///     
    /// キャリア断絶による、1ブロックのデータ伝送と判断。ここで初めて読み出しができる。    
    /// 遷移条件：
    ///     NoCarrier →OutOfSync
    ///         符号なし整数でスライスレベル以上なら＋1、以下なら－1(0クリップ)。レベルが4以上で遷移。
    /// </summary>
    public enum RecoveryState { SYNC_EDGE, SYNC, DATA_EDGE, DATA };
    class ClockDataRecovery
    {
        #region Variables        
        readonly byte _syncCharactor;        

        readonly Queue<Byte[]> _queue;
        readonly List<Byte> _rcvBuf;        
        
        RecoveryState _state;
        byte _recoveryData;
        int  _recoveryBitPos;
        int _indexOffset;
        #endregion

        #region Properties
        public RecoveryState State { get {return _state;} }
        public bool HasReceived { get { return _queue.Count > 0; } }
        #endregion

        #region Construcotr
        public ClockDataRecovery(byte sync_char)
        {
            _rcvBuf = new List<byte>();
            _queue  = new Queue<byte[]>();
            _indexOffset = 0;

            _syncCharactor = sync_char;            
        }
        #endregion

        #region Private methods
        /// <summary>
        /// エッジ検出。
        /// </summary>
        int syncEdgeDetection(int[] waveform, int offset)
        {
            int index = offset;
            for (; index < waveform.Length; index++)
            {
                // edge detection
                if (waveform[index] > 0)
                {                    
                    _state = RecoveryState.SYNC;
                    return (index + 1 + 4);
                }
            }
            return index;
        }
        int dataEdgeDetection(int[] waveform, int offset)
        {
            int index = offset;
            int cnt   = 4;
            for (; index < waveform.Length; index++)
            {
                // edge detection
                if (waveform[index] > 0)
                {
                    _recoveryData = 0;
                    _state = RecoveryState.DATA;
                    return (index + 1 + 4);
                }
                cnt--;
                if (cnt < 0)
                {
                    _queue.Enqueue(_rcvBuf.ToArray());
                    _rcvBuf.Clear();
                    _state = RecoveryState.SYNC_EDGE;
                    return index;
                }
            }
            return index;
        }
        /// <summary>
        /// 同期キャラクタ検出を行う。キャラクタ検出を行ったら、その完了ポイントへの、インデックスを返す。
        /// 同期キャラクタは、読み出しバッファに収納される。
        /// キャラクタ検出できなければ、インデックスはバッファ長と同じ。
        /// </summary>
        int synchronation(int[] waveform, int offset)
        {            
            byte data = 0;
            int cnt   = 16;
            int index = offset;            
            for (; index < waveform.Length; index+=4)
            {
                // char sync
                if (waveform[index] > 0)
                    data |= 0x80;
                
                if (data == _syncCharactor)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Sync: 0x{0:X}.", data));

                    _recoveryData   = 0;
                    _recoveryBitPos = 0;
                    _state = RecoveryState.DATA_EDGE;
                    return (index + 4);
                }
                // nosignal
                cnt--;
                if (cnt < 0)
                {
                    _state = RecoveryState.SYNC_EDGE;
                    return index;
                }
                data >>= 1;
            }
            return index;
        }

        /// <summary>
        /// データ再生を行う。
        /// </summary>
        int dataRecovery(int[] waveform, int offset)
        {
            int index = offset;
            for (; index < waveform.Length; index += 4)
            {
                // char sync
                if (waveform[index] > 0)
                    _recoveryData |= 0x80;
                
                if (_recoveryBitPos >= 7)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Receiving: 0x{0:X}.", _recoveryData));
                    _rcvBuf.Add(_recoveryData);
                    _recoveryData = 0;
                    _recoveryBitPos = 0;
                    _state = RecoveryState.DATA_EDGE;
                    return (index + 4);
                }
                _recoveryBitPos++;
                _recoveryData >>= 1;
            }
            return index;            
        }
        #endregion

        #region Public methods
        /// <summary>
        /// データをリカバリします。これを読み出したあとは、HasReceivedが有効ならば、データバッファを読み出してください。次に呼び出したとき、このバッファは破壊されています。
        /// </summary>
        /// <param name="waveform"></param>
        public void Recovery(int[] waveform)
        {
            int index = _indexOffset;
            while (index < waveform.Length)
            {
                switch (_state)
                {
                    case RecoveryState.SYNC_EDGE:
                        index = syncEdgeDetection(waveform, index);
                        break;
                    case RecoveryState.SYNC:
                        index = synchronation(waveform, index);
                        break;
                    case RecoveryState.DATA_EDGE:
                        index = dataEdgeDetection(waveform, index);
                        break;
                    case RecoveryState.DATA:
                        index = dataRecovery(waveform, index);
                        break;
                    default: _state = RecoveryState.SYNC_EDGE;
                        return;                        
                }
            }
            _indexOffset = index - waveform.Length;
        }
        /// <summary>
        /// バッファのブロックデータを読みだします
        /// </summary>        
        public Byte[] Read()
        {
            if (_queue.Count <= 0)
                return new byte[] { };

            return _queue.Dequeue();
        }
        /// <summary>
        /// 強制的に同期状態に遷移します
        /// </summary>
        public void Resynchronize()
        {
            _rcvBuf.Clear();
            _state = RecoveryState.SYNC_EDGE;
        }     
        #endregion
    }
}
