using System.Text.Json.Serialization;

namespace TSRACT.Models.OpenAi
{
    public class ChatMessageAuthor
    {
        [JsonPropertyName("role")] public string Role { get; set; } = "";
        [JsonPropertyName("name")] public string? Name { get; set; } = null;
        [JsonPropertyName("metadata")] public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
