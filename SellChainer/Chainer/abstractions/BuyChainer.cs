public class BuyChainer : Chainer
{
    public override CoinPair Pair { get => _Pair; set => _Pair = value; }
    internal BuyChainer() { }

    public override double CalcAmount() { 
        return (GetAmount() / GetPrice()) * (1 - GetFee());
    }
}
