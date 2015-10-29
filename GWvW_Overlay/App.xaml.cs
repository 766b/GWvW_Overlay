using Logitech_LCD;
using System;
using System.Windows;
using ArenaNET;
using GWvW_Overlay.Properties;
using Logitech_LED;

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
            Request.Lang = Utils.ConvertLanguageToCode(Settings.Default.show_names_lang);

            Console.WriteLine(LogitechLcd.Instance.Init("GWvW Timers", LcdType.Color | LcdType.Mono));

            if (LogitechLed.Instance.IsInit)
            {
                LogitechLed.Instance.SaveCurrentLighting();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            KListener.Dispose();

            if (LogitechLed.Instance.IsInit)
            {
                LogitechLed.Instance.RestoreLighting();
            }
            GWvW_Overlay.MainWindow.DataLink.Dispose();
        }
    }
}


