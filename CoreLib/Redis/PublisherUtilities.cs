using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using StackExchange.Redis;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class SingletonUtility { 
    public static SingletonUtility Instance { get; private set; } = new SingletonUtility();
    public volatile int _queue = 0;
    public volatile List<Thread> threads = new List<Thread>();

}
public static class PublisherUtilities
{
    static ConnectionMultiplexer _redis = null;
    static ConnectionMultiplexer redis
    {
        get
        {

            if (_redis is null)
            {
                _redis = ConnectionMultiplexer.Connect("192.168.0.20:6379,abortConnect=false,connectTimeout=30000,responseTimeout=30000");
                SingletonUtility.Instance.threads = new List<Thread>();
                
                SingletonUtility.Instance.threads.Add(new Thread(() => UploadLoop(thrdcnt2)));
                SingletonUtility.Instance.threads.Last().IsBackground = false;
                SingletonUtility.Instance.threads.Last().Start();
                SingletonUtility.Instance.threads.Add(new Thread(() => UploadLoop(thrdcnt2)));
                SingletonUtility.Instance.threads.Last().IsBackground = false;
                SingletonUtility.Instance.threads.Last().Start();
            }

            return _redis;
        }
    }
    static readonly IDatabase __db = redis.GetDatabase();



    static long index = 0;
    static  int queue { get { return SingletonUtility.Instance._queue; } set { SingletonUtility.Instance._queue = value; Console.Title = " " + SingletonUtility.Instance._queue + " in queue over "+thrdcnt+" threads"; } }
    static object sync = new object();
    static ConcurrentStack<Tuple<byte[], string[]>> pubDir = new ConcurrentStack<Tuple<byte[], string[]>>();
    //static List<Tuple<byte[],string>> pubDir = new List<Tuple<byte[],string>>();

    static object thrdlock = new object();
    public static int thrdcnt
    {
        get {

            return SingletonUtility.Instance.threads.Where(a => a != null && a.IsAlive).Count(); } 
    }
    public static int thrdcnt2
    {
        get { return SingletonUtility.Instance.threads.Count(); }
    }
    public static void UploadLoop(int index) {
        index--;
        var rds = ConnectionMultiplexer.Connect("192.168.0.20:6379,abortConnect=false,connectTimeout=30000,responseTimeout=30000");
        var db = rds.GetDatabase();
        int buffer = 250;
        Tuple<byte[], string[]>[] temp = new Tuple<byte[], string[]>[250];
        bool cont = true;
        while (cont || thrdcnt <= 2) {

            int len = pubDir.TryPopRange(temp, 0, Math.Min(buffer, Math.Max((int)queue, 0)));

            cont = len == buffer;

            if (len == 0 || temp[0] == null)
            {
                Thread.Sleep(1);
                continue;
            }

            try
            {
                var tran = db.CreateTransaction();
                for (int i = 0; i < len; i++)
                {
                    var item = temp[i];
                    if (item == null) break;

                    for (int j = 0; j < item.Item2.Length; j++) {
                        db.Publish(item.Item2[j], Encoding.ASCII.GetString(item.Item1), CommandFlags.FireAndForget);
                    }
                    queue--;
                }
                db.Ping(CommandFlags.FireAndForget);
                tran.Execute();
            }catch(Exception ex)
            {
                Console.Error.WriteLine(ex);
                lock (thrdlock)
                {
                    SingletonUtility.Instance.threads.Add(new Thread(() => UploadLoop(thrdcnt2)));
                    SingletonUtility.Instance.threads.Last().IsBackground = false;
                    SingletonUtility.Instance.threads.Last().Start();
                }
                pubDir.PushRange(temp);
                return;
            }
            if ((thrdcnt < (queue/10000) +2)|| thrdcnt<2)
            {
                lock (thrdlock)
                {

                    SingletonUtility.Instance.threads.Add(new Thread(() => UploadLoop(thrdcnt2)));
                    SingletonUtility.Instance.threads.Last().IsBackground = false;
                    SingletonUtility.Instance.threads.Last().Start();
                }
            }
            else
            if (queue > 300000)
            {
                lock (thrdlock)
                {
                    SingletonUtility.Instance.threads.Add(new Thread(() => UploadLoop(thrdcnt2)));
                    SingletonUtility.Instance.threads.Last().IsBackground = false;
                    SingletonUtility.Instance.threads.Last().Start();
                }
            }
        }
        rds.Dispose();
        rds.Close();
        //lock (thrdlock)
        //{
        //    SingletonUtility.Instance.threads[index] = null;
        //}
    }
    public static RedisValue get(byte[] key)
    {
        return __db.StringGet(key);
    }

     public static string[] getList(string key)
    {
        return Array.ConvertAll(__db.ListRange(key), x => (string)x);
    }

    public static RedisValue[] getKeys() {
        return __db.ListRange("keys");
    }



    public static void ince(byte[] key)
    {
        if (!__db.KeyExists(key)) { 
            __db.ListLeftPush("keys", key, flags: CommandFlags.FireAndForget);
        }


        __db.StringIncrement(key,1);
    }

    private static void AppendData<T>(T data) where T : BasicObj {
        DateTime dt = DateTime.Now;
        int min = dt.Minute;
        int hour = dt.Hour;
        int day = dt.Day;
        int month = dt.Month;
        int year = dt.Year;

        var dat = data.Serialize();

        if ((data.objType is ObjectType.TradesSubscription || data.objType is ObjectType.OrderbookSubscription))
        {
            __db.StringSet(data.symbol + data.source + data.objType,new BasicObj {dt = dt, SpecialIDentifier = dat.ToString() }.Serialize(), flags: CommandFlags.FireAndForget);
            return;
        }
        __db.ListLeftPush(data.symbol + data.source + data.objType, dat, flags: CommandFlags.FireAndForget);
        __db.ListLeftPush(data.symbol + data.source + data.objType+"|"+year+":"+month+":"+day+":"+hour+":"+min, dat, flags: CommandFlags.FireAndForget);
        //db.ListLeftPush(data.symbol + data.source + data.objType,, data.Serialize());
    }
    private static void PublishData(this byte[] data,params string[] channels) {
        pubDir.Push(new Tuple<byte[], string[]>(data, channels));
        queue++;
        if (thrdcnt == 0) {
            SingletonUtility.Instance.threads = new List<Thread>();
            SingletonUtility.Instance.threads.Add(new Thread(() => UploadLoop(thrdcnt2)));
            SingletonUtility.Instance.threads.Last().Start();
            SingletonUtility.Instance.threads.Add(new Thread(() => UploadLoop(thrdcnt2)));
            SingletonUtility.Instance.threads.Last().Start();
        }
    }
    public async static Task PublishData<T>(this T data, params string[] channels) where T : BasicObj
    {
        lock (sync)
        {
            
            if (!(data.objType is ObjectType.TradesSubscription || data.objType is ObjectType.OrderbookSubscription))
            {
                data.Serialize().PublishData(channels);
            }
            //var tran = db.CreateTransaction();
            ////AppendData(data);
            //tran.Execute();
            //Thread.Sleep(16);
        }
    }
    public static object Cast(this Type Type, object data)
    {
        var DataParam = Expression.Parameter(typeof(object), "data");
        var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), Type));

        var Run = Expression.Lambda(Body, DataParam).Compile();
        var ret = Run.DynamicInvoke(data);
        return ret;
    }
}
