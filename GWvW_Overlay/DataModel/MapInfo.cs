using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GWvW_Overlay.DataModel
{
    public class MapInfo
    {
        private readonly static Dictionary<int, MapInfo> MapCache = new Dictionary<int, MapInfo>();

        public int Id;
        public String Name;
        public int MinLevel;
        public int MaxLevel;
        public int DefaultFloor;
        public int[] Floors;
        public int RegionId;
        public String RegionName;
        public int ContinentId;
        public String ContinentName;
        public int[][] MapRect;
        public int[][] ContinentRect;

        public MapInfo()
        {
        }

        [JsonConstructor]
        public MapInfo(int id, string name, int min_level, int max_level, int default_floor, int[] floors, int region_id, string region_name, int continent_id, string continent_name, int[][] map_rect, int[][] continent_rect)
        {
            Id = id;
            Name = name;
            MinLevel = min_level;
            MaxLevel = max_level;
            DefaultFloor = default_floor;
            Floors = floors;
            RegionId = region_id;
            RegionName = region_name;
            ContinentId = continent_id;
            ContinentName = continent_name;
            MapRect = map_rect;
            ContinentRect = continent_rect;
        }

        public static MapInfo GetMapInfo(int id)
        {
            if (MapCache.ContainsKey(id))
            {
                return MapCache[id];
            }
            var result = JsonConvert.DeserializeObject<MapInfo>(
                Utils.GetJson(String.Format("https://api.guildwars2.com/v2/maps/{0}", id)));

            MapCache[id] = result;
            return result;
        }

    }
}
