using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Serializable]
public class Kraken : IBasic
{
    public string @event { get; set; } = "";
    public string channelName { get; set; } = "";
    public string Symbol { get; set; } = "";


    public Kraken() { }

    public BasicObj GetBasic(string Symbol)
    {
        return new TradeObj
        {
            objType = channelName.Contains("book")?ObjectType.OrderbookUpdate:ObjectType.TradesUpdate,
            source = "kraken",
            symbol = Symbol.Replace("/", "")
        };
    }

    public Type GetTradeType()
    {
        return typeof(KrakenTrades);
    }

    public Type GetOrderBookType()
    {
        return typeof(KrakenOrderBook);
    }
    public virtual IBasic GetObject(string message)
    {
        if (message[0] == '{') { 
            return JsonConvert.DeserializeObject<Kraken>(message);
        }

        var deserializedObject = JsonConvert.DeserializeObject<object[]>(message);
        Kraken ret = new Kraken();
        if (deserializedObject.Count() == 4)
        {
            ret.channelName = deserializedObject[2].ToString();
            ret.Symbol = deserializedObject[3].ToString();
        }
        else
        {
            ret.channelName = deserializedObject[3].ToString();
            ret.Symbol = deserializedObject[4].ToString();
        }
        return ret;
    }
   
}
[Serializable]
public class KrakenTrades : Kraken, ITradeObject
{
    public List<List<string>> trades = new List<List<string>>();
    public KrakenTrades() { }

    public List<TradeObj> GetTrades(string Symbol)
    {
        List<TradeObj> ret = new List<TradeObj>();
        BasicObj obj = GetBasic(Symbol);
        foreach (var item in trades)
        {
            TradeObj temp = new TradeObj(obj);
            temp.Price = double.Parse(item[0]);
            temp.Quantity = double.Parse(item[1]);
            temp.TimeStamp = long.Parse(item[2].Replace(".", "") + "0");
            temp.Dir = item[3] =="b" ? BuyDirection.BUY : BuyDirection.SELL;
            temp.SpecialIDentifier = item[5];
            ret.Add(temp);
        }
        return ret;
    }
    public override IBasic GetObject(string message)
    {
        if (message[0] == '{')
        {
            return JsonConvert.DeserializeObject<KrakenTrades>(message);
        }

        var deserializedObject = JsonConvert.DeserializeObject<object[]>(message);
        KrakenTrades ret = new KrakenTrades();
        ret.trades = JsonConvert.DeserializeObject<List<List<string>>>(deserializedObject[1].ToString());
        ret.channelName = deserializedObject[2].ToString();
        ret.Symbol = deserializedObject[3].ToString();
        return ret;
    }
}

[Serializable]
public class KrakenOrderBook : Kraken, IOrderBook
{
    public KrakenOrderBookobject orders = new KrakenOrderBookobject();
    public string id = "";
    public KrakenOrderBook() { }

    public List<OrderBookObj> GetOrderBook(string Symbol)
    {
        List<OrderBookObj> ret = new List<OrderBookObj>();
        BasicObj obj = GetBasic(Symbol);
        foreach (var item in orders.@as)
        {
            if (item.Count == 4) continue;
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = id != ""?long.Parse(id):-1;
            obo.Dir = BuyDirection.SELL;
            obo.price = double.Parse(item[0]);
            obo.Quantity = double.Parse(item[1]);
            obo.TimeStamp = long.Parse(item[2].Replace(".", "") + "0");
            ret.Add(obo);
        }

        foreach (var item in orders.bs)
        {
            if (item.Count == 4) continue;
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = id != ""?long.Parse(id):-1;
            obo.Dir = BuyDirection.BUY;
            obo.price = double.Parse(item[0]);
            obo.Quantity = double.Parse(item[1]);
            obo.TimeStamp = long.Parse(item[2].Replace(".", "") + "0");
            ret.Add(obo);
        }
        foreach (var item in orders.a)
        {
            if (item.Count == 4) continue;
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = id != "" ? long.Parse(id) : -1;
            obo.Dir = BuyDirection.BUY;
            obo.price = double.Parse(item[0]);
            obo.Quantity = double.Parse(item[1]);
            obo.TimeStamp = long.Parse(item[2].Replace(".", "") + "0");
            ret.Add(obo);
        }

        foreach (var item in orders.b)
        {
            if (item.Count == 4) continue;
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = id != "" ? long.Parse(id) : -1;
            obo.Dir = BuyDirection.SELL;
            obo.price = double.Parse(item[0]);
            obo.Quantity = double.Parse(item[1]);
            obo.TimeStamp = long.Parse(item[2].Replace(".", "") + "0");
            ret.Add(obo);
        }
        return ret;
    }
    public override IBasic GetObject(string message)
    {
        if (message[0] == '{')
        {
            return JsonConvert.DeserializeObject<KrakenOrderBook>(message);
        }


        message = message.Replace($@"}},{{", ",");
        var deserializedObject = JsonConvert.DeserializeObject<object[]>(message);



        KrakenOrderBook ret = new KrakenOrderBook();
        if (deserializedObject.Count() == 4)
        {
            ret.orders = JsonConvert.DeserializeObject<KrakenOrderBookobject>(deserializedObject[1].ToString());
            ret.channelName = deserializedObject[2].ToString();
            ret.Symbol = deserializedObject[3].ToString();
        }
        else {
            var a = JsonConvert.DeserializeObject<KrakenOrderBookobject>(deserializedObject[1].ToString());
            var b = JsonConvert.DeserializeObject<KrakenOrderBookobject>(deserializedObject[2].ToString());
            ret.orders = new KrakenOrderBookobject();
            ret.orders.@as = a.a;
            ret.orders.@as = b.b;
            ret.id = b.c;
            ret.channelName = deserializedObject[3].ToString();
            ret.Symbol = deserializedObject[4].ToString();
        }


        return ret;
    }
}


[Serializable]
public class KrakenOrderBookobject
{
    public List<List<string>> @as = new List<List<string>>();
    public List<List<string>> bs = new List<List<string>>();
    public List<List<string>> a = new List<List<string>>();
    public List<List<string>> b = new List<List<string>>();
    public string c;
}

[Serializable]
public class KrakenSymbol
{
    public List<string> Error { get; set; }
    public Dictionary<string, KrakenSymbolObjects> Result { get; set; } = new Dictionary<string, KrakenSymbolObjects>();
    public KrakenSymbol() { }
}

[Serializable]
public class KrakenSymbolObjects
{
    public string altname { get; set; } = "";
    public string wsname { get; set; } = "";
    public string aclass_base { get; set; } = "";
    public string @base { get; set; } = "";
    public string aclass_quote { get; set; } = "";
    public string quote { get; set; } = "";
    public string lot { get; set; } = "";
    public decimal costdecimals { get; set; } = 0;
    public decimal pairdecimals { get; set; } = 0;
    public decimal lotdecimals { get; set; } = 0;
    public decimal lotmultiplier { get; set; } = 0;
    public List<List<decimal>> fees { get; set; } = new List<List<decimal>>();
    public List<List<decimal>> feesmaker { get; set; } = new List<List<decimal>>();
    public string feevolumecurrency { get; set; } = "";
    public decimal margincall { get; set; } = 0;
    public decimal marginstop { get; set; } = 0;
    public string ordermin { get; set; } = "";
    public string costmin { get; set; } = "";
    public string ticksize { get; set; } = "";
    public string status { get; set; } = "";
    public KrakenSymbolObjects() { }
}