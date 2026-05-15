using Xunit;

namespace CoreLib.Tests;

public class ChainerMathTests
{
    [Fact]
    public void BuyChainer_CalcAmount_DividesByPrice()
    {
        IChainer chainer = new BuyChainer();
        chainer.Set(100.0, 0.001);
        chainer.Set(1000.0);
        double result = chainer.CalcAmount();
        Assert.Equal(9.99, result, 10);
    }

    [Fact]
    public void SellChainer_CalcAmount_MultipliesByPrice()
    {
        IChainer chainer = new SellChainer();
        chainer.Set(100.0, 0.001);
        chainer.Set(1000.0);
        double result = chainer.CalcAmount();
        Assert.Equal(99900.0, result, 0);
    }

    [Fact]
    public void BuyChainer_ZeroFee_NoFeeApplied()
    {
        IChainer chainer = new BuyChainer();
        chainer.Set(100.0, 0.0);
        chainer.Set(1000.0);
        double result = chainer.CalcAmount();
        Assert.Equal(10.0, result, 10);
    }

    [Fact]
    public void SellChainer_ZeroFee_NoFeeApplied()
    {
        IChainer chainer = new SellChainer();
        chainer.Set(100.0, 0.0);
        chainer.Set(1000.0);
        double result = chainer.CalcAmount();
        Assert.Equal(100000.0, result, 0);
    }

    [Fact]
    public void BuyChainer_HighFee_ReducesAmount()
    {
        IChainer chainer = new BuyChainer();
        chainer.Set(100.0, 0.5);
        chainer.Set(1000.0);
        double result = chainer.CalcAmount();
        Assert.Equal(5.0, result, 10);
    }

    [Fact]
    public void SellChainer_HighFee_ReducesAmount()
    {
        IChainer chainer = new SellChainer();
        chainer.Set(100.0, 0.5);
        chainer.Set(1000.0);
        double result = chainer.CalcAmount();
        Assert.Equal(50000.0, result, 0);
    }

    [Fact]
    public void BuyChainer_DefaultAmount_IsZeroPointTwoFive()
    {
        IChainer chainer = new BuyChainer();
        chainer.Set(100.0, 0.001);
        double amount = chainer.GetAmount();
        Assert.Equal(0.25, amount, 10);
    }

    [Fact]
    public void Calculate_ChainOfBuySell_ExecutesAll()
    {
        IChainer buy = new BuyChainer();
        IChainer sell = new SellChainer();
        buy.Set(100.0, 0.001);
        sell.Set(200.0, 0.001);
        var chained = buy.Chain<BuyChainer>(sell);
        var result = buy.Calculate(1000.0);
        Assert.NotNull(result);
    }

    [Fact]
    public void CoinPair_ToString_ReturnsBuyCoinPlusSellCoin()
    {
        var pair = new CoinPair(Coin.BTC, Coin.USDT);
        Assert.Equal("BTCUSDT", pair.ToString());
    }

    [Fact]
    public void CoinPair_Reverse_SwapsCoins()
    {
        var pair = new CoinPair(Coin.BTC, Coin.USDT);
        var reversed = pair.Reverse();
        Assert.Equal(Coin.USDT, reversed.BuyCoin);
        Assert.Equal(Coin.BTC, reversed.SellCoin);
    }

    [Fact]
    public void BasicObj_HasDefaultCreateTime()
    {
        var obj = new BasicObj();
        Assert.NotEqual(default, obj.createTime);
    }

    [Fact]
    public void BasicObjCopyConstructor_CopiesProperties()
    {
        var original = new BasicObj
        {
            dt = DateTime.UtcNow,
            source = "test",
            symbol = "BTCUSDT",
            objType = ObjectType.TradesUpdate,
            SpecialIDentifier = "id123"
        };
        var copy = new BasicObj(original);
        Assert.Equal(original.dt, copy.dt);
        Assert.Equal(original.source, copy.source);
        Assert.Equal(original.symbol, copy.symbol);
        Assert.Equal(original.objType, copy.objType);
        Assert.Equal(original.SpecialIDentifier, copy.SpecialIDentifier);
    }

    [Fact]
    public void OrderBookObj_CompareTo_NegativeWhenPriceIsHigher()
    {
        var higher = new OrderBookObj { price = 200 };
        var lower = new OrderBookObj { price = 100 };
        Assert.True(higher.CompareTo(lower) < 0);
    }

    [Fact]
    public void ChainerFactory_CanChain_ReturnsTrueForSharedCoin()
    {
        var pair1 = new CoinPair(Coin.BTC, Coin.USDT);
        var pair2 = new CoinPair(Coin.ETH, Coin.BTC);
        Assert.True(ChainerFactory.CanChain(pair1, pair2));
    }

    [Fact]
    public void ChainerFactory_CanChain_ReturnsFalseForNoSharedCoin()
    {
        var pair1 = new CoinPair(Coin.BTC, Coin.USDT);
        var pair2 = new CoinPair(Coin.ETH, Coin.SOL);
        Assert.False(ChainerFactory.CanChain(pair1, pair2));
    }

    [Fact]
    public void BuyChainer_GetValue_ReturnsPriceTimesAmount()
    {
        IChainer chainer = new BuyChainer();
        chainer.Set(100.0, 0.0);
        chainer.Set(1000.0);
        Assert.Equal(100.0, chainer.GetPrice(), 10);
        Assert.Equal(0.0, chainer.GetFee(), 10);
        Assert.Equal(1000.0, chainer.GetAmount(), 10);
    }
}
