public class NiceHashClient : AClient
{
    NiceHashWs[] MarketConnections;


    public NiceHashClient(){

        RateLimiting.Register("nicehash", 300, new TimeSpan(0, 5, 0));
        markets = PublisherUtilities.getList("nicehashex");
        MarketConnections = new NiceHashWs[markets.Count()];

    }


    public override async Task ConnectAll(){
        try{
            for (int i = 0; i < markets.Count(); i++)
            {
                MarketConnections[i] = new NiceHashWs(markets[i]);
                MarketConnections[i].RateSymbol = "nicehash";
                await MarketConnections[i].Connect(token.Token);

                Thread.Sleep(50);
            }
        }catch(Exception ex){
            System.Console.Error.WriteLine(ex);
        }
        foreach (var symbol in markets)
        {
            // Console.WriteLine($"Symbol: {symbol}");
        }
    }

}