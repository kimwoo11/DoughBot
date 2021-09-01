using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot
{
    public enum MarketZones
    {
        PreMarket,
        MarketOpen,
        TradingOpen,
        TradingClose,
        MarketClose,
        AfterMarket,
        None,
    }
    public class ZoneAnalysis
    {
        public static MarketZones GetZone(long currentTime)
        {
            var currTime = Bot.ConvertUtcTimeStampToEst(currentTime);
            var preMarket = currTime.Date + new TimeSpan(4, 0, 0);
            var marketOpen = currTime.Date + new TimeSpan(9, 30, 0);
            var tradingOpen = currTime.Date + new TimeSpan(9, 40, 0);
            var tradingClose = currTime.Date + new TimeSpan(15, 58, 0);
            var marketClose = currTime.Date + new TimeSpan(15, 58, 0);
            if (currTime >= preMarket && currTime < marketOpen)
            {
                return MarketZones.PreMarket;
            }
            else if (currTime >= marketOpen && currTime < tradingOpen)
            {
                return MarketZones.MarketOpen;
            }
            else if (currTime >= tradingOpen && currTime < tradingClose)
            {
                return MarketZones.TradingOpen;
            }
            else if (currTime >= tradingClose && currTime < marketClose)
            {
                return MarketZones.TradingClose;
            }
            else if (currTime >= marketClose)
            {
                return MarketZones.MarketClose;
            }
            return MarketZones.None;
        }
    }
}
