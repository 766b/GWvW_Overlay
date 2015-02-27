using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GWvW_Overlay.Properties;
using GWvW_Overlay.Resources.Lang;

namespace GWvW_Overlay.DataModel
{
    public class Options_ : INotifyPropertyChanged
    {
        public string HomeServerColor;
        private string _active_bl = "Center";
        private string _active_bl_title = "Eternal Battlegrounds";
        public int _active_blid;

        private string _active_map_img = "Resources/mapeb_normal.png";

        public string _active_match; // = "1-1";
        private double _height = 400;
        private double _width = 400;
        public Dictionary<string, int> blid;

        public int active_blid
        {
            get { return blid[_active_bl]; }
        }

        public string active_bl_title
        {
            get { return _active_bl_title; }
            set
            {
                _active_bl_title = value;
                OnPropertyChanged();
            }
        }

        public string active_match
        {
            get { return _active_match; }
            set
            {
                if (value != _active_match)
                {
                    _active_match = value;
                    OnPropertyChanged();
                }
            }
        }

        public string active_bl
        {
            get { return _active_bl; }
            set
            {
                switch (value)
                {
                    case "RedHome":
                        active_bl_title = Strings.redBorderland;
                        active_map_img = "Resources/mapbl_normal.png";
                        ChangeWindowSize(false);
                        break;
                    case "GreenHome":
                        active_bl_title = Strings.greenBorderland;
                        active_map_img = "Resources/mapbl_normal.png";
                        ChangeWindowSize(false);
                        break;
                    case "BlueHome":
                        active_bl_title = Strings.blueBorderland;
                        active_map_img = "Resources/mapbl_normal.png";
                        ChangeWindowSize(false);
                        break;
                    default:
                        active_bl_title = Strings.eternalBattlegrounds;
                        active_map_img = "Resources/mapeb_normal.png";
                        ChangeWindowSize(true);
                        break;
                }

                _active_bl = value;
                OnPropertyChanged();
            }
        }

        public string active_map_img
        {
            get { return _active_map_img; }
            set
            {
                if (value != _active_map_img)
                {
                    _active_map_img = value;
                    OnPropertyChanged();
                }
            }
        }

        public double min_width
        {
            get
            {
                if (active_bl == "Center") return Settings.Default.main_eb_width;
                return Settings.Default.main_bl_width;
            }
        }

        public double min_height
        {
            get
            {
                if (active_bl == "Center") return Settings.Default.main_eb_height;
                return Settings.Default.main_bl_height;
            }
        }

        public double width
        {
            get { return _width; }
            set
            {
                if (value != _width)
                {
                    Console.WriteLine("W:{0}", value);
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        public double height
        {
            get { return _height; }
            set
            {
                if (value != _height)
                {
                    Console.WriteLine("H:{0}", value);
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ChangeWindowSize(bool etrnBattle)
        {
            if (etrnBattle)
                ChangeWindowSize(Settings.Default.main_eb_width, Settings.Default.main_eb_height);
            else
                ChangeWindowSize(Settings.Default.main_bl_width, Settings.Default.main_bl_height);
        }

        public void ChangeWindowSize(double Width, double Height)
        {
            width = Width;
            height = Height;
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = "none passed")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
