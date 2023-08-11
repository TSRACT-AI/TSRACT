using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TSRACT.PythonScripts
{
    public class DumpTokenizer
    {
        public async Task<Dictionary<int, string>> GetTokenizerVocabulary(string modelPath, bool bypassConda = false)
        {
            Dictionary<int, string> result = new();
            Process process = new Process();
            string script = "DumpTokenizer_llama.py";
            string scriptArguments = $"--model_path \"{modelPath}\"";
            string envPath = Path.Combine("..", "python", "env");
            string fileName = "";
            string arguments = "";

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

            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            process.OutputDataReceived += async (sender, args) =>
            {
                if (args.Data != null && args.Data.Trim() != "")
                {
                    var parts = args.Data.Split('\t');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var tokenId))
                    {
                        result.Add(tokenId, parts[1]);
                    }
                }
            };

            // get stderr
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null && args.Data.Trim() != "")
                {
                    Console.WriteLine($"[ERROR] tokendump: {args.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();

            return result;
        }
    }
}
