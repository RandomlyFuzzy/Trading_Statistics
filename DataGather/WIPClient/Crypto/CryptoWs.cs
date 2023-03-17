class CryptoWs : AWebSocketClient<Crypto>
{
    static int totalEvents = 0;
    CancellationToken cancellationToken;
    static Random random = new Random();
    public CryptoWs(string symbol):base(false) { 
        this.symbol = symbol;
    }
    public async Task Connect(CancellationToken cancellationToken = default) {
        this.Prefix = "Crypto " + symbol.Replace("_","");
        this.cancellationToken = cancellationToken;
        await init();
    }
    public async override Task init()
    {
        await ConnectAsync(new Uri("wss://stream.crypto.com/exchange/v1/market"),cancellationToken);
        await SendAsync($@"{{
  ""id"": {random.Next()},
  ""method"": ""subscribe"",
  ""params"": {{
    ""channels"": [""trade.{symbol}"",""book.{symbol}""]
  }}
}}");

    }

    protected override ObjectType MessageType(Crypto message, ObjectDecoder obj)
    {
        totalEvents++;

        switch (message.method)
        {
            case "public/heartbeat":
                SendAsync($@"{{
                            ""id"": {message.id},
                            ""method"": ""public/respond-heartbeat""
                        }}");
                break;
            default:
                break;
        }


        //System.Console.Write(Prefix+" "+message.m+" ");
        switch (message.result.channel)
        {
            case "book":
                return ObjectType.OrderbookUpdate;
            case "trade":
                return ObjectType.TradesUpdate;
            case "":
                return ObjectType.None;
            default:
                throw new Exception(" invalid message " + message.result.channel);
        }
    }

}