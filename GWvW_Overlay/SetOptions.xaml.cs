using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
//using System.Configuration;

using System.Windows.Threading;

namespace GWvW_Overlay
{
    /// <summary>
    /// Interaction logic for SetOptions.xaml
    /// </summary>
    public partial class SetOptions : Window
    {
        Keyboard.KeyboardListener KListener = new Keyboard.KeyboardListener();
        public bool listenForKey = false;
        public SetOptions()
        {
            InitializeComponent();
            KListener.KeyDown += new Keyboard.RawKeyEventHandler(KListener_KeyDown);

            txtbox_hotkey.Text = Properties.Settings.Default["hotkey"].ToString();
            //chkAlwaysTop.IsChecked = (bool)Properties.Settings.Default["alwaysTop"];
            //chkShowNames.IsChecked = (bool)Properties.Settings.Default["show_names"];
            //opticity_slider.Value = (double)Properties.Settings.Default["opticity"];
            //Console.WriteLine(opticity_slider.Value + " " + (double)Properties.Settings.Default["opticity"]);
        }

        void KListener_KeyDown(object sender, Keyboard.RawKeyEventArgs args)
        {
            //if (args.Key.ToString() == "Home")
            if (listenForKey)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    txtbox_hotkey.Text = args.Key.ToString();
                }));
                
            }
        }

        private void btnNewHotkey_Click(object sender, RoutedEventArgs e)
        {
            if(btnNewHotkey.Content == "Save")
            {
                Properties.Settings.Default["hotkey"] = txtbox_hotkey.Text;
                Properties.Settings.Default.Save();
            }
            listenForKey = (!listenForKey) ? true : false;
            btnNewHotkey.Content = (btnNewHotkey.Content != "Save") ? "Save" : "New Hotkey";

        }

        /*private void chkAlwaysTop_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["alwaysTop"] = true;
            Properties.Settings.Default.Save();
        }

        private void chkAlwaysTop_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["alwaysTop"] = false;
            Properties.Settings.Default.Save();
        }

        private void opticity_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider x = (Slider)sender;
            Properties.Settings.Default["opticity"] = x.Value;
            Properties.Settings.Default.Save();
        }

        private void chkShowNames_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["show_names"] = true;
            Properties.Settings.Default.Save();
        }

        private void chkShowNames_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["show_names"] = false;
            Properties.Settings.Default.Save();
        }*/

        private void saveSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
