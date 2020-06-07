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
        private readonly Regex versionRegex = new Regex(@"Tor version (?<version>\S+)");
        private readonly TorUpdater torUpdater;
        
        private readonly string torDirectory;
        private readonly string torExecutable;

        private readonly int socksPort;
        private readonly int controlPort;
        private readonly string controlPassword;

        public Tor(string torDirectory, bool x86 = true, int socksPort = 9450, int controlPort = 9451, string controlPassword = "")
        {
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

            this.socksPort = socksPort;
            this.controlPort = controlPort;
            this.controlPassword = controlPassword;
        }

        private bool IsTorRunning()
        {
            var torProcesses = Process.GetProcessesByName("tor");

            foreach (var torProcess in torProcesses)
            {
                try
                {
                    if (torProcess.MainModule.FileName == torExecutable)
                        return true;
                }
                catch
                { }
            }

            return false;
        }

        private void KillTorProcess()
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

        private async Task<string> GetCurrentVersion()
        {
            if (!File.Exists(torExecutable))
                return string.Empty;

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
        
        public async Task<bool> CheckForUpdates()
        {
            var currentVersion = await GetCurrentVersion();
            if (currentVersion == string.Empty)
                return true;

            var latestVersion = await torUpdater.GetLatestVersion();
            return currentVersion != latestVersion;
        }

        public async Task Install()
        {
            if (!Directory.Exists(torDirectory))
                Directory.CreateDirectory(torDirectory);
            else
                KillTorProcess();
            
            var updateZip = await torUpdater.DownloadUpdate();
            var archive = new ZipArchive(updateZip);

            foreach (var entry in archive.Entries)
            {
                var path = $@"{torDirectory}\{entry.FullName}";
                if (entry.CompressedLength == 0)
                {
                    // Directory
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
                else
                {
                    // File
                    var s = entry.Open();
                    var fs = new FileStream(path, FileMode.Create);

                    await s.CopyToAsync(fs);

                    fs.Dispose();
                    s.Dispose();
                }
            }
        }

        public void Uninstall()
        {
            KillTorProcess();

            if (Directory.Exists(torDirectory))
                Directory.Delete(torDirectory, true);
        }

        public Client InitializeClient(bool killExistingTor = false)
        {
            Client client;
            
            if (!killExistingTor && IsTorRunning())
            {
                var createParams = new ClientRemoteParams("127.0.0.1", controlPort, controlPassword);
                client = Client.CreateForRemote(createParams);
            }
            else
            {
                KillTorProcess();

                var createParams = new ClientCreateParams(torExecutable, controlPort, controlPassword);
                client = Client.Create(createParams);
            }

            client.Configuration.ClientUseIPv6 = true;
            client.Configuration.HardwareAcceleration = true;
            client.Configuration.SocksPort = socksPort;
            client.Configuration.Save();

            return client;
        }
    }
}
