using Newtonsoft.Json;

namespace ArenaNET.DataStructures
{
    public class ServerValue<T>
    {
        [JsonProperty("red")]
        public T Red;
        [JsonProperty("blue")]
        public T Blue;
        [JsonProperty("green")]
        public T Green;
    }
}