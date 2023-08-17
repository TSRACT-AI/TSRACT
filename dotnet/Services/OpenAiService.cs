using Azure;
using Azure.AI.OpenAI;
using System.Text.Json;
using TSRACT.Models.OpenAi;

namespace TSRACT.Services
{
    public class OpenAiService
    {
        private string _apiKey;
        private OpenAIClient? _openAiClient;

        public OpenAiService(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAiApiKey"];
            if (String.IsNullOrEmpty(_apiKey)) return;
            _openAiClient = new OpenAIClient(_apiKey, new OpenAIClientOptions());
        }

        public async IAsyncEnumerable<string> ChatStream(string model, ChatCompletionsOptions options)
        {
            if (_openAiClient == null) throw new InvalidOperationException("OpenAI Client not initialized (API key likely not set)");
            Response<StreamingChatCompletions> response = await _openAiClient.GetChatCompletionsStreamingAsync(deploymentOrModelName: model, options);
            using StreamingChatCompletions streamingChatCompletions = response.Value;

            await foreach (StreamingChatChoice choice in streamingChatCompletions.GetChoicesStreaming())
            {
                await foreach (ChatMessage message in choice.GetMessageStreaming())
                {
                    yield return message.Content;
                }
            }
        }

        public bool IsApiKeySet()
        {
            return !string.IsNullOrEmpty(_apiKey);
        }

        public List<ChatLogConversation> LoadFromPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty or null.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Specified file was not found.", filePath);

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<ChatLogConversation>>(json);
        }

        public async Task SetApiKey(string apiKey)
        {
            _apiKey = apiKey;
            _openAiClient = new OpenAIClient(_apiKey, new OpenAIClientOptions());

            // TODO: This is sloppy, should not be doing this from here!
            var json = File.ReadAllText("appsettings.json");
            var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(json);
            jsonObj["OpenAiApiKey"] = apiKey;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("appsettings.json", output);
        }
    }
}
