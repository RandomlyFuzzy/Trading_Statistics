using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IChainer
{
    public CoinPair Pair { get; set; }
    public BuyDirection Dir { get; set; }

    public Guid ChainId { get; internal set; }

    public void RunLoop(double amount);
    public void SetPair(CoinPair pair,BuyDirection dir);
    public IChainer Chain<T>(IChainer chain) where T : IChainer;
    public IChainer Calculate(double amount);
    public bool hasChanged();
    internal IChainer Set(double price, double fee);
    IChainer Set(double amount);
    IChainer SetStack(Queue<IChainer> Chain,IChainer current);

    internal IChainer Next();
    public IChainer GetLast();
    public IChainer LogValue(bool force = false);
    public IChainer SetPrinting(bool printing);
    public bool GetPrinting();
    public double GetValue();
    public double GetPrice();
    public double GetFee();
    public double GetAmount();
    public double CalcAmount();
    public CoinPair GetSymbol();
}
