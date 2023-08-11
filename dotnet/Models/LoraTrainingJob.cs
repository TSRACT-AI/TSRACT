namespace TSRACT.Models
{
    public class LoraTrainingJob : IdBase
    {
        public bool IsComplete { get; set; }
        public string ModelPath { get; set; } = "openlm-research/open_llama_7b";
        public string DatasetPath { get; set; } = "./data/demo.json";
        public string OutputDir { get; set; } = "./loras/open_llama_7b_demo";
        public bool BnbLoadIn4Bit { get; set; } = true;
        public bool BnbLoadIn8Bit { get; set; } = false;
        public bool Bnb4BitUseDoubleQuant { get; set; } = true;
        public string Bnb4BitQuantType { get; set; } = "nf4";
        public int LoraR { get; set; } = 32;
        public int LoraAlpha { get; set; } = 64;
        public List<string> LoraTargetModules { get; set; } = new() { "q_proj", "k_proj", "v_proj" };
        public float LoraDropout { get; set; } = 0.05f;
        public string LoraBias { get; set; } = "none";
        public string LoraTaskType { get; set; } = "CAUSAL_LM";
        public int PerGpuTrainBatchSize { get; set; } = 1;
        public int GradientAccumulationSteps { get; set; } = 4;
        public float WarmupRatio { get; set; } = 0.1f;
        public int NumTrainEpochs { get; set; } = 3;
        public double LearningRate { get; set; } = 2e-5;
        public bool Fp16 { get; set; } = true;
        public string Optimizer { get; set; } = "paged_adamw_8bit";
        public string ReportTo { get; set; } = "none"; // See https://huggingface.co/docs/transformers/main_classes/trainer#transformers.TrainingArguments.report_to
        public bool AutoUploadInProgress { get; set; } = false; // Automatically upload checkpoints that are saved during training
        public bool AutoUploadFinal { get; set; } = false; // Automatically upload the final checkpoint after training
        public string ScriptStatus { get; set; } = "";
        public string Stdout { get; set; } = "";
        public string Stderr { get; set; } = "";
        public string TimeRemaining { get; set; } = "";
        //public JobStatus CurrentStatus { get; set; } = new();
        public TrainingJobStatus CurrentTrainingStatus { get; set; } = new();
        public List<LoraCheckpoint> Checkpoints { get; set; } = new();

        public LoraTrainingJob() { }

        public LoraTrainingJob(LoraTrainingJob copy)
        {
            ModelPath = copy.ModelPath;
            DatasetPath = copy.DatasetPath;
            OutputDir = copy.OutputDir;
            BnbLoadIn4Bit = copy.BnbLoadIn4Bit;
            BnbLoadIn8Bit = copy.BnbLoadIn8Bit;
            Bnb4BitUseDoubleQuant = copy.Bnb4BitUseDoubleQuant;
            Bnb4BitQuantType = copy.Bnb4BitQuantType;
            LoraR = copy.LoraR;
            LoraAlpha = copy.LoraAlpha;
            LoraTargetModules = new();
            copy.LoraTargetModules.ForEach(x => LoraTargetModules.Add(x));
            LoraDropout = copy.LoraDropout;
            LoraBias = copy.LoraBias;
            LoraTaskType = copy.LoraTaskType;
            PerGpuTrainBatchSize = copy.PerGpuTrainBatchSize;
            GradientAccumulationSteps = copy.GradientAccumulationSteps;
            NumTrainEpochs = copy.NumTrainEpochs;
            LearningRate = copy.LearningRate;
            Fp16 = copy.Fp16;
            Optimizer = copy.Optimizer;
            ReportTo = copy.ReportTo;
            AutoUploadFinal = copy.AutoUploadFinal;
            AutoUploadInProgress = copy.AutoUploadInProgress;
        }
    }
}
