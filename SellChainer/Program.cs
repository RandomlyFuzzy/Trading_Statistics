// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");



ChainerFactory factory = new ChainerFactory();


IChainer temp = ChainerFactory.GetFromPairs<BuyKeyPairChainer, SellKeyPairChainer>(Coin.BTC, 
    new CoinPair(Coin.ETH, Coin.BTC), 
    new CoinPair(Coin.ETH, Coin.USDT), 
    new CoinPair(Coin.SUSHI, Coin.USDT), 
    new CoinPair(Coin.SUSHI, Coin.BTC), 
    new CoinPair(Coin.BTC, Coin.USDT));




temp.Calculate(0.25);
return;


//BuyKeyPairChainer ethbtc = new BuyKeyPairChainer(new CoinPair(Coin.ETH, Coin.BTC).ToString(), 0.004);
//SellKeyPairChainer ethusdt = new SellKeyPairChainer(new CoinPair(Coin.ETH, Coin.USDT).ToString(), 0.004);
//BuyKeyPairChainer btcusdt = new BuyKeyPairChainer(new CoinPair(Coin.BTC, Coin.USDT).ToString(), 0.004);

//ethbtc.SetPrinting(true);



//while (true) {
//    var temp = ethbtc.Chain<BuyKeyPairChainer>(ethusdt).Chain<SellKeyPairChainer>(btcusdt);
//    while (!temp.hasChanged()) Thread.Sleep(1);
//    temp.Calculate(0.25);
//}



