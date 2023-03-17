public class BitStampClient : AClient
{

    BitStampWs[] MarketConnections;


    public BitStampClient(){
        RateLimiting.Register("bitstamp", 300, new TimeSpan(0, 5, 0));
        markets = PublisherUtilities.getList("bitstampex");
        MarketConnections = new BitStampWs[markets.Count()];
    }


    public override async Task ConnectAll(){
        try{
            for (int i = 0; i < markets.Count(); i++)
            {
                MarketConnections[i] = new BitStampWs(markets[i]);
                MarketConnections[i].RateSymbol = "bitstamp";
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