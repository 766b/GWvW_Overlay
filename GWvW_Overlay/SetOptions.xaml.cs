﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GWvW_Overlay
{
    public partial class SetOptions
    {
        readonly Keyboard.KeyboardListener _kListener = new Keyboard.KeyboardListener();
        public Utils Utils = new Utils();
        public bool ListenForKey = false;

        private readonly CampLogger _track;

        public SetOptions(CampLogger tracker, WvwMatch_ matchUp)
        {
            InitializeComponent();
            _track = tracker;
            DataContext = matchUp;

            _kListener.KeyDown += KListener_KeyDown;
            txtbox_hotkey.Text = Properties.Settings.Default["hotkey"].ToString();
        }

        void KListener_KeyDown(object sender, Keyboard.RawKeyEventArgs args)
        {
            if (ListenForKey)
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
            ListenForKey = (!ListenForKey);
            btnNewHotkey.Content = (btnNewHotkey.Content.ToString() != "Save") ? "Save" : "New Hotkey";

        }

        private void SaveSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void btnResetPos_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["tracker_position_top"] = 10.0;
            Properties.Settings.Default["tracker_position_left"] = 10.0;
            Properties.Settings.Default.Save();
            _track.cnvsPromt.Visibility = Visibility.Visible;
            _track.ClickTroughVoid();
        }

        private void chkTrackerDisplay_Click(object sender, RoutedEventArgs e)
        {
            _track.Visibility = _track.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        private void chkEventLog_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)Properties.Settings.Default["tracker_show_event"])
            {
                Properties.Settings.Default["tracker_height"] = 157.0;
                _track.Height = 157;
            }
            else
            {
                Properties.Settings.Default["tracker_height"] = 300.0;
                _track.Height = 300;
            }
            Properties.Settings.Default.Save();

        }

        private void langSelection_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["show_names_lang"] = ((RadioButton)sender).Tag;
            Properties.Settings.Default.Save();
            Console.WriteLine(Properties.Settings.Default["show_names_lang"]);
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            lblCacheSize.Content = string.Format("Guild_Details Cache File Size: {0}", Utils.FileSize("Resources/guild_details.json"));

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
