using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

public class ChainerFactory
{
    public async Task<string> GetCoin(string coin) {
        var ret = new Uri($"https://crypto.com/price/{coin}").GetString();

        if (ret.DocumentElement is null) return "";
        XmlNode root = ret.DocumentElement;
        XmlNode node = root.SelectSingleNode("//div/h2/span");
        return node.InnerText;
    }


    public async Task<double> GetCoinValue(string coin) {
        var value = ""+PublisherUtilities.get(coin);

        if (value == null) { 
            value = await GetCoin(coin);
            PublisherUtilities.set(coin, value, new TimeSpan(0,0,20));
        }

        return double.Parse(value);
    }

    public async Task<double> GetCoinValue(Coin coin) => await GetCoinValue(KeyIndexer.Get(coin));


    public static IChainer GetFromPairs<BUY, SELL>(Coin Start, params CoinPair[] coinPair) where BUY : IChainer, new() where SELL : IChainer, new() {
        if (Start != coinPair[0].BuyCoin && Start != coinPair[0].SellCoin) {
            throw new Exception("invalide paramiter set you must have a base currentcy of which to start");
        }
        IChainer ret;
        bool Buy = true;
        Console.WriteLine(coinPair[0].ToString());
        if (Start == coinPair[0].BuyCoin)
        {
            Console.WriteLine("SELL");
            ret = new SELL();
            Buy = false;
        }
        else { 
            Console.WriteLine("BUY");
            ret = new BUY();
        }
        ret.Pair = coinPair[0];
        for (int i = 1; i < coinPair.Count(); i++)
        {
            Console.WriteLine(coinPair[i].ToString());
            IChainer c;
            if (!Buy)
            {
                if ((coinPair[i - 1].BuyCoin == coinPair[i].BuyCoin || coinPair[i - 1].SellCoin == coinPair[i].BuyCoin))
                {
                    Console.WriteLine("SELL");
                    c = new SELL();
                    c.Pair = coinPair[i];
                    ret.Chain<SELL>(c);
                    Buy = false;
                }
                else if ((coinPair[i - 1].SellCoin == coinPair[i].SellCoin || coinPair[i - 1].BuyCoin == coinPair[i].SellCoin))
                {
                    Console.WriteLine("BUY");
                    c = new BUY();
                    c.Pair = coinPair[i];
                    ret.Chain<BUY>(c);
                    Buy = true;
                }
                else
                {
                    throw new Exception(" invalide Path to complete");
                }
            }
            else
            {

                if (coinPair[i - 1].SellCoin == coinPair[i].SellCoin || coinPair[i - 1].BuyCoin == coinPair[i].SellCoin)
                {
                    Console.WriteLine("BUY");
                    c = new BUY();
                    c.Pair = coinPair[i];
                    ret.Chain<BUY>(c);
                    Buy = false;
                }
                else if ((coinPair[i - 1].BuyCoin == coinPair[i].BuyCoin || coinPair[i - 1].SellCoin == coinPair[i].BuyCoin))
                {
                    Console.WriteLine("SELL");
                    c = new SELL();
                    c.Pair = coinPair[i];
                    ret.Chain<SELL>(c);
                    Buy = true;
                }
                else
                {
                    throw new Exception(" invalide Path to complete");
                }
            }

            Buy = !Buy;
        }

        return ret;
    }

}
