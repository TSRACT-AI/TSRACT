using TSRACT.Models;
using TSRACT.PythonScripts;
using TSRACT.Repositories;

namespace TSRACT.Services
{
    public class LanguageModelService
    {
        private readonly IdBaseRepository<LanguageModel> _languageModelRepository;

        public LanguageModelService(IdBaseRepository<LanguageModel> languageModelRepository)
        {
            _languageModelRepository = languageModelRepository;
        }

        public async Task<List<LanguageModel>> GetAll()
        {
            return (await _languageModelRepository.GetAll()).OrderBy(x => x.Path).ToList();
        }

        public async Task<LanguageModel> GetById(Guid id)
        {
            return await _languageModelRepository.GetById(id);
        }

        public async Task<LanguageModel?> GetByModelName(string modelName)
        {
            return (await _languageModelRepository.GetAll()).Where(x => x.Path == modelName).FirstOrDefault();
        }

        public async Task<LanguageModel> Save(LanguageModel languageModel)
        {
            return await _languageModelRepository.Save(languageModel);
        }

        public async Task ScanHfCache()
        {
            ScanHfCache scanHfCache = new ScanHfCache();
            Console.WriteLine("Scanning Huggingface Cache...");
            try
            {
                foreach (var cacheItem in await scanHfCache.GetHfCacheItems())
                {
                    LanguageModel? model = await GetByModelName(cacheItem.RepoId);
                    if (model == null)
                    {
                        await _languageModelRepository.Save(new LanguageModel()
                        {
                            Name = cacheItem.RepoId,
                            Path = cacheItem.RepoId
                        });
                        Console.WriteLine($"Added {cacheItem.RepoId} to model database.");
                    }
                }
                Console.WriteLine("Finished scanning Huggingface Cache.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to scan Huggingface Cache: {ex.Message}");
            }
        }
    }
}
