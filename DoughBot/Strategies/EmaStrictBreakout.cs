using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoughBot.Indicators;

namespace DoughBot.Strategies
{
    class EmaStrictBreakout : Strategy
    {
        private readonly double risk;
        private readonly double reward;
        private readonly double atrLimiter;
        public EmaStrictBreakout(int fastEmaPeriod, int slowEmaPeriod, int trendEmaPeriod, double risk, double reward, double atrLimiter) : base()
        {
            this.risk = risk;
            this.reward = reward;
            this.atrLimiter = atrLimiter;
            Indicators.Add(new Atr("atrs", 14));
            Indicators.Add(new StrictEmaFractal("strictFractals", fastEmaPeriod, slowEmaPeriod, trendEmaPeriod));
        }

        public override SignalType BuySignal(SecurityData security)
        {
            var tickPrice = security.CurrentPrice;
            var technicals = security.TechnicalIndicators;
            var atr = technicals["atrs"].Last();
            var bearishFractal = technicals["bearishFractals"].Last();
            var bullishFractal = technicals["bullishFractals"].Last();


            if (atr >= tickPrice * atrLimiter)
            {
                // Bullish Signal
                if (bearishFractal != -1)
                {
                    if (tickPrice > bearishFractal)
                    {
                        security.PurchaseAtr = atr;
                        security.PurchasePrice = bearishFractal;
                        security.CurrentStopLoss = bearishFractal - bearishFractal * risk;
                        security.CurrentTakeProfit = bearishFractal + bearishFractal * reward;
                        return SignalType.BuyCall;
                    }
                }

                // Bearish Signal
                if (bullishFractal != -1)
                {
                    if (tickPrice < bullishFractal)
                    {
                        security.PurchaseAtr = atr;
                        security.PurchasePrice = bullishFractal;
                        security.CurrentStopLoss = bullishFractal + bullishFractal * risk;
                        security.CurrentTakeProfit = bullishFractal - bullishFractal * reward;
                        return SignalType.BuyPut;
                    }
                }
            }
            return SignalType.None;
        }

        public override SignalType SellSignal(SecurityData security)
        {
            var tickPrice = security.CurrentPrice;
            if (security.CurrentSignal == SignalType.BuyCall)
            {
                if (tickPrice >= security.CurrentTakeProfit || tickPrice <= security.CurrentStopLoss)
                {
                    security.TechnicalIndicators = new Dictionary<string, List<double>>();
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
            }
            return SignalType.None;
        }
    }
}
