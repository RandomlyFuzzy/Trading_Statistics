using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

public abstract class Chainer : IChainer, ICloneable
{ 

    internal Queue<IChainer> ChainStack = new Queue<IChainer>();
    internal IChainer Next = null;

    public bool change { internal set; get; } = true;

    internal bool LogPrice = false;

    internal double price = 0;
    internal double amount = 0;
    internal double fee = 0;
    public CoinPair _Pair;
    public virtual CoinPair Pair { get =>_Pair; set => _Pair = value; }
    internal string SymbolPair
    {
        set {
            this.Pair = KeyIndexer.GetSymbolPair(value);
        } 
    }


    public Chainer() {
    }
    public Chainer(double price, double fee) => Set(price, fee);

   

    public IChainer LogValue(){
        if(LogPrice)Console.WriteLine(GetSymbol() + " : " + GetAmount() + " at cost " + GetPrice() + " with a fee of " + GetFee() + " has GBP value of " + GetValue());

        return this;
    }   

    public IChainer SetPrinting(bool printing)
    {
        LogPrice = printing;
        return this;
    }
    public IChainer Chain<T>(IChainer chain) where T : IChainer
    {   
        if (ChainStack.Count == 0) {
            ChainStack.Enqueue(this);
        }

        Next = chain.SetStack(ChainStack, this);
        chain.SetPrinting(LogPrice);
        return chain;
    }

    public double GetPrice() => price;
    public double GetFee() => fee;
    public double GetAmount() {
        if (this.amount == 0) throw new Exception("I Amount is not set");
        
        return amount;
    }
    public CoinPair GetSymbol() => Pair;

    public virtual double GetValue() => -1;

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
    public IChainer SetStack(Queue<IChainer> Chain, IChainer current)
    {
        this.ChainStack = Chain;
        this.ChainStack.Enqueue(current);
        return this;
    }

    public IChainer Calculate(double FirstValue)
    {
        var temp = new Queue<IChainer>(this.ChainStack);

        IChainer Current = temp.Dequeue();
        Current.Set(FirstValue);
        IChainer chain;
        while (temp.Count > 0)
        {
            chain = temp.Dequeue();
            //Calc last Amount
            //put into current 
            //print value if applicable
            //store 

            chain.Set(Current.CalcAmount());
            chain.SetPrinting(Current.GetPrinting());
            chain.LogValue();
            Current = chain;
        }
        chain = Current.Next();

        chain.Set(Current.CalcAmount());
        chain.SetPrinting(Current.GetPrinting());
        chain.LogValue();
        Current = chain.Next();

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
}
