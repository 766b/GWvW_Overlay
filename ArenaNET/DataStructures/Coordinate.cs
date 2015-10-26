using System;
using System.CodeDom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArenaNET.DataStructures
{
    public class Coordinate
    {
        public double X, Y;

    }

    internal class CoordinatesConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(((Coordinate)value).X);
            writer.WriteValue(((Coordinate)value).Y);
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var coords = new Coordinate();
            reader.Read();
            var obj = serializer.Deserialize<JValue>(reader);
            coords.X = Convert.ToDouble(obj.Value);
            reader.Read();
            obj = serializer.Deserialize<JValue>(reader);
            coords.Y = Convert.ToDouble(obj.Value);

            reader.Read();

            return coords;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Coordinate);
        }
    }
}