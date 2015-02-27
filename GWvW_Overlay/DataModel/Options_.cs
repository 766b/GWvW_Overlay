using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GWvW_Overlay.Resources.Lang;

namespace GWvW_Overlay.DataModel
{
    public class Options_ : INotifyPropertyChanged
    {
        private string _active_bl = "Center";
        public Dictionary<string, int> blid;
        public int _active_blid;
        public string HomeServerColor;

        private string _active_map_img = "Resources/mapeb_normal.png";

        private double _width = 400;
        private double _height = 400;

        //private double _min_width;
        //private double _min_height;

        private string _active_bl_title = "Eternal Battlegrounds";
        public string _active_match;// = "1-1";

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
            get { if (active_bl == "Center") return Properties.Settings.Default.main_eb_width; else return Properties.Settings.Default.main_bl_width; }
        }
        public double min_height
        {
            get { if (active_bl == "Center") return Properties.Settings.Default.main_eb_height; else return Properties.Settings.Default.main_bl_height; }
        }
        public double width
        {
            get { return _width; }
            set
            {
                if (value != _width)
                {
                    Console.WriteLine("W:{0}", value.ToString());
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
                    Console.WriteLine("H:{0}", value.ToString());
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        public void ChangeWindowSize(bool etrnBattle)
        {
            if (etrnBattle)
                ChangeWindowSize(Properties.Settings.Default.main_eb_width, Properties.Settings.Default.main_eb_height);
            else
                ChangeWindowSize(Properties.Settings.Default.main_bl_width, Properties.Settings.Default.main_bl_height);
        }

        public void ChangeWindowSize(double Width, double Height)
        {
            width = Width;
            height = Height;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "none passed")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}