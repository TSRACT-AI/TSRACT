using Newtonsoft.Json;
using System.Collections.Concurrent;
using TSRACT.Models;

namespace TSRACT.Repositories
{
    public class IdBaseRepository<T> where T : IdBase
    {
        private readonly string _dbPath;
        private readonly ConcurrentDictionary<Guid, T> _cache;

        public IdBaseRepository()
        {
            string folderPath = "db";
            Directory.CreateDirectory(folderPath);
            _dbPath = Path.Combine(folderPath, typeof(T).Name + ".json");

            // Load the items into the cache when the repository is created
            _cache = new ConcurrentDictionary<Guid, T>(LoadItems().Result.ToDictionary(item => item.Id));
        }

        public async Task Delete(Guid id)
        {
            // Remove the item from the cache and save the updated cache to disk
            _cache.TryRemove(id, out _);
            await SaveItems();
        }

        public async Task<List<T>> GetAll()
        {
            // Return the items in the cache
            return _cache.Values.ToList();
        }

        public async Task<T> GetById(Guid id)
        {
            // Try to get the item from the cache
            _cache.TryGetValue(id, out T item);
            return item;
        }

        public async Task<T> Save(T item)
        {
            // Add or update the item in the cache, then save the updated cache to disk
            _cache[item.Id] = item;
            await SaveItems();
            return item;
        }

        private async Task<List<T>> LoadItems()
        {
            if (!File.Exists(_dbPath))
            {
                return new List<T>();
            }

            try
            {
                string json = File.ReadAllText(_dbPath);
                return JsonConvert.DeserializeObject<List<T>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task SaveItems()
        {
            var json = JsonConvert.SerializeObject(_cache.Values.ToList());
            await File.WriteAllTextAsync(_dbPath, json);
        }
    }
}
