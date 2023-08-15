using System.Text.Json.Serialization;

namespace TSRACT.Models.OpenAi
{
    public class ChatMapping
    {
        [JsonPropertyName("id")]  public string Id { get; set; } = "";
        [JsonPropertyName("message")] public ChatMessage? Message { get; set; } = null;
        [JsonPropertyName("parent")] public string? Parent { get; set; } = null;
        [JsonPropertyName("children")] public List<string> Children { get; set; } = new();
    }
}
