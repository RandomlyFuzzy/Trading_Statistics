using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientBook:IClient,IDisposable
{
    public List<AClient> Clients = new List<AClient>();

    public List<string> clientInitaials = new List<string>()
    {
        "NH",
        "CR",
        "BS",
        "KR",
        "OK",
        "BI",
    };


    public ClientBook() {
        var args = Environment.GetCommandLineArgs();

        int i = 0;
        if (args.Contains("-"+clientInitaials[i++]))Clients.Add(new NiceHashClient());
        if (args.Contains("-"+clientInitaials[i++]))Clients.Add(new CryptoClient());
        if (args.Contains("-"+clientInitaials[i++]))Clients.Add(new BitStampClient());
        if (args.Contains("-"+clientInitaials[i++]))Clients.Add(new KrakenClient());
        if (args.Contains("-"+clientInitaials[i++]))Clients.Add(new OKXClient());
        if (args.Contains("-" + clientInitaials[i++]))Clients.Add(new BinanceClient());

        if (Clients.Count == 0) {
            foreach (var item in clientInitaials)
            {
                StartSameApplication("-" + item);
            }
            Environment.Exit(0);
        }


    }

    public void StartSameApplication(string args) {
        string name = AppDomain.CurrentDomain.FriendlyName;
        Console.WriteLine(name);

        //Process.Start(name+".exe", args);
        Process p = new Process();
        p.StartInfo = new ProcessStartInfo
        {
            FileName = name + ".exe" ,
            Arguments = args,
            UseShellExecute = true,
            CreateNoWindow = false,
        };
        p.Start();
    }

    public async Task ConnectAll()
    {
        foreach (var client in Clients) client.ConnectAll();
    }

    public void Dispose()
    {
        Stop();
    }

    public void Stop()
    {
        foreach (var client in Clients) client.Stop();
    }
}
