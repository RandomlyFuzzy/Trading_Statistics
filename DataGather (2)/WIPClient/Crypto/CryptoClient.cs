public class CryptoClient : AClient
{
    CryptoWs[] MarketConnections;


    public CryptoClient(){

        RateLimiting.Register("crypto", 300, new TimeSpan(0, 5, 0));
        markets = PublisherUtilities.getList("cryptoex");
        MarketConnections = new CryptoWs[markets.Count()];

    }


    public override async Task ConnectAll(){
        try{
            for (int i = 0; i < markets.Count(); i++)
            {
                MarketConnections[i] = new CryptoWs(markets[i]);
                MarketConnections[i].RateSymbol = "crypto";
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