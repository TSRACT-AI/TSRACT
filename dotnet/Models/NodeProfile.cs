namespace TSRACT.Models
{
    public class NodeProfile
    {
        public string Name { get; set; } = "TSRACT";
        public string IpAddress { get; set; } = "";
        public List<GpuProfile> Gpus { get; set; } = new();
        public SoftwareDependency PythonRuntime { get; set; } = new();
        public List<SoftwareDependency> PythonModules { get; set; } = new();
    }
}
