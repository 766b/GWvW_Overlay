using System;
using System.Collections.Generic;
using System.Net;
using ArenaNET.DataStructures;
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
        public String Id { get; set; }
        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }
        [JsonProperty("end_time")]
        public DateTime EndTime { get; set; }
        [JsonProperty("scores")]
        public ServerValue<int> Scores { get; set; }
        [JsonProperty("worlds")]
        [JsonConverter(typeof(ServerWorldConverter))]
        public ServerValue<World> Worlds { get; set; }
        [JsonProperty("maps")]
        public List<Map> Maps { get; set; }

        public override WvWMatch GetResource(params String[] parameters)
        {
            if (!String.IsNullOrEmpty(Id))
            {
                parameters = new[] { Id };
            }

            if (parameters.Length != 1) throw new ArgumentException("Should contain only 1 parameter");

            try
            {
                String json;
                var response = GetJSON(String.Format(_parameterizedEndPoint, parameters[0]), out json);
#if DEBUG2
                Console.WriteLine("Response HTTP Code : {0}", response);
                Console.WriteLine("Response : {0}", json);
#endif
                if (response == HttpStatusCode.OK)
                {
                    JsonConvert.PopulateObject(json, this);
                    return this;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;


        }

        public override List<WvWMatch> GetResourceBulk(params string[] parameters)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return String.Format("{0}. {3} vs {2} vs {1}", Id,
                                                           Worlds.Red.Name,
                                                           Worlds.Blue.Name,
                                                           Worlds.Green.Name);
        }
    }
}