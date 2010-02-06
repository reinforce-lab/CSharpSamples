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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace com.ReinforceLab.mjpeg_server
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Window1 : Window
    {
        const string url = "http://localhost:8081/";
        StreamingController _controller;

        public Window1()
        {
            InitializeComponent();

            var uri       = new Uri(url);
            _controller   = new StreamingController(uri);
            _textbox.Text = url;

            this.Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {            
        }

        void _provider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
        }

        private void _button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(url);         
        }
    }
}
