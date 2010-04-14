using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using com.ReinforceLab.AudioModem;

namespace WpfApplication1
{
    /// <summary>
    /// TerminalWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TerminalWindow : Window
    {
        const int baudRate = 9600;
        BPSKModem _modem;

        System.Windows.Threading.DispatcherTimer _timer;
        
        public TerminalWindow()
        {
            InitializeComponent();

            Unloaded += new RoutedEventHandler(TerminalWindow_Unloaded);
            _modem = new BPSKModem(baudRate);
            _modem.Start();

            // receive buffer
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick    += new EventHandler(_timer_Tick);
            _timer.Start();
        }

        void TerminalWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _modem.Stop();
        }

        void sendText()
        {
            if (String.IsNullOrEmpty(_inputTextBox.Text))
                return;

            byte[] data = Encoding.ASCII.GetBytes(_inputTextBox.Text + "\n");
            _modem.Write(data);

            _inputTextBox.Text = string.Empty;
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            var buf = _modem.Read();
            if (buf.Length <= 0)
                return;

            var str = Encoding.ASCII.GetString(buf);
            if (!String.IsNullOrEmpty(str))
            {
                _terminalTextBox.Text = _terminalTextBox.Text + str;
            }            
        }

        private void inputTextSendButton_Click(object sender, RoutedEventArgs e)
        {
            sendText();
        }

        private void inputTextClearButton_Click(object sender, RoutedEventArgs e)
        {
            _inputTextBox.Text = string.Empty;
        }

        private void terminalTextClearButton_Click(object sender, RoutedEventArgs e)
        {
            _terminalTextBox.Text = string.Empty;
        }

        private void _inputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {            
            //sendText();
        }

        private void _testButton_Click(object sender, RoutedEventArgs e)
        {
            com.ReinforceLab.SignalTrace.TracerFactory.StartRecording();

            byte[] data = Encoding.ASCII.GetBytes("ffff");
            _modem.Write(data);
            
            System.Threading.Thread.Sleep(500);
            com.ReinforceLab.SignalTrace.TracerFactory.StopRecording();
            // dump
            com.ReinforceLab.SignalTrace.TracerFactory.DumpSpiceTr0(@"c:\bpsk_test.tr0");
            com.ReinforceLab.SignalTrace.TracerFactory.Clear();
        }
    }
}
