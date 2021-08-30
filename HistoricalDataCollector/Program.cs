using System;

namespace HistoricalDataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            HistoricalDataCollector hdc = new HistoricalDataCollector("MSFT");
            hdc.Run("2020/01/01", "2021/08/27");
            //Console.ReadLine();
        }
    }
}
