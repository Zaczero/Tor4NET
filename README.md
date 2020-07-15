# ![Zaczero/Tor4NET logo](https://github.com/Zaczero/Tor4NET/blob/master/icons/Tor4NET_small.png)

![github version](https://img.shields.io/github/release/Zaczero/Tor4NET.svg)
![nuget version](https://img.shields.io/nuget/v/Tor4NET.svg)
![license type](https://img.shields.io/github/license/Zaczero/Tor4NET.svg)

An all-in-one solution to fulfill your .NET dark web needs.

Learn more about Tor [here](https://www.torproject.org/).  
This library is built over [Tor.NET](https://www.codeproject.com/Articles/1072864/%2fArticles%2f1072864%2fTor-NET-A-managed-Tor-network-library) *- thanks to Chris Copeland*.

## 🌤️ Installation

### Install with NuGet (recommended)

`Install-Package Tor4NET`

### Install manually

[Browse latest GitHub release](https://github.com/Zaczero/Tor4NET/releases/latest)

## 🏁 Getting started

### Sample code

```cs
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
}

// Finally, you can remove all previously downloaded Tor files (optional).
tor.Uninstall();
```

## Footer

### 📧 Contact

* Email: [kamil@monicz.pl](mailto:kamil@monicz.pl)

### 📃 License

* [Zaczero/Tor4NET](https://github.com/Zaczero/Tor4NET/blob/master/LICENSE)
* [Tor.NET](https://www.codeproject.com/info/cpol10.aspx)