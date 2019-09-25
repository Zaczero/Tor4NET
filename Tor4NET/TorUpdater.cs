using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tor4NET
{
    internal class TorUpdater
    {
        private const string BaseUrl = "https://dist.torproject.org/torbrowser/";

        private readonly Regex _releaseRegex;
        private readonly Regex _versionRegex;
        private readonly HttpClient _httpClient;

        private readonly bool _x86;

        public TorUpdater(HttpClient httpClient, bool x86 = true)
        {
            _releaseRegex = new Regex(@"alt=""\[DIR\]""> <a href=""(?<release>\d+(?:\.\S+)*?)\/"">(?:\d+(?:\.\S+)*?)\/<\/a>\s*(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})", RegexOptions.Compiled);
            _versionRegex = new Regex($@"tor-win{(x86 ? "32" : "64")}-(?<version>\S+?)\.zip", RegexOptions.Compiled);
            _httpClient = httpClient;

            _x86 = x86;
        }

        public async Task<string> GetLatestRelease()
        {
            var html = await _httpClient.GetStringAsync(BaseUrl);
            var matches = _releaseRegex.Matches(html);

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
            {
                release = await GetLatestRelease();
            }

            var url = $"{BaseUrl}{release}/";
            var html = await _httpClient.GetStringAsync(url);
            var match = _versionRegex.Match(html);

            return match.Groups["version"].Value;
        }

        public async Task<Stream> DownloadUpdate(string release = null, string version = null)
        {
            if (release == null)
            {
                release = await GetLatestRelease();
            }

            if (version == null)
            {
                version = await GetLatestVersion(release);
            }

            var downloadUrl = $"{BaseUrl}{release}/tor-win{(_x86 ? "32" : "64")}-{version}.zip";

            return await _httpClient.GetStreamAsync(downloadUrl);
        }
    }
}
