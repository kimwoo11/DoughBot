using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoughBot.Indicators;

namespace DoughBot.Strategies
{
    // check for signal only at 9:32
    public class McDouble : Strategy
    {
        private readonly double rr;
        private bool madeTrade = false;

        public McDouble(int superTrendLookback, int superTrendMultiplier, int fastEma, int slowEma, int signal, double rr) : base()
        {
            this.rr = rr;
            Indicators.Add(new Ema("fastEma", 9));
            Indicators.Add(new Ema("slowEma", 21));
            Indicators.Add(new Ema("trendEma", 50));
            Indicators.Add(new SuperTrend("superTrend", superTrendLookback, superTrendMultiplier));
            Indicators.Add(new Macd("macd", fastEma, slowEma, signal));
        }

        public override SignalType BuySignal(SecurityData security)
        {
            var superTrend = security.TechnicalIndicators["superTrend"].Last();
            var macd = security.TechnicalIndicators["macd"].Last();
            var tickPrice = security.CurrentPrice;
            var fastEma = security.TechnicalIndicators["fastEma"].Last();
            var slowEma = security.TechnicalIndicators["slowEma"].Last();
            var trendEma = security.TechnicalIndicators["trendEma"].Last();
            var currTime = security.CurrentTime;
            var currDateTime = Bot.ConvertUtcTimeStampToEst(currTime);

            if (madeTrade || !(currDateTime.Hour == 9 && currDateTime.Minute == 32))
            {
                return SignalType.None;
            }
            // bullish
            if (tickPrice > superTrend)
            {
                if (macd > 0 && fastEma > slowEma) //&& slowEma > trendEma)
                {
                    security.PurchasePrice = tickPrice;
                    security.CurrentStopLoss = tickPrice - tickPrice * rr;
                    security.CurrentTakeProfit = tickPrice + tickPrice * rr;
                    madeTrade = true;
                    return SignalType.BuyCall;
                }
            }

            // bearish
            else if (tickPrice < superTrend)
            {
                if (macd < 0 && fastEma < slowEma) //&& slowEma < trendEma)
                {
                    security.PurchasePrice = tickPrice;
                    security.CurrentStopLoss = tickPrice + tickPrice * rr;
                    security.CurrentTakeProfit = tickPrice - tickPrice * rr;
                    madeTrade = true;
                    return SignalType.BuyPut;
                }
            }
            return SignalType.None;
        }
    }
}
