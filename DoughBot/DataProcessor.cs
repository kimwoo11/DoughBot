using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoughBot.Indicators;

namespace DoughBot
{
    public class DataProcessor
    {
        public static void Process(SecurityData security)
        {
            var strategy = security.Strategy;
            var indicators = strategy.Indicators;

            Task[] tasks = new Task[indicators.Count];

            int i = 0;
            foreach (var indicator in indicators)
            {
                //tasks[i] = Task.Factory.StartNew(() => CalculateIndicator(indicator, security));
                //i++;
                CalculateIndicator(indicator, security);
            }
            //Task.WaitAll(tasks);
        }
        public static void CalculateIndicator(Indicator indicator, SecurityData security)
        {
            var name = indicator.Name;
            var securityIndicators = security.TechnicalIndicators;

            if (indicator.GetType() == typeof(Ema))
            {
                var result = indicator.Calculate(security.Bars);
                if (securityIndicators.ContainsKey(name))
                {
                    securityIndicators[name] = result["emas"];
                }
                else
                {
                    securityIndicators.Add(name, result["emas"]);
                }
            }
            else if (indicator.GetType() == typeof(Atr))
            {
                var result = indicator.Calculate(security.Bars);
                if (securityIndicators.ContainsKey(name))
                {
                    securityIndicators[name] = result["atrs"];
                }
                else
                {
                    securityIndicators.Add(name, result["atrs"]);
                }
            }
            else if (indicator.GetType() == typeof(Fractal))
            {
                var result = indicator.Calculate(security.Bars);
                if (securityIndicators.ContainsKey("bearishFractals") && securityIndicators.ContainsKey("bullishFractals"))
                {
                    securityIndicators["bearishFractals"] = result["bearishFractals"];
                    securityIndicators["bullishFractals"] = result["bullishFractals"];
                }
                else
                {
                    securityIndicators.Add("bearishFractals", result["bearishFractals"]);
                    securityIndicators.Add("bullishFractals", result["bullishFractals"]);
                }
            }
            else if (indicator.GetType() == typeof(StrictEmaFractal))
            {
                var result = indicator.Calculate(security.Bars);
                if (securityIndicators.ContainsKey("bearishFractals") && securityIndicators.ContainsKey("bullishFractals"))
                {
                    securityIndicators["bearishFractals"] = result["bearishFractals"];
                    securityIndicators["bullishFractals"] = result["bullishFractals"];
                }
                else
                {
                    securityIndicators.Add("bearishFractals", result["bearishFractals"]);
                    securityIndicators.Add("bullishFractals", result["bullishFractals"]);
                }
            }
            else if (indicator.GetType() == typeof(SuperTrend))
            {
                var result = indicator.Calculate(security.Bars);
                if (securityIndicators.ContainsKey(name))
                {
                    securityIndicators[name] = result["superTrend"];
                }
                else
                {
                    securityIndicators.Add(name, result["superTrend"]);
                }
            }
            else if (indicator.GetType() == typeof(Macd))
            {
                var result = indicator.Calculate(security.Bars);
                if (securityIndicators.ContainsKey(name))
                {
                    securityIndicators[name] = result["histogram"];
                }
                else
                {
                    securityIndicators.Add(name, result["histogram"]);
                }
            }
            else
            {
                throw new ArgumentException("Indicator currently not supported.");
            }
        }
    }
}
