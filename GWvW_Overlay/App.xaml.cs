using Logitech_LCD;
using System;
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
            Console.WriteLine(LogitechLcd.Instance.init("GWvW Timers", LcdType.Color | LcdType.Mono));
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            KListener.Dispose();
        }
    }
}


