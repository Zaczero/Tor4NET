using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Tor4NET.Sandbox
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Directory where Tor files are going to be stored.
            // If the directory does not exist, it will create one.
            var torDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tor4NET");

            // Use 64-bit Tor with 64-bit process.
            // It's *very* important for the architecture of Tor process match the one used by your app.
            // If no parameter is given Tor constructor will check Environment.Is64BitProcess property (the same one as below).
            var is32Bit = !Environment.Is64BitProcess;

            var tor = new Tor(torDirectory, is32Bit);

            // Check for updates and install latest version.
            if (tor.CheckForUpdates().Result)
                tor.Install().Wait();

            // Disposing the client will exit the Tor process automatically.
            using (var client = tor.InitializeClient())
            {
                var http = new WebClient
                {
                    // And now let's use Tor as a proxy.
                    Proxy = client.Proxy.WebProxy
                };

                var html = http.DownloadString("http://facebookcorewwwi.onion");
                Debugger.Break();
            }

            // Finally, you can remove all previously downloaded Tor files (optional).
            tor.Uninstall();
        }
    }
}
