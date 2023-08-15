using System.Text.Json;
using TSRACT.Models.OpenAi;

namespace TSRACT.Services
{
    public class OpenAiService
    {
        public List<ChatConversation> LoadFromPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty or null.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Specified file was not found.", filePath);

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<ChatConversation>>(json);
        }
    }
}
