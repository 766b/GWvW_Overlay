using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GWvW_Overlay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        Keyboard.KeyboardListener KListener = new Keyboard.KeyboardListener();

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            Console.WriteLine(Logitech_LCD.NativeMethods.Init("GWvW Timers", Logitech_LCD.NativeMethods.LcdType.Color | Logitech_LCD.NativeMethods.LcdType.Mono));
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Logitech_LCD.NativeMethods.Shutdown();
            KListener.Dispose();
        }
    }
}


