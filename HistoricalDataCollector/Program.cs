using System;

namespace HistoricalDataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            HistoricalDataCollector hdc = new HistoricalDataCollector("TSLA");
            hdc.Run("2021/08/27", "2021/09/03");
            //Console.ReadLine();
        }
    }
}
