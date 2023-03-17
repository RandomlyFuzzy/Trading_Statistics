using static IOrderBook;

[Serializable]
public class NiceHash:ABasic
{
    public string m { get; set; } = "";
    public NiceHash(){}

    public BasicObj GetBasic(string Symbol){
        return new TradeObj{
            objType = m=="t.s"?ObjectType.TradesSubscription:m=="t.u"?ObjectType.TradesUpdate:m=="ob.s"?ObjectType.OrderbookSubscription:ObjectType.OrderbookUpdate,
            source = "nicehash",
            symbol = Symbol
        };
    }

    public override Type GetTradeType()
    {
        return typeof(NiceHashTrades);
    }

    public override Type GetOrderBookType()
    {
        return typeof(NicehashOrderBook);
    }
}
[Serializable]
public class NiceHashTrades:NiceHash, ITradeObject
{
    public List<NiceHashTradesObj> t { get; set; } = new List<NiceHashTradesObj>();
    public NiceHashTrades(){}

    public List<TradeObj> GetTrades(string Symbol){
        List<TradeObj> ret = new List<TradeObj>();
        BasicObj obj = GetBasic(Symbol);
        foreach (var item in t)
        {
            TradeObj temp = new TradeObj(obj);
            temp.Dir = item.d=="BUY"?BuyDirection.SELL:BuyDirection.BUY;
            temp.Price = item.p;
            temp.Quantity = item.q+item.sq;
            temp.TimeStamp = item.ts;
            ret.Add(temp);
        }
        return ret;
    }
}
[Serializable]
public class NiceHashTradesObj{
    public string d { get; set; } = "";
    public double p { get; set; } = 0;
    public double q { get; set; } = 0;
    public double sq { get; set; } = 0;
    public long ts { get; set; } = 0;
    public NiceHashTradesObj(){}
}

[Serializable]
public class NicehashOrderBook:NiceHash, IOrderBook
{
    public long t { get; set; } = 0;
    public List<double[]> b { get; set; }  = new List<double[]>();
    public List<double[]> s { get; set; } = new List<double[]>();

    public NicehashOrderBook(){}


    public List<OrderBookObj> GetOrderBook(string Symbol){
        List<OrderBookObj> ret = new List<OrderBookObj>();
        BasicObj obj = GetBasic(Symbol);
        foreach (var item in b)
        {
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = t;
            obo.Dir = BuyDirection.BUY;
            obo.price = item[0];
            obo.Quantity = item[1];
            ret.Add(obo);
        }   

        foreach (var item in s)
        {
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = t;
            obo.Dir = BuyDirection.SELL;
            obo.price = item[0];
            obo.Quantity = item[1];
            ret.Add(obo);
        }
        return ret;
    }


}
[Serializable]
public class NicehashOrderStream:NiceHash{
    public NicehashOrderStreamObj o = new NicehashOrderStreamObj();

    public NicehashOrderStream(){}
}
[Serializable]
public class NicehashOrderStreamObj{

    public string i = "",t= "",d= "",s= "";
    public double p=0,oq=0,osq=0,eq=0,esq=0;
    public long sts=0,uts=0;
    public NicehashOrderStreamObj(){}

}


[Serializable]
public class NiceHashSymbolObj
{
    public string Symbol { get; set; }
    public string Status { get; set; }
    public string[] OrderTypes { get; set; }
    public int BaseAssetPrecision { get; set; }
    public int QuoteAssetPrecision { get; set; }
    public int PriceAssetPrecision { get; set; }
    public decimal PriMinAmount { get; set; }
    public decimal PriMaxAmount { get; set; }
    public decimal SecMinAmount { get; set; }
    public decimal SecMaxAmount { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string BaseAsset { get; set; }
    public string QuoteAsset { get; set; }
    public NiceHashSymbolObj(){}
}
[Serializable]
public class NiceHashSymbol
{
    public NiceHashSymbolObj[] Symbols { get; set; }
    public NiceHashSymbol(){}
}