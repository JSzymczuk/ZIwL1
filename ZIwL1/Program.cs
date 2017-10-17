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
            new Problem
            {
                StorageCapacity = 5,
                KeepCost = 1,
                OrderCost = 2,
                Demands = new int[] { 1, 2, 3, 1 },
                UnitPrices = new int[] { 1, 2, 1, 2 }
            }.Solve();

            Console.ReadKey();
        }
    }
}
