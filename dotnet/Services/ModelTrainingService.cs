using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TSRACT.Models;

namespace TSRACT.Services
{
    public class ModelTrainingService
    {
        public List<LoraTrainingJob> TrainingQueue { get; set; } = new();

        private readonly CloudStorageService _cloudStorageService;

        public ModelTrainingService(CloudStorageService cloudStorageService)
        {
            _cloudStorageService = cloudStorageService;
        }

        public async Task StartQueue()
        {
            while (TrainingQueue.Count(x => !x.IsComplete) > 0)
            {
                Console.WriteLine("******************** STARTING NEW LoRA TRAINING ********************");
                var job = TrainingQueue.First(x => !x.IsComplete);
                await TrainLoraLlama(job);
                job.IsComplete = true;
            }
        }

        public int EstimateSpeed(int datasetSize)
        {
            // Define the known data points
            int[] dataCount = { 80, 233, 387, 541, 695, 1311, 2543, 3774, 5006, 5622, 7469, 9932, 19785 };
            double[] secPerIteration = { 8.66, 6.20, 6.39, 6.19, 6.11, 6.02, 5.88, 5.80, 5.79, 5.77, 5.60, 5.60, 5.56 };

            if (datasetSize < dataCount[0]) return (int)Math.Round(secPerIteration[0]);
            if (datasetSize > dataCount[dataCount.Length - 1]) return (int)Math.Round(secPerIteration[dataCount.Length - 1]);

            // Find the segment of the dataset size
            for (int i = 0; i < dataCount.Length - 1; i++)
            {
                if (datasetSize >= dataCount[i] && datasetSize <= dataCount[i + 1])
                {
                    // Perform linear interpolation
                    double t = (double)(datasetSize - dataCount[i]) / (dataCount[i + 1] - dataCount[i]);
                    double estimatedSpeed = secPerIteration[i] + t * (secPerIteration[i + 1] - secPerIteration[i]);

                    // Return the result as an integer (rounded)
                    return (int)Math.Round(estimatedSpeed);
                }
            }

            // Return a default value if outside the known range
            return -1;
        }

        public async Task TrainLoraLlama(LoraTrainingJob loraTrainingJob)
        {
            Process process = new Process();
            string scriptPath = Path.Combine("python", "TrainLora_llama.py");
            string scriptArgs = $"--model_path \"{loraTrainingJob.ModelPath}\" --dataset_path \"{loraTrainingJob.DatasetPath}\" --output_dir \"{loraTrainingJob.OutputDir}\" --bnb_load_in_4bit {loraTrainingJob.BnbLoadIn4Bit} --bnb_4bit_use_double_quant {loraTrainingJob.Bnb4BitUseDoubleQuant} --bnb_4bit_quant_type \"{loraTrainingJob.Bnb4BitQuantType}\" --lora_r {loraTrainingJob.LoraR} --lora_alpha {loraTrainingJob.LoraAlpha} --lora_target_modules \"{string.Join(",", loraTrainingJob.LoraTargetModules)}\" --lora_dropout {loraTrainingJob.LoraDropout} --lora_bias \"{loraTrainingJob.LoraBias}\" --lora_task_type \"{loraTrainingJob.LoraTaskType}\" --per_gpu_train_batch_size {loraTrainingJob.PerGpuTrainBatchSize} --gradient_accumulation_steps {loraTrainingJob.GradientAccumulationSteps} --warmup_ratio {loraTrainingJob.WarmupRatio} --num_train_epochs {loraTrainingJob.NumTrainEpochs} --learning_rate {loraTrainingJob.LearningRate} --fp16 {loraTrainingJob.Fp16} --optimizer \"{loraTrainingJob.Optimizer}\"";

            Directory.CreateDirectory(loraTrainingJob.OutputDir);

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-u {scriptPath} {scriptArgs}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            process.OutputDataReceived += async (sender, args) =>
            {
                if (args.Data == null || args.Data.Trim() == "") return;
                Console.WriteLine($"[{loraTrainingJob.OutputDir}] {args.Data}");
                loraTrainingJob.Stdout = args.Data;

                if (args.Data.StartsWith("TSRACT: "))
                {
                    loraTrainingJob.ScriptStatus = args.Data.Replace("TSRACT: ", "");
                }
                else
                {
                    // args.Data will be a string: "{'loss': 1.9628, 'learning_rate': 2.0000000000000002e-07, 'epoch': 0.0}"
                    try
                    {
                        var data = JObject.Parse(args.Data.Replace("'", "\""));
                        float loss = 0.0f;
                        float learningRate = 0.0f;
                        float epoch = 0.0f;
                        if (data["loss"] != null) loss = data["loss"].ToObject<float>();
                        if (data["learning_rate"] != null) learningRate = data["learning_rate"].ToObject<float>();
                        if (data["epoch"] != null) epoch = data["epoch"].ToObject<float>();

                        loraTrainingJob.CurrentTrainingStatus = new TrainingJobStatus()
                        {
                            Loss = loss,
                            LearningRate = learningRate,
                            Epoch = epoch
                        };
                    }
                    catch (JsonReaderException) { } // Ignore all other output for parsing purposes
                }

                // Monitor for new checkpoints
                // ./output/name/checkpoint-100/*
                // ./output/name/checkpoint-200/*
                // etc...

                foreach (string dirPath in Directory.EnumerateDirectories(loraTrainingJob.OutputDir, "checkpoint-*", SearchOption.AllDirectories))
                {
                    string dirName = Path.GetFileName(dirPath);
                    int step = int.Parse(dirName.Replace("checkpoint-", ""));

                    if (loraTrainingJob.Checkpoints.Count(x => x.Step == step) == 0)
                    {
                        loraTrainingJob.Checkpoints.Add(new LoraCheckpoint()
                        {
                            Step = step,
                            DirectoryPath = dirPath
                        });

                        await Task.Delay(10000); // Wait for the script to finish writing the files

                        loraTrainingJob.Checkpoints.First(x => x.Step == step).FilePaths = Directory.GetFiles(dirPath).ToList();

                        if (loraTrainingJob.AutoUploadInProgress == true && _cloudStorageService.AzureBlobSasUrl != "")
                        {
                            foreach (string filePath in Directory.GetFiles(dirPath))
                            {
                                await _cloudStorageService.UploadFileToBlob(filePath, $"{loraTrainingJob.OutputDir}/{dirName}/{Path.GetFileName(filePath)}");
                            }
                        }
                    }
                }
            };

            process.ErrorDataReceived += async (sender, args) =>
            {
                if (args.Data == null || args.Data.Trim() == "") return;
                Console.WriteLine($"[{loraTrainingJob.OutputDir}] {args.Data}");
                loraTrainingJob.Stderr = args.Data;

                try
                {
                    string pattern = @"(\d+)%\|.*\| (\d+)/(\d+) \[((\d+:\d+)|(\d+:\d+:\d+))<(.+?),\s+(\d+\.\d+)s/it\]";
                    Regex regex = new Regex(pattern);
                    Match match = regex.Match(args.Data);

                    if (match.Success)
                    {
                        loraTrainingJob.TimeRemaining = match.Groups[2].Value + " / " + match.Groups[3].Value; // time parsing not working right, just show steps for now
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{loraTrainingJob.OutputDir}] Error parsing status: {ex.Message}");
                }
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("Python script failed.");
            }

            loraTrainingJob.Checkpoints.Add(new LoraCheckpoint()
            {
                Step = 0, // loraTrainingJob.CurrentStatus.TotalSteps,
                DirectoryPath = loraTrainingJob.OutputDir
            });

            await Task.Delay(10000); // Wait for the script to finish writing the files
            loraTrainingJob.Checkpoints.Last().FilePaths = Directory.GetFiles(loraTrainingJob.OutputDir).ToList();

            if (loraTrainingJob.AutoUploadFinal == true && _cloudStorageService.AzureBlobSasUrl != "")
            {
                foreach (string filePath in Directory.GetFiles(loraTrainingJob.OutputDir))
                {
                    await _cloudStorageService.UploadFileToBlob(filePath, $"{loraTrainingJob.OutputDir}/checkpoint-final/{Path.GetFileName(filePath)}");
                }
            }
        }
    }
}
