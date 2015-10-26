using System;
using System.Collections;
using System.Collections.Generic;
using ArenaNET;
using GWvW_Overlay.Properties;

namespace GWvW_Overlay.DataModel
{
    public class WvwMatchup
    {
        public List<WvWMatch> Matches { get; set; }

        public WvWMatch Details { get; set; }

        public Options_ Options { get; private set; }

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
    }

}
