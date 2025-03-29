using System;
using System.Text;
using StackExchange.Redis;
using System.Runtime.InteropServices;

Console.SetError(new StreamWriter("error" + string.Join(",", Environment.GetCommandLineArgs().Skip(1)) + ".txt"));


ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(SingletonUtility.REDIS_CONNECTION_STRING);




Dictionary<string,Tuple<List<string>,Dictionary<string,List<string>>>> dict = new Dictionary<string,Tuple<List<string>,Dictionary<string,List<string>>>>();
string[] exchanges = new string[6]{"nicehash","okx","binance","bitstamp", "kraken", "crypto" };
var symbolResponse = new Uri("https://api.binance.com/api/v3/exchangeInfo").Get<BinanceSymbol>().GetAwaiter().GetResult();
var temp = symbolResponse.symbols.Where((a) => a.status != "BREAK").ToList();
for (int i = 0; i < temp.Count(); i++)
{
    string Symbol = temp[i].symbol;
    string exchange = "binance";

    if(!dict.ContainsKey(Symbol)){
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(),new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange+Symbol);

    if(!dict[Symbol].Item2.ContainsKey(exchange)){
        dict[Symbol].Item2[exchange] = new List<string>();
    }
    dict[Symbol].Item2[exchange].Add(Symbol);
    // System.Console.WriteLine(temp[i].symbol);
    // markets[i] = temp[i].symbol;
}


var symbolResponse2 = new Uri("https://api2.nicehash.com/exchange/api/v2/info/status").Get<NiceHashSymbol>().GetAwaiter().GetResult();
var temp2 = symbolResponse2.Symbols.Where((a)=>a.Status != "REMOVED").ToList();
for (int i = 0; i < temp2.Count(); i++)
{
    string Symbol = temp2[i].Symbol;
     string exchange = "nicehash";

    if(!dict.ContainsKey(Symbol)){
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(),new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange+Symbol);

    if(!dict[Symbol].Item2.ContainsKey(exchange)){
        dict[Symbol].Item2[exchange] = new List<string>();
    }
    dict[Symbol].Item2[exchange].Add(Symbol);
    // System.Console.WriteLine(temp2[i].Symbol);
    // markets[i] = temp[i].Symbol;
}



var symbolResponse3 = new Uri("https://www.okx.com/api/v5/market/tickers?instType=SPOT").Get<okxObjectSymbol>().GetAwaiter().GetResult();
var temp3 = symbolResponse3.data;//.Where(a=> !a.instId.Contains("-USD-")).ToList();
for (int i = 0; i < temp3.Count(); i++)
{
    string Symbol = temp3[i].instId;
    Symbol = Symbol.Replace("-","");
    string exchange = "okx";

    if(!dict.ContainsKey(Symbol)){
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(),new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange+Symbol);

    if(!dict[Symbol].Item2.ContainsKey(exchange)){
        dict[Symbol].Item2[exchange] = new List<string>();
    }
    dict[Symbol].Item2[exchange].Add(temp3[i].instId);

    // System.Console.WriteLine(temp3[i].instId);
    // markets[i] = temp[i].symbol;
}


var symbolResponse4 = new Uri("https://www.bitstamp.net/api/v2/ticker/").Get<List<BitstampSymbol>>().GetAwaiter().GetResult();
var temp4 = symbolResponse4;//.Where(a=> !a.instId.Contains("-USD-")).ToList();
for (int i = 0; i < temp4.Count(); i++)
{
    string Symbol = temp4[i].pair;
    Symbol = Symbol.Replace("/","");
    string exchange = "bitstamp";

    if(!dict.ContainsKey(Symbol)){
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(),new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange+Symbol);

    if(!dict[Symbol].Item2.ContainsKey(exchange)){
        dict[Symbol].Item2[exchange] = new List<string>();
    }
    dict[Symbol].Item2[exchange].Add(Symbol);
}


var symbolResponse5 = new Uri("https://api.kraken.com/0/public/AssetPairs?").Get<KrakenSymbol>().GetAwaiter().GetResult();
var temp5 = symbolResponse5.Result.Where(a=> a.Value.status=="online").ToList();
//for (int i = 0; i < temp5.Count(); i++)
foreach (var item in temp5)
{
    string Symbol = item.Value.altname;
    Symbol = Symbol.Replace("/", "");
    string exchange = "kraken";

    if (!dict.ContainsKey(Symbol))
    {
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(), new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange + Symbol);

    if (!dict[Symbol].Item2.ContainsKey(exchange))
    {
        dict[Symbol].Item2[exchange] = new List<string>();
    }
    dict[Symbol].Item2[exchange].Add(item.Value.wsname);
}

var symbolResponse6 = new Uri("https://api.crypto.com/exchange/v1/public/get-instruments").Get<CryptoSymbols>().GetAwaiter().GetResult();
var temp6 = symbolResponse6.result.data.Where(a => a.contract_size == "").ToList();
//for (int i = 0; i < temp5.Count(); i++)
foreach (var item in temp6)
{
    string Symbol = item.symbol;
    Symbol = Symbol.Replace("_", "");
    string exchange = "crypto";

    if (!dict.ContainsKey(Symbol))
    {
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(), new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange + Symbol);

    if (!dict[Symbol].Item2.ContainsKey(exchange))
    {
        dict[Symbol].Item2[exchange] = new List<string>();
    }
    dict[Symbol].Item2[exchange].Add(item.symbol);
}





List<Tuple<string,int>> amt = new List<Tuple<string, int>>();
foreach (var item in dict.Keys)
{
    amt.Add(new Tuple<string, int>(item,dict[item].Item1.Count));
}

amt.Sort((a,b)=>{
    if(a.Item2 == b.Item2) return 0;
    if(a.Item2<b.Item2) return 1;
    return -1;
});








List<string> subto = new List<string>();
amt = amt.Where(a => a.Item1.Length > 3).Where(a => a.Item1.Substring(a.Item1.Length - 3) != "USD").ToList();

foreach (var item in exchanges)
{
    _redis.GetDatabase().KeyDelete(item + "ex");
}

foreach (var item in amt)
{
    _redis.GetDatabase().KeyDelete(item.Item1);
}
foreach (var item in amt)
{
    if(item.Item2<2) continue;


    if(item.Item2>=5 /*|| item.Item1 == "ETHBTC" || item.Item1 == "ETHUSDT" || item.Item1 == "BTCUSDT"*/)
    {
        subto.Add(item.Item1);

        foreach (var item2 in dict[item.Item1].Item2)
        {
            var trac =  _redis.GetDatabase().CreateTransaction();
            System.Console.WriteLine(item2.Key);
            foreach (var item3 in item2.Value)
            {
                _redis.GetDatabase().ListLeftPush(item2.Key+"ex",item3);
                _redis.GetDatabase().ListLeftPush(item.Item1, item2.Key);
            }
            trac.Execute();
        }
        Console.WriteLine(item.Item1);
    }
}


foreach (var item in exchanges)
{
    var entries = _redis.GetDatabase().ListRange(item + "ex");
    System.Console.Write(item + " = ");
    foreach (var item2 in entries)
    {
        System.Console.Write(item2 + " ");
    }
    System.Console.WriteLine();
}


// return;

// string symbol = "";
// do{
//     System.Console.WriteLine("Which symbol do you want to look at");
//     symbol = Console.ReadLine()+"";
//     System.Console.WriteLine(symbol);
// }while(!dict.ContainsKey(symbol));

Console.Clear();
object ob = new object();

Dictionary<string,OrderBookCataloge> cats = new Dictionary<string, OrderBookCataloge>();

// if(Directory.Exists("data")){
//     Directory.Delete("data",true);
// }
// Directory.CreateDirectory("data");


_redis.GetDatabase().KeyDelete("exchanges");



bool Update = true;
int updateAmounts = 0;
int updateTotalsAmounts = 0;
foreach (var item in subto)
{
    _redis.GetDatabase().ListLeftPush("exchanges", item);

    cats[item] = new OrderBookCataloge(item,dict[item].Item1);
    cats[item].PrintHeaders();
    RedisSubscribed.SubscribeTo(item,(obj1,b2)=>{
        updateTotalsAmounts++;
        var obj2 = b2.Deserialize<TradeObj>();
        if(((int)obj1.objType<=2))
        {
            if (cats[item].SubEntry(obj2))
            {
                updateAmounts++;
                Update = true;
                cats[item].PrintMargins();
            }
            return;
        }
        var obj = b2.Deserialize<OrderBookObj>();
        // System.Console.WriteLine(obj.source+obj.symbol+obj.SpecialIDentifier);
        if (cats[item].AddEntry(obj)){
            updateAmounts++;
            Update = true;
            cats[item].PrintMargins();
        }
    });
}


while(!Console.KeyAvailable){
    Thread.Sleep(100);
if(!Update){
    continue;
}
    Console.SetCursorPosition(0,0);
    System.Console.WriteLine(updateAmounts+" / "+updateTotalsAmounts);
    foreach(var cat in cats){
        cat.Value.PrintValues();
        Console.WriteLine();
    }
    Update = false;
}


Console.ReadLine();

foreach (var item in cats)
{
    item.Value.Dispose();
}
