namespace TSRACT.Models
{
    public class GpuProfile
    {
        public int Index { get; set; } = -1;
        public string Name { get; set; } = "";
        public int MemoryTotal { get; set; } = 0;
        public float PowerLimit { get; set; } = 0;
        public GpuStatus CurrentStatus { get; set; } = new GpuStatus();
    }
}
