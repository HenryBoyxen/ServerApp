using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dashboard _dashboard = new Dashboard();
        Server _server = new Server();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = _dashboard;
        }
        private void btn_Server_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = _server;
        }

        private void btn_Server_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
