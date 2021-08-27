using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot.Indicators
{
    public class Fractal : Indicator 
    {
        public Fractal(string name) : base(name)
        {
        }

        public override Dictionary<string, List<double>> Calculate(BarList bars)  // {bearishFractals, bullishFractals}
        {
            var bearishFractals = new List<double>();
            var bullishFractals = new List<double>();

            var highs = bars.Highs;
            var lows = bars.Lows;
            var size = bars.Size;

            for (int i = 0; i < size; i++)
            {
                if (i < 5)
                {
                    bearishFractals.Add(-1);
                    bullishFractals.Add(-1);
                }
                else
                {
                    var bearishCandidate = highs[i - 2];
                    if (bearishCandidate >= highs[i - 4] && bearishCandidate >= highs[i - 3] && bearishCandidate >= highs[i - 1] && bearishCandidate >= highs[i])
                    {
                        bearishFractals.Add(bearishCandidate);
                    }

                    var bullishCandidate = lows[i - 2];
                    if (bullishCandidate <= lows[i - 4] && bullishCandidate <= lows[i - 3] && bullishCandidate <= lows[i - 1] && bullishCandidate <= lows[i])
                    {
                        bullishFractals.Add(bullishCandidate);
                    }
                }
            }

            return new Dictionary<string, List<double>> { { "bearishFractals", bearishFractals }, { "bullishFractals", bullishFractals } };
        }
    }
}
