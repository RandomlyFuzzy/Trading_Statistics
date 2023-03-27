using System.Threading;

class BinanceWs : AWebSocketClient<BinanceBasicObject>
{
    static int totalEvents = 0;

    CancellationToken cancellationToken;
    public BinanceWs(string symbol) : base(false)
    {
        this.symbol = symbol;
    }
    public async Task Connect(CancellationToken cancellationToken = default){
        this.Prefix = "binance " +symbol;
        this.cancellationToken = cancellationToken;
        await init();
    }

    public async override Task init()
    {   
        Random rdm = new Random();
        await ConnectAsync(new Uri("wss://stream.binance.com:9443/ws"), cancellationToken);
        SendAsync($@"
        {{
            ""method"": ""SUBSCRIBE"",
            ""params"":
            [
                ""{symbol.ToLower()}@bookTicker""
            ],
            ""id"": {rdm.Next()}
    }}");

    }

    protected override ObjectType MessageType(BinanceBasicObject message, ObjectDecoder obj)
    {

        if (message.result == null)
        {
            return ObjectType.None;
        }
        return ObjectType.OrderbookUpdate;

        totalEvents++;
        //System.Console.Write(Prefix+" "+message.m+" ");
        switch (message.e){
            case "depthUpdate":
                return ObjectType.OrderbookUpdate;
            case "trade":
                return ObjectType.TradesUpdate;
            //case "t.s":
            //    return ObjectType.TradesSubscription;
            //case "ob.s":
            //    return ObjectType.OrderbookSubscription;
            default:
                throw new Exception(" invalid message " + message.e);
        }
    }

}