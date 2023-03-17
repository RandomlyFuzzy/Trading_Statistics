
Dictionary<ObjectType,Dictionary<string, List<BasicObj>>> obj = new Dictionary<ObjectType, Dictionary<string, List<BasicObj>>>();

foreach(var item in Enum.GetValues(typeof(ObjectType))) {
    obj.Add((ObjectType)item, new Dictionary<string, List<BasicObj>>());
    RedisSubscribed.SubscribeTo(item.ToString(), (obj1, b2) =>
    {
        string key = obj1.source + " " + obj1.symbol;
        if (!obj[obj1.objType].ContainsKey(key))
        {
            lock (obj)
            {
                lock (obj[obj1.objType])
                {
                    obj[obj1.objType][key] = new List<BasicObj> { obj1 };
                }
            }
        }
        else {
            lock (obj)
            {
                lock (obj[obj1.objType])
                {
                    lock (obj[obj1.objType][key])
                    {
                        obj[obj1.objType][key].Add(obj1);
                    }
                }
            }
        }
    });
}
int msDelay = 2000;
while (!Console.KeyAvailable) { 
    PrintValues();
    Thread.Sleep(msDelay);
}


Console.Read();
void PrintValues() {
    int index = 1;
    lock (obj)
    {
        Console.SetCursorPosition(0, 1);
        foreach (var item in obj)
        {
            Console.WriteLine("                                               ");
            Console.SetCursorPosition(0, index++);
            Console.WriteLine(item.Key.ToString());
            lock (obj[item.Key])
            {
                foreach (var val in item.Value)
                {
                    Console.WriteLine("                                          ");
                    Console.SetCursorPosition(0, index++);
                    Console.Write("\t\t" + val.Key);
                    lock (obj[item.Key][val.Key])
                    {

                        var arr = val.Value.Where((a) =>
                        {
                            var before = DateTime.Now.AddMilliseconds(-msDelay);
                            return a.createTime > before;
                        });
                        obj[item.Key][val.Key] = arr.ToList();
                        Console.WriteLine(" = "+obj[item.Key][val.Key].Count+"/s");
                        new Uri("http://192.168.0.99/give/" + item.Key + "%20" + val.Key + "/" + obj[item.Key][val.Key].Count).GetWeb();
                    }
                }
            }
        }
    Console.SetCursorPosition(0, 0);
    }
}