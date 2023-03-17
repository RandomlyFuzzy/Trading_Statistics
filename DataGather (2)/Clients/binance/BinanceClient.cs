//https://api.binance.com/api/v3/exchangeInfo


public class BinanceClient: AClient
{
    BinanceWs[] MarketConnections;

    public BinanceClient():base()
    {
        RateLimiting.Register("binance", 300, new TimeSpan(0,5,0));
        this.markets = PublisherUtilities.getList("binanceex");
        MarketConnections = new BinanceWs[markets.Count()];
    }


    public override async Task ConnectAll()
    {
        try
        {
            for (int i = 0; i < markets.Count(); i++)
            {
                MarketConnections[i] = new BinanceWs(markets[i]);
                MarketConnections[i].RateSymbol = "binance";
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