// See https://aka.ms/new-console-template for more information
using StackExchange.Redis;

Console.WriteLine("Hello, World!");

PublisherUtilities.redis = ConnectionMultiplexer.Connect(SingletonUtility.REDIS_CONNECTION_STRING);

ChainerFactory factory = new ChainerFactory();


IChainer temp = ChainerFactory.GetFromPairs<BuyKeyPairChainer, SellKeyPairChainer>(Coin.ETH,false, 
    new CoinPair(Coin.ETH, Coin.BTC), 
    new CoinPair(Coin.BTC, Coin.USDT),
    new CoinPair(Coin.ETH, Coin.USDT) 
    );
Console.WriteLine();
IChainer temp2 = ChainerFactory.GetFromPairs<BuyKeyPairChainer, SellKeyPairChainer>(Coin.BTC, false,
    new CoinPair(Coin.ETH, Coin.BTC),
    new CoinPair(Coin.ETH, Coin.USDT),
    new CoinPair(Coin.BTC, Coin.USDT)
    );
Console.WriteLine();

//temp.Calculate(0.25);
//Console.WriteLine();
//temp2.Calculate(0.25);
//return;


temp.RunLoop(0.25);
temp2.RunLoop(0.25);



//BuyKeyPairChainer ethbtc = new BuyKeyPairChainer(new CoinPair(Coin.ETH, Coin.BTC).ToString(), 0.004);
//SellKeyPairChainer ethusdt = new SellKeyPairChainer(new CoinPair(Coin.ETH, Coin.USDT).ToString(), 0.004);
//BuyKeyPairChainer btcusdt = new BuyKeyPairChainer(new CoinPair(Coin.BTC, Coin.USDT).ToString(), 0.004);

//ethbtc.SetPrinting(true);



//while (true) {
//    while (!temp.hasChanged()) Thread.Sleep(1);
//    temp.Calculate(0.25);
//}



