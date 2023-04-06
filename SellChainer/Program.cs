// See https://aka.ms/new-console-template for more information
using StackExchange.Redis;

Console.WriteLine("Hello, World!");

PublisherUtilities.redis = ConnectionMultiplexer.Connect(SingletonUtility.REDIS_CONNECTION_STRING);

ChainerFactory factory = new ChainerFactory();


List<string> strs = new List<string>();

int len = Convert.ToInt32(PublisherUtilities.get("-1"));

for (int i = 0; i < len; i++) {
    strs.Add(Convert.ToString((PublisherUtilities.get(""+i))));
}


List<List<CoinPair>> coinPairs = new List<List<CoinPair>>();
foreach (var item in strs) { 
    coinPairs.Add(new List<CoinPair>());
    string[] pairs = item.Split(" <--> ");
    foreach (var item1 in pairs)
    {
        coinPairs.Last().Add(KeyIndexer.GetSymbolPair(item1));        
    }
}
List<IChainer> chains = new List<IChainer>();

foreach (var coinPair in coinPairs)
{
    chains.Add(ChainerFactory.GetFromPairs<BuyKeyPairChainer, SellKeyPairChainer>(coinPair.First().BuyCoin, true, coinPair.ToArray()));
    chains.Add(ChainerFactory.GetFromPairs<BuyKeyPairChainer, SellKeyPairChainer>(coinPair.First().SellCoin, true, coinPair.ToArray()));
}

foreach (var coinPair in chains) { 
    coinPair.RunLoop(0.25);
}

//IChainer temp = ChainerFactory.GetFromPairs<BuyKeyPairChainer, SellKeyPairChainer>(Coin.ETH,false, 
//    new CoinPair(Coin.ETH, Coin.BTC), 
//    new CoinPair(Coin.BTC, Coin.USDT),
//    new CoinPair(Coin.ETH, Coin.USDT) 
//);
//Console.WriteLine();

//IChainer temp2 = ChainerFactory.GetFromPairs<BuyKeyPairChainer, SellKeyPairChainer>(Coin.BTC, false,
//    new CoinPair(Coin.ETH, Coin.BTC),
//    new CoinPair(Coin.ETH, Coin.USDT),
//    new CoinPair(Coin.BTC, Coin.USDT)
//);
//Console.WriteLine();

////temp.Calculate(0.25);
////Console.WriteLine();
////temp2.Calculate(0.25);
////return;


//temp .RunLoop(0.25);
//temp2.RunLoop(0.25);



//BuyKeyPairChainer ethbtc = new BuyKeyPairChainer(new CoinPair(Coin.ETH, Coin.BTC).ToString(), 0.004);
//SellKeyPairChainer ethusdt = new SellKeyPairChainer(new CoinPair(Coin.ETH, Coin.USDT).ToString(), 0.004);
//BuyKeyPairChainer btcusdt = new BuyKeyPairChainer(new CoinPair(Coin.BTC, Coin.USDT).ToString(), 0.004);

//ethbtc.SetPrinting(true);



//while (true) {
//    while (!temp.hasChanged()) Thread.Sleep(1);
//    temp.Calculate(0.25);
//}



