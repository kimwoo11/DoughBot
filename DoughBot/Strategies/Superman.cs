using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoughBot.Indicators;

namespace DoughBot.Strategies
{
    public class Superman: Strategy
    {
        private readonly double risk;
        private readonly double reward;
        private readonly double atrLimiter;
        private bool prevIsBullish;

        public Superman(int lookback, int multiplier, int fastEma, int slowEma, int trendEma, double risk, double reward, double atrLimiter) : base()
        {
            this.risk = risk;
            this.reward = reward;
            this.atrLimiter = atrLimiter;
            Indicators.Add(new Atr("atrs", 14));
            Indicators.Add(new Ema("fastEmas", fastEma));
            Indicators.Add(new Ema("slowEmas", slowEma));
            Indicators.Add(new Ema("trendEmas", trendEma));
            Indicators.Add(new SuperTrend("superTrend", lookback, multiplier));
        }

        public override SignalType BuySignal(SecurityData security)
        {
            var tickPrice = security.CurrentPrice;
            var close = security.Bars.Closes.Last();
            var technicals = security.TechnicalIndicators;
            var atr = technicals["atrs"].Last();
            var superTrend = technicals["superTrend"].Last();
            var fastEma = technicals["fastEmas"].Last();
            var slowEma = technicals["slowEmas"].Last();
            var trendEma = technicals["trendEmas"].Last();
            var currentIsBullish = (close > superTrend);
            var signal = SignalType.None;

            if (atr >= tickPrice * atrLimiter && currentIsBullish != prevIsBullish)
            {
                // Bullish Signal
                if (currentIsBullish && fastEma > slowEma && slowEma > trendEma)
                {
                    security.PurchaseAtr = atr;
                    security.PurchasePrice = tickPrice;
                    security.CurrentStopLoss = tickPrice - tickPrice * risk;
                    security.CurrentTakeProfit = tickPrice + tickPrice * reward;
                    signal = SignalType.BuyCall;
                }
                // Bearish Signal
                else if (!currentIsBullish && fastEma < slowEma && slowEma < trendEma)
                {
                    security.PurchaseAtr = atr;
                    security.PurchasePrice = tickPrice;
                    security.CurrentStopLoss = tickPrice + tickPrice * risk;
                    security.CurrentTakeProfit = tickPrice - tickPrice * reward;
                    signal = SignalType.BuyPut;
                }
            }
            
            prevIsBullish = currentIsBullish;

            return signal;
        }
    }
}
