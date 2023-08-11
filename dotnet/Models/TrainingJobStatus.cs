namespace TSRACT.Models
{
    public class TrainingJobStatus
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public float Loss { get; set; } = 0.0f;
        public float LearningRate { get; set; } = 0.0f;
        public float Epoch { get; set; } = 0.0f;
    }
}
