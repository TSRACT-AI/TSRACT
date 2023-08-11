namespace TSRACT.Models
{
    public class LanguageModel : IdBase
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = ""; // This gets plugged into HF Transformers, can use disk path or HF Hub path
    }
}
