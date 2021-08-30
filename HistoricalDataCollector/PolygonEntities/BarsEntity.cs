using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HistoricalDataCollector.PolygonEntities
{
    public class BarsEntity
    {
        [JsonProperty("ticker")]
        public string Ticker { get; set; }
        [JsonProperty("adjusted")]
        public bool Adjusted { get; set; }
        [JsonProperty("queryCount")]
        public int QueryCount { get; set; }
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("resultsCount")]
        public int ResultsCount { get; set; }
        [JsonProperty("results")]
        public List<BarsResult> Results { get; set; }

    }
    public class BarsResult
    {
        [JsonProperty("c")]
        public double Close { get; set; }
        [JsonProperty("h")]
        public double High { get; set; }
        [JsonProperty("l")]
        public double Low { get; set; }
        [JsonProperty("n")]
        public double NumberOfTransactions { get; set; }
        [JsonProperty("o")]
        public double Open { get; set; }
        [JsonProperty("t")]
        public long Time { get; set; }
        [JsonProperty("v")]
        public long Volume { get; set; }
        [JsonProperty("vw")]
        public double Vwap { get; set; }
    }
}
