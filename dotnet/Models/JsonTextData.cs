using System.Text.Json.Serialization;

namespace TSRACT.Models
{
    public class JsonTextData
    {
        [JsonPropertyName("text")] public string Text { get; set; } = "";
    }
}
