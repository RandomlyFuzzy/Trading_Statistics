public class SellChainer : Chainer
{
    public override CoinPair Pair { get => _Pair; set => _Pair = value; }
    internal SellChainer() { }
    public override double CalcAmount()
    {
        return (GetAmount() * GetPrice()) * (1 - GetFee());
    }
}