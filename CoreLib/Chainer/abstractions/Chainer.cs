using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

public abstract class Chainer : IChainer, ICloneable
{
    internal Guid _ChainId { get; set; } = Guid.NewGuid();
    public Guid ChainId { get => _ChainId; set => _ChainId = value; }
    internal string id { get {  return ChainId.ToString().Substring(0,6); } }


    internal Queue<IChainer> ChainStack = new Queue<IChainer>();
    internal IChainer Next = null;
    public bool change { internal set; get; } = true;

    internal bool LogPrice = false;

    internal double price = 0;
    internal double amount = 0;
    internal double fee = 0;
    public CoinPair _Pair;
    public virtual CoinPair Pair
    {
        get => _Pair; set
        {
            _Pair = value;
            SetPair(Pair, Dir);
            RedisSubscribed.SimpleSub(Pair + "UPD", Update);
        }
    }

    private void Update(string arg1, string arg2)
    {
        SetPair(Pair, Dir);
        change = true;
    }

    public BuyDirection _Dir;
    public BuyDirection Dir { get => _Dir; set => _Dir = value; }
    internal string SymbolPair { set { this.Pair = KeyIndexer.GetSymbolPair(value); } }
    public Chainer() {}
    public Chainer(double price, double fee) => Set(price, fee);

   

    public IChainer LogValue(bool force = false)
    {
        if(LogPrice||force)Console.WriteLine(id+" -> "+GetSymbol() + " : " + CalcAmount() + " at cost " + GetPrice() + " with a fee of " + GetFee() + " has GBP value of " + GetValue());

        return this;
    }   

    public IChainer SetPrinting(bool printing)
    {
        foreach (var item in ChainStack) { 
            ((Chainer)item).LogPrice = printing;
        }
        LogPrice = printing;
        return this;
    }
    public IChainer Chain<T>(IChainer chain) where T : IChainer
    {
        Console.WriteLine(chain.GetType().ToString());
        if(ChainStack.Count == 0) { 
            ChainStack.Enqueue(this);
        }

        Next = chain.SetStack(ChainStack, chain);
        chain.SetPrinting(LogPrice);
        chain.ChainId = ChainId;
        return chain;
    }

    public double GetPrice() => price;
    public double GetFee() => fee;
    public double GetAmount() {
        if (this.amount == 0) {
            this.amount = 0.25;
        }
        //throw new Exception("I Amount is not set");
        
        return amount;
    }
    public CoinPair GetSymbol() => Pair;

    public virtual double GetValue() {

        switch (Dir)
        {
            case BuyDirection.BUY:
                return ChainerFactory.GetCoinValue(Pair.BuyCoin).GetAwaiter().GetResult() * CalcAmount();
            case BuyDirection.SELL:
                return ChainerFactory.GetCoinValue(Pair.SellCoin).GetAwaiter().GetResult() * CalcAmount();
            default:
                break;
        }

        return -1;
    }

    public IChainer Set(double price, double fee)
    {
        this.price = price;
        this.fee = fee;
        return this;
    }

    IChainer IChainer.Set(double amount)
    {
        this.amount = amount;

        return this;
    }

    private IChainer LogSimpleValue(bool force = false)
    {
        if (LogPrice|| force) Console.WriteLine(id + " -> " + GetSymbol() + " : " + GetAmount() + " at cost " + GetPrice() + " with a fee of " + GetFee() + " has GBP value of " + GetValue());

        return this;
    }

    public IChainer SetStack(Queue<IChainer> Chain, IChainer current)
    {
        this.ChainStack = Chain;
        this.ChainStack.Enqueue(current);
        return this;
    }

    public IChainer Calculate(double FirstValue)
    {
        var temp = new Queue<IChainer>(ChainStack);

        IChainer Current = temp.Dequeue();
        Current.Set(FirstValue);
        IChainer chain;
        ((Chainer)Current).LogSimpleValue();
        while (temp.Count > 0)
        {
            chain = temp.Dequeue();
            //Calc last Amount
            //put into current 
            //print value if applicable
            //store 

            chain.Set(Current.CalcAmount());
            chain.SetPrinting(Current.GetPrinting());
            ((Chainer)chain).LogSimpleValue();
            Current = chain;
        }
        //chain = Current.Next();

        //chain.Set(Current.CalcAmount());
        //chain.SetPrinting(Current.GetPrinting());
        Current.LogValue();
        //Current = chain.Next();

        //Current = Set(Current.CalcAmount()).SetPrinting(Current.GetPrinting()).LogValue();
        change = false;

        foreach (var item in this.ChainStack)
        {
            (item as Chainer).change = false;
        }

        return Current;
    }

    public abstract double CalcAmount();


    public bool GetPrinting()
    {
        return LogPrice;
    }

    public IChainer GetLast()
    {
        IChainer ret = this;
        while (ret.Next() != null) { 
            ret = ret.Next();
        }
        return ret;
    }

    IChainer IChainer.Next()
    {
        return Next;
    }

    public bool hasChanged()
    {
        return ChainStack.Any(a => (a as Chainer).change);
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }

    public void SetPair(CoinPair pair, BuyDirection d)
    {
        bnds b = (pair.ToString() + "bounds").GetJsonFromRedis<bnds>();
        
        string setTitle = pair.ToString();
        setTitle += " " + price;
        switch (d)
        {
            case BuyDirection.BUY:
                price = b.bm;

                break;
            case BuyDirection.SELL:
                price = b.sm;
                break;
        }
        setTitle += " -> " + price;
        setTitle += " " + Dir.ToString();

        Console.Title = setTitle;



    }

    public void RunLoop(double amount)
    {
        ((Chainer)ChainStack.First()).amount = amount;
        new Thread(() =>
        {
            while (true)
            {
                while (!hasChanged()) Thread.Sleep(1);
                ((Chainer)ChainStack.First()).LogSimpleValue(true);
                Calculate(amount).LogValue(true);
            }
        }).Start();

    }
}
