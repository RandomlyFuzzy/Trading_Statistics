using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public abstract class CryptoBase<T> : ABasic where T : class,new ()
{
    public long id { get; set; } = -1;
    public long code { get; set; } = 0;
    public string method { get; set; } = "";
    public string channel { get; set; } = "";
    public T result { get; set; } = new T();

    public override Type GetOrderBookType() {
        return typeof(CryptoOrderBook);
    }

    public override Type GetTradeType() { 
    
        return typeof(CryptoTrades);

    }

    public BasicObj GetBasic(string symbol)
    {
        return new BasicObj
        {
            source = "crypto",
            //objType = obj,
            symbol = symbol.Replace("_","")
        };
    }

}
public class Crypto : CryptoBase<CryptoSimple<object>> { }

public class CryptoOrderBook : CryptoBase<CryptoSimple<List<CryptoBook>>>, IOrderBook
{
    public List<OrderBookObj> GetOrderBook(string Symbol)
    {
        List<OrderBookObj> ret = new List<OrderBookObj>();
        BasicObj obj = GetBasic(Symbol);
        obj.objType = ObjectType.OrderbookUpdate;
        foreach (var item in result.data[0].bids)
        {
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = id;
            obo.Dir = BuyDirection.BUY;
            obo.price = double.Parse(item[0]);
            obo.Quantity = double.Parse(item[1]);
            ret.Add(obo);
        }

        foreach (var item in result.data[0].asks   )
        {
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = id;
            obo.Dir = BuyDirection.SELL;
            obo.price = double.Parse(item[0]);
            obo.Quantity = double.Parse(item[1]);
            ret.Add(obo);
        }
        return ret;
    }
}

public class CryptoTrades : CryptoBase<CryptoSimple<List<CryptoTradesObject>>>, ITradeObject
{
    public List<TradeObj> GetTrades(string symbol){
        List<TradeObj> ret = new List<TradeObj>();
        BasicObj obj = GetBasic(symbol);
        obj.objType = ObjectType.TradesUpdate;
        foreach (var item in result.data)
        {
            TradeObj temp = new TradeObj(obj);
            temp.Dir = item.s == "BUY" ? BuyDirection.SELL : BuyDirection.BUY;
            temp.Price = double.Parse(item.p);
            temp.Quantity = double.Parse(item.q);
            temp.TimeStamp = item.t;
            temp.SpecialIDentifier = item.i;
            ret.Add(temp);
        }
        return ret;
    }
}

public class CryptoTradesObject{
    public string s { get; set; } = "";
    public string i { get; set; } = "";
    public string p { get; set; } = "";
    public string q { get; set; } = "";
    public long t { get; set; } = 0;
    public string d { get; set; } = "";
    public CryptoTradesObject() { }
}

public class CryptoSimple<T> where T:class, new()
{
    public string instrument_name { get; set; } = "";
    public string subscription { get; set; } = "";
    public string channel { get; set; } = "";
    public T data { get; set; } = new T();

}

public class CryptoBook
{
    public List<List<string>> asks { get; set; } = new List<List<string>>();  
    public List<List<string>> bids { get; set; } = new List<List<string>>();
    public long t { get; set; } = 0;
    public long tt { get; set; } = 0;
    public long u { get; set; } = 0;
}


public class CryptoSymbols : CryptoBase<CryptoSimple<List<CryptoSymbolsObject>>> { }

public class CryptoSymbolsObject {
    public string symbol { get; set; } = "";
    public string contract_size { get; set; } = "";


}