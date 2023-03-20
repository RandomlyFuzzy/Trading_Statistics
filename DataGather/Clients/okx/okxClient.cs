using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OKXClient:AClient
{
    okxWs[] MarketConnections;
    public OKXClient() {

        RateLimiting.Register("okx", 300, new TimeSpan(0, 5, 0));
        markets = PublisherUtilities.getList("okxex");
        MarketConnections = new okxWs[markets.Count()];
    }
    public override async Task ConnectAll()
    {
        try
        {
            for (int i = 0; i < markets.Count(); i++)
            {
                MarketConnections[i] = new okxWs(markets[i]);
                MarketConnections[i].RateSymbol = "okx";
                MarketConnections[i].Connect(token.Token);
                Thread.Sleep(100);
            }
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(ex);
        }
        foreach (var symbol in markets)
        {
            // Console.WriteLine($"Symbol: {symbol}");
        }
    }

}

