using System.Drawing;
using System.Security.Cryptography;

namespace test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(CalculatePi());
        }
        public static double CalculatePi()
        {
            int iter = 0;
            double amount = 0;
            double amtinside = 0;
            while (iter++ < 1_000_000)
            {
                Parallel.For(0, 10, (i) =>
                {
                    var x = Random.Shared.NextDouble();
                    var y = Random.Shared.NextDouble();
                    if (isInside(x, y))
                    {
                        amtinside++;
                    }
                    amount++;
                });


                Console.Title = ""+((amtinside / amount)*4);
            }
            return ((amtinside / amount) * 4);
        }

        public static bool isInside(double x, double y)
        {
            var x2 = x * x;
            var y2 = y * y;
            return x2 + y2 <= 1;
        }
    }
}