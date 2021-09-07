using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot.Indicators
{
    public class Macd : Indicator
    {
        private readonly int fastEmaLookback;
        private readonly int slowEmaLookback;
        private readonly int signalLookback;

        public Macd(string name, int fastEma, int slowEma, int signal): base(name)
        {
            this.fastEmaLookback = fastEma;
            this.slowEmaLookback = slowEma;
            this.signalLookback = signal;
        }

        public override Dictionary<string, List<double>> Calculate(BarList bars)
        {
            var fastEmaObj = new Ema("fastEma", fastEmaLookback);
            var slowEmaObj = new Ema("slowEma", slowEmaLookback);

            var fastEma = fastEmaObj.Calculate(bars)["emas"];
            var slowEma = slowEmaObj.Calculate(bars)["emas"];

            var macd = new List<double>();
            
            for (int i = 0; i < bars.Size; i++)
            {
                macd.Add(fastEma[i] - slowEma[i]);
            }

            var signalObj = new Ema("signal", signalLookback);
            var tempBars = new BarList();
            tempBars.Closes = macd;
            var signal = signalObj.Calculate(tempBars)["emas"];

            var histogram = new List<double>();

            for (int i = 0; i < bars.Size; i++)
            {
                histogram.Add(macd[i] - signal[i]);
            }
            
            return new Dictionary<string, List<double>> { {"histogram", histogram } };
        }
    }
}