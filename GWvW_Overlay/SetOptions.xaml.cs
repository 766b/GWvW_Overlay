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

        private CampLogger track;

        public SetOptions(CampLogger tracker, WvwMatch_ matchUp)
        {
            InitializeComponent();
            track = tracker;
            DataContext = matchUp;

            KListener.KeyDown += new Keyboard.RawKeyEventHandler(KListener_KeyDown);
            txtbox_hotkey.Text = Properties.Settings.Default["hotkey"].ToString();
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
            track.Visibility = track.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
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

        private void langSelection_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["show_names_lang"] = (string)((RadioButton)sender).Tag;
            Properties.Settings.Default.Save();
            Console.WriteLine(Properties.Settings.Default["show_names_lang"]);
        }

        private void onLoad(object sender, RoutedEventArgs e)
        {
            lblCacheSize.Content = string.Format("Guild_Details Cache File Size: {0}", Utils.fileSize("Resources/guild_details.json"));

            switch (Properties.Settings.Default["show_names_lang"].ToString())
            {
                case "English":
                    langEnglish.IsChecked = true; break;
                case "German":
                    langGerman.IsChecked = true; break;
                case "Spanish":
                    langSpanish.IsChecked = true; break;
                case "French":
                    langFrench.IsChecked = true; break;
                default:
                    Properties.Settings.Default["show_names_lang"] = "English";
                    Properties.Settings.Default.Save();
                    langEnglish.IsChecked = true;
                    break;
            }


            foreach (World_Names_ item in CmbbxMatchSelection.Items)
            {
                if (item.id == (int)Properties.Settings.Default["home_server"])
                {
                    CmbbxMatchSelection.SelectedItem = item;
                }
            }
        }
    }
}
