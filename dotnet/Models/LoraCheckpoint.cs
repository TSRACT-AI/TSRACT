namespace TSRACT.Models
{
    public class LoraCheckpoint
    {
        public int Step { get; set; } = 0;
        public bool IsFinal { get; set; } = false;
        public string DirectoryPath { get; set; } = "";
        public List<string> FilePaths { get; set; } = new();
    }
}
