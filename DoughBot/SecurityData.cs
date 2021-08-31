using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using DoughBot.Strategies;

namespace DoughBot
{
    public class SecurityData
    {
        // Symbol's metadata
        public int Id { get; }
        public string Name { get; }
        public Contract StockContract { get; }
        public Contract OptionContract { get; }
        public Strategy Strategy { get; }

        // Current values of the Symbol; CurrentBar is an incomplete bar
        public long CurrentTime { get; set; } // current tick time
        public double CurrentPrice { get; set; }
        public Bar CurrentBar { get; set; }

        // Complete Bars and Indicators
        public BarList Bars = new BarList();
        public Dictionary<string, List<double>> TechnicalIndicators = new Dictionary<string, List<double>>();

        // Additional
        public List<double> StrikePrices = new List<double>();
        public SignalType CurrentSignal = SignalType.None;
        public double PurchasePrice = -1;
        public double PurchaseAtr = -1;
        public double CurrentStopLoss = -1;
        public double CurrentTakeProfit = -1;

        public SecurityData(int id, string symbol, Strategy strategy)
        {
            Id = id;
            Name = symbol;
            StockContract = ContractGenerator.GenerateStockContract(symbol);
            OptionContract = ContractGenerator.GenerateOptionContract(symbol);
            Strategy = strategy;
        }

        public void Add(Bar bar)
        {
            if (CurrentBar == null)
            {
                CurrentBar = bar;
                Bars.Add(CurrentBar);
            }
            if (CurrentBar.Time == bar.Time)
            {
                CurrentBar = bar;
            }
            else if (CurrentBar.Time != bar.Time)
            {
                Bars.Add(CurrentBar);
                DataProcessor.Process(this);

                var bullishFractal = TechnicalIndicators["bullishFractals"].Last();
                var bearishFractal = TechnicalIndicators["bearishFractals"].Last();
                var currentDateTime = Bot.ConvertUtcTimeStampToEst(Convert.ToInt64(CurrentBar.Time));
                var currentTickDateTime = Bot.ConvertUtcTimeStampToEst(CurrentTime);
                Console.WriteLine($"Time: {CurrentBar.Time}, Symbol: {Name}, Current Bullish: {bullishFractal}, Current Bearish: {bearishFractal}");
                Console.WriteLine($"Time: {currentTickDateTime}, Symbol: {Name}, Tick Price: {CurrentPrice}");
                //Console.WriteLine($"Symbol: {Name}, Time:{ currentDateTime}");
                CurrentBar = bar;
            }
        }

        public void Add(long time, double tickPrice)
        {
            CurrentTime = time;
            CurrentPrice = tickPrice;
        }

        public double GetStrikePrice(double tickPrice, SignalType signal)
        {
            int start = 0;
            int end = StrikePrices.Count;
            while (start < end)
            {
                int mid = (start + end) / 2;
                if (tickPrice > StrikePrices[mid])
                    start = mid + 1;
                else if (tickPrice < StrikePrices[mid])
                    end = mid - 1;
                else
                    return StrikePrices[mid];
            }
            if (signal == SignalType.BuyCall)
            {
                if (StrikePrices[start] < tickPrice)
                    return StrikePrices[start];
                else
                    return StrikePrices[start - 1];
            }
            else
            {
                if (StrikePrices[start] > tickPrice)
                    return StrikePrices[start];
                else
                    return StrikePrices[start + 1];
            }
        }
    }
}
