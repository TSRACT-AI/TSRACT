using System.Text.Json;
using System.Text.Json.Serialization;

namespace TSRACT
{
    public class UnixTimestampConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var unixTimestamp = reader.GetDouble();
            return DateTimeOffset.FromUnixTimeSeconds((long)unixTimestamp).DateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var unixTimestamp = ((DateTimeOffset)value).ToUnixTimeSeconds();
            writer.WriteNumberValue(unixTimestamp);
        }
    }
}
