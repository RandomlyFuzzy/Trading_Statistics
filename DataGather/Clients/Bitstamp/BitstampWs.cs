class BitStampWs : AWebSocketClient<Bitstamp>
{
    static int totalEvents = 0;
    CancellationToken cancellationToken;
    public BitStampWs(string symbol) { 
        this.symbol = symbol.ToLower();
    }
    public async Task Connect(CancellationToken cancellationToken = default) {
        this.Prefix = "bitstamp " + symbol.ToUpper();
        this.cancellationToken = cancellationToken;
        await init();
    }
    public async override Task init()
    {

        await ConnectAsync(new Uri("wss://ws.bitstamp.net/"),cancellationToken);

        await SendAsync($@"
        {{
            ""event"": ""bts:subscribe"",
            ""data"": {{
                ""channel"": ""live_trades_{this.symbol}""
            }}
        }}");
        await SendAsync($@"
        {{
            ""event"": ""bts:subscribe"",
            ""data"": {{
                ""channel"": ""live_orders_{this.symbol}""
            }}
        }}");
     

    }

    protected override ObjectType MessageType(Bitstamp message, ObjectDecoder obj)
    {
        totalEvents++;

        if(message.data.type >=0){
            return ObjectType.TradesUpdate;
        }

        if(message.data.order_type >=0){
            return ObjectType.OrderbookUpdate;
        }

        return ObjectType.None;

    }

}