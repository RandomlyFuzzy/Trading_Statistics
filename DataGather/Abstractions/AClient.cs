using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AClient :IClient
{
    public CancellationTokenSource token = new CancellationTokenSource();
    public string[] markets = new string[0];

    public AClient() { }
    public virtual Task ConnectAll() { 
        throw new NotImplementedException();
    }
    public void Stop() => token.Cancel();
}
