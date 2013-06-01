using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWvW_Overlay
{
    //Match Details
    public class Match_Details_
    {
        public string match_id { get; set; }
        public List<int> scores { get; set; }
        public List<Map> maps { get; set; }
    }

    public class Objective
    {
        public int id { get; set; }
        public string owner { get; set; }
        public string owner_guild { get; set; }
        public DateTime last_change { get; set; }
    }

    public class Map
    {
        public string type { get; set; }
        public List<int> scores { get; set; }
        public List<Objective> objectives { get; set; }
    }

    // Matches
    public class WvwMatch_
    {
        public List<Matches> Match { get; set; }
        public List<World_Names_> World { get; set; }
        public Match_Details_ Details { get; set; }

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
