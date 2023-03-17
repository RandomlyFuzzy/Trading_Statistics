using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientBook:IClient,IDisposable
{
    public List<AClient> Clients = new List<AClient>();

    public ClientBook() {
        Clients.Add(new NiceHashClient());
        Clients.Add(new CryptoClient());
        Clients.Add(new BitStampClient());
        Clients.Add(new KrakenClient());
        Clients.Add(new okxClient());
        Clients.Add(new BinanceClient());
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
