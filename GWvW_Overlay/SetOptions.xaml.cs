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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GWvW_Overlay
{
    /// <summary>
    /// Interaction logic for SetOptions.xaml
    /// </summary>
    public partial class SetOptions : Window 
    {
        Keyboard.KeyboardListener KListener = new Keyboard.KeyboardListener();
        Utils Utils = new Utils();
        public bool listenForKey = false;

        CampLogger track;
        public SetOptions(CampLogger tracker)
        {
            InitializeComponent();
            track = tracker;
            KListener.KeyDown += new Keyboard.RawKeyEventHandler(KListener_KeyDown);
            txtbox_hotkey.Text = Properties.Settings.Default["hotkey"].ToString();

            lblCacheSize.Content = string.Format("Guild_Details Cache File Size: {0}", Utils.fileSize("Resources/guild_details.json"));
        }

        void KListener_KeyDown(object sender, Keyboard.RawKeyEventArgs args)
        {
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
            if(btnNewHotkey.Content.ToString() == "Save")
            {
                Properties.Settings.Default["hotkey"] = txtbox_hotkey.Text;
                Properties.Settings.Default.Save();
            }
            listenForKey = (!listenForKey) ? true : false;
            btnNewHotkey.Content = (btnNewHotkey.Content.ToString() != "Save") ? "Save" : "New Hotkey";

        }

        private void saveSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void btnResetPos_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["tracker_position_top"] = 10.0;
            Properties.Settings.Default["tracker_position_left"] = 10.0;
            Properties.Settings.Default.Save();
            track.cnvsPromt.Visibility = Visibility.Visible;
            track.ClickTroughVoid();
        }

        private void chkTrackerDisplay_Click(object sender, RoutedEventArgs e)
        {
            if (track.Visibility == Visibility.Hidden)
                track.Visibility = Visibility.Visible;
            else
                track.Visibility = Visibility.Hidden;
        }

        private void chkEventLog_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)Properties.Settings.Default["tracker_show_event"])
            {
                Properties.Settings.Default["tracker_height"] = 140.0;
                track.Height = 140;
            }
            else
            {
                Properties.Settings.Default["tracker_height"] = 300.0;
                track.Height = 300;
            }
            Properties.Settings.Default.Save();

        }
    }
}
