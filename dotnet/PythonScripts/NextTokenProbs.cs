using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TSRACT.PythonScripts
{
    public class NextTokenProbs
    {
        public string Stdout { get; set; } = "";
        public string Stderr { get; set; } = "";
        public event EventHandler OnOutput;

        private Process _process;

        public async Task LoadModel(string modelPath, string loraPath, bool bypassConda = false)
        {
            TaskCompletionSource<bool> completionSource = new();
            _process = new Process();
            string script = "Inference_llama.py";
            string scriptArguments = $"--model_path \"{modelPath}\"";
            string envPath = Path.Combine("..", "python", "env");
            string fileName = "";
            string arguments = "";
            
            if (loraPath != "")
            {
                scriptArguments = $"--model_path \"{modelPath}\" --use_lora True --lora_path \"{loraPath}\"";
            }

            if (!bypassConda)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    fileName = Path.Combine("..", "python", "conda", "condabin", "conda.bat");
                    arguments = "activate \"" + envPath + "\" >nul && python -u " + Path.Combine("..", "python", script) + " " + scriptArguments;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    fileName = Path.Combine("..", "python", "conda", "condabin", "conda.sh")
                        + " && conda activate "
                        + envPath
                        + " && python";
                }
                else
                {
                    throw new Exception("Unsupported OS");
                }
            }
            else
            {
                fileName = "python";
                arguments = "-u " + Path.Combine("..", "python", script) + " " + scriptArguments;
            }

            Console.WriteLine("Executing: " + fileName + " " + arguments);

            _process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            DataReceivedEventHandler outputHandler = async (sender, args) =>
            {
                if (args.Data == null || args.Data.Trim() == "") return;
                Stdout = args.Data;
                OnOutput?.Invoke(this, EventArgs.Empty);
                Console.WriteLine($"{script} stdout {args.Data}");
                if (args.Data.Trim() == "TSRACT: Ready.")
                {
                    completionSource.SetResult(true); // Signal that the expected output has been received
                    return;
                }
            };

            DataReceivedEventHandler errorHandler = async (sender, args) =>
            {
                if (args.Data == null || args.Data.Trim() == "") return;
                Stderr = args.Data;
                Console.WriteLine($"{script} stderr {args.Data}");
                OnOutput?.Invoke(this, EventArgs.Empty);
            };

            _process.OutputDataReceived += outputHandler;
            _process.ErrorDataReceived += errorHandler;

            _process.Start();
            _process.BeginOutputReadLine();

            // Wait for the specific signal from the Python script
            await completionSource.Task;

            _process.OutputDataReceived -= outputHandler;
            _process.ErrorDataReceived -= errorHandler;
        }

        public void StopModel()
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    _process.CancelOutputRead(); // Stop reading output
                    _process.StandardInput.Close(); // Close the standard input to signal the end
                    _process.WaitForExit(1000); // Wait for a graceful exit
                    if (!_process.HasExited)
                    {
                        _process.CloseMainWindow(); // Send a close event
                        _process.WaitForExit(1000); // Wait again for a graceful exit
                        if (!_process.HasExited)
                            _process.Kill(); // Forcefully terminate if still running
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] StopModel: {ex.Message}");
                }
                finally
                {
                    _process.Dispose();
                    _process = null;
                }
            }
        }

        public async Task<List<int>> GenerateText(string prompt, int maxNewTokens)
        {
            if (_process == null || _process.HasExited)
            {
                throw new InvalidOperationException("Model must be loaded before getting token probabilities.");
            }

            List<int> result = new();
            TaskCompletionSource<bool> completionSource = new();

            DataReceivedEventHandler outputHandler = (sender, args) =>
            {
                if (args.Data != null && args.Data.Trim() != "")
                {
                    if (args.Data == "TSRACT: Finished")
                    {
                        completionSource.SetResult(true); // Signal that the expected output has been received
                        return;
                    }

                    if (int.TryParse(args.Data, out var tokenId))
                    {
                        result.Add(tokenId);
                    }
                }
            };

            DataReceivedEventHandler errorHandler = (sender, args) =>
            {
                if (args.Data != null && args.Data.Trim() != "")
                {
                    Console.WriteLine($"[ERROR] GetNextTokenProbs: {args.Data}");
                }
            };

            _process.OutputDataReceived += outputHandler;
            _process.ErrorDataReceived += errorHandler;

            // Send prompt as console input
            _process.StandardInput.WriteLine("G" + maxNewTokens + "\"" + prompt.Replace("\n", "\\n") + "\"");

            // Wait for the specific signal from the Python script
            await completionSource.Task;

            _process.OutputDataReceived -= outputHandler;
            _process.ErrorDataReceived -= errorHandler;

            return result;
        }

        public async Task<Dictionary<int, double>> GetNextTokenProbs(string prompt)
        {
            if (_process == null || _process.HasExited)
            {
                throw new InvalidOperationException("Model must be loaded before getting token probabilities.");
            }

            Dictionary<int, double> result = new();
            TaskCompletionSource<bool> completionSource = new();

            DataReceivedEventHandler outputHandler = (sender, args) =>
            {
                if (args.Data != null && args.Data.Trim() != "")
                {
                    if (args.Data == "TSRACT: Finished")
                    {
                        completionSource.SetResult(true); // Signal that the expected output has been received
                        return;
                    }
                    var parts = args.Data.Split('\t');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var tokenId) && double.TryParse(parts[1], out var prob))
                    {
                        if (!result.ContainsKey(tokenId))
                        {
                            result.Add(tokenId, prob);
                        }
                        else
                        {
                            Console.WriteLine($"[WARNING] GetNextTokenProbs: Duplicate token ID {tokenId}.");
                        }
                    }
                }
            };

            DataReceivedEventHandler errorHandler = (sender, args) =>
            {
                if (args.Data != null && args.Data.Trim() != "")
                {
                    Console.WriteLine($"[ERROR] GetNextTokenProbs: {args.Data}");
                }
            };

            _process.OutputDataReceived += outputHandler;
            _process.ErrorDataReceived += errorHandler;

            // Send prompt as console input
            _process.StandardInput.WriteLine("P\"" + prompt.Replace("\n", "\\n") + "\"");

            // Wait for the specific signal from the Python script
            await completionSource.Task;

            _process.OutputDataReceived -= outputHandler;
            _process.ErrorDataReceived -= errorHandler;

            return result;
        }
    }
}
