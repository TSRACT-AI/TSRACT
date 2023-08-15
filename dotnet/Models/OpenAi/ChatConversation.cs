using System.Text.Json.Serialization;

namespace TSRACT.Models.OpenAi
{
    public class ChatConversation
    {
        [JsonPropertyName("title")] public string Title { get; set; } = "";
        
        [JsonPropertyName("create_time")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("update_time")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("mapping")] public Dictionary<string, ChatMapping> Mapping { get; set; } = new();
    }
}
