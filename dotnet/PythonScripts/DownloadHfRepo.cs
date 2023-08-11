using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TSRACT.PythonScripts
{
    public class DownloadHfRepo
    {
        public string Stdout { get; set; } = "";
        public string Stderr { get; set; } = "";
        public event EventHandler OnOutput;

        public async Task<bool> Download(string repoId, string repoType, bool bypassConda = false)
        {
            string script = "DownloadHfRepo.py";
            string scriptArguments = $"\"{repoId}\" \"{repoType}\"";
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

            // Start the Python script
            Process pyProcess = new Process();
            pyProcess.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            pyProcess.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null || args.Data.Trim() == "") return;
                Console.WriteLine($"[DownloadHfRepo.py] {args.Data}");
                Stdout = args.Data;
                OnOutput?.Invoke(this, EventArgs.Empty);
            };

            pyProcess.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data == null || args.Data.Trim() == "") return;
                Console.WriteLine($"[DownloadHfRepo.py] {args.Data}");
                Stderr = args.Data;
                OnOutput?.Invoke(this, EventArgs.Empty);
            };

            pyProcess.Start();
            pyProcess.BeginErrorReadLine();
            pyProcess.BeginOutputReadLine();

            await pyProcess.WaitForExitAsync();

            if (pyProcess.ExitCode != 0)
            {
                Console.WriteLine("Python script failed.");
                return false;
            }

            return true;
        }
    }
}
