using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ArenaNET.DataStructures;
using Newtonsoft.Json;

namespace ArenaNET
{
    public class Objective : ANetResource<Objective>, IANetResource
    {
        private const String CacheFile = "ObjectiveNames.json";
        private static readonly String _endPoint = "wvw/objectives";
        private static readonly String _parameterizedEndPoint = _endPoint + "/{0}?lang={1}";
        private static Dictionary<String, Objective> _cache;

        #region Properties
        [JsonIgnore]
        public string Name
        {
            get
            {
                if (!String.IsNullOrEmpty(Id))
                {
                    GetResource();
                }

                return _name;

            }
            private set { _name = value; }
        }

        [JsonIgnore]
        public int SectorId
        {
            get
            {
                if (!_sectorId.HasValue && !String.IsNullOrEmpty(Id))
                {
                    GetResource();
                }
                return _sectorId ?? -1;
            }
            private set { _sectorId = value; }
        }

        [JsonIgnore]
        public string MapType
        {
            get
            {
                if (String.IsNullOrEmpty(_mapType) && !String.IsNullOrEmpty(Id))
                {
                    GetResource();
                }

                return _mapType;

            }
            private set { _mapType = value; }
        }

        [JsonIgnore]
        public string MapId
        {
            get
            {
                if (String.IsNullOrEmpty(_mapId) && !String.IsNullOrEmpty(Id))
                {
                    GetResource();
                }
                return _mapId;

            }
            private set { _mapId = value; }
        }

        [JsonIgnore]
        public Coordinate Coordinates
        {
            get
            {
                if (_coordinates == null && !String.IsNullOrEmpty(Id))
                {
                    GetResource();
                }

                return _coordinates;

            }
            private set { _coordinates = value; }
        }

        public string EndPoint()
        {
            return _endPoint;
        }

        #endregion

        #region Serialization Preferences
        public bool ShouldSerializeOwner()
        {
            return false;
        }

        public bool ShouldSerializeLastFlipped()
        {
            return false;
        }

        public bool ShouldSerializeClaimedBy()
        {
            return false;
        }

        public bool ShouldSerializeClaimedAt()
        {
            return false;
        }
        #endregion


        [JsonProperty("id")]
        public string Id;
        [JsonProperty("type")]
        public String Type;
        [JsonProperty("owner")]
        public String Owner;
        [JsonProperty("last_flipped")]
        public DateTime? LastFlipped;
        [JsonProperty("claimed_by")]
        public String ClaimedBy;
        [JsonProperty("claimed_at")]
        public DateTime? ClaimedAt;
        [JsonProperty("name")]
        private String _name;
        [JsonProperty("sector_id")]
        private int? _sectorId;
        [JsonProperty("map_type")]
        private String _mapType;
        [JsonProperty("map_id")]
        private String _mapId;
        [JsonProperty("coord")]
        [JsonConverter(typeof(CoordinatesConverter))]
        private Coordinate _coordinates;

        public override Objective GetResource(params String[] parameters)
        {
            if (_cache == null)
            {
                if (File.Exists(CacheFile))
                {
                    _cache =
                        JsonConvert.DeserializeObject<Dictionary<String, Objective>>(
                            File.ReadAllText(CacheFile));
                }
                else
                {
                    _cache = new Dictionary<string, Objective>();
                }
            }



            if (Id != null) parameters = new[] { Id };

            if (parameters.Length != 1) throw new ArgumentException("Should contain only 1 parameter");

            var endpoint = String.Format(_parameterizedEndPoint, parameters[0], Request.Lang);

            if (_cache.ContainsKey(endpoint))
            {
                var result = _cache[endpoint];
                Name = result._name;
                SectorId = result._sectorId ?? -1;
                MapType = result._mapType;
                MapId = result._mapId;
                Coordinates = result._coordinates;
                return this;
            }

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


}