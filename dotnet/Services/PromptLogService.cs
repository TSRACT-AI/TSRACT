using TSRACT.Models.PromptLog;
using TSRACT.Repositories;

namespace TSRACT.Services
{
    public class PromptLogService
    {
        private readonly PromptLogRepository _promptLogRepository;
        private PromptLog? _promptLog = null;

        public PromptLogService(PromptLogRepository promptLogRepository)
        {
            _promptLogRepository = promptLogRepository;
        }

        public bool IsLoaded()
        {
            return _promptLog != null;
        }

        public async Task Load(string path)
        {
            _promptLog = await _promptLogRepository.Load(path);
        }
        
        public async Task Log(PromptLogEntry entry)
        {
            if (_promptLog == null) return;

            _promptLog.Entries.Add(entry);
            await _promptLogRepository.Save(_promptLog);
        }
    }
}
