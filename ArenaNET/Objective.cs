using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Arena.NET;
using Arena.NET.DataStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ArenaNET
{
    public class Objective : ANetResource<Objective>, IANetResource
    {
        private const String CacheFile = "ObjectiveNames.json";
        private static readonly String _endPoint = "wvw/objectives";
        private static readonly String _parameterizedEndPoint = _endPoint + "/{0}?lang={1}";
        private static Dictionary<String, Objective> _cache;

        #region Properties

        public string Name
        {
            get
            {
                if (String.IsNullOrEmpty(_name) && !String.IsNullOrEmpty(Id))
                {
                    GetResource();
                }

                return _name;

            }
            private set { _name = value; }
        }

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

            if (_cache.ContainsKey(parameters[0]))
            {
                var result = _cache[parameters[0]];
                Name = result.Name;
                SectorId = result.SectorId;
                MapType = result.MapType;
                MapId = result.MapId;
                Coordinates = result.Coordinates;
                return this;
            }

            try
            {
                String json;
                var response = GetJSON(String.Format(_parameterizedEndPoint, parameters[0], Request.Lang), out json);
                Console.WriteLine("Response HTTP Code : {0}", response);
                Console.WriteLine("Response : {0}", json);
                if (response == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<Objective>(json);

                    Name = result.Name;
                    SectorId = result.SectorId;
                    MapType = result.MapType;
                    MapId = result.MapId;
                    Coordinates = result.Coordinates;

                    _cache.Add(parameters[0], this);

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