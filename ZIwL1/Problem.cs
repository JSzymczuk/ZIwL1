using System;
using System.Collections.Generic;

namespace ZIwL1
{
    class Problem
    {
        /// <summary>
        /// Maksymalna pojemność magazynu
        /// </summary>
        public int StorageCapacity;

        /// <summary>
        /// Koszt złożenia zamówienia
        /// </summary>
        public int OrderCost;

        /// <summary>
        /// Okresowy koszt utrzymania jednej sztuki produktu
        /// </summary>
        public int KeepCost;

        /// <summary>
        /// Popyt w poszczególnych okresach
        /// </summary>
        public int[] Demands;

        /// <summary>
        /// Cena towaru w poszczególnych okresach
        /// </summary>
        public int[] UnitPrices;


        private int timePeriods;                // liczba okresów
        private int[] totalRemainingDemands;    // popyt skumulowany
        private int[] prevMaxRemainingUnits;    // maksymalna liczba produktów pozostałych w magazynie w poprzednim okresie 

        private float[][,] matrices;            // tabele kosztów w poszczególnych okresach
                                                // matrices[k][i,j] 
                                                // k - okres, i - sztuki na początku okresu, j - zamówione sztuki 

        private MinCostInfo[][] minCosts;       // minimalny koszt ponoszony w poszczególnych okresach 
                                                // wraz z informacjami o liczbie zamawianych sztuk zapewniających koszt minimalny
                                                // minCosts[k][i]
                                                // k - okres, i - sztuki na początku okresu

        private const float NoValue = -1;


        public void Solve()
        {
            Initialize();
            FillMatrices();
            CalculateMinCosts();
            PrintOutput();
        }

        /// <summary>
        /// Tworzy macierz w k-tym kroku na podstawie macierzy poprzedniego kroku.
        /// </summary>
        private void FillMatrix(int k)
        {
            int totalColumns = GetNextStepColumns(k);
            int totalRows = GetNextStepRows(k);

            float[,] matrix = matrices[k] = new float[totalColumns, totalRows];

            // q - popyt w k-tym okresie
            int q = Demands[k];
            // przyszły popyt skumulowany (od k-tego okresu włącznie)
            int d = totalRemainingDemands[k];

            // Na bieżąco będziemy określać również maksymalną 
            // liczbę pozostałych sztuk po okresie.
            int max = 0;

            for (int j = 0; j < totalRows; j++)
            {
                // Zmienna j odpowiada liczbie zamówionych produktów.

                for (int i = 0; i < totalColumns; i++)
                {
                    // Zmienna i odpowiada liczbie produktów znajdujących się
                    // w magazynie przed złożeniem zamówienia.

                    // Podaż - łączna liczba produktów po dokonaniu zamówienia.
                    int p = i + j;

                    matrix[i, j] =
                        // Popyt nie może być zaspokojony.
                        p < q ? NoValue :

                        // Podaż przekroczy ładowność magazynu.
                        // Obecna podaż jest wyższa niż całkowity przyszły popyt.
                        p > StorageCapacity || p > d ? NoValue :

                        // Regularny przypadek, który może zostać zrealizowany.
                        // j > 0 ? OrderCost : 0                <-- Jeżeli zamówiono przynajmniej 1 sztukę, ponosi się koszty zamówienia.
                        // j * UnitPrices[k]                    <-- Łączny koszt zamówionych produktów.
                        // KeepCost * (i + j - q / 2.0f)        <-- Koszty składowania: 
                        //                                          uśredniony liczba sztuk na początku okresu (po dokonaniu zamówienia) 
                        //                                          oraz liczba sztuk na końcu okresu (po zaspokojeniu popytu);
                        //                                          ((i + j) + (i + j - q)) / 2 = i + j - q / 2 
                        (j > 0 ? OrderCost : 0) + j * UnitPrices[k] + KeepCost * (i + j - q / 2.0f);

                    // Aktualizacja maksymalnej liczby produktów pozostałych po okresie.
                    if (matrix[i, j] != NoValue && max < p) { max = p; }
                }
            }

            // Zapisanie maksymalnej liczby produktów pozostałych po okresie.
            prevMaxRemainingUnits[k + 1] = max - q;

        }

        /// <summary>
        /// Oblicz minimalne koszty oraz odpowiadające im wielkości zamówień.
        /// </summary>
        private void CalculateMinCosts()
        {
            // Proces rozpoczyna się od ostatniego kroku. 
            // Wypełniamy kolejne macierze od końca aż wrócimy do pierwszej.
            for (int k = timePeriods - 1; k >= 0; --k)
            {
                int n = matrices[k].GetColumns();
                MinCostInfo[] costs = minCosts[k] = new MinCostInfo[n];
                
                // Dla i-tej kolumny wybierz zbiór wierszy dających minimalny koszt w k-tym kroku.
                for (int i = 0; i < n; i++)
                {
                    costs[i] = GetMinCostRow(k, i);
                }
            }
        }
        
        private MinCostInfo GetMinCostRow(int k, int col)
        {
            int q = Demands[k];                 // popyt w k-tym okresie
            float[,] matrix = matrices[k];
            int n = matrix.GetRows();
            
            float minCost = float.MaxValue;         // minimalny koszt
            List<int> minRows = new List<int>(n);   // zbiór wierszy zapewniających minimalny koszt

            for (int j = 0; j < n; ++j)
            {
                float cost = matrix[col, j];

                // Jeśli nie jest określony, to ignorujemy.
                if (cost != NoValue)
                {
                    // Liczba sztuk pozostałych po k-tym okresie po zaspokojeniu popytu.
                    int remaining = col + j - q;

                    // Uwzględnik przyszły koszt, jeśli jest to możliwe (nie jest to ostatni okres).
                    if (k < minCosts.Length - 1 /* && 0 <= remaining && remaining < minCosts[k + 1].Length*/)
                    {
                        // Dodaj (wyliczony wcześniej) koszt minimalny w następnym okresie 
                        // dla liczby sztuk, z którymi zostanie on rozpoczety.
                        cost += minCosts[k + 1][remaining].TotalCost;
                    }

                    if (cost < minCost)
                    {
                        minCost = cost;
                        minRows.Clear();
                        minRows.Add(j);
                    }
                    else if (cost == minCost)
                    {
                        minRows.Add(j);
                    }
                }
            }

            return new MinCostInfo
            {
                PossibleRows = minRows,
                TotalCost = minCost
            };
        }

        private void Initialize()
        {
            timePeriods = Demands.Length;
            matrices = new float[timePeriods][,];
            totalRemainingDemands = new int[timePeriods];
            prevMaxRemainingUnits = new int[timePeriods + 1];
            minCosts = new MinCostInfo[timePeriods][];
            int total = 0;
            for (int i = timePeriods - 1; i >= 0; --i)
            { totalRemainingDemands[i] = total += Demands[i]; }
        }

        private void FillMatrices()
        {
            for (int i = 0; i < timePeriods; i++) { FillMatrix(i); }
        }

        private int GetNextStepColumns(int k)
        { return 1 + prevMaxRemainingUnits[k]; }

        private int GetNextStepRows(int k)
        { return 1 + Math.Min(StorageCapacity, totalRemainingDemands[k]); }

        private void PrintOutput()
        {
            for (int k = 0; k < timePeriods; k++)
            {
                Console.WriteLine("Tabela {0}:", k + 1);
                Console.WriteLine();
                WriteMatrix(matrices[k]);
                Console.WriteLine();

                int n = matrices[k].GetColumns();
                for (int i = 0; i < n; i++)
                {
                    var rows = minCosts[k][i].PossibleRows;
                    if (rows.Count > 0)
                    {
                        Console.Write("Dla {0}: {1,8:0.00} {{", i, minCosts[k][i].TotalCost);
                        int m = rows.Count - 1;
                        for (int l = 0; l < m; ++l)
                        { Console.Write("{0}, ", rows[l]); }
                        Console.WriteLine("{0}}}", rows[m]);
                    }
                    else { Console.WriteLine("Dla {0}: X", i); }
                }

                Console.WriteLine();
            }
        }

        private static void WriteMatrix(float[,] matrix)
        {
            int m = matrix.GetRows();
            int n = matrix.GetColumns();

            Console.Write("         ");
            for (int i = 0; i < n; i++)
            {
                Console.Write("{0,8} ", i);
            }
            Console.WriteLine();
            for (int j = 0; j < m; j++)
            {
                Console.Write("{0,8} ", j);
                for (int i = 0; i < n; i++)
                {
                    if (matrix[i, j] == -1)
                    {
                        Console.Write("       X ");
                    }
                    else
                    {
                        Console.Write(string.Format("{0,8:0.00} ", matrix[i, j]));
                    }
                }
                Console.WriteLine();
            }
        }

        class MinCostInfo
        {
            public float TotalCost;
            public List<int> PossibleRows;
        }
    }
}
