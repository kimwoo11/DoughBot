using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoughBot.Indicators;
using DoughBot;

namespace DoughBot.Strategies
{
    public class OneShot : Strategy
    { 
        private double prevBearishFractal;
        private double prevBullishFractal;
        private bool isBullishReady = false;
        private bool isBearishReady = false;
        private bool strategyActive = true;
        public OneShot() 
        {
            Indicators.Add(new Atr("atrs", 14));
            Indicators.Add(new Fractal("fractals"));
        }
        public override void Initialize()
        {
            isBullishReady = false;
            isBearishReady = false;
            strategyActive = true;
        }

        public override SignalType BuySignal(SecurityData security)
        {
            var tickPrice = security.CurrentPrice;
            var bearishFractal = security.TechnicalIndicators["bearishFractals"].Last();
            var bullishFractal = security.TechnicalIndicators["bullishFractals"].Last();
            var atr = security.TechnicalIndicators["atrs"].Last();
            var currentTime = Convert.ToInt64(security.CurrentBar.Time);

            if (ZoneAnalysis.GetZone(currentTime) == MarketZones.MarketOpen)
            {
                strategyActive = true;
                prevBearishFractal = bearishFractal;
                prevBullishFractal = bullishFractal;
            } 
            else if (ZoneAnalysis.GetZone(currentTime) == MarketZones.TradingOpen)
            {
                if (prevBearishFractal != bearishFractal && !isBearishReady)
                {
                    prevBearishFractal = bearishFractal;
                    isBearishReady  = true;
                }
                else if (prevBullishFractal != bullishFractal && !isBullishReady)
                {
                    prevBullishFractal = bullishFractal;
                    isBullishReady = true;
                }
            }

            if (!strategyActive)
            {
                return SignalType.None;
            }

            // Bullish
            if (isBearishReady)
            {
                if (tickPrice > prevBearishFractal)
                {
                    security.PurchasePrice = prevBearishFractal;
                    //double rr = Math.Max(Math.Floor(atr), 1.0);
                    security.CurrentStopLoss = prevBearishFractal - prevBearishFractal*0.0015;
                    security.CurrentTakeProfit = prevBearishFractal + prevBearishFractal*0.0015;
                    security.PurchaseAtr = atr;
                    return SignalType.BuyCall;
                }
            }

            // Bearish
            if (isBullishReady)
            {
                if (tickPrice < prevBullishFractal)
                {
                    security.PurchasePrice = prevBullishFractal;
                    //double rr = Math.Max(Math.Floor(atr), 1.0);
                    security.CurrentStopLoss = prevBullishFractal + prevBullishFractal* 0.0015;
                    security.CurrentTakeProfit = prevBullishFractal - prevBullishFractal * 0.0015;
                    security.PurchaseAtr = atr;
                    return SignalType.BuyPut;
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
                    strategyActive = false;
                    isBearishReady = false; 
                    return SignalType.SellCall;
                }
            } 
            else if (security.CurrentSignal == SignalType.BuyPut)
            {
                if (tickPrice <= security.CurrentTakeProfit || tickPrice >= security.CurrentStopLoss)
                {
                    strategyActive = false;
                    isBullishReady = false;
                    return SignalType.SellPut;
                }
            }
            return SignalType.None;
        }

    }
}
