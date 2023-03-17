using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class KrakenWs : AWebSocketClient<Kraken>
{
    CancellationToken cancellationToken;
    public string SymbolUnclean = "";
    public KrakenWs(string Symbok){
        SymbolUnclean = Symbok;
        this.symbol = SymbolUnclean.Replace("/","");
    }
    public async Task Connect(CancellationToken cancellationToken = default)
    {
        this.Prefix = "kraken " + symbol;
        this.cancellationToken = cancellationToken;
        await init();
    }

    public override async Task init()
    {
        await ConnectAsync(new Uri("wss://ws.kraken.com/"), cancellationToken);
        SendAsync($@"
        {{
          ""event"": ""subscribe"",
          ""pair"": [
            ""{SymbolUnclean}""
          ],
          ""subscription"": {{
            ""name"": ""book""
          }}
        }}");
        SendAsync($@"
        {{
          ""event"": ""subscribe"",
          ""pair"": [
            ""{SymbolUnclean}""
          ],
          ""subscription"": {{
            ""name"": ""trade""
          }}
        }}");
    }

    protected override ObjectType MessageType(Kraken message, ObjectDecoder obj)
    {
        switch (message.@event)
        {
            case "systemStatus":
            case "subscriptionStatus":
            case "heartbeat":
            case "error":
                return ObjectType.None;
            default:
                break;
        }
        switch (message.channelName)
        {
            case "book-10":
                return ObjectType.OrderbookUpdate;
            case "trade":
                return ObjectType.TradesUpdate;
            default:
                break;
        }
        throw new NotImplementedException();
    }
}
