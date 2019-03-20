using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tor;

namespace Tor4NET
{
    public class Tor
    {
        private const int SocksPort = 9450;
        private const int ControlPort = 9451;
        
        private readonly Regex versionRegex;
        private readonly TorUpdater torUpdater;

        private readonly string torDirectory;
        private readonly string torExecutable;
        private readonly string controlPassword = "Tor4NET_zkGnHjviJ5dJH77KaaxTA5kf";

        public Tor(string torDirectory, bool x86 = true, string torControlPassword = null)
        {
            versionRegex = new Regex(@"Tor version (?<version>\S+)", RegexOptions.Compiled);

            var httpHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12,
                AllowAutoRedirect = false,
            };

            var httpClient = new HttpClient(httpHandler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Tor4NET (+https://github.com/Zaczerp/Tor4NET)");
            httpClient.DefaultRequestHeaders.Connection.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");

            torUpdater = new TorUpdater(httpClient, x86);

            this.torDirectory = torDirectory;
            torExecutable = $@"{this.torDirectory}\Tor\tor.exe";

            if (!string.IsNullOrEmpty(torControlPassword))
            {
                controlPassword = torControlPassword;
            }
        }

        private async Task<string> GetCurrentVersion()
        {
            if (!File.Exists(torExecutable))
            {
                return string.Empty;
            }

            var psi = new ProcessStartInfo
            {
                FileName = torExecutable,
                Arguments = "--version",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            var process = Process.Start(psi);
            var output = await process.StandardOutput.ReadToEndAsync();
            var match = versionRegex.Match(output);

            return match.Groups["version"].Value;
        }

        private bool IsTorRunning()
        {
            var torProcesses = Process.GetProcessesByName("tor");

            foreach (var torProcess in torProcesses)
            {
                try
                {
                    if (torProcess.MainModule.FileName == torExecutable)
                    {
                        return true;
                    }
                }
                catch
                { }
            }

            return false;
        }

        private void KillTorProcesses()
        {
            var torProcesses = Process.GetProcessesByName("tor");

            foreach (var torProcess in torProcesses)
            {
                try
                {
                    if (torProcess.MainModule.FileName == torExecutable)
                    {
                        torProcess.Kill();
                        torProcess.WaitForExit();
                    }
                }
                catch
                { }
            }
        }
        
        public async Task<bool> CheckForUpdates()
        {
            var currentVersion = await GetCurrentVersion();
            if (currentVersion == string.Empty)
            {
                return true;
            }

            var latestVersion = await torUpdater.GetLatestVersion();
            return currentVersion != latestVersion;
        }

        public async Task InstallUpdates()
        {
            if (!Directory.Exists(torDirectory))
            {
                Directory.CreateDirectory(torDirectory);
            }
            else
            {
                KillTorProcesses();
            }
            
            var updateZip = await torUpdater.DownloadUpdate();
            var archive = new ZipArchive(updateZip);

            foreach (var entry in archive.Entries)
            {
                var path = $@"{torDirectory}\{entry.FullName}";
                if (entry.CompressedLength == 0)
                {
                    // directory
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                else
                {
                    // file
                    var s = entry.Open();
                    var fs = new FileStream(path, FileMode.Create);

                    await s.CopyToAsync(fs);

                    fs.Dispose();
                    s.Dispose();
                }
            }
        }

        public Client InitializeClient(bool killExistingTor = false)
        {
            Client client;
            
            if (!killExistingTor && IsTorRunning())
            {
                var createParams = new ClientRemoteParams("127.0.0.1", ControlPort, controlPassword);
                client = Client.CreateForRemote(createParams);
            }
            else
            {
                KillTorProcesses();

                var createParams = new ClientCreateParams(torExecutable, ControlPort, controlPassword);
                client = Client.Create(createParams);
            }

            client.Configuration.ClientUseIPv6 = true;
            client.Configuration.HardwareAcceleration = true;
            client.Configuration.SocksPort = SocksPort;
            client.Configuration.Save();

            return client;
        }
    }
}
