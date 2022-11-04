using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExampleGUI
{
    /// <summary>
    /// Interaktionslogik für Server.xaml
    /// </summary>
    public partial class Server : Page
    {
        CancellationTokenSource _tokenSource = null;
        public Server()
        {
            InitializeComponent();
        }

        private async void btn_server_start_Click(object sender, RoutedEventArgs e)
        {
            btn_server_start.IsEnabled = false;
            btn_server_stop.IsEnabled = true;
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            try
            {
                await Task.Run(() => ServerLogik.StartListener(this, token));
            }
            finally
            {
                _tokenSource.Dispose();
            }
            
        }

        private void btn_server_stop_Click(object sender, RoutedEventArgs e)
        {
            btn_server_stop.IsEnabled = false;
            txt_server.Text = "Der Server wird gestoppt!\n" + txt_server.Text;
            _tokenSource?.Cancel();
            btn_server_start.IsEnabled = true;
        }
    }
}

