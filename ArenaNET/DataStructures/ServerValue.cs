using Newtonsoft.Json;

namespace Arena.NET.DataStructures
{
    public struct ServerValue<T>
    {
        [JsonProperty("red")]
        public T Red;
        [JsonProperty("blue")]
        public T Blue;
        [JsonProperty("green")]
        public T Green;
    }
}