using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoughBot.Indicators;

namespace DoughBot.Strategies
{
    public class Strategy
    {
        public List<Indicator> Indicators;
        
        public Strategy()
        {
            Indicators = new List<Indicator>();
        }

        public virtual SignalType GetSignal(SecurityData security)
        {
            SignalType signal = SignalType.None;
            if (security.TechnicalIndicators.Count == 0)
            {
                signal = SignalType.None;
            }
            else if (security.CurrentSignal == SignalType.None)
            {
                signal = BuySignal(security);
            }
            else if (security.CurrentSignal == SignalType.BuyCall || security.CurrentSignal == SignalType.BuyPut)
            {
                signal = SellSignal(security);
            }

            return signal;
        }

        public virtual SignalType BuySignal(SecurityData security) { return SignalType.None; }
        public virtual SignalType SellSignal(SecurityData security) { return SignalType.None; }
        public virtual void Initialize() { }
    }
}
