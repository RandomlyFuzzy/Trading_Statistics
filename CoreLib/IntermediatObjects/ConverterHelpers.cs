using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

[Serializable]
public enum ObjectType{
    None,
    TradesUpdate,
    TradesSubscription,
    OrderbookUpdate,
    OrderbookSubscription
}
[Serializable]
public enum BuyDirection{
    BUY,
    SELL
}

public abstract class ABasic : IBasic
{
    protected Type GetCurret { get { return this.GetType(); } }
    public virtual IBasic GetObject(string message)
    {
        MethodInfo method3 = GetCurret.GetMethod("GetObjectAs");
        method3 = method3.MakeGenericMethod(GetCurret);
        var oto2 = method3.Invoke(this, new object[1] { message }) as IBasic;

        return oto2;
    }
    public virtual T GetObjectAs<T>(string message) where T : IBasic, new()
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true,

        };
        string temp = "{\"m\":\"t.s\"";
        if (message.Substring(0, temp.Length) == temp) {
            temp = "";
        
        }

        message = message.Trim();
        if (message.Last() == '@')
        {
            message = message.Substring(0, message.Count() - 1);
        }


        message = message.Replace("]P", "]");
        message = message.Replace("@_ ", "0\"");
        message = message.Replace((char)0, ' ');
        message = message.Replace("@Y", "00\"");
        message = message.Replace("p_", "00\"");
        message = message.Replace("}00\"", "}");
        message = message.Replace("`", "\"");
        message = message.Replace("\"0_", "\"");
        message = message.Replace("\"P_", "\"}]}");
        message = message.Replace("^", "");
        message = message.Replace("@_","\"");
        message = message.Replace(" _", "\"");
        message = message.Replace("}}]", "}]}");

        if (message.First() == '{' && message.Last() !='}') {
            message += "}";
        }
        if (message.First() == '[' && message.Last() != ']')
        {
            message += "]";
        }
        if (message.Count(a => a == '{' || a == '}') % 2 == 1) { 
            message += "}";
        }
        if (message.Count(a => a == '[' || a == ']') % 2 == 1)
        {
            message += "]";
        }

        T ret = new ();
        try { 
            ret = JsonSerializer.Deserialize<T>(message, options); ;
        
        }catch(Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
        }

        return ret;
    }
    public abstract Type GetOrderBookType();
    public abstract Type GetTradeType();
}


[Serializable]
public class BasicObj{
    public readonly DateTime createTime = DateTime.Now;
    public DateTime dt;
    public long TimeStamp = 0;
    public string source = "";
    public string symbol = "";
    public ObjectType objType = ObjectType.None;
    public string SpecialIDentifier = "";
    public BuyDirection Dir;

    public BasicObj(){
        dt = DateTime.Now;
    }
    public BasicObj(BasicObj ob){
        dt = ob.dt;
        TimeStamp = ob.TimeStamp;
        source = ob.source;
        symbol = ob.symbol;
        objType = ob.objType;
        SpecialIDentifier = ob.SpecialIDentifier;
    }
}
[Serializable]
public class TradeObj:BasicObj{
    public double Price = 0;
    public double Quantity = 0; 
    public TradeObj(){}
public TradeObj(BasicObj obj):base(obj){}
}
[Serializable]
public class OrderBookObj:BasicObj,IComparable<OrderBookObj>{

    public long id = -1;
    public double price = 0;
    public double Quantity = 0;

    public OrderBookObj(){}
    public OrderBookObj(BasicObj obj):base(obj){}

    public int CompareTo(OrderBookObj? other)
    {
        if(other is null||price>other.price) return -1;
        return 1;
    }
}


public interface IBasic {
    public Type GetTradeType();
    public Type GetOrderBookType();
    IBasic GetObject(string message);
}
public interface IOrderBook
{
    public List<OrderBookObj> GetOrderBook(string Symbol);
}
public interface ITradeObject {
    public List<TradeObj> GetTrades(string symbol);
}


public class BasicObjCompairer : IComparer<BasicObj>
{
    public int Compare(BasicObj? x, BasicObj? y)
    {
        if (x.createTime < y.createTime) return -1;
        return 1;
    }
}