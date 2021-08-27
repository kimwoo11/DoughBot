using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace DoughBot
{
    public class BarList
    {
        public List<long> Times = new List<long>();
        public List<double> Opens = new List<double>();
        public List<double> Highs = new List<double>();
        public List<double> Lows = new List<double>();
        public List<double> Closes = new List<double>();
        public int Size = 0;

        public void Add(Bar bar)
        {
            Times.Add(Convert.ToInt64(bar.Time));
            Opens.Add(bar.Open);
            Highs.Add(bar.High);
            Lows.Add(bar.Low);
            Closes.Add(bar.Close);
            Size++;
        }
    }
}
