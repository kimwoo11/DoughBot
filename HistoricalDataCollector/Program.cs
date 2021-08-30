using System;

namespace HistoricalDataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            HistoricalDataCollector hdc = new HistoricalDataCollector("AMC");
            hdc.Run("2021/02/01", "2021/08/27");
            //Console.ReadLine();
        }
    }
}
