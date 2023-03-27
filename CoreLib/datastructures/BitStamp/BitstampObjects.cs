
[Serializable]
public class Bitstamp: ABasic
{
    public string @event { get; set; } = "";
    public string channel { get; set; } = "";

    public BitstampObj data { get; set; } = new BitstampObj();


    public Bitstamp() { }
   public  BasicObj GetBasic(string Symbol){

        return new TradeObj{
            objType = data. type>=0?ObjectType.TradesUpdate:ObjectType.OrderbookUpdate,
            source = "bitstamp",
            symbol = Symbol.ToUpper(),
        };
    }
    public override Type GetTradeType()
    {
        return typeof(BitstampTrade);
    }

    public override Type GetOrderBookType()
    {
        return typeof(BitstampOrderbook);
    }
}


public class BitstampObj {
    public long id { get; set; } = 0;
    public string timestamp { get; set; } = "";
    public string microtimestamp { get; set; } = "";
    public double amount { get; set; } = 0;
    public double price { get; set; } = 0;
    public int type { get; set; } = -1;
    public int order_type { get; set; } = -1;
    public BitstampObj() { }
}





[Serializable]

public class BitstampOrderbook:Bitstamp,IOrderBook{

    public BitstampOrderbook() { }

    public List<OrderBookObj> GetOrderBook(string Symbol){
        List<OrderBookObj> ret = new List<OrderBookObj>();
        BasicObj obj = GetBasic(Symbol);
        OrderBookObj obj2 = new OrderBookObj(obj);  
        obj2.Quantity = data.amount;
        obj2.price = data.price;
        obj2.Dir = (BuyDirection)((data.order_type));
        obj2.id = data.id;
        ret.Add(obj2);
        return ret;
    }
}


[Serializable]
public class BitstampTrade:Bitstamp,ITradeObject{

    public long buy_order_id  {get; set;}= 0;
    public long sell_order_id  {get; set;}=0;
    public BitstampTrade() { }

    public List<TradeObj> GetTrades(string Symbol){
        List<TradeObj> ret = new List<TradeObj>();
        BasicObj obj = GetBasic(Symbol);
        TradeObj obj2 = new TradeObj(obj);  
        obj2.Dir = (BuyDirection)data.type;
        obj2.Price = data.price;
        obj2.Quantity = data.amount;
        ret.Add(obj2);
        return ret;
    }
    
}



[Serializable]
public class BitstampSymbol{
    public string pair {get; set;}= "";
    public BitstampSymbol(){}
}