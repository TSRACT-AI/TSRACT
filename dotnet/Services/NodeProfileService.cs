using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using TSRACT.Models;

namespace TSRACT.Services
{
    public class NodeProfileService
    {
        public bool BypassConda { get; set; } = false;
        public NodeProfile Profile { get; set; } = new NodeProfile();
        public event EventHandler GpuStatusUpdated;

        public NodeProfileService()
        {
            Profile.IpAddress = GetIpAddress();
            Profile.Gpus = InventoryGpus();
            Profile.PythonRuntime = GetPythonVersion();
            Profile.PythonModules = GetPythonModules();
        }

        private string GetIpAddress()
        {
            string[] providers = new string[]
            {
                "https://api.ipify.org",
                "https://checkip.amazonaws.com",
                "https://icanhazip.com",
                "https://ifconfig.me"
            };

            using (HttpClient httpClient = new HttpClient())
            {
                foreach (string provider in providers)
                {
                    try
                    {
                        HttpResponseMessage response = httpClient.GetAsync(provider).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            return response.Content.ReadAsStringAsync().Result.Trim();
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"WARNING: Could not reach IP address provider {provider}");
                    }
                }
            }

            return "ERROR: Could not get IP";
        }

        private List<SoftwareDependency> GetPythonModules()
        {
            List<SoftwareDependency> result = new();

            Console.WriteLine("Validating python modules...");

            try
            {
                foreach (string line in File.ReadAllLines(Path.Combine("..", "python", "requirements.txt")))
                {
                    SoftwareDependency dependency = ParseRequirement(line);
                    if (dependency == null)
                    {
                        continue;
                    }

                    result.Add(dependency);
                }
            }
            catch (Exception ex)
            {
                result.Add(new SoftwareDependency { Name = "Error reading requirements.txt", InstalledVersion = ex.Message });
            }

            // go through each module and run `pip show <module>` for each
            // parse Name: <name> and Version: <version> from output
            // if error, set InstalledVersion to error message

            foreach (SoftwareDependency module in result)
            {
                try
                {
                    Console.WriteLine($"Validating python module {module.Name}...");
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "pip",
                            Arguments = $"show {module.Name}",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    string[] lines = output.Split('\n');

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Name:"))
                        {
                            module.Name = line.Replace("Name:", "").Trim();
                        }
                        else if (line.StartsWith("Version:"))
                        {
                            module.InstalledVersion = line.Replace("Version:", "").Trim();
                            module.IsPresent = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    module.InstalledVersion = $"Error when running pip: {ex.Message}";
                }
            }

            return result;
        }

        private SoftwareDependency GetPythonVersion()
        {
            SoftwareDependency result = new() { Name = "python" };

            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();

                result.InstalledVersion = process.StandardOutput.ReadToEnd();
                result.IsPresent = true;
            }
            catch (Exception ex)
            {
                result.InstalledVersion = $"Error when running python: {ex.Message}";
            }

            return result;
        }

        private List<GpuProfile> InventoryGpus()
        {
            if (!IsNvidiaSmiAvailable()) return new List<GpuProfile>();

            List<GpuProfile> result = new();
            StringBuilder output = new StringBuilder();
            Process process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = "--query-gpu=index,name,memory.total,power.limit --format=csv,noheader,nounits",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process.OutputDataReceived += (sender, args) =>
            {
                if (!String.IsNullOrEmpty(args.Data))
                {
                    output.AppendLine(args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception("Failed to execute nvidia-smi process.");
            }

            // parse each line into a GpuProfile
            foreach (string line in output.ToString().Split(Environment.NewLine))
            {
                if (String.IsNullOrEmpty(line)) continue;
                string[] data = line.Split(',');
                result.Add(new GpuProfile()
                {
                    Index = int.Parse(data[0]),
                    Name = data[1],
                    MemoryTotal = int.Parse(data[2]),
                    PowerLimit = float.Parse(data[3])
                });
            }

            return result;
        }

        private bool IsNvidiaSmiAvailable()
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "nvidia-smi",
                        UseShellExecute = false,
                        //RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task MonitorGpus()
        {
            if (!IsNvidiaSmiAvailable()) return;
            
            Process process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = "--query-gpu=index,utilization.gpu,utilization.memory,memory.reserved,memory.used,memory.free,power.draw,temperature.gpu --format=csv,noheader,nounits --loop=1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process.OutputDataReceived += async (sender, args) =>
            {
                string[] data = args.Data.Split(',');
                if (data.Length == 8)
                {
                    int index;
                    if (!int.TryParse(data[0], out index)) index = 0;

                    GpuProfile? gpu = Profile.Gpus.FirstOrDefault(g => g.Index == index);
                    if (gpu == null) return;

                    int gpuUtilization, memoryUtilization, memoryReserved, memoryUsed, memoryFree, temperature;
                    float powerDraw;

                    if (!int.TryParse(data[1], out gpuUtilization)) gpuUtilization = 0;
                    if (!int.TryParse(data[2], out memoryUtilization)) memoryUtilization = 0;
                    if (!int.TryParse(data[3], out memoryReserved)) memoryReserved = 0;
                    if (!int.TryParse(data[4], out memoryUsed)) memoryUsed = 0;
                    if (!int.TryParse(data[5], out memoryFree)) memoryFree = 0;
                    if (!float.TryParse(data[6], out powerDraw)) powerDraw = 0;
                    if (!int.TryParse(data[7], out temperature)) temperature = 0;

                    gpu.CurrentStatus = new GpuStatus()
                    {
                        Index = index,
                        GpuUtilization = gpuUtilization,
                        MemoryUtilization = memoryUtilization,
                        MemoryReserved = memoryReserved,
                        MemoryUsed = memoryUsed,
                        MemoryFree = memoryFree,
                        PowerDraw = powerDraw,
                        Temperature = temperature
                    };

                    if (index == 0) GpuStatusUpdated?.Invoke(this, EventArgs.Empty);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
        }

        static SoftwareDependency ParseRequirement(string line)
        {
            var dependency = new SoftwareDependency();

            if (line.StartsWith("https://github.com/jllllll/bitsandbytes-windows-webui/releases/download/wheels/bitsandbytes-0.41.1-py3-none-win_amd64.whl"))
            {
                dependency.Name = "bitsandbytes";
                dependency.RequiredVersion = "0.41.1";
                return dependency;
            }

            if (line.Contains("@"))
            {
                // Handle git or URL based dependencies
                var atParts = line.Split(new[] { " @ " }, StringSplitOptions.None);
                if (atParts.Length > 1)
                {
                    dependency.Name = atParts[0].Trim();
                    return dependency;
                }
                else
                {
                    return null; // Malformed line
                }
            }

            if (line.StartsWith("http") || line.StartsWith("git"))
            {
                // Handle URL or VCS based dependencies differently
                return null;
            }

            var parts = line.Split(';');
            var packageParts = parts[0].Split("==");

            dependency.Name = packageParts[0].Trim();

            if (packageParts.Length > 1)
            {
                dependency.RequiredVersion = packageParts[1].Trim();
            }

            if (parts.Length > 1)
            {
                var condition = parts[1].Trim();

                if (condition == "platform_system != \"Windows\"" && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return null;
                }

                if (condition == "platform_system == \"Windows\"" && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return null;
                }
            }

            return dependency;
        }

        public async Task StartMonitoring()
        {
            await MonitorGpus();
        }
    }
}
