using System.Text.Json;
using TSRACT.Models.OpenAi;

namespace TSRACT.Services
{
    public class OpenAiService
    {
        private string _apiKey;
        
        public OpenAiService(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAiApiKey"];
        }
        
        public bool IsApiKeySet()
        {
            return !string.IsNullOrEmpty(_apiKey);
        }

        public List<ChatConversation> LoadFromPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty or null.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Specified file was not found.", filePath);

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<ChatConversation>>(json);
        }

        public async Task SetApiKey(string apiKey)
        {
            _apiKey = apiKey;

            // TODO: This is sloppy, should not be doing this from here!
            var json = File.ReadAllText("appsettings.json");
            var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(json);
            jsonObj["OpenAiApiKey"] = apiKey;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("appsettings.json", output);
        }
    }
}
