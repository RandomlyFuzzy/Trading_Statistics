public class OrderBookCataloge:IDisposable{

    string symbol = "";
    Dictionary<string,OrderBook> orderbooks = new Dictionary<string,OrderBook>();
    object ob = new object();
    StreamWriter sw;
    bool needHeader = false;

    public OrderBookCataloge(string Symbol,List<string> exchanges){
        this.symbol = Symbol;
        foreach(var item in exchanges){
            orderbooks[item] = new OrderBook();
        }
        if (!Directory.Exists("data")) { 
            Directory.CreateDirectory("data");
        }
        if (!File.Exists("data/" + this.symbol + ".csv"))
        {
            needHeader = true;
            this.sw = new StreamWriter("data/" + this.symbol + ".csv", false);
        }
        else { 
            this.sw = new StreamWriter("data/" + this.symbol + ".csv", true);
        }
        this.sw.AutoFlush = true;
    }
    public bool SubEntry(TradeObj obj)
    {
        string key = obj.source+obj.symbol;
        return orderbooks[key].Subtract(obj);

    }
    public bool AddEntry(OrderBookObj obj){
        string key = obj.source + obj.symbol;

        if (obj.objType == ObjectType.OrderbookUpdate){
            return orderbooks[key].Update(obj);
        }

        return orderbooks[key].Set(obj);

    }

    public void Dispose()
    {
        sw.Close();
    }

    public void PrintHeaders(){
        if(!needHeader) return;
        lock(sw){
            sw.Write("time,");
            foreach(var item in orderbooks){
                sw.Write(item.Key+"bidMax");
                sw.Write(",");
                sw.Write(item.Key+"sellMin");
                sw.Write(",");
            }
            sw.WriteLine();
        }
    }
    public void PrintMargins(){
        lock (sw) {
            var v = DateTime.Now;
            string toWrite = v.ToLongDateString()+" "+v.ToLongTimeString()+ ",";

            foreach (var item in orderbooks) {
                toWrite += (item.Value.PrintMargins() + ",");
                item.Value.UpdateOrderBook(item.Key);
            }
            if (toWrite.IndexOf(",0,") != -1)
            {
                return;
            }
            PublisherUtilities.PublishData(symbol, "UPD");

            sw.Write(toWrite);
            sw.WriteLine();
        }
    }



    public void PrintValues(){
        if(orderbooks.Keys.Count() == 0) return;
        lock(ob){
            foreach (var item in orderbooks)
            {
                System.Console.Write(item.Key+" ");
                Console.WriteLine(item.Value.Print());
            }
        }
    }

   
}