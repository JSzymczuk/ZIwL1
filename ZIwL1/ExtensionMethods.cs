using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZIwL1
{
    static class ExtensionMethods
    {
        public static int GetColumns<T>(this T[,] matrix) { return matrix.GetLength(0); }

        public static int GetRows<T>(this T[,] matrix) { return matrix.GetLength(1); }
    }
}
