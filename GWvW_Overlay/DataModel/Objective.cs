using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GWvW_Overlay.DataModel
{
    public class Objective : INotifyPropertyChanged
    {
        public DateTime _last_change;
        private string _owner;
        private DateTime _ownerChange = DateTime.Now;
        private string _owner_guild;
        public string _time_left;

        public Objective()
        {
            ObjData = new WvwObjective();
        }

        public WvwObjective ObjData { get; set; }
        public int id { get; set; }

        public String ownedTime
        {
            get { return DateTime.Now.Subtract(_ownerChange).ToString("hh\\:mm\\:ss"); }
            set { OnPropertyChanged(); }
        }

        public String ownerName
        {
            get
            {
                if (_owner_guild != null)
                {
                    var guild = new Guild();
                    List<String> guildInfo = guild.GetGuildById(_owner_guild);
                    return String.Format("[{0}] {1}", guildInfo[1], guildInfo[0]);
                }
                return "";
            }
        }

        public string time_left
        {
            get { return _time_left; }
            set
            {
                if (value != _time_left)
                {
                    _time_left = value;
                    OnPropertyChanged();
                }
            }
        }

        public string owner_guild_icon
        {
            get
            {
                if (owner_guild != null)
                    return "Resources/claimed2.png";
                return "Resources/empty.png";
            }
        }

        public DateTime last_change
        {
            get { return _last_change; }
            set
            {
                if (value != _last_change)
                {
                    _last_change = value;
                    OnPropertyChanged();
                }
            }
        }

        public string owner_guild
        {
            get { return _owner_guild; }
            set
            {
                if (_owner_guild == value)
                    return;

                _owner_guild = value;
                OnPropertyChanged();
            }
        }

        public string owner
        {
            get { return _owner; }
            set
            {
                if (_owner == null)
                {
                    _owner = value;
                    OnPropertyChanged();
                }
                if (value != _owner && _owner != null)
                {
                    _owner = value;
                    _ownerChange = DateTime.Now;
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
}
