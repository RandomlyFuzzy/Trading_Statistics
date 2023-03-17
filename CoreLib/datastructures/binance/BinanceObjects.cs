public class BinanceBasicObject : ABasic
{
    public string? result  {get;set;}= "";
    public long  id        {get;set;}= -1;
    public string e        {get;set;}="";
    public long E          {get;set;} =0 ;
    public string s        {get;set;}= "";


    public BinanceBasicObject() { }
    public override Type GetOrderBookType()
    {
        return typeof(BinanceOrderBook);
    }

    public override Type GetTradeType()
    {
        return typeof(BinanceTrade);
    }


    public BasicObj GetBasic(string symbol){
        return new TradeObj{
            objType = e=="depthUpdate"?ObjectType.OrderbookUpdate:ObjectType.TradesUpdate,
            source = "binance",
            symbol = symbol
        };


    }

}

public class BinanceTrade : BinanceBasicObject, ITradeObject
{
    public List<BinanaceTradeObject> trades { get; set; } = new List<BinanaceTradeObject>();

    public BinanceTrade() { }
    public List<TradeObj> GetTrades(string symbol)
    {
        List<TradeObj> ret = new List<TradeObj>();
        BasicObj obj = GetBasic(symbol);
        foreach (var item in trades)
        {
            TradeObj temp = new TradeObj(obj);
            temp.Dir = item.isBuyerMaker ? BuyDirection.BUY : BuyDirection.SELL;
            temp.Price = item.price;
            temp.Quantity = item.qty;
            temp.TimeStamp = item.time;
            ret.Add(temp);
        }
        
        return ret;
    }
}

public class BinanaceTradeObject
{
    public long id { get; set; } = 0;
    public double price { get; set; } = 0;
    public double qty { get; set; } = 0;
    public long time { get; set; } = 0;
    public bool isBuyerMaker { get; set; } = false;
    public bool isBestMatch { get; set; } = false;

}




public class BinanceOrderBook : BinanceBasicObject, IOrderBook
{
    public long U { get; set; } = 0;
    public long u {get;set;}= 0;
    public List<string[]> b {get;set;}= new List<string[]>();
    public List<string[]> a { get; set; } = new List<string[]>();

    public BinanceOrderBook() { }
    public List<OrderBookObj> GetOrderBook(string Symbol)
    {
        List<OrderBookObj> ret = new List<OrderBookObj>();
        BasicObj obj = GetBasic(Symbol);
        foreach (var item in a)
        {
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = E;
            obo.Dir = BuyDirection.SELL;
            obo.price = double.Parse(item[0]);
            obo.Quantity = double.Parse(item[1]);
            ret.Add(obo);
        }   

        foreach (var item in b)
        {
            OrderBookObj obo = new OrderBookObj(obj);
            obo.id = E;
            obo.Dir = BuyDirection.BUY;
            obo.price = double.Parse(item[0]);
            obo.Quantity = double.Parse(item[1]);
            ret.Add(obo);
        }
        return ret;
    }
}


public class BinanceSymbol{
    public string timezone = "";
    public long serverTime = 0;
    public List<BinanceSymbolObject> symbols = new List<BinanceSymbolObject>();

}

public class BinanceSymbolObject{
    public string symbol;
    public string status;
}

public class BinanceError{
    public string msg;
    public long code;
}