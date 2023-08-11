using Newtonsoft.Json;
using TSRACT.Models.PromptLog;

namespace TSRACT.Repositories
{
    public class PromptLogRepository
    {
        public PromptLogRepository()
        {
            // It seems we don't need to load any specific thing in the constructor as we're dealing with paths.
        }

        public async Task<PromptLog> Load(string path)
        {
            if (!File.Exists(path))
            {
                PromptLog newLog = new PromptLog() { Path = path };
                await Save(newLog);
                return newLog;
            }

            try
            {
                string json = await File.ReadAllTextAsync(path);
                var log = JsonConvert.DeserializeObject<PromptLog>(json);
                log.Path = path;
                return log;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load prompt log from {path}. Error: {ex.Message}");
                throw;
            }
        }

        public async Task Save(PromptLog log)
        {
            if (string.IsNullOrEmpty(log.Path))
            {
                throw new InvalidOperationException("PromptLog path cannot be empty or null.");
            }

            // Ensure the directory exists
            var directory = System.IO.Path.GetDirectoryName(log.Path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                var json = JsonConvert.SerializeObject(log);
                await File.WriteAllTextAsync(log.Path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save prompt log to {log.Path}. Error: {ex.Message}");
                throw;
            }
        }
    }
}
