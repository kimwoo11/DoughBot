using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot.Indicators
{
    public class Ema: Indicator
    {
        private readonly int lookback;
        public Ema(string name, int lookback) : base(name)
        {
            this.lookback = lookback;
        }
        public override Dictionary<string, List<double>> Calculate(BarList bars)
        {
            var closes = bars.Closes;
            List<double> results = new List<double>();

            double k = 2 / (double)(lookback + 1);
            double prevEma = closes[0];
            results.Add(prevEma);

            for (int i = 1; i < closes.Count; i++)
            {
                double result = prevEma + k * (closes[i] - prevEma);
                prevEma = result;
                results.Add(result);
            }
            return new Dictionary<string, List<double>> { { "emas", results } };
        }
    }
}
