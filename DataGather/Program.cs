using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

Console.SetError(new StreamWriter("error.txt"));









while (!Console.KeyAvailable)
{
    try
    {
        using (ClientBook book = new ClientBook()) { 
        
            await book.ConnectAll();

            string s = Console.ReadLine();
            System.Console.WriteLine(s);

        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.ToString());
    } 
}


///TODO add special Identifiers for okx and multiExchange exchanges