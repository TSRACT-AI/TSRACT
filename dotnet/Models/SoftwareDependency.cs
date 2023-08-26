namespace TSRACT.Models
{
    public class SoftwareDependency
    {
        public string Name { get; set; } = "";
        public bool IsPresent { get; set; } = false;
        public string RequiredVersion { get; set; } = "";
        public string InstalledVersion { get; set; } = "";
    }
}
