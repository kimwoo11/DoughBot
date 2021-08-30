using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HistoricalDataCollector.PolygonEntities
{
    class TradesEntity
    {
        [JsonProperty("db_latency")]
        public int DbLatency { get; set; }
        [JsonProperty("results_count")]
        public int ResultsCount { get; set; }
        [JsonProperty("success")]
        public bool Sucesss { get; set; }
        [JsonProperty("ticker")]
        public string Ticker { get; set; }
        [JsonProperty("results")]
        public List<TradesResult> Results { get; set; }
    }

    class TradesResult
    {
        [JsonProperty("T")]
        public string ExchangeSymbol { get; set; }
        [JsonProperty("f")]
        public long TRFTimestamp { get; set; }
        [JsonProperty("q")]
        public int SequenceNumber { get; set; }
        [JsonProperty("t")]
        public long NanoTimestamp { get; set; }
        [JsonProperty("y")]
        public long NanoParticipantTimestamp { get; set; }
        [JsonProperty("c")]
        public int[] ConditionCodes { get; set; }
        [JsonProperty("e")]
        public int CorrectionIndicator { get; set; }
        [JsonProperty("i")]
        public int TradeId { get; set; }
        [JsonProperty("p")]
        public double Price { get; set; }
        [JsonProperty("r")]
        public int TradeReportingFacilityId { get; set; }
        [JsonProperty("s")]
        public int Size { get; set; }
        [JsonProperty("x")]
        public int ExchangeId { get; set; }
        [JsonProperty("z")]
        public int Tape { get; set; }


    }
}
