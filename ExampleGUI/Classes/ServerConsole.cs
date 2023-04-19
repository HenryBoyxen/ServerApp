using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ExampleGUI.Classes
{
    internal static class ServerConsole
    {
        public static TextBlock _Console;
        public static Task WriteLine(string message)
        {
            _Console.Text = message + "\n" + _Console.Text;
            Task.Delay(1);
            return Task.CompletedTask;
        }
    }
}
