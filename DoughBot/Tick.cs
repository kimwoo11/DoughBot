using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot
{
    public class Tick
    {
        public long Time { get; set; }
        public double Price { get; set; }
        public string Symbol { get; set; }
        public Tick(long time, double price, string symbol)
        {
            this.Time = time;
            this.Price = price;
            this.Symbol = symbol;
        }
    }
}
