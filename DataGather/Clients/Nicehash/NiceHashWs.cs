class NiceHashWs : AWebSocketClient<NiceHash>
{
    static int totalEvents = 0;
    CancellationToken cancellationToken;
    public NiceHashWs(string symbol) { 
        this.symbol = symbol;
    }
    public async Task Connect(CancellationToken cancellationToken = default) {
        this.Prefix = "Nicehash " + symbol;
        this.cancellationToken = cancellationToken;
        await init();
    }
    public async override Task init()
    {
        await ConnectAsync(new Uri("wss://"+symbol+".ws.nicex.com/"),cancellationToken);
        await SendAsync("{\"m\":\"subscribe.trades\"}");
        await SendAsync("{\"m\":\"subscribe.orderbook\"}");
        // await SendAsync("{\"m\":\"subscribe.orders\"}");

    }

    protected override ObjectType MessageType(NiceHash message, ObjectDecoder obj)
    {
        totalEvents++;
        //System.Console.Write(Prefix+" "+message.m+" ");
        switch (message.m)
        {
            case "ob.u":
                return ObjectType.OrderbookUpdate;
            case "ob.s":
                return ObjectType.OrderbookSubscription;
            case "t.u":
                return ObjectType.TradesUpdate;
            case "t.s":
                return ObjectType.TradesSubscription;

            case "o.u":
            case "o.s":
            default:
                throw new Exception(" invalid message " + message.m);
        }
    }

}