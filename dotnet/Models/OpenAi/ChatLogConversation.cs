using System.Text.Json.Serialization;

namespace TSRACT.Models.OpenAi
{
    public class ChatLogConversation
    {
        [JsonPropertyName("title")] public string Title { get; set; } = "";
        [JsonPropertyName("create_time")][JsonConverter(typeof(UnixTimestampConverter))] public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("update_time")][JsonConverter(typeof(UnixTimestampConverter))] public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("mapping")] public Dictionary<string, ChatLogMapping> Mapping { get; set; } = new();
    }

    public class ChatLogMapping
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("message")] public ChatLogMessage? Message { get; set; } = null;
        [JsonPropertyName("parent")] public string? Parent { get; set; } = null;
        [JsonPropertyName("children")] public List<string> Children { get; set; } = new();
    }

    public class ChatLogMessage
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("author")] public ChatLogMessageAuthor Author { get; set; } = new();
        [JsonPropertyName("create_time")][JsonConverter(typeof(UnixTimestampConverter))] public DateTime? CreateTime { get; set; } = null;
        [JsonPropertyName("update_time")][JsonConverter(typeof(UnixTimestampConverter))] public DateTime? UpdateTime { get; set; } = null;
        [JsonPropertyName("content")] public ChatLogMessageContent Content { get; set; } = new();
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("end_turn")] public bool? EndTurn { get; set; } = null;
        [JsonPropertyName("weight")] public float Weight { get; set; } = 0.0f;
        [JsonPropertyName("metadata")] public Dictionary<string, object> Metadata { get; set; } = new();
        [JsonPropertyName("recipient")] public string Recipient { get; set; } = "";
    }

    public class ChatLogMessageAuthor
    {
        [JsonPropertyName("role")] public string Role { get; set; } = "";
        [JsonPropertyName("name")] public string? Name { get; set; } = null;
        [JsonPropertyName("metadata")] public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ChatLogMessageContent
    {
        [JsonPropertyName("content_type")] public string ContentType { get; set; } = "";
        [JsonPropertyName("parts")] public List<string> Parts { get; set; } = new();
    }
}
