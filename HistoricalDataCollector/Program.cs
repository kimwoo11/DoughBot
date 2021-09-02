using System;

namespace HistoricalDataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            HistoricalDataCollector hdc = new HistoricalDataCollector("ROKU");
            hdc.Run("2020/06/01", "2021/08/27");
            //Console.ReadLine();
        }
    }
}
