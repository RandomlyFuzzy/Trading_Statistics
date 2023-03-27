using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Net.Sockets;
using System.Timers;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Xml;

public abstract class AWebSocketClient<T> : IDisposable where T: class, IBasic,new()
{
    public string Prefix;
    public string symbol;
    private ClientWebSocket _socket;
    public CancellationTokenSource _cancellationTokenSource;
    private System.Timers.Timer _timer;
    private DateTime lastSend = DateTime.MinValue;
    public string RateSymbol = "";
    int handle = -1;
    int messageQueue = 1;

    public bool ping = false;

    public int MaxRetrys { get; private set; } = 5;

    public AWebSocketClient(bool usePing = true) {
        ping = usePing;
    }



    public abstract Task init();





    public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        _socket = new ClientWebSocket();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

        var obj = RateLimiting.trySend(RateSymbol);
        lock (obj)
        {
            for (int i = 0; i < MaxRetrys && _socket.State != WebSocketState.Open; i++)
            {
                try
                {
                    _socket.ConnectAsync(uri, _cancellationTokenSource.Token).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    Console.Error.WriteLine(GetType().Name+" crashed while connecting ");
                }
                if(!(_socket.State == WebSocketState.Open))Thread.Sleep(i * 10000);
            }
            if (!(_socket.State == WebSocketState.Open)) {
                throw new Exception("unable to connect to endpoint");
            }
        }


        if (ping)
        {
            _timer = new System.Timers.Timer(300000);

            _timer.Elapsed += (a,b) =>
            {
                SendAsync("ping");
            };
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        if (handle == -1)
        {
            handle = Window.Subscribe(Prefix);
            new Thread(async ()=>{
                while (true)
                {
                    if (handle != -1)
                    {
                        if (lastSend.AddSeconds(60*5) > DateTime.Now) {
                            Thread.Sleep((int)(lastSend.AddSeconds(60 * 5) - DateTime.Now).TotalMilliseconds);
                        }
                        await init();
                    }
                    try
                    {
                        await ReceiveLoopAsync();
                    }catch(Exception ex){
                        Window.SendData(handle, 2, "-----");
                        Window.SendData(handle, 3, "-----");
                        System.Console.Error.WriteLine(ex);
                        System.Console.WriteLine(Prefix+ " read thread crashed");
                        _cancellationTokenSource.Cancel();
                        if (ping) { 
                            _timer.Stop();
                        }
                    }
                    _cancellationTokenSource.TryReset();
                }
            }).Start();


        }

        var split = Prefix.Split(' ');
        Window.SendData(handle, 0, split[0]);
        Window.SendData(handle, 1, split[1]);
        //Window.SendData(handle, 4, symbol);

    }

    private async Task ReceiveLoopAsync()
    {


        var buffer = new byte[2<<16];
        var sb = new StringBuilder();

        while (_socket.State == WebSocketState.Open)
        {
            var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                sb.Append(Encoding.ASCII.GetString(buffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    var message = sb.ToString();
                    sb.Clear();
                    Window.SendData(handle, 2, message);
                    switch (message)
                    {
                        case "pong":
                            continue;
                        case "ping":
                            SendAsync("pong", force: true);
                            continue;
                        default:
                            T obj = GetDefaultValue<T>(typeof(T));
                            try
                            {
                                obj = (T)obj.GetObject(message);

                                if (obj is null)
                                {
                                    Window.SendData(handle, 2, "error");
                                    //System.Console.WriteLine(Prefix + " failed to convert " + message);
                                    continue;
                                }
                                // Call the abstract method with the decoded message
                                OnMessageReceived(obj, new ObjectDecoder(message));
                            }
                            catch (Exception ex){

                                System.Console.Error.WriteLine(ex);
                                System.Console.Error.WriteLine(message + " len " + message.Length);
                                System.Console.Error.WriteLine(Prefix + " crashed");
                            }
                            break;
                    }
                   
                }
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cancellationTokenSource.Token);
            }
        }
    }
    public T GetDefaultValue<T>(Type type) where T:class
    {
        T ret = Activator.CreateInstance(type) as T;
        if (ret == null) { 
            return default(T);
        }
        return ret;
    }
    protected abstract ObjectType MessageType(T message, ObjectDecoder obj);

    protected void OnMessageReceived(T message, ObjectDecoder obj) {

        messageQueue++;
        List<string> channels = new List<string>();// Prefix.Split(' '));
        if (!channels.Contains(symbol))
        {
            channels.Add(symbol);
        }
        var val = MessageType(message, obj);
        //Console.WriteLine(" " + val) ;
        switch (val)
        {
            case ObjectType.TradesUpdate:
            case ObjectType.TradesSubscription:

                MethodInfo method = typeof(ObjectDecoder).GetMethod("getAs");
                method = method.MakeGenericMethod(message.GetTradeType());
                var oto = method.Invoke(obj, null) as ITradeObject;

                MethodInfo method2 = message.GetTradeType().GetMethod("GetTrades");
                var objto = method2.Invoke(oto, new object[] { symbol });

                foreach (var item in objto as List<TradeObj>)
                {
                    //channels.Add(item.objType.ToString());
                    item.PublishData(channels.ToArray());
                    //channels.Remove(item.objType.ToString());
                }

                break;
            case ObjectType.OrderbookUpdate:
            case ObjectType.OrderbookSubscription:
                MethodInfo method3 = typeof(ObjectDecoder).GetMethod("getAs");
                method3 = method3.MakeGenericMethod(message.GetOrderBookType());
                var oto2 = method3.Invoke(obj, null) as IOrderBook;

                MethodInfo method4 = message.GetOrderBookType().GetMethod("GetOrderBook");
                var oto3 = method4.Invoke(oto2, new object[] { symbol });

                foreach (var item in oto3 as List<OrderBookObj>)
                {
                    //channels.Add(item.objType.ToString());
                    item.PublishData(channels.ToArray());
                    //channels.Remove(item.objType.ToString());
                }
                break;
            default:
                break;
        }
        //Window.SendData(handle,2,"queue = "+messageQueue.ToString());
        messageQueue--;
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken = default, bool force = false)
    {
        //todo Rate Limit
        var obj = RateLimiting.trySend(RateSymbol, force);
        lock (obj)
        {
            while (_socket.State == WebSocketState.Connecting) Thread.Sleep(1);



            message = message.Trim().Replace("  ", "").Replace("\n", "").Replace("\r", "").Replace("\t", " ");
            Window.SendData(handle, 3, message);
            // System.Console.WriteLine(Prefix +" message sent "+ message);
            var buffer = Encoding.UTF8.GetBytes(message);
            _socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken).GetAwaiter().OnCompleted(()=> { 
                lastSend = DateTime.Now;
            });
        }
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
        _cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        _socket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
public class ObjectDecoder{

    public string data = "";

    public ObjectDecoder(string dat){
        data = dat;
    }

    public T getAs<T>() where T:class,IBasic,new(){
        T obj = new T();

        //System.Console.WriteLine(data);
        return (T)obj.GetObject(data) as T;// Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
    }
}
public static class HelperFunctions{
    static HttpClient httpClient = new HttpClient();

    public static T GetFromJsonAs<T>(this string data){
        var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        return ret;
    }
    public async static Task GetWeb(this Uri location)
    {
        var response = await httpClient.GetAsync(location);

        if (!response.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to make GET request. Status code: {response.StatusCode}");
        }
    }
    public static XmlDocument GetString(this Uri location) 
    {
        WebRequest request = WebRequest.Create(location);
        request.Credentials = CredentialCache.DefaultCredentials; 
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
            {
                using (Stream dataStream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                        XmlDocument xmlDocument = new XmlDocument();
                        string vale = reader.ReadToEnd();
                        xmlDocument.LoadXml(vale.Replace("defer ", ""));
                        return xmlDocument;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Failed to make GET request. Status code: {response.StatusCode}");
            }
        }
        return null;
    }

    public async static Task<T> Get<T>(this Uri location) where T: new(){
        var response = await httpClient.GetAsync(location);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody.GetFromJsonAs<T>();
        }
        else
        {
            Console.WriteLine($"Failed to make GET request. Status code: {response.StatusCode}");
        }
        return default(T);
    }
    public static T GetJsonFromRedis<T>(this string key) {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(PublisherUtilities.get(key));
    }
    public static string GetFromRedis(this string key)
    {
        return ""+PublisherUtilities.get(key);
    }
}
[Serializable]
public class bnds
{
    public double min { get; set; } = 0;
    public double max { get; set; } = 0;
    public string minsrc { get; set; } = "";
    public string maxsrc { get; set; } = "";

    public bnds() { }
}