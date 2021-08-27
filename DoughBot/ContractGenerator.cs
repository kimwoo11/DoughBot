using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace DoughBot
{
    public class ContractGenerator
    {
        public static Contract GenerateStockContract(string symbol)
        {
            Contract contract = new Contract();
            contract.Symbol = symbol.ToUpper();
            contract.SecType = "STK";
            contract.Exchange = "SMART";
            contract.Currency = "USD";

            return contract;
        }

        public static Contract GenerateOptionContract(string symbol)
        {
            Contract contract = new Contract();
            contract.Symbol = symbol.ToUpper();
            contract.SecType = "OPT";
            contract.Exchange = "SMART";
            contract.LastTradeDateOrContractMonth = GetThisFriday().ToString("yyyyMMdd");    // Friday
            contract.Currency = "USD";

            return contract;
        }

        public static DateTime GetThisFriday()
        {
            var today = DateTime.Today;
            return today.AddDays(-(int)today.DayOfWeek).AddDays(5);
        }
    }
}
