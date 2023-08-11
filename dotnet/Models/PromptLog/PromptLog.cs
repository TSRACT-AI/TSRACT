using Newtonsoft.Json;

namespace TSRACT.Models.PromptLog
{
    public class PromptLog
    {
        [JsonIgnore] public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public List<PromptLogEntry> Entries { get; set; } = new();
    }
}
