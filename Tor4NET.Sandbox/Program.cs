using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Tor4NET.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            // directory where the tor files are going to be stored
            // if directory doesn't exist, it will create one
            var torDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tor");

            // support 64 bit tor on 64 bit os (optional)
            var is32Bit = !Environment.Is64BitOperatingSystem;

            var tor = new Tor(torDirectory, is32Bit);

            // install updates if available
            if (tor.CheckForUpdates().Result)
                tor.InstallUpdates().Wait();

            var client = tor.InitializeClient();

            // wait for tor to fully initialize
			Thread.Sleep(5 * 1000);

            while (!client.Proxy.IsRunning)
                Thread.Sleep(100);

            var wc = new WebClient();

            // use the tor proxy !!
            wc.Proxy = client.Proxy.WebProxy;

            var html = wc.DownloadString("http://facebookcorewwwi.onion");

            client.Dispose();
        }
    }
}
