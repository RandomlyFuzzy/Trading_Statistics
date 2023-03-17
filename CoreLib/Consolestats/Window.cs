public static class Window{


    static List<Tuple<string,int>> keys = new List<Tuple<string,int>>();
    static Dictionary<string,List<string>> clients = new Dictionary<string, List<string>>();
    static List<List<string>> data = new List<List<string>>();

    static object obj = new object();


    public static void UnSubscribe(int client){
        string key = keys[client].Item1;
        int index = keys[client].Item2;
        clients[key][index] = null;
    }

    public static int Subscribe(string Client){
        string exchange = Client.Split(" ")[0];
        string Symbol = Client.Split(" ")[1];

        if(!clients.ContainsKey(Symbol))
        {
            clients[Symbol] = new List<string>();
        }
        clients[Symbol].Add(exchange);

        keys.Add(new Tuple<string, int>(Symbol,clients[Symbol].Count-1));
        data.Add(new List<string>(){Symbol,exchange});
        InsertTable(keys.Count-1);
        return keys.Count-1;
    }

    public static void SendData(int handle, int index,string value){
        if(data[handle].Count<index){
            for (int i = data[handle].Count-1; i < index; i++){
                data[handle].Add("");
            }
        }
        data[handle][index] = value;
        UpdateTable(handle, index);
        //InsertTable(handle);
    }

    private async static Task UpdateTable(int handle,int index)
    {
        var kKeys = clients.Keys.ToList();
        kKeys.Sort();
        string ind = keys[handle].Item1;
        int kindex = kKeys.IndexOf(keys[handle].Item1);
        int startindex = 0;
        for (int i = 0; i < kindex; i++)
        {
            startindex += clients[kKeys[i]].Count;
            //startindex++;
        }
        startindex += keys[handle].Item2;
        lock (obj)
        {

            Console.SetCursorPosition(0, startindex);
            var temp = keys[handle];
            DrawRow(temp.Item1, temp.Item2, handle);
        }
    }

    private async static Task InsertTable(int handle){
        var kKeys = clients.Keys.ToList();
        kKeys.Sort();
        int kindex = kKeys.IndexOf(keys[handle].Item1);

        int startindex = 0;
        for (int i = 0; i < kindex; i++)
        {
            startindex += clients[kKeys[i]].Count;
            //startindex++;
        }
        startindex += keys[handle].Item2;

        var keysAfter = keys.Where(a=>kKeys.IndexOf(a.Item1)>kindex||(kKeys.IndexOf(a.Item1)==kindex && keys[handle].Item2<a.Item2));
        lock (obj)
        {
            Console.SetCursorPosition(0,startindex);
            kKeys.RemoveRange(0, kindex);
            foreach (var item in kKeys)
            {
                int index = 0;
                foreach (var item2 in clients[item])
                {
                    int tempi = keys.IndexOf(new Tuple<string, int>(item, index));
                    DrawRow(item, index, tempi);
                    index++;
                    if (index > Console.BufferHeight) {
                        Console.BufferHeight = index + 10 ;
                    }
                }
            }
        }

    }
    private static void DrawRow(string key,int index,int handle){
        //for (int i = 0; i < Console.BufferWidth-1; i++)
        //{
        //    System.Console.Write("#");
        //}
        //System.Console.WriteLine();
        int len = Console.WindowWidth/data[handle].Count;
        for (int i = 0; i < data[handle].Count; i++)
        {
            for (int j = 0; j < len; j++)
            {
                try
                {
                    if (j > data[handle][i].Count() - 1)
                    {
                        System.Console.Write(" ");
                        continue;
                    }
                }
                catch {
                }
                System.Console.Write(data[handle][i][j]);
            }
        }
        System.Console.WriteLine();
        //for (int i = 0; i < Console.BufferWidth - 1; i++)
        //{
        //    System.Console.Write("#");
        //}
        //System.Console.WriteLine();
    }
}