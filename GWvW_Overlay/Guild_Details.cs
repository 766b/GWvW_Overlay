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
        Dictionary<string, List<string>> GuildDict = new Dictionary<string, List<string>>();

        private int _changeCounter = 0;
        private const string JsonCacheFile = "Resources/guild_details.json";

        public Guild()
        {
            // Load cached guild details
            if (File.Exists(JsonCacheFile))
                GuildDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(JsonCacheFile));
        }

        public List<string> GetGuildById(string id)
        {
            if (id == null)
                return null;

            if (!GuildDict.ContainsKey(id))
            {
                var data = JsonConvert.DeserializeObject<Guild_Details_>(Utils.GetJson(string.Format(@"https://api.guildwars2.com/v1/guild_details.json?guild_id={0}", id)));
                try
                {
                    GuildDict.Add(id, new List<string>() { data.guild_name, data.tag });
                }
                catch (Exception e)
                {
                    
                    Console.WriteLine("Error " + "occurred: {0}", e);
                }
                
                _changeCounter++;
            }

            if (_changeCounter > 3)
            {
                _changeCounter = 0;
                Save();
            }
            return new List<string>() { GuildDict[id][0], GuildDict[id][1] };
        }

        public void Save()
        {
            Utils.SaveJson(JsonCacheFile, JsonConvert.SerializeObject(GuildDict));
        }

    }

    
}
