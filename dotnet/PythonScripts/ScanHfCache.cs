using System.Diagnostics;
using System.Text;
using TSRACT.Models;

namespace TSRACT.PythonScripts
{
    public class ScanHfCache
    {
        public async Task<List<HfCacheItem>> GetHfCacheItems()
        {
            string fileName = "huggingface-cli";
            string arguments = "scan-cache -v";
            Console.WriteLine("Executing: " + fileName + " " + arguments);
            
            // Start huggingface-cli scan-cache
            Process hfProcess = new Process();
            hfProcess.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            StringBuilder output = new StringBuilder();
            hfProcess.OutputDataReceived += (sender, args) =>
            {
                if (!String.IsNullOrEmpty(args.Data))
                {
                    output.AppendLine(args.Data);
                }
            };

            hfProcess.Start();
            hfProcess.BeginOutputReadLine();
            hfProcess.WaitForExit();

            if (hfProcess.ExitCode != 0)
            {
                throw new Exception("Failed to invoke huggingface-cli process.");
            }

            return ParseHfCacheOutput(output.ToString());
        }

        private List<HfCacheItem> ParseHfCacheOutput(string output)
        {
            var lines = output.Split('\n');
            var cacheItems = new List<HfCacheItem>();
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                // Ignore lines that are not valid cache items
                if (string.IsNullOrEmpty(trimmedLine)
                    || !Char.IsLetterOrDigit(trimmedLine[0])
                    || trimmedLine.StartsWith("REPO ID") // check for table header
                    || trimmedLine.StartsWith("Done")) // check for table footer
                    continue;

                var parts = trimmedLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 8) // There should be at least 8 parts for a valid cache item
                {
                    cacheItems.Add(new HfCacheItem
                    {
                        RepoId = parts[0],
                        RepoType = parts[1],
                        Revision = parts[2],
                        SizeOnDisk = parts[3],
                        NbFiles = int.Parse(parts[4]),
                        LastModified = parts[5] + " " + parts[6] + " " + parts[7], // combining LastModified parts
                        Refs = parts[8],
                        LocalPath = string.Join(' ', parts.Skip(9)) // LocalPath can contain spaces
                    });
                }
            }
            return cacheItems;
        }
    }
}
