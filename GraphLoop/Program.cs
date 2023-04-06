
using System.Linq;

Dictionary<string, Tuple<List<string>, Dictionary<string, List<string>>>> dict = new Dictionary<string, Tuple<List<string>, Dictionary<string, List<string>>>>();
string[] exchanges = new string[6] { "nicehash", "okx", "binance", "bitstamp", "kraken", "crypto" };
var symbolResponse = new Uri("https://api.binance.com/api/v3/exchangeInfo").Get<BinanceSymbol>().GetAwaiter().GetResult();
var temp = symbolResponse.symbols.Where((a) => a.status != "BREAK").ToList();
for (int i = 0; i < temp.Count(); i++)
{
    string Symbol = temp[i].symbol;
    string exchange = "binance";

    if (!dict.ContainsKey(Symbol))
    {
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(), new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange + Symbol);

    if (!dict[Symbol].Item2.ContainsKey(exchange))
    {
        dict[Symbol].Item2[exchange] = new List<string>();
    }
    dict[Symbol].Item2[exchange].Add(Symbol);
    // System.Console.WriteLine(temp[i].symbol);
    // markets[i] = temp[i].symbol;
}


var symbolResponse2 = new Uri("https://api2.nicehash.com/exchange/api/v2/info/status").Get<NiceHashSymbol>().GetAwaiter().GetResult();
var temp2 = symbolResponse2.Symbols.Where((a) => a.Status != "REMOVED").ToList();
for (int i = 0; i < temp2.Count(); i++)
{
    string Symbol = temp2[i].Symbol;
    string exchange = "nicehash";

    if (!dict.ContainsKey(Symbol))
    {
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(), new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange + Symbol);

    if (!dict[Symbol].Item2.ContainsKey(exchange))
    {
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
    Symbol = Symbol.Replace("-", "");
    string exchange = "okx";

    if (!dict.ContainsKey(Symbol))
    {
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(), new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange + Symbol);

    if (!dict[Symbol].Item2.ContainsKey(exchange))
    {
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
    Symbol = Symbol.Replace("/", "");
    string exchange = "bitstamp";

    if (!dict.ContainsKey(Symbol))
    {
        dict[Symbol] = new Tuple<List<string>, Dictionary<string, List<string>>>(new List<string>(), new Dictionary<string, List<string>>());
    }
    dict[Symbol].Item1.Add(exchange + Symbol);

    if (!dict[Symbol].Item2.ContainsKey(exchange))
    {
        dict[Symbol].Item2[exchange] = new List<string>();
    }
    dict[Symbol].Item2[exchange].Add(Symbol);
}


var symbolResponse5 = new Uri("https://api.kraken.com/0/public/AssetPairs?").Get<KrakenSymbol>().GetAwaiter().GetResult();
var temp5 = symbolResponse5.Result.Where(a => a.Value.status == "online").ToList();
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


Dictionary<Coin, List<Coin>> connections = new Dictionary<Coin, List<Coin>>();

List<Tuple<string, int>> amt = new List<Tuple<string, int>>();
foreach (var item in dict.Keys)
{
    amt.Add(new Tuple<string, int>(item, dict[item].Item1.Count));
}

amt.Sort((a, b) => {
    if (a.Item2 == b.Item2) return 0;
    if (a.Item2 < b.Item2) return 1;
    return -1;
});
amt = amt.Where(a => a.Item2 > 2).ToList();

List<CoinPair> pairs = new List<CoinPair>();

foreach (var item in amt)
{
    CoinPair t = KeyIndexer.GetSymbolPair(item.Item1);
    if (t == null) continue;

    if (!connections.ContainsKey(t.BuyCoin))
    {
        connections.Add(t.BuyCoin, new List<Coin>());
    }
    if (!connections.ContainsKey(t.SellCoin))
    {
        connections.Add(t.SellCoin, new List<Coin>());
    }

    if (pairs.Contains(t)) continue;

    pairs.Add(t);
}


foreach (var item1 in connections.Keys)
{
    foreach (var item2 in pairs) {

        Coin b = item2.BuyCoin;
        Coin s = item2.SellCoin;

        if (!(item1 == item2.BuyCoin || item1 == item2.SellCoin)) continue;

        if ((item1 == item2.BuyCoin)) { 
            if (connections[item1].Contains(s)) continue;
            connections[item1].Add(s);
        }
        if ((item1 == item2.SellCoin))
        {
            if (connections[item1].Contains(b)) continue;
            connections[item1].Add(b);
        }
    }
}

int min = 4;

var t2 = connections.Where(a => a.Value.Count >= min);




List<Coin> list = new List<Coin>();
List<CoinPair> linkedPairs = new List<CoinPair>();
foreach (var item in t2)
{
    foreach (var item1 in item.Value)
    {
        if (connections[item1].Count < min) continue;

        CoinPair t = null;
        if ((int)item.Key < (int)item1)
        {
            t = new CoinPair(item1, item.Key);
        }
        else { 
            t = new CoinPair(item.Key, item1);
        }

        if (linkedPairs.Any(a => a.BuyCoin == t.BuyCoin && a.SellCoin == t.SellCoin)) continue;


        linkedPairs.Add(t);
        if(!list.Contains(t.BuyCoin)) list.Add(t.BuyCoin);
    }
}

list.Remove(Coin.BTC);
list.Remove(Coin.USDT);
list.Remove(Coin.ETH);


StreamWriter sw = new StreamWriter("data.linker");
int j = 0;
foreach (var item in list)
{
    foreach (var item1 in GetLoops(item))
    {
        string line = "";
        foreach (var item2 in item1)
        {
            line += item2 + " <--> ";
        }
        line = line.Substring(0, line.Length - 6);
        sw.WriteLine(line);
        PublisherUtilities.set(""+(j++), line);
    }
}


sw.WriteLine("BTCUSDT <--> ETHUSDT <--> ETHBTC");
sw.WriteLine("ETHUSDT <--> BTCUSDT <--> ETHBTC");
PublisherUtilities.set("" + (j++), "BTCUSDT <--> ETHUSDT <--> ETHBTC");
PublisherUtilities.set("" + (j++), "ETHUSDT <--> BTCUSDT <--> ETHBTC");
PublisherUtilities.set("-1", ""+j);


sw.Flush();
sw.Close();

return;
List<List<CoinPair>> GetLoops(Coin coin) { 
    List<List<CoinPair>> ret = new List<List<CoinPair>>();
    List<List<CoinPair>> Computed = new List<List<CoinPair>>() {
        new List<CoinPair>(){
            new CoinPair(Coin.NONE, Coin.BTC),
            new CoinPair( Coin.BTC, Coin.USDT),
            new CoinPair( Coin.NONE, Coin.USDT)
        },
        new List<CoinPair>(){
            new CoinPair(Coin.NONE, Coin.ETH),
            new CoinPair( Coin.ETH, Coin.USDT),
            new CoinPair( Coin.NONE, Coin.USDT)
        },
        new List<CoinPair>(){
            new CoinPair(Coin.NONE, Coin.ETH),
            new CoinPair( Coin.ETH, Coin.BTC),
            new CoinPair( Coin.NONE, Coin.BTC)
        },
        new List<CoinPair>(){
            new CoinPair(Coin.NONE, Coin.BTC),
            new CoinPair( Coin.ETH, Coin.BTC),
            new CoinPair( Coin.NONE, Coin.ETH)
        },
        new List<CoinPair>(){
            new CoinPair( Coin.NONE, Coin.USDT),
            new CoinPair( Coin.BTC, Coin.USDT),
            new CoinPair(Coin.NONE, Coin.BTC)
        },
        new List<CoinPair>(){
            new CoinPair( Coin.NONE, Coin.USDT),
            new CoinPair( Coin.ETH, Coin.USDT),
            new CoinPair(Coin.NONE, Coin.ETH)
        },
        new List<CoinPair>(){
            new CoinPair( Coin.NONE, Coin.BTC),
            new CoinPair( Coin.ETH, Coin.BTC),
            new CoinPair(Coin.NONE, Coin.ETH)
        },
        new List<CoinPair>(){
            new CoinPair( Coin.NONE, Coin.ETH),
            new CoinPair( Coin.ETH, Coin.BTC),
            new CoinPair(Coin.NONE, Coin.BTC)
        }
    };
    foreach (var item in Computed)
    {
        ret.Add(PlaceEnd(coin, item));
    }
   
    return ret;
}


List<CoinPair> PlaceEnd(Coin c ,List<CoinPair> data) {
    data[0].BuyCoin = c;
    data[data.Count-1].BuyCoin = c;
    return data;
}