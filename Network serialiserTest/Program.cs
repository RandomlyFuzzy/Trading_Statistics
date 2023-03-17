using System.Text;
BasicObj obj = new BasicObj();
obj.source = "test";
obj.symbol = "test";
obj.Dir = BuyDirection.BUY;
obj.objType = ObjectType.OrderbookUpdate;

var bytes = obj.Serialize();

Console.WriteLine(bytes.Length);


var obj2 = bytes.Deserialize<BasicObj>();
