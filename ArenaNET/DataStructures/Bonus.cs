using System;
using Newtonsoft.Json;

namespace Arena.NET.DataStructures
{
    public class Bonus
    {
        [JsonProperty("type")]
        public String Type;

        [JsonProperty("owner")]
        public String Owner;
    }
}