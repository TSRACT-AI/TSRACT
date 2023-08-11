namespace TSRACT.Models.PromptLog
{
    public class PromptLogEntry
    {
        public string BaseModel { get; set; } = "";
        public string Lora { get; set; } = "";
        public string PromptText { get; set; } = "";
        public List<int> ResponseTokens { get; set; } = new();
        public string ResponseText { get; set; } = "";
        public List<(int, int, double)> TokenWatchList { get; set; } = new(); // (token, rank, probability)
        public bool Flag { get; set; } = false;
        public string Note { get; set; } = "";
    }
}
