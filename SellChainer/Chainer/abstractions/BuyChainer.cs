public class BuyChainer : Chainer
{
    internal BuyChainer() { }

    public override double CalcAmount() { 
        return (GetAmount() / GetPrice()) * (1 - GetFee());
    }
}
