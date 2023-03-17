using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IOrderBook;

[Serializable]
public class okxObjectsbasic : ABasic
{
    public string @event        {get; set;}= "";
    public string op            {get; set;}= "";
    public string code          {get; set;}= "";
    public string msg           {get; set;}= "";
    public string action { get; set; }= "";
    public okxObjectsbasicargs arg { get; set; } = new okxObjectsbasicargs();
    public okxObjectsbasic() { }
    public BasicObj GetBasic(string symbol){
        ObjectType obj;
        switch (this.arg.channel+this.@event+this.action)
        {
            case "tradessubscribe":
                obj = ObjectType.TradesSubscription;
                break;
            case "trades":
                obj = ObjectType.TradesUpdate;
                break;
            case "bookssubscribe":
            case "bookssnapshot":
                obj = ObjectType.OrderbookSubscription;
                break;
            case "booksupdate":
                obj = ObjectType.OrderbookUpdate;
                break;
            default:
                obj = ObjectType.None;
                throw new Exception("invalide ObjectType "+this.arg.channel+this.@event+this.action);
            break;
        }
        return new BasicObj{
            source="okx",
            objType = obj,
            symbol = symbol
        };
    }

    public override Type GetTradeType()
    {
        return typeof(okxObjectsTrades);
    }

    public override Type GetOrderBookType()
    {
        return typeof(okxObjectsOrderBook);
    }

}
[Serializable]
public class okxObjectsbasicargs
{
    public string channel       {get; set;}= "";
    public string ccy           {get; set;}= "";
    public string uid           {get; set;}= "";
    public string instType      {get; set;}= "";
    public string instId        {get; set;}= "";
    public okxObjectsbasicargs() { }
}
[Serializable]
public class okxObjectsTrades: okxObjectsbasic, ITradeObject
{
    public List<okxObjectsTradesData> data { get; set; } = new List<okxObjectsTradesData>();
    public okxObjectsTrades() { }
    public List<TradeObj> GetTrades(string symbol){
        List<TradeObj> ret = new List<TradeObj>();
        BasicObj obj = GetBasic(symbol);
        foreach (var item in data)
        {
            TradeObj temp = new TradeObj(obj);
            temp.Dir = item.side =="buy"?BuyDirection.SELL:BuyDirection.BUY;
            temp.Price = Double.Parse(item.px);
            temp.Quantity = Double.Parse(item.sz);
            temp.TimeStamp = long.Parse(item.ts);
            temp.SpecialIDentifier = arg.instId;
            ret.Add(temp);
        }
        return ret;
    }
}
[Serializable]
public class okxObjectsTradesData
{
    public string instId    {get; set;}= "";
    public string tradeId   {get; set;}= "";
    public string px        {get; set;}= "";
    public string sz        {get; set;}= "";
    public string side      {get; set;}= "";
    public string ts        {get; set;}= "";
    public okxObjectsTradesData() { }
}

public class okxObjectsOrderBook : okxObjectsbasic, IOrderBook
{
    public List<okxObjectsOrderBookData> data { get; set; } = new List<okxObjectsOrderBookData>();
    public okxObjectsOrderBook() { }
    public List<OrderBookObj> GetOrderBook(string Symbol){
        List<OrderBookObj> ret = new List<OrderBookObj>();
        BasicObj obj = GetBasic(Symbol);
        foreach (var item2 in data)
        {
            var b = item2.asks;
            var s = item2.bids;
            var t = item2.checksum;
            foreach (var item in b)
            {
                OrderBookObj obo = new OrderBookObj(obj);
                obo.id = t;
                obo.Dir = BuyDirection.SELL;
                obo.price = double.Parse(item[0]);
                obo.Quantity = double.Parse(item[1]);
                obo.SpecialIDentifier = arg.instId;
                ret.Add(obo);
            }   

            foreach (var item in s)
            {
                OrderBookObj obo = new OrderBookObj(obj);
                obo.id = t;
                obo.Dir = BuyDirection.BUY;
                obo.price = double.Parse(item[0]);
                obo.Quantity = double.Parse(item[1]);
                obo.SpecialIDentifier = arg.instId;
                ret.Add(obo);
            }
        }

        return ret;
    }

}
public class okxObjectsOrderBookData {
    public List<string[]> asks {get; set;}= new List<string[]>();
    public List<string[]> bids { get; set; } = new List<string[]>();
    public string ts {get; set;}= "";
    public int checksum = 0;
    public okxObjectsOrderBookData() { }
}


public class okxObjectSymbol:okxObjectsbasic{
    public List<okxObjectSymbolData> data { get; set; } = new List<okxObjectSymbolData>();
    public okxObjectSymbol() { }
}

public class okxObjectSymbolData {
    public string instId {get; set;}= "";
    public okxObjectSymbolData() { }
}