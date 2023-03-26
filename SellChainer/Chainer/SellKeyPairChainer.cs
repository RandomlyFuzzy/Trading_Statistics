using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class SellKeyPairChainer : SellChainer
{
    string pair = "";

    public SellKeyPairChainer() { }

    public SellKeyPairChainer(string PAIR, double fee)
    {
        this.SymbolPair = PAIR;
        this.pair = PAIR;
        this.fee = fee;
        RedisSubscribed.SubscribeTo(pair+"UPD", Updated);
        Updated2();
    }



    public override double CalcAmount()
    {
        return (GetAmount() * GetPrice()) * (1 - GetFee());
    }

    public void Updated(BasicObj arg1, byte[] arg2)
    {
        Updated2();
    }
    public void Updated2()
    {
        string json = "" + PublisherUtilities.get(pair + "bounds");

        if(json == "") throw new NotImplementedException(pair+" bounds");

        var bds = Newtonsoft.Json.JsonConvert.DeserializeObject<bnds>(json);

        double price = bds.max;
        Set(price, fee);

        //todo update fee with src;

        change = true;
    }
}
