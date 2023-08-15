using System.Text.Json.Serialization;

namespace TSRACT.Models.OpenAi
{
    public class ChatMessage
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("author")] public ChatMessageAuthor Author { get; set; } = new();
        
        [JsonPropertyName("create_time")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? CreateTime { get; set; } = null;
        
        [JsonPropertyName("update_time")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? UpdateTime { get; set; } = null;

        [JsonPropertyName("content")] public ChatMessageContent Content { get; set; } = new();
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("end_turn")] public bool? EndTurn { get; set; } = null;
        [JsonPropertyName("weight")] public float Weight { get; set; } = 0.0f;
        [JsonPropertyName("metadata")] public Dictionary<string, object> Metadata { get; set; } = new();
        [JsonPropertyName("recipient")] public string Recipient { get; set; } = "";
    }
}
