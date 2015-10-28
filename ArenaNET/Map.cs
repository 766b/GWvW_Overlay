using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ArenaNET.DataStructures;
using Utils.Text;
using Newtonsoft.Json;

namespace ArenaNET
{
    public class Map : ANetResource<Map>, IANetResource
    {
        private const String CacheFile = "MapNames.json";

        #region IANetResource Implementation

        private static readonly String _endPoint = "maps";
        private static readonly String _parameterizedEndPoint = _endPoint + "/{0}?lang={1}";
        private static Dictionary<String, Map> _cache;

        public string EndPoint()
        {
            return _endPoint;
        }

        #endregion


        [JsonProperty("id")]
        public int Id;

        #region WvW Attributes

        [JsonProperty("type")]
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

        #endregion

        #region "Classic" Attributes
        [JsonProperty("name")]
        private String _name;
        [JsonProperty("min_level")]
        private int? _minLevel;
        [JsonProperty("max_level")]
        private int? _maxLevel;
        [JsonProperty("default_floor")]
        private int? _defaultFloor;
        [JsonProperty("floors")]
        private List<int> _floors;
        [JsonProperty("region_id")]
        private int? _regionId;
        [JsonProperty("region_name")]
        private String _regionName;
        [JsonProperty("continent_id")]
        private int? _continentId;
        [JsonProperty("continent_name")]
        private String _continentName;
        [JsonProperty("map_rect")]
        private double[][] _mapRect;
        [JsonProperty("continent_rect")]
        private double[][] _continentRect;

        #endregion

        #region Properties

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (Id != 0 && _name.IsEmpty())
                {
                    GetResource();
                }
                return _name;
            }
            private set { _name = value; }
        }

        [JsonIgnore]
        public int MinLevel
        {
            get
            {
                if (Id != 0 && !_minLevel.HasValue)
                {
                    GetResource();
                }
                return _minLevel ?? -1;
            }
            private set { _minLevel = value; }
        }

        [JsonIgnore]
        public int MaxLevel
        {
            get
            {
                if (Id != 0 && !_maxLevel.HasValue)
                {
                    GetResource();
                }
                return _maxLevel ?? -1;
            }
            private set { _maxLevel = value; }
        }

        [JsonIgnore]
        public int DefaultFloor
        {
            get
            {
                if (Id != 0 && !_defaultFloor.HasValue)
                {
                    GetResource();
                }
                return _defaultFloor ?? -1;
            }
            private set { _defaultFloor = value; }
        }

        [JsonIgnore]
        public List<int> Floors
        {
            get
            {
                if (Id != 0 && _floors == null || _floors.Count < 1)
                {
                    GetResource();
                }
                return _floors;
            }
            private set { _floors = value; }
        }

        [JsonIgnore]
        public int RegionId
        {
            get
            {
                if (Id != 0 && !_regionId.HasValue)
                {
                    GetResource();
                }
                return _regionId ?? -1;
            }
            private set { _regionId = value; }
        }

        [JsonIgnore]
        public string RegionName
        {
            get
            {
                if (Id != 0 && _regionName.IsEmpty())
                {
                    GetResource();
                }
                return _regionName;
            }
            private set { _regionName = value; }
        }

        [JsonIgnore]
        public int ContinentId
        {
            get
            {
                if (Id != 0 && !_continentId.HasValue)
                {
                    GetResource();
                }
                return _continentId ?? -1;
            }
            private set { _continentId = value; }
        }

        [JsonIgnore]
        public string ContinentName
        {
            get
            {
                if (Id != 0 && _continentName.IsEmpty())
                {
                    GetResource();
                }
                return _regionName;
            }
            private set { _continentName = value; }
        }

        [JsonIgnore]
        public Coordinate[] MapRect
        {
            get
            {
                if (Id != 0 && _mapRect == null)
                {
                    GetResource();
                }

                var coords = new Coordinate[_mapRect.Length];
                for (int i = 0; i < _mapRect.Length; i++)
                {
                    if (_mapRect[i].Length != 2) break;
                    coords[i] = new Coordinate()
                    {
                        X = _mapRect[i][0],
                        Y = _mapRect[i][1]
                    };
                }
                return coords;
            }
            private set
            {

                double[][] coords = new double[value.Length][];

                for (int i = 0; i < value.Length; i++)
                {
                    coords[i] = new[] { value[i].X, value[i].Y };
                }

                _mapRect = coords;
            }
        }

        [JsonIgnore]
        public Coordinate[] ContinentRect
        {
            get
            {
                if (Id != 0 && _continentRect == null)
                {
                    GetResource();
                }

                var coords = new Coordinate[_continentRect.Length];
                for (int i = 0; i < _continentRect.Length; i++)
                {
                    if (_continentRect[i].Length != 2) break;
                    coords[i] = new Coordinate()
                    {
                        X = _continentRect[i][0],
                        Y = _continentRect[i][1]
                    };
                }
                return coords;

            }
            private set
            {
                double[][] coords = new double[value.Length][];

                for (int i = 0; i < value.Length; i++)
                {
                    coords[i] = new[] { value[i].X, value[i].Y };
                }

                _continentRect = coords;
            }
        }

        #endregion

        public override Map GetResource(params String[] parameters)
        {
            if (_cache == null)
            {
                if (File.Exists(CacheFile))
                {
                    _cache =
                        JsonConvert.DeserializeObject<Dictionary<String, Map>>(
                            File.ReadAllText(CacheFile));
                }
                else
                {
                    _cache = new Dictionary<string, Map>();
                }
            }



            if (Id != 0) parameters = new[] { Id.ToString() };

            if (parameters.Length != 1) throw new ArgumentException("Should contain only 1 parameter");

            var endpoint = String.Format(_parameterizedEndPoint, parameters[0], Request.Lang);

            if (_cache.ContainsKey(endpoint))
            {
                var result = _cache[endpoint];
                Name = result._name;
                _minLevel = result._minLevel;
                _maxLevel = result._maxLevel;
                _defaultFloor = result._defaultFloor;
                _floors = result._floors;
                _regionId = result._regionId;
                _regionName = result._regionName;
                _continentId = result._continentId;
                _continentName = result._continentName;
                _mapRect = result._mapRect;
                _continentRect = result._continentRect;
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
