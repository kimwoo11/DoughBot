using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using DoughBot.Strategies;
using IBApi;
using Newtonsoft.Json;

namespace DoughBot
{
    public class BacktestEngine
    {
        public readonly string RootFilePath = @"C:\Users\steve\source\repos\DoughBot\";
        private List<List<OrderEntry>> allOrderEntries = new List<List<OrderEntry>>();
        private List<OrderEntry> flattenedOrderEntries;
        private readonly double rr;
        private readonly string csvName;
        private readonly string barSize;

        public BacktestEngine(double rr, string csvName, string barSize)
        {
            this.rr = rr;
            this.csvName = csvName;
            this.barSize = barSize;
        }

        public void Run(string symbol)
        {
            var historicalTicksPath = RootFilePath + @$"data\{symbol}\historicalTicks\";
            string historicalBarsPath;
            int barOffset;

            if (barSize == "1 min")
            {
                historicalBarsPath = RootFilePath + @$"data\{symbol}\historicalBars1min\";
                barOffset = 60;
            }
            else // "2 mins"
            {
                historicalBarsPath = RootFilePath + @$"data\{symbol}\historicalBars2mins\";
                barOffset = 120;
            }

            List<string> tickFiles = Directory.GetFiles(historicalTicksPath).ToList();
            List<string> barFiles = Directory.GetFiles(historicalBarsPath).ToList();
            int tickFilesLen = tickFiles.Count;
            int startIndex = 0;
            tickFiles = tickFiles.GetRange(startIndex, tickFilesLen - startIndex);

            int numFiles = tickFiles.Count;
            int processed = 0;

            Parallel.ForEach(tickFiles, new ParallelOptions { MaxDegreeOfParallelism = 8 }, filepath =>
            {
                var tempBot = new Bot(0, "backtest", 0, new Dictionary<string, Strategy> { { symbol, new EmaStrictBreakout(9, 21, 50, rr, rr, rr) } }, null);
                tempBot.Run(true);
                int barReqId = tempBot.DataDictionary[symbol].Id + tempBot.StockRequestIdBase;
                int tickReqId = tempBot.DataDictionary[symbol].Id + tempBot.TickRequestIdBase;

                int i = 0;
                tempBot.IsBotRunning = true;
                var filename = filepath.Split('\\').Last();
                if (barFiles.Contains(historicalBarsPath + filename))
                {
                    tempBot.InitializeDataDictionary();
                    var historicalTicks = JsonHandler.ReadFromJsonFile<List<Tick>>(historicalTicksPath + filename);
                    var historicalBars = JsonHandler.ReadFromJsonFile<List<Bar>>(historicalBarsPath + filename);

                    int numHistoricalTicks = historicalTicks.Count;

                    foreach (var bar in historicalBars)
                    {
                        if (!tempBot.IsBotRunning)
                        {
                            break;
                        }
                        var barTime = Convert.ToInt64(bar.Time);
                        var nextBarTime = barTime + barOffset; // change this depending on bar size
                        tempBot.IbWrapper.historicalDataUpdate(barReqId, bar);

                        while (i < numHistoricalTicks && historicalTicks[i].Time < barTime)
                        {
                            i++;
                        }
                        while (i < numHistoricalTicks && historicalTicks[i].Time < nextBarTime)
                        {
                            if (!tempBot.IsBotRunning)
                            {
                                break;
                            }
                            var tick = historicalTicks[i];
                            tempBot.IbWrapper.tickByTickAllLast(tickReqId, 0, tick.Time, tick.Price, 0, null, "", "");
                            i++;
                        }
                    }
                    allOrderEntries.Add(tempBot.OrderEntries);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    processed++;
                    Console.Write($"Processed {processed} out of {numFiles} files.");
                }
            });
            Console.Write("\n");
            flattenedOrderEntries = allOrderEntries.SelectMany(x => x).ToList();
            flattenedOrderEntries = flattenedOrderEntries.OrderBy(o => o.DateTime).ToList();
            DownloadOrderEntries();
            RunBacktestAnalysis();
        }

        public void DownloadOrderEntries()
        {
            var writer = new StreamWriter(RootFilePath + $"{csvName}.csv");
            //var writer = new StreamWriter(Console.OpenStandardOutput());
            var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);
            csvWriter.WriteHeader<OrderEntry>();
            csvWriter.NextRecord();
            csvWriter.WriteRecords(flattenedOrderEntries);

            writer.Flush();

            // save to txt
            JsonHandler.WriteToJsonFile<List<OrderEntry>>(RootFilePath + $"{csvName}.txt", flattenedOrderEntries);
        }
        public void RunBacktestAnalysis()
        {
            double prevPrice = 0;
            int wins = 0;
            int losses = 0;

            foreach (var order in flattenedOrderEntries)
            {
                if (order.Action == "BUY")
                {
                    prevPrice = order.Price;
                }
                else if (order.Action == "SELL")
                {
                    double diff = prevPrice - order.Price;
                    if (order.Right == "C")
                    {
                        if (diff < 0)
                            wins++;
                        else
                            losses++;
                    }
                    else
                    {
                        if (diff < 0)
                            losses++;
                        else
                            wins++;
                    }
                }
            }
            var numTrades = wins + losses;
            Console.WriteLine($"Wins: {wins}, Losses: {losses}, Ratio: {(double)wins / numTrades}");
        }

        public static T DeepCopy<T>(T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}