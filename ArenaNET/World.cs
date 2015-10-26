using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ArenaNET.DataStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArenaNET
{
    public class World : ANetResource<World>, IANetResource
    {
        private const String CacheFile = "ServerNames.json";

        #region IANetResource Implementation

        private static readonly String _endPoint = "worlds";
        private static readonly String _parameterizedEndPoint = _endPoint + "/{0}?lang={1}";
        private static Dictionary<String, World> _cache;

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (Id != 0)
                {
                    GetResource(Id.ToString());
                }
                return _name;
            }
            private set { _name = value; }
        }

        [JsonIgnore]
        public string Population
        {
            get
            {
                if (Id != 0)
                {
                    GetResource(Id.ToString());
                }
                return _population;
            }
            private set { _population = value; }
        }


        public string EndPoint()
        {
            return _endPoint;
        }

        #endregion

        [JsonProperty("id")]
        public int Id;
        [JsonProperty("name")]
        private String _name;
        [JsonProperty("population")]
        private String _population;

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
                _name = result._name;
                Id = result.Id;
                _population = result._population;
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
                    JsonConvert.PopulateObject(json, this);

                    _cache.Add(endpoint, this);

                    File.WriteAllText(CacheFile, JsonConvert.SerializeObject(_cache));

                    return this;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;


        }

    }

    internal class ServerWorldConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var worlds = new ServerValue<World>();
            for (int i = 0; i < 3; i++)
            {
                reader.Read();
                var color = Convert.ToString(reader.Value);
                reader.Read();
                var id = Convert.ToInt32(reader.Value);
                AssociateColorToValue(worlds, color, id);

            }
            reader.Read();
            return worlds;
        }

        private void AssociateColorToValue(ServerValue<World> worlds, string color, int id)
        {
            switch (color)
            {
                case "red":
                    worlds.Red = new World { Id = id };
                    break;
                case "blue":
                    worlds.Blue = new World { Id = id };
                    break;
                case "green":
                    worlds.Green = new World { Id = id };
                    break;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }
    }
}