using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public class CoinPair : IEqualityComparer<CoinPair>
{
    public Coin BuyCoin;
    public Coin SellCoin;

    public CoinPair(Coin buyCoin, Coin sellCoin)
    {
        BuyCoin = buyCoin;
        SellCoin = sellCoin;
    }
    public bool Equals(CoinPair? x, CoinPair? y)
    {
        return x.BuyCoin == y.BuyCoin && x.SellCoin == y.SellCoin;
    }

    public int GetHashCode([DisallowNull] CoinPair obj)
    {
        return (int)obj.BuyCoin<<16+ (int)obj.SellCoin;
    }

    public override string ToString()
    {
        return BuyCoin.ToString() + SellCoin.ToString();
    }
}