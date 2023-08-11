using System.Diagnostics;
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
                        RedirectStandardOutput = true,
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


        public async Task StartMonitoring()
        {
            await MonitorGpus();
        }
    }
}
