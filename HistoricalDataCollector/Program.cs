using System;

namespace HistoricalDataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            HistoricalDataCollector hdc = new HistoricalDataCollector("AMZN");
            hdc.Run("2020/06/01", "2021/09/02");
            //Console.ReadLine();
        }
    }
}
