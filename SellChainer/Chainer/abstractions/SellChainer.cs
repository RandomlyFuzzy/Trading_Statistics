public class SellChainer : Chainer
{
    internal SellChainer() { }
    public override double CalcAmount()
    {
        return (GetAmount() * GetPrice()) * (1 - GetFee());
    }
}