using Ecowitt.Controller.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ecowitt.Controller
{
    public class SensorConverter : JsonConverter<ISensor>
    {
        public override void WriteJson(JsonWriter writer, ISensor? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            //// Create a new serializer without this converter to avoid recursion
            //var tempSerializer = new JsonSerializer
            //{
            //    NullValueHandling = serializer.NullValueHandling,
            //    DefaultValueHandling = serializer.DefaultValueHandling,
            //    DateFormatHandling = serializer.DateFormatHandling,
            //    DateTimeZoneHandling = serializer.DateTimeZoneHandling,
            //    FloatFormatHandling = serializer.FloatFormatHandling,
            //    Formatting = serializer.Formatting
            //};

            //// Copy all converters except this one
            //foreach (var converter in serializer.Converters)
            //{
            //    if (converter.GetType() != typeof(SensorConverter))
            //    {
            //        tempSerializer.Converters.Add(converter);
            //    }
            //}

            //// Serialize with the temp serializer to avoid recursion
            //var json = JObject.FromObject(value, tempSerializer);
            //json["$type"] = value.GetType().AssemblyQualifiedName;
            //json.WriteTo(writer);

            // Manually create JSON object to avoid recursion
            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(value.Name);

            writer.WritePropertyName("Alias");
            writer.WriteValue(value.Alias);

            writer.WritePropertyName("TimestampUtc");
            writer.WriteValue(value.TimestampUtc);

            writer.WritePropertyName("SensorType");
            writer.WriteValue((int)value.SensorType);

            writer.WritePropertyName("SensorState");
            writer.WriteValue((int)value.SensorState);

            writer.WritePropertyName("SensorClass");
            writer.WriteValue((int)value.SensorClass);

            writer.WritePropertyName("SensorCategory");
            writer.WriteValue((int)value.SensorCategory);

            writer.WritePropertyName("UnitOfMeasurement");
            writer.WriteValue(value.UnitOfMeasurement);

            writer.WritePropertyName("Value");
            serializer.Serialize(writer, value.Value);

            writer.WritePropertyName("DataType");
            writer.WriteValue(value.DataType.AssemblyQualifiedName);

            writer.WritePropertyName("DiscoveryUpdate");
            writer.WriteValue(value.DiscoveryUpdate);

            writer.WriteEndObject();
        }

        public override ISensor? ReadJson(JsonReader reader, Type objectType, ISensor? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var json = JObject.Load(reader);

            // Get the DataType to determine what Sensor<T> to create
            var dataTypeToken = json["DataType"];
            if (dataTypeToken == null)
                throw new JsonSerializationException("DataType property is required for ISensor deserialization");

            var dataTypeString = dataTypeToken.Value<string>();
            var dataType = Type.GetType(dataTypeString!);

            if (dataType == null)
                throw new JsonSerializationException($"Could not resolve type: {dataTypeString}");

            // Create the appropriate Sensor<T> type
            var sensorType = typeof(Sensor<>).MakeGenericType(dataType);

            // Remove the DataType from JSON since it's not part of the actual object
            json.Remove("DataType");
            json.Remove("HasChanged"); // Remove since it's JsonIgnore and computed

            return (ISensor)json.ToObject(sensorType, serializer)!;
        }

        public new bool CanConvert(Type objectType)
        {
            return objectType == typeof(ISensor) || objectType.IsAssignableFrom(typeof(ISensor));
        }
    }
}
