using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GWvW_Overlay.DataModel
{
    public class Guild
    {
        private const string JsonCacheFile = "Resources/guild_details.json";
        private readonly Dictionary<string, List<string>> GuildDict = new Dictionary<string, List<string>>();
        private Utils Utils = new Utils();

        public Guild()
        {
            // Load cached guild details
            if (File.Exists(JsonCacheFile))
            {
                var file =
                    JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(JsonCacheFile));
                if (file != null)
                {
                    GuildDict = file;
                }

            }

        }

        public List<string> GetGuildById(string id)
        {
            if (id == null)
                return null;

            if (!GuildDict.ContainsKey(id))
            {
                var data =
                    JsonConvert.DeserializeObject<Guild_Details_>(
                        Utils.GetJson(string.Format(@"https://api.guildwars2.com/v1/guild_details.json?guild_id={0}", id)));
                try
                {
                    GuildDict.Add(id, new List<string> { data.guild_name, data.tag });
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error " + "occurred: {0}", e);
                }
            }


            Save();
            return new List<string> { GuildDict[id][0], GuildDict[id][1] };
        }

        public void Save()
        {
            Utils.SaveJson(JsonCacheFile, JsonConvert.SerializeObject(GuildDict));
        }
    }
}
