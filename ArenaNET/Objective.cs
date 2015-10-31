using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using ArenaNET.DataStructures;
using Newtonsoft.Json;

namespace ArenaNET
{
    public class Objective : ANetResource<Objective>, IANetResource, INotifyPropertyChanged
    {
        public Objective()
        {
            DisplaySizePropertyChanged += SetDisplayCoordinates;
        }

        private const String CacheFile = "ObjectiveNames.json";
        private static readonly TimeSpan ProtectionTimeSpan = TimeSpan.FromMinutes(5);
        private static readonly String _endPoint = "wvw/objectives";
        private static readonly String _parameterizedEndPoint = _endPoint + "/{0}?lang={1}";
        private static Dictionary<String, Objective> _cache;
        private static Double _displayWidth = 0.0;
        private static Double _displayHeight = 0.0;
        private Map _map;

        private void SetDisplayCoordinates(object sender, PropertyChangedEventArgs args)
        {
            if (_map == null) return;
            var mapSize = new Coordinate()
            {
                X = Math.Abs(Math.Abs(_map.ContinentRect[1].X) - Math.Abs(_map.ContinentRect[0].X)),
                Y = Math.Abs(Math.Abs(_map.ContinentRect[1].Y) - Math.Abs(_map.ContinentRect[0].Y))
            };

            _displayCoordinates.X = _displayWidth * (Coordinates.X - _map.ContinentRect[0].X) / mapSize.X + 30;
            _displayCoordinates.Y = _displayHeight * (Coordinates.Y - _map.ContinentRect[0].Y) / mapSize.Y - 14;
            OnPropertyChanged("DisplayCoordinates");
        }

        #region Properties
        [JsonIgnore]
        public static double DisplayWidth
        {
            get { return _displayWidth; }
            set
            {
                _displayWidth = value;

                if (DisplaySizePropertyChanged != null)
                {
                    DisplaySizePropertyChanged(null, null);
                }
            }
        }
        [JsonIgnore]
        public static double DisplayHeight
        {
            get { return _displayHeight; }
            set
            {
                _displayHeight = value;
                if (DisplaySizePropertyChanged != null)
                {
                    DisplaySizePropertyChanged(null, null);
                }
            }
        }


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
            private set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
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
        public int MapId
        {
            get
            {
                if (!_mapId.HasValue && !String.IsNullOrEmpty(Id))
                {
                    GetResource();
                }
                return _mapId ?? -1;

            }
            private set
            {
                _map = Request.GetResource<Map>(value.ToString());
                _mapId = value;

            }
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
            private set
            {
                _coordinates = value;
                if (DisplaySizePropertyChanged != null)
                {
                    DisplaySizePropertyChanged(null, null);
                }
            }
        }

        [JsonProperty("coord")]
        [JsonConverter(typeof(CoordinatesConverter))]
        private Coordinate JsonCoordinateAccessor
        {
            get { return _coordinates; }
            set { Coordinates = value; }
        }


        public string EndPoint()
        {
            return _endPoint;
        }


        [JsonIgnore]
        public TimeSpan TimeLeft
        {
            get
            {
                if (LastFlipped.HasValue)
                {
                    TimeSpan span = DateTime.UtcNow - LastFlipped.Value;
                    if (span < ProtectionTimeSpan)
                    {
                        return ProtectionTimeSpan - span;
                    }
                }
                return new TimeSpan(0);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Coordinate DisplayCoordinates
        {
            get { return _displayCoordinates; }
            set
            {
                _displayCoordinates = value;
                OnPropertyChanged();
            }
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
        public string Id { get; set; }
        [JsonProperty("type")]
        public String Type { get; set; }

        [JsonProperty("owner")]
        public String Owner
        {
            get { return _owner; }
            set
            {
                if (_owner == value) return;
                _owner = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("last_flipped")]
        public DateTime? LastFlipped
        {
            get
            {
                return _lastFlipped;

            }
            set
            {
                if (_lastFlipped == value) return;
                _lastFlipped = value;
                OnPropertyChanged("TimeLeft");
                OnPropertyChanged();
            }
        }

        [JsonProperty("claimed_by")]
        public String ClaimedBy
        {
            get { return _claimedBy; }
            set
            {
                if (_claimedBy == value) return;
                _claimedBy = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("claimed_at")]
        public DateTime? ClaimedAt { get; set; }

        [JsonProperty("name")]
        private String _name;
        [JsonProperty("sector_id")]
        private int? _sectorId;
        [JsonProperty("map_type")]
        private String _mapType;
        [JsonProperty("map_id")]
        private int? _mapId;
        private Coordinate _coordinates;

        private Coordinate _displayCoordinates = new Coordinate();
        private DateTime? _lastFlipped;
        private string _owner;
        private string _claimedBy;

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
                MapId = result._mapId ?? -1;
                Coordinates = result._coordinates;
                return this;
            }

            try
            {
                String json;
                var response = GetJSON(endpoint, out json);
#if DEBUG2
                Console.WriteLine("Response HTTP Code : {0}", response);
                Console.WriteLine("Response : {0}", json);
#endif
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

        public override List<Objective> GetResourceBulk(params string[] parameters)
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private static event PropertyChangedEventHandler DisplaySizePropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return String.Format("{0}. {1} ({2}) : {3}", Id, Name, _map.Name, Owner);
        }
    }


}