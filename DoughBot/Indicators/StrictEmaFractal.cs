using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot.Indicators
{
    public class StrictEmaFractal : Indicator
    {
        private readonly int fastLookback;
        private readonly int slowLookback;
        private readonly int trendLookback;
        
        public StrictEmaFractal(string name, int fastLookback, int slowLookback, int trendLookback) : base(name)
        {
            this.fastLookback = fastLookback;
            this.slowLookback = slowLookback;
            this.trendLookback = trendLookback;
        }

        public override Dictionary<string, List<double>> Calculate(BarList bars)  // {bearishFractals, bullishFractals}
        {
            var bearishFractals = new List<double>();
            var bullishFractals = new List<double>();

            var fastEmaIndicator = new Ema("fastEmas", fastLookback);
            var slowEmaIndicator = new Ema("slowEmas", slowLookback);
            var trendEmaIndicator = new Ema("trendEmas", trendLookback);

            var fastEmaTask = Task<Dictionary<string, List<double>>>.Factory.StartNew(() => fastEmaIndicator.Calculate(bars));
            var slowEmaTask = Task<Dictionary<string, List<double>>>.Factory.StartNew(() => slowEmaIndicator.Calculate(bars));
            var trendEmaTask = Task<Dictionary<string, List<double>>>.Factory.StartNew(() => trendEmaIndicator.Calculate(bars));

            var fastEmas = fastEmaTask.Result["emas"];
            var slowEmas = slowEmaTask.Result["emas"];
            var trendEmas = trendEmaTask.Result["emas"];

            var highs = bars.Highs;
            var lows = bars.Lows;
            var closes = bars.Closes;

            var size = bars.Size;

            double currentBearishFractal = double.MinValue;
            bool bearishFractalActive = false;
            double currentBullishFractal = double.MaxValue;
            bool bullishFractalActive = false;

            for (int i = 0; i < size; i++)
            {
                if (i < 4)
                {
                    bearishFractals.Add(-1);
                    bullishFractals.Add(-1);
                }
                else
                {
                    var bearishCandidate = highs[i - 2];
                    if (bearishCandidate >= highs[i - 4] && bearishCandidate >= highs[i - 3] && bearishCandidate >= highs[i - 1] && bearishCandidate >= highs[i])
                    {
                        currentBearishFractal = Math.Max(currentBearishFractal, bearishCandidate);
                    }
                    if (lows[i] <= fastEmas[i] | lows[i - 1] <= fastEmas[i - 1])
                    {
                        if (closes[i] <= slowEmas[i] || closes[i] <= trendEmas[i] || closes[i - 1] <= slowEmas[i - 1] || closes[i - 1] <= trendEmas[i - 1])
                        {
                            currentBearishFractal = double.MinValue;
                            bearishFractalActive = false;
                        }
                        else
                        {
                            if (currentBearishFractal != double.MinValue)
                            {
                                bearishFractalActive = true;
                            }
                        }
                    }
                    if (highs[i] > currentBearishFractal)
                    {
                        currentBearishFractal = double.MinValue;
                        bearishFractalActive = false;
                    }

                    if (currentBearishFractal != double.MinValue && bearishFractalActive && (fastEmas[i] > slowEmas[i] && slowEmas[i] > trendEmas[i]))
                    {
                        bearishFractals.Add(currentBearishFractal);
                    }

                    else
                    {
                        bearishFractals.Add(-1);
                    }


                    var bullishCandidate = lows[i - 2];
                    if (bullishCandidate <= lows[i - 4] && bullishCandidate <= lows[i - 3] && bullishCandidate <= lows[i - 1] && bullishCandidate <= lows[i])
                    {
                        currentBullishFractal = Math.Min(currentBullishFractal, bullishCandidate);
                    }
                    if (highs[i] >= fastEmas[i] || highs[i-1] >= fastEmas[i-1])
                    {
                        if (closes[i] >= slowEmas[i] || closes[i] >= trendEmas[i] || closes[i-1] >= slowEmas[i-1] || closes[i] >= trendEmas[i-1])
                        {
                            currentBullishFractal = double.MaxValue;
                            bullishFractalActive = false;
                        }
                        else
                        {
                            if (currentBullishFractal != double.MaxValue)
                            {
                                bullishFractalActive = true;
                            }
                        }
                    }

                    if (lows[i] < currentBullishFractal)
                    {
                        currentBullishFractal = double.MaxValue;
                        bullishFractalActive = false;
                    }

                    if (currentBullishFractal != double.MaxValue && bullishFractalActive && (fastEmas[i] < slowEmas[i] && slowEmas[i] < trendEmas[i]))
                    {
                        bullishFractals.Add(currentBullishFractal);
                    }
                    else
                    {
                        bullishFractals.Add(-1);
                    }
                }
            }

            return new Dictionary<string, List<double>> { 
                { "bearishFractals", bearishFractals }, 
                { "bullishFractals", bullishFractals },
            };
        }
    }
}
