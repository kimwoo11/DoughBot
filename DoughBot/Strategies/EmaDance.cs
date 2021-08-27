using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoughBot.Indicators;

namespace DoughBot.Strategies
{
    public class EmaDance: Strategy
    {
        private readonly double atrLimiter;
        private readonly double rr;
        public EmaDance(int fastLookbck, int slowLookback, int trendLookback, double rr, double atrLimiter)
        {
            Indicators.Add(new Ema("fastEmas", fastLookbck));
            Indicators.Add(new Ema("slowEmas", slowLookback));
            Indicators.Add(new Ema("trendEmas", trendLookback));
            Indicators.Add(new Atr("atrs", 14));
            this.atrLimiter = atrLimiter;
            this.rr = rr;
        }

        public override SignalType BuySignal(SecurityData security)
        {
            var fastEmas = security.TechnicalIndicators["fastEmas"];
            var slowEmas = security.TechnicalIndicators["slowEmas"];
            var trendEmas = security.TechnicalIndicators["trendEmas"];

            var currAtr = security.TechnicalIndicators["atrs"].Last();

            var currFastEma = fastEmas.Last();
            var prevFastEma = fastEmas[fastEmas.Count - 2];
            var currSlowEma = slowEmas.Last();
            var prevSlowEma = slowEmas[slowEmas.Count - 2];
            var currTrendEma = trendEmas.Last();
            
            var tickPrice = security.CurrentPrice;
            var atrLimit = tickPrice * atrLimiter;

            if (currAtr > atrLimit)
            {
                if (currFastEma > currSlowEma && prevFastEma < prevSlowEma & currFastEma > currSlowEma && currSlowEma > currTrendEma)
                {
                    security.PurchaseAtr = currAtr;
                    security.PurchasePrice = tickPrice;
                    security.CurrentTakeProfit = tickPrice + tickPrice * rr;
                    security.CurrentStopLoss = tickPrice - tickPrice * rr;
                    return SignalType.BuyCall;
                }
                else if (currFastEma < currSlowEma && prevFastEma > prevSlowEma & currFastEma < currSlowEma && currSlowEma < currTrendEma)
                {
                    security.PurchaseAtr = currAtr;
                    security.PurchasePrice = tickPrice;
                    security.CurrentTakeProfit = tickPrice - tickPrice * rr;
                    security.CurrentStopLoss = tickPrice + tickPrice * rr;
                    return SignalType.BuyPut;
                }
            }

            return SignalType.None;
        }

        public override SignalType SellSignal(SecurityData security)
        {
            var tickPrice = security.CurrentPrice;

            var fastEmas = security.TechnicalIndicators["fastEmas"];
            var slowEmas = security.TechnicalIndicators["slowEmas"];

            var currFastEma = fastEmas.Last();
            var prevFastEma = fastEmas[fastEmas.Count - 2];
            var currSlowEma = slowEmas.Last();
            var prevSlowEma = slowEmas[slowEmas.Count - 2];
            
            if (security.CurrentSignal == SignalType.BuyCall)
            {
                if (tickPrice >= security.CurrentTakeProfit || tickPrice <= security.CurrentStopLoss)
                {
                    security.TechnicalIndicators = new Dictionary<string, List<double>>();
                    return SignalType.SellCall;
                }
                else if (currFastEma < currSlowEma && prevFastEma > prevSlowEma)
                {
                    return SignalType.SellCall;
                }
            }
            else if (security.CurrentSignal == SignalType.BuyPut)
            {
                if (tickPrice <= security.CurrentTakeProfit || tickPrice >= security.CurrentStopLoss)
                {
                    security.TechnicalIndicators = new Dictionary<string, List<double>>();
                    return SignalType.SellPut;
                }
                else if (currFastEma > currSlowEma && prevFastEma < prevSlowEma)
                {
                    return SignalType.SellPut;
                }
            }

            return SignalType.None;
        }
    }
}
