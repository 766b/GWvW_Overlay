using System;
using Newtonsoft.Json;

namespace ArenaNET.DataStructures
{
    public class Bonus
    {
        [JsonProperty("type")]
        public String Type;

        [JsonProperty("owner")]
        public String Owner;
    }
}