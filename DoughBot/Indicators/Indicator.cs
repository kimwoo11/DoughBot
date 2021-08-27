using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot.Indicators
{
    public class Indicator
    {
        public string Name;

        public Indicator(string name)
        {
            Name = name;
        }
        public virtual Dictionary<string, List<double>> Calculate(BarList bars) { 
            return default; 
        }
    }
}
