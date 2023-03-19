using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class RedisSubscribed
{
    static ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(SingletonUtility.REDIS_CONNECTION_STRING);
    static ConnectionMultiplexer redis
    {
        get
        {
            if (_redis is null) _redis = ConnectionMultiplexer.Connect(SingletonUtility.REDIS_CONNECTION_STRING);

            return _redis;
        }
    }
    static readonly IDatabase __db = redis.GetDatabase();
    static object LOCK = new object();
    static readonly ISubscriber sub = redis.GetSubscriber(LOCK);

    static string compairable = "";
    public static void SubscribeTo(string key, Action<BasicObj, byte[]> action) {

        sub.Subscribe(key, (a, b) => {
            // System.Console.WriteLine("got data ");
            byte[]? b2 = (byte[])Convert.ChangeType(b, typeof(byte[]));

            var obj1 = b2.Deserialize<BasicObj>();

            action(obj1, b2);
        });
    }

}
