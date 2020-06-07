using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tor4NET
{
    internal class TorUpdater
    {
        private const string BaseUrl = "https://dist.torproject.org/torbrowser/";

        private readonly Regex releaseRegex = new Regex(@"alt=""\[DIR\]""> <a href=""(?<release>\d+(?:\.\S+)*?)\/"">(?:\d+(?:\.\S+)*?)\/<\/a>\s*(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})");
        private readonly Regex versionRegex;
        private readonly HttpClient httpClient;

        private readonly bool x86;

        public TorUpdater(HttpClient httpClient, bool x86 = true)
        {
            versionRegex = x86 ?
                new Regex(@"tor-win32-(?<version>\S+?)\.zip") :
                new Regex(@"tor-win64-(?<version>\S+?)\.zip");
            
            this.httpClient = httpClient;
            this.x86 = x86;
        }

        private async Task<string> GetLatestRelease()
        {
            var html = await httpClient.GetStringAsync(BaseUrl);
            var matches = releaseRegex.Matches(html);

            var version = string.Empty;

            foreach (Match match in matches)
            {
                var matchVersion = match.Groups["release"].Value;

                if (version == string.Empty || string.CompareOrdinal(matchVersion, version) > 0)
                    version = matchVersion;
            }

            return version;
        }

        public async Task<string> GetLatestVersion(string release = null)
        {
            if (release == null)
                release = await GetLatestRelease();

            var url = $"{BaseUrl}{release}/";
            var html = await httpClient.GetStringAsync(url);
            var match = versionRegex.Match(html);

            return match.Groups["version"].Value;
        }

        public async Task<Stream> DownloadUpdate(string release = null, string version = null)
        {
            if (release == null)
                release = await GetLatestRelease();

            if (version == null)
                version = await GetLatestVersion(release);

            var downloadUrl = $"{BaseUrl}{release}/tor-win{(x86 ? "32" : "64")}-{version}.zip";

            return await httpClient.GetStreamAsync(downloadUrl);
        }
    }
}
