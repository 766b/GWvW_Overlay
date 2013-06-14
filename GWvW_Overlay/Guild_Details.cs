using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace GWvW_Overlay
{
    /* Sample https://api.guildwars2.com/v1/guild_details.json?guild_id=F4CFE0DD-69BF-47DE-8CAA-12E712EE6430
     * {
     *  "guild_id":"F4CFE0DD-69BF-47DE-8CAA-12E712EE6430",
     *  "guild_name":"Pain Train Choo",
     *  "tag":"Choo",
     *  "emblem":{"background_id":5,"foreground_id":107,"flags":[],"background_color_id":473,"foreground_primary_color_id":64,"foreground_secondary_color_id":473}
     *  }
     */

    public class Guild_Details_
    {
        public string guild_id { get; set; }
        public string guild_name { get; set; }
        public string tag { get; set; }
        //public Emblem emblem { get; set; } /* Skip */
    }

    public class Guild
    {
        Utils Utils = new Utils();
        Guild_Details_ Guild_Details = new Guild_Details_();
        Dictionary<string, List<string>> GuildDict = new Dictionary<string, List<string>>();

        private int changeCounter = 0;
        private string jsonCacheFile = "Resources/guild_details.json";

        public Guild()
        {
            // Load cached guild details
            if (File.Exists(jsonCacheFile))
                GuildDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(jsonCacheFile));
        }

        public List<string> getGuildByID(string ID)
        {
            if (ID == null)
                return null;

            if (!GuildDict.ContainsKey(ID))
            {
                var Data = JsonConvert.DeserializeObject<Guild_Details_>(Utils.getJSON(string.Format(@"https://api.guildwars2.com/v1/guild_details.json?guild_id={0}", ID)));
                GuildDict.Add(ID, new List<string>() { Data.guild_name, Data.tag });
                changeCounter++;
            }

            if (changeCounter > 3)
            {
                changeCounter = 0;
                Save();
            }
            return new List<string>() { GuildDict[ID][0], GuildDict[ID][1] };
        }

        public void Save()
        {
            Utils.saveJson(jsonCacheFile, JsonConvert.SerializeObject(GuildDict));
        }

    }

    
}
