using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public static class RateLimiting
{

    static double                       perLimit = .8;
    static TimeSpan Grace = new TimeSpan(0, 0, 3);
    static List<string>                 exchanges = new List<string>();
    static List<Tuple<int, TimeSpan>>   amtTimeframe = new List<Tuple<int, TimeSpan>>();
    static List<List<DateTime>>         messagesSent = new List<List<DateTime>>();
    static List<object> objs = new List<object>();








    private static void init() {
        perLimit = .8;
        Grace = new TimeSpan( 0, 0, 3);
        exchanges = new List<string>();
        amtTimeframe = new List<Tuple<int, TimeSpan>>();
        messagesSent = new List<List<DateTime>>();
        objs = new List<object>();
    }

    public static void Register(string exchange,int amt, TimeSpan frame) {
        if (exchanges == null) {
            init();
        }
        if(exchanges.Contains(exchange)) return;

        exchanges.Add(exchange);
        amtTimeframe.Add(new Tuple<int, TimeSpan>(amt,frame));
        messagesSent.Add(new List<DateTime>());
        objs.Add(new Mutex());
    }
    public static TimeSpan trySendAmt(string symol, bool force = false)
    {
        int index = exchanges.IndexOf(symol);
        if (index == -1)
        {
            throw new Exception("invalid symbol " + symol);
        }

        //cleanup.
        CleanUp();
        messagesSent[index].Add(DateTime.Now);
        CleanUp();
        messagesSent[index].Add(DateTime.Now);
        if (messagesSent[index].Count < amtTimeframe[index].Item1 * perLimit || force)
        {
            return Grace;
        }

        return ((DateTime.Now - messagesSent[index][0])- amtTimeframe[index].Item2);
    }
    public static object trySend(string symol,bool force = false) { 
        int index = exchanges.IndexOf(symol);
        if (index == -1) {
            throw new Exception("invalid symbol " + symol);
        }

        //cleanup.
        CleanUp();
        messagesSent[index].Add(DateTime.Now);
        if (messagesSent[index].Count < amtTimeframe[index].Item1 * perLimit||force) {
            return new object();
        }

        return objs[index];
    }

   


    private static void CleanUp() {
        for (int i = 0; i < amtTimeframe.Count; i++)
        {
            var a = amtTimeframe[i];
            var b = messagesSent[i];
            DateTime trimtime = DateTime.Now.AddTicks(-a.Item2.Ticks);

            var d = b.FindIndex(c => c > trimtime);
            if (d == -1) continue;

            messagesSent[i].RemoveRange(0, d);
        }
    }
}
