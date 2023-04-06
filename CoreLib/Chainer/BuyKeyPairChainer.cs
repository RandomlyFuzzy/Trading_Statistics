using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class BuyKeyPairChainer : BuyChainer
{
    string pair = "";




            public BuyKeyPairChainer() { }


    public BuyKeyPairChainer(string PAIR, double fee) {
        this.SymbolPair = PAIR;
        this.pair = PAIR;
        this.fee = fee;
        RedisSubscribed.SubscribeTo(pair + "UPD", Updated);
        Updated2();
    }



    public override double CalcAmount()
    {
        return (GetAmount() / GetPrice()) * (1 - GetFee());
    }

    public void Updated(BasicObj arg1, byte[] arg2) {
        Updated2();
    }
    public void Updated2()
    {
        string json = "" + PublisherUtilities.get(pair + "bounds");

        if(json == "") throw new NotImplementedException(pair+" bounds");

        var bnds = Newtonsoft.Json.JsonConvert.DeserializeObject<bnds>(json);

        double price = bnds.min;
        Set(price, fee);
        //todo update fee with src;

        change = true;
    }
}
[Serializable]
 public class Bounds
{
    public double min { get; set; } = 0;
    public double max { get; set; } = 0;
    public string minsrc { get; set; } = "";
    public string maxsrc { get; set; } = "";

    public Bounds() { }
}