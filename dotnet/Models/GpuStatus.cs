namespace TSRACT.Models
{
    public class GpuStatus
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int Index { get; set; } = -1;
        public int GpuUtilization { get; set; } = -1;
        public int MemoryUtilization { get; set; } = -1;
        public int MemoryReserved { get; set; } = -1;
        public int MemoryUsed { get; set; } = -1;
        public int MemoryFree { get; set; } = -1;
        public float PowerDraw { get; set; } = -1;
        public int Temperature { get; set; } = -1;
    }
}
