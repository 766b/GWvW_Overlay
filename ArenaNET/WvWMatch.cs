using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using Arena.NET.DataStructures;
using Newtonsoft.Json;

namespace ArenaNET
{
    public class WvWMatch : ANetResource<WvWMatch>, IANetResource
    {
        #region IANetResource Implementation

        private static readonly String _endPoint = "wvw/matches";
        private static readonly String _parameterizedEndPoint = _endPoint + "/{0}";

        public string EndPoint()
        {
            return _endPoint;
        }

        #endregion

        [JsonProperty("id")]
        public String Id;
        [JsonProperty("start_time")]
        public DateTime StartTime;
        [JsonProperty("end_time")]
        public DateTime EndTime;
        [JsonProperty("scores")]
        public Dictionary<String, int> Scores;
        [JsonProperty("worlds")]
        public Dictionary<String, int> Worlds;
        [JsonProperty("maps")]
        public List<Map> Maps;


        public override WvWMatch GetResource(params String[] parameters)
        {
            if (parameters.Length != 1) throw new ArgumentException("Should contain only 1 parameter");

            try
            {
                String json;
                var response = GetJSON(String.Format(_parameterizedEndPoint, parameters[0]), out json);
                Console.WriteLine("Response HTTP Code : {0}", response);
                Console.WriteLine("Response : {0}", json);
                if (response == HttpStatusCode.OK)
                {
                    var match = JsonConvert.DeserializeObject<WvWMatch>(json);
                    return match;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;


        }


    }
}