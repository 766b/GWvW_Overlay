﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GWvW_Overlay
{
    //Match Details
    public class WvwObjective : INotifyPropertyChanged
    {
        private double _left;
        private double _top;
        private double _left_base;
        private double _top_base;

        public string name { get; set; }
        public int points { get; set; }
        public string type {get; set; }
        public double res_width { get; set; }
        public double res_height { get; set; }

        public double top_base
        {
            set { _top_base = value; }
            get { return _top_base; }
        }

        public double left_base
        {
            set { _left_base = value; }
            get { return _left_base;  }
        }

        public double top
        {
            get { return _top; }
            set
            {
                if (_top_base == 0.0)
                    _top_base = value;

                if (value != _top)
                {
                    _top = value;
                    OnPropertyChanged();
                }
            }
        }

        public double left
        {
            get { return _left; }
            set
            {
                if (value != _left)
                {
                    if (_left_base == 0.0)
                        _left_base = value;

                    _left = value;
                    OnPropertyChanged();
                }
            }
        }

        //8888888888888888888888888888
        public int id { get; set; }
        private string _owner;
        public string _owner_guild;
        public DateTime last_change { get; set; }

        public string owner_guild
        {
            get
            {
                if (_owner_guild != null)
                    return "claimed2";

                return _owner_guild;
            }
            set
            {
                if (value != _owner_guild)
                {
                    _owner_guild = value;
                    OnPropertyChanged();
                }
            }
        }

        public string owner 
        {
            get { return _owner; }
            set
            {
                if (value != _owner)
                {
                    _owner = value;

                    // If owner changes, timer needs to be reset
                    last_change = DateTime.Now;
                    OnPropertyChanged();
                }

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "none passed")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


    }
    public class getIMG : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(values.Length == 1)
                return getPNG(values[0], null);
            if(values.Length == 2)
                return getPNG(values[0], values[1]);

            return null;
        }
        public object[] ConvertBack(object values, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ImageSource getPNG(object type, object color)
        {
            string y;
            if (color == null || color.ToString() == "none")
            {
                y = string.Format("Resources/{0}.png", type);
            }
            else
            {
                y = string.Format("Resources/{0}_{1}.png", type, color.ToString().ToLower());

            }
            ImageSource x = new BitmapImage(new Uri(y, UriKind.Relative));
            return x;
        }
    }
    public class ObjectiveNames_
    {
        public List<WvwObjective> wvw_objectives { get; set; }
    }
    public class Match_Details_
    {
        public string match_id { get; set; }
        public List<int> scores { get; set; }
        public List<Map> maps { get; set; }
    }

    public class Objective
    {
        public int id { get; set; }
        public string owner { get; set; } //TODO: Reset last_change if owner changed
        public string owner_guild { get; set; }
        public DateTime last_change { get; set; }
    }

    public class Map
    {
        public string type { get; set; }
        public List<int> scores { get; set; }
        public List<WvwObjective> objectives { get; set; }
    }

    public class Options_ : INotifyPropertyChanged
    {
        private string _active_bl;
        private string _active_map_img = "Resources/mapeb.png";

        private double _width = 500;
        private double _height = 500;

        public string active_bl_title { get; set; }
        public string active_match { get; set; }

        public string active_bl
        {
            get { return _active_bl; }
            set
            {
                if (value != _active_bl)
                {
                    if (value == "RedHome")
                    {
                        active_bl_title = "Red Borderlands";
                        active_map_img = "Resources/mapbl.png";
                        ChangeWindowSize(580.0, 771.637);
                    }
                    else if (value == "GreenHome")
                    {
                        active_bl_title = "Green Borderlands";
                        active_map_img = "Resources/mapbl.png";
                        ChangeWindowSize(580.0, 771.637);
                    }
                    else if (value == "BlueHome")
                    {
                        active_bl_title = "Blue Borderlands";
                        active_map_img = "Resources/mapbl.png";
                        ChangeWindowSize(580.0, 771.637);
                    }
                    else
                    {
                        active_bl_title = "Eternal Battlegrounds";
                        active_map_img = "Resources/mapeb.png";
                        ChangeWindowSize(600, 600);
                    }

                    _active_bl = value;
                    OnPropertyChanged();
                }
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

        public double width
        {
            get { return _width; }
            set
            {
                if (value != _width)
                {
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
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        public void ChangeWindowSize(double Width, double Height)
        {
            if (_width != Width)
                width = Width;
            if (_height != Height)
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

    // Matches
    public class WvwMatch_
    {
        public WvwMatch_()
        {
            Options = new Options_();
        }
        public Options_ Options { get; set; }
        public List<Matches> Match { get; set; }
        public List<World_Names_> World { get; set; }
        public Match_Details_ Details { get; set; }
        public List<WvwObjective> ObjectiveNames { get; set; }

        /*public Map DetailsByMap(string MapType)
        {
            for (int i = 0; i < Details.maps.Count; i++)
            {
                if (Details.maps[i].type == MapType)
                    //return Details.maps[i];
            }
            return new Map();
        }*/

        public Dictionary<string, string> getMatchesList()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (var x in Match)
            {
                ret.Add(x.wvw_match_id, string.Format("{0}. {1} vs {2} vs {3}", x.wvw_match_id, getServerName(x.red_world_id), getServerName(x.blue_world_id), getServerName(x.green_world_id)));
            }
            return ret.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        public string getServerName(string match_id, string color)
        { 
            foreach(var x in Match)
            {
                if (match_id == x.wvw_match_id)
                {
                    switch (color)
                    {
                        case "red": return getServerName(x.red_world_id); 
                        case "blue": return getServerName(x.blue_world_id); 
                        case "green": return getServerName(x.green_world_id); 
                    }
                }
            }
            return string.Format("MATCH_ID-{0}_NOT_FOUND", match_id);
        }

        public string getServerName(int ID)
        {
            foreach (var x in World)
            {
                if (ID == x.id)
                    return x.name;
            }
            return string.Format("SERVER_{0}_NOT_FOUND", ID);
        }
    }

    public class Matches_
    {
        public List<Matches> wvw_matches { get; set; }
    }

    public class Matches
    {
        public string wvw_match_id { get; set; }
        public int red_world_id { get; set; }
        public int blue_world_id { get; set; }
        public int green_world_id { get; set; }
    }

    // World Names
    public class World_Names_
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
