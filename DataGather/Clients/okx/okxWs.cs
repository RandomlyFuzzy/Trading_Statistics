using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class okxWs : AWebSocketClient<okxObjectsbasic>
{
    string url = "wss://ws.okx.com:8443/ws/v5/public";
    public string symbolCleaned ="";

    CancellationToken cancellationToken;
    public okxWs(string symbol) {
        this.symbolCleaned = symbol;
        this.symbol = symbol.Replace("-","");
        this.Prefix = "OKX " + this.symbol+" "+ symbolCleaned;
    }
    public async Task Connect(CancellationToken cancellationToken = default)
    {
        this.cancellationToken  = cancellationToken;
        await init();
    }


    public async override Task init()
    {
        await ConnectAsync(new Uri(url), cancellationToken);
        SendAsync(@$"
        {{
          ""op"": ""subscribe"",
          ""args"": [{{
              ""channel"": ""books"",
              ""instId"": ""{symbolCleaned}""
            }},
            {{
              ""channel"": ""trades"",
             ""instId"": ""{symbolCleaned}""
            }}]  
        }}
        ");
        // await SendAsync("{\"m\":\"subscribe.orders\"}");

    }
    protected override ObjectType MessageType(okxObjectsbasic message, ObjectDecoder obj)
    {
        // Console.WriteLine(message.arg.channel+" "+ message.arg.instId+" "+message.@event+""+message.action);
        switch (message.arg.channel + message.@event + message.action)
        {
            case "tradessubscribe":
                return ObjectType.TradesSubscription;
            case "trades":
                return ObjectType.TradesUpdate;
            case "bookssubscribe":
            case "bookssnapshot":
                return ObjectType.OrderbookSubscription;
            case "booksupdate":
                return ObjectType.OrderbookUpdate;
            default:
                throw new Exception("invalide ObjectType " + message.arg.channel + message.@event + message.action);
        }
    }
}
