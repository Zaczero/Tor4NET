using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tor4NET
{
    internal class TorUpdater
    {
        private readonly struct TorVersion
        {
            private readonly string source;

            public readonly int Major;
            public readonly int Minor;
            public readonly int Patch;
            public readonly string Suffix;

            public TorVersion(string version)
            {
                source = version;

                var match = ReleaseVersioningRegex.Match(version);
                if (!match.Success)
                {
                    Major = 0;
                    Minor = 0;
                    Patch = 0;
                    Suffix = string.Empty;
                    return;
                }
                
                var matchMajor = match.Groups["major"].Value;
                if (matchMajor.Length == 0)
                    matchMajor = "0";

                var matchMinor = match.Groups["minor"].Value;
                if (matchMinor.Length == 0)
                    matchMinor = "0";

                var matchPatch = match.Groups["patch"].Value;
                if (matchPatch.Length == 0)
                    matchPatch = "0";

                var matchSuffix = match.Groups["suffix"].Value;
                if (matchSuffix.Length == 0)
                    matchSuffix = string.Empty;

                Major = int.TryParse(matchMajor, out var matchMajorInt) ? matchMajorInt : 0;
                Minor = int.TryParse(matchMinor, out var matchMinorInt) ? matchMinorInt : 0;
                Patch = int.TryParse(matchPatch, out var matchPatchInt) ? matchPatchInt : 0;
                Suffix = matchSuffix;
            }

            public override string ToString()
            {
                return source;
            }
        }

        private const string BaseUrl = "https://dist.torproject.org/torbrowser/";

        private static readonly Regex ReleaseHtmlRegex = new Regex(@"alt=""\[DIR\]""> <a href=""(?<release>\d+(?:\.\S+)*?)\/"">(?:\d+(?:\.\S+)*?)\/<\/a>\s*(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})");
        private static readonly Regex ReleaseVersioningRegex = new Regex(@"(?<major>\d+)(?:\.(?<minor>\d+)(?:\.(?<patch>\d+))?)?(?<suffix>\S+)?");

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

        private async Task<string[]> GetReleaseList()
        {
            var html = await httpClient.GetStringAsync(BaseUrl);

            var releaseMatches = ReleaseHtmlRegex.Matches(html);
            var releases = new TorVersion[releaseMatches.Count];
            var releaseIndex = 0;

            foreach (Match match in releaseMatches)
                releases[releaseIndex++] = new TorVersion(match.Groups["release"].Value);

            Array.Sort(releases, (left, right) =>
            {
                if (left.Major > right.Major)
                    return -1;
                if (left.Major < right.Major)
                    return 1;

                if (left.Minor > right.Minor)
                    return -1;
                if (left.Minor < right.Minor)
                    return 1;

                if (left.Patch > right.Patch)
                    return -1;
                if (left.Patch < right.Patch)
                    return 1;

                return string.Compare(right.Suffix, left.Suffix, StringComparison.Ordinal);
            });

            var result = new string[releases.Length];

            for (var i = 0; i < releases.Length; i++)
                result[i] = releases[i].ToString();

            return result;
        }

        public async Task<(string, string)> GetLatestVersion()
        {
            var releases = await GetReleaseList();
            if (releases.Length == 0)
                return (string.Empty, string.Empty);

            foreach (var release in releases)
            {
                var (_, version) = await GetLatestVersion(release);
                if (version.Length > 0)
                    return (release, version);
            }

            return (string.Empty, string.Empty);
        }

        private async Task<(string, string)> GetLatestVersion(string release)
        {
            // backwards compatibility when `release` parameter is null
            if (release == null)
                return await GetLatestVersion();

            var url = $"{BaseUrl}{release}/";
            var html = await httpClient.GetStringAsync(url);

            return (release, versionRegex.Match(html).Groups["version"].Value);
        }

        public async Task<Stream> DownloadUpdate(string release = null, string version = null)
        {
            if (version == null)
                (release, version) = await GetLatestVersion(release);

            return await httpClient.GetStreamAsync($"{BaseUrl}{release}/tor-win{(x86 ? "32" : "64")}-{version}.zip");
        }
    }
}
