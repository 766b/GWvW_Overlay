using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace ArenaNET
{
    public class World : ANetResource<World>, IANetResource
    {
        private const String CacheFile = "ServerNames.json";
        #region IANetResource Implementation

        private static readonly String _endPoint = "worlds";
        private static readonly String _parameterizedEndPoint = _endPoint + "/{0}?lang={1}";
        private static Dictionary<String, World> _cache;


        public string EndPoint()
        {
            return _endPoint;
        }

        #endregion

        [JsonProperty("id")]
        public int Id;
        [JsonProperty("name")]
        public String Name;
        [JsonProperty("population")]
        public String Population;

        public override World GetResource(params String[] parameters)
        {
            var endpoint = String.Format(_parameterizedEndPoint, parameters[0], Request.Lang);
            if (_cache == null)
            {
                if (File.Exists(CacheFile))
                {
                    _cache =
                        JsonConvert.DeserializeObject<Dictionary<String, World>>(
                            File.ReadAllText(CacheFile));
                }
                else
                {
                    _cache = new Dictionary<string, World>();
                }
            }


            if (_cache.ContainsKey(endpoint))
            {
                var result = _cache[endpoint];
                Name = result.Name;
                Id = result.Id;
                Population = result.Population;
                return this;
            }

            if (parameters.Length != 1) throw new ArgumentException("Should contain only 1 parameter");

            try
            {
                String json;
                var response = GetJSON(endpoint, out json);
                Console.WriteLine("Response HTTP Code : {0}", response);
                Console.WriteLine("Response : {0}", json);
                if (response == HttpStatusCode.OK)
                {
                    var world = JsonConvert.DeserializeObject<World>(json);
                    Id = world.Id;
                    Name = world.Name;
                    Population = world.Population;


                    _cache.Add(endpoint, this);

                    File.WriteAllText(CacheFile, JsonConvert.SerializeObject(_cache));

                    return world;
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