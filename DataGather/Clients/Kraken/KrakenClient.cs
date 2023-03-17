using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class KrakenClient : AClient
{
    KrakenWs[] MarketConnections;
    public KrakenClient()
    {
        RateLimiting.Register("kraken", 300, new TimeSpan(0, 5, 0));
        this.markets = PublisherUtilities.getList("krakenex");
        MarketConnections = new KrakenWs[markets.Count()];
    }


    public override async Task ConnectAll()
    {
        try
        {
            for (int i = 0; i < markets.Count(); i++)
            {
                MarketConnections[i] = new KrakenWs(markets[i]);
                MarketConnections[i].RateSymbol = "kraken";
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
