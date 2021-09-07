using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot.Indicators
{
    public class SuperTrend : Indicator
    {
        private int lookback;
        private int multiplier;

        public SuperTrend(string name, int lookback, int multiplier): base(name)
        {
            this.lookback = lookback;
            this.multiplier = multiplier;
        }

        public override Dictionary<string, List<double>> Calculate(BarList bars)
        {
            var results = new List<double>();
            var closes = bars.Closes;
            var highs = bars.Highs;
            var lows = bars.Lows; 

            var atrObj = new Atr(Name, lookback);
            var atrResults = atrObj.Calculate(bars);
            var atrs = atrResults["atrs"];

            bool isBullish = true;
            double upperBand = 0;
            double lowerBand = 0;

            for (int i = 0; i < bars.Size; i++)
            {
                double supertrend = -1;
                if (i >= lookback - 1)
                {
                    var mid = (highs[i] + lows[i]) / 2;
                    var atr = atrs[i];
                    var prevClose = closes[i - 1];

                    // potential bands
                    var upperEval = mid + multiplier * atr;
                    var lowerEval = mid - multiplier * atr;
                    
                    // initial values
                    if (i == lookback - 1)
                    {
                        isBullish = (closes[i] >= mid);

                        upperBand = upperEval;
                        lowerBand = lowerEval;
                    }

                    // new upper band
                    if (upperEval < upperBand || prevClose > upperBand)
                    {
                        upperBand = upperEval;
                    }

                    // new lower band
                    if (lowerEval > lowerBand || prevClose < lowerBand)
                    {
                        lowerBand = lowerEval;
                    }

                    // supertrend
                    if (closes[i] <= ((isBullish) ? lowerBand : upperBand))
                    {
                        supertrend = upperBand;
                        isBullish = false;
                    }
                    else
                    {
                        supertrend = lowerBand;
                        isBullish = true;
                    }
                }
                results.Add(supertrend);
            }

            return new Dictionary<string, List<double>> { { "superTrend", results } };
        }
    }
}
