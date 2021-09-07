using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot.Indicators
{
    public class Atr: Indicator
    {
        private readonly int lookback;
        public Atr(string name, int lookback) : base(name)
        {
            this.lookback = lookback;
        }
        public override Dictionary<string, List<double>> Calculate(BarList bars)
        {
            var highs = bars.Highs;
            var lows = bars.Lows;
            var closes = bars.Closes; 

            List<double> results = new List<double>();
            double prevAtr = 0;
            double prevClose = 0;
            double highMinusPrevClose = 0;
            double lowMinusPrevClose = 0;
            double sumTr = 0;

            for (int i = 0; i < closes.Count; i++)
            {
                var h = highs[i];
                var l = lows[i];
                var c = closes[i];

                if (i != 0)
                {
                    highMinusPrevClose = Math.Abs(h - prevClose);
                    lowMinusPrevClose = Math.Abs(l - prevClose);
                }

                double tr = Math.Max((h - l), Math.Max(highMinusPrevClose, lowMinusPrevClose));
                double result = 0;

                if (i > lookback - 1)
                {
                    result = (prevAtr * (lookback - 1) + tr) / lookback;
                    prevAtr = result;
                }
                else if (i == lookback - 1)
                {
                    sumTr += tr;
                    result = sumTr / lookback;
                    prevAtr = result;
                }
                else
                {
                    sumTr += tr;
                }

                results.Add(result);
                prevClose = c;
            }
            return new Dictionary<string, List<double>>{ { "atrs", results } };
        }
    }
}
