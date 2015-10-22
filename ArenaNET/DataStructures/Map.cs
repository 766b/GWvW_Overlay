using System;
using System.Collections.Generic;
using ArenaNET;
using Newtonsoft.Json;

namespace Arena.NET.DataStructures
{
    public class Map
    {
        [JsonProperty("id")]
        public int Id;
        public String Type;
        [JsonProperty("scores")]
        public ServerValue<int> Scores;
        [JsonProperty("bonuses")]
        public List<Bonus> Bonuses;
        [JsonProperty("objectives")]
        public List<Objective> Objectives;
        [JsonProperty("deaths")]
        public ServerValue<int> Deaths;
        [JsonProperty("kills")]
        public ServerValue<int> Kills;
    }
}
