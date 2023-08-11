namespace TSRACT.Models
{
    public class JobStatus
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int CurrentStep { get; set; } = 0;
        public int TotalSteps { get; set; } = 0;
        public string ElapsedTime { get; set; } = "";
        public string RemainingTime { get; set; } = "";
        public float SecondsPerStep { get; set; } = 0.0f;
    }
}
