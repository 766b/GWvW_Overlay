using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ArenaNET;
using GWvW_Overlay.Annotations;
using GWvW_Overlay.Properties;

namespace GWvW_Overlay.DataModel
{
    public class WvwMatchup : INotifyPropertyChanged
    {
        public List<WvWMatch> Matches { get; set; }


        private List<World> _worlds = new List<World>();
        public List<World> Worlds
        {
            get
            {
                if (_worlds.Count == 0)
                {
                    _worlds = Request.GetResourceBulk<World>("all");
                }
                return _worlds;
            }
        }

        private readonly Positions _positions = new Positions();
        public Positions PlayerPositions
        {
            get { return _positions; }
        }

        public WvWMatch Details
        {
            get { return _details; }
            set
            {
                _details = value;
                OnPropertyChanged();
            }
        }

        public Options_ Options { get; private set; }

        private Visibility _markersVisibility = Visibility.Hidden;
        private WvWMatch _details;

        public Visibility MarkersVisibility
        {
            get
            {
                return _markersVisibility;
            }
            set
            {
                _markersVisibility = value;
                OnPropertyChanged();
            }
        }

        public WvwMatchup()
        {
            Options = new Options_();

        }


        public string HomeServerColor
        {
            get { return Options.HomeServerColor ?? (Options.HomeServerColor = GetServerColor()); }
        }

        public string GetServerColor()
        {
            foreach (WvWMatch x in Matches)
            {
                if (Settings.Default.home_server == x.Worlds.Red.Id) // && x.red_world_id == id)
                    return "red";
                if (Settings.Default.home_server == x.Worlds.Green.Id) // && x.green_world_id == id)
                    return "green";
                if (Settings.Default.home_server == x.Worlds.Blue.Id) // && x.blue_world_id == id)
                    return "blue";
            }
            return "white";
        }


        public String GetServerName(string color)
        {
            switch (color)
            {
                case "Red":
                    if (Details != null)
                    {
                        return Details.Worlds.Red.Name;
                    }
                    break;
                case "Blue":
                    if (Details != null)
                    {
                        return Details.Worlds.Blue.Name;
                    }
                    break;
                case "Green":
                    if (Details != null)
                    {
                        return Details.Worlds.Green.Name;
                    }
                    break;
                default:
                    return "";
            }

            return "";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void InitBlid()
        {
            for (int i = 0; i < Matches.First().Maps.Count; i++)
            {
                Options.blid.Add(Matches.First().Maps[i].Type, i);
            }
        }
    }

}
