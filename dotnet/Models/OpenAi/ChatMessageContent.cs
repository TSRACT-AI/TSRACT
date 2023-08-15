using System.Text.Json.Serialization;

namespace TSRACT.Models.OpenAi
{
    public class ChatMessageContent
    {
        [JsonPropertyName("content_type")] public string ContentType { get; set; } = "";
        [JsonPropertyName("parts")] public List<string> Parts { get; set; } = new();
    }
}
