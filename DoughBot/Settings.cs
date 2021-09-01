using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DoughBot
{
    public class Settings
    {
        [JsonProperty("ibUserName")]
        public string IbUserName { get; set; }
        [JsonProperty("ibPassword")]
        public string IbPassword { get; set; }
        [JsonProperty("ibTradingMode")]
        public string IbTradingMode { get; set; }
        [JsonProperty("ibPort")]
        public int IbPort { get; set; }
        [JsonProperty("ibVersion")]
        public string IbVersion { get; set; }
        [JsonProperty("num_contracts")]
        public Dictionary<string, int> NumContracts { get; set; }
    }
}
