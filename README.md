# ![](https://github.com/Zaczero/Tor4NET/blob/master/icons/Tor4NET_small.png)

![](https://img.shields.io/github/release/Zaczero/Tor4NET.svg)
![](https://img.shields.io/nuget/v/Tor4NET.svg)
![](https://img.shields.io/github/license/Zaczero/Tor4NET.svg)

An all-in-one solution to fulfill your .NET dark web needs.

Learn more about Tor [here](https://www.torproject.org/).  
This library is built over [Tor.NET](https://www.codeproject.com/Articles/1072864/%2fArticles%2f1072864%2fTor-NET-A-managed-Tor-network-library) *- thanks to Chris Copeland*.

## 🔗 Download

* https://github.com/Zaczero/Tor4NET/releases/latest

## 🏁 Getting started

```cs
// directory where the tor files are going to be stored
// if directory doesn't exist, it will create one
var torDirectory = "C:\\Users\\CHANGE_ME\\Documents\\tor";

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
```

## ☕ Support me

* Bitcoin: `35n1y9iHePKsVTobs4FJEkbfnBg2NtVbJW`

## 📎 Licenses
### Tor4NET MIT license

MIT License

Copyright (c) 2019 Kamil Monicz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

### Tor.NET CPOL license

* http://www.codeproject.com/info/cpol10.aspx
