using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZIwL1
{ 
    class Program
    {
        static void Main(string[] args)
        {
            Problem p = new Problem();

            Console.WriteLine("Podaj maksymalną pojemność magazynu:");
            p.StorageCapacity = int.Parse(Console.ReadLine());

            Console.WriteLine("Podaj koszt złożenia zamówienia:");
            p.OrderCost = int.Parse(Console.ReadLine());

            Console.WriteLine("Podaj koszt okresowego utrzymania jednej sztuki produktu:");
            p.KeepCost = int.Parse(Console.ReadLine());
            
            Console.WriteLine("Podaj popyt w poszczególnych okresach (oddzielone przecinkami bez spacji):");
            p.Demands = Array.ConvertAll(Console.ReadLine().Split(','), s => int.Parse(s));

            Console.WriteLine("Podaj cenę jednostkową w poszczególnych okresach (oddzielone przecinkami bez spacji):");
            p.UnitPrices = Array.ConvertAll(Console.ReadLine().Split(','), s => int.Parse(s));

            Console.WriteLine("\n");
            
            p.Solve();

            Console.ReadKey();
        }
    }
}
