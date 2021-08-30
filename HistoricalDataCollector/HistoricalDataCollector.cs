using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IBApi;
using System.Net.Http;
using System.Net.Http.Headers;
using DoughBot;
using Newtonsoft.Json;
using HistoricalDataCollector.PolygonEntities;

namespace HistoricalDataCollector
{
    public class HistoricalDataCollector
    {
        private string symbol;
        public EClientSocket IbClient;
        public List<Bar> HistoricalBars = new List<Bar>();
        public List<Tick> HistoricalTicks = new List<Tick>();
        public bool DataCollectionCompleted = false;
        public long CurrentTime;
        public static readonly string RootFilePath = @"C:\Users\steve\source\repos\DoughBot\data\";

        public HistoricalDataCollector()
        {

        }
        public HistoricalDataCollector(string symbol)
        {
            //IBWrapperDC wrapper = new IBWrapperDC(this);
            //EReaderSignal readerSignal = wrapper.Signal;
            //IbClient = wrapper.ClientSocket;

            //// Connect to TWS
            //IbClient.eConnect("127.0.0.1", 7497, 1);

            //// Create reader to consume messages from the TWS
            //EReader reader = new EReader(IbClient, readerSignal);
            //reader.Start();

            //// New thread is started to fetch messages coming into queue
            //new Thread(() => { while (IbClient.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            //// Wait until nextOrderId is populated/the client is ready
            //while (wrapper.NextOrderId <= 0) { Thread.Sleep(1000); }

            this.symbol = symbol;
        }

        public void Run(string startDate, string endDate)
        {
            //// Bars
            //GetHistoricalBars(barSize);
            //DownloadHistoricalBars();

            //// Tick
            //GetTickbyTickPolygon();
            //DownloadHistoricalTicks();
            var startDateTime = Convert.ToDateTime(startDate);
            var endDateTime = Convert.ToDateTime(endDate);
            GetHistoricalDataPolygon(startDateTime, endDateTime);
            Console.WriteLine("Completed");
        }
        public void GetHistoricalBars(string barSize)
        {
            Contract contract = ContractGenerator.GenerateStockContract(symbol);
            IbClient.reqHistoricalData(1000, contract, "", "1 W", barSize, "TRADES", 0, 2, false, null);
            while (!DataCollectionCompleted)
            {
                Thread.Sleep(100);
            }
        }
        public void DownloadHistoricalBars()
        {
            var prevDate = convertUtcTimeStampToEst(Convert.ToInt64(HistoricalBars[0].Time)).ToString("yyyy-MM-dd");
            List<Bar> bars = new List<Bar>();
            string date = "";

            foreach (var bar in HistoricalBars)
            {
                date = convertUtcTimeStampToEst(Convert.ToInt64(bar.Time)).ToString("yyyy-MM-dd");
                if (date == prevDate)
                {
                    bars.Add(bar);
                }
                else
                {
                    JsonHandler.WriteToJsonFile<List<Bar>>(RootFilePath + String.Format(@"historicalBars\{0}.txt", prevDate), bars);
                    prevDate = date;
                    bars = new List<Bar>();
                }
            }
            JsonHandler.WriteToJsonFile<List<Bar>>(RootFilePath + String.Format(@"historicalBars\{0}.txt", date), bars);
        }
        public void GetTickbyTick()
        {
            Contract contract = ContractGenerator.GenerateStockContract(symbol);
            CurrentTime = Convert.ToInt64(HistoricalBars[0].Time);
            long startDate = CurrentTime;
            long endDate = Convert.ToInt64(HistoricalBars[HistoricalBars.Count - 1].Time);
            
            DataCollectionCompleted = true;

            while (startDate < endDate)
            {
                while (!DataCollectionCompleted)
                {
                    Thread.Sleep(500);
                    Console.WriteLine(convertUtcTimeStampToEst(CurrentTime));
                }
                startDate = CurrentTime + 1;
                var startDateTime = convertUtcTimeStampToEst(startDate);
                IbClient.reqHistoricalTicks(1000, contract, startDateTime.ToString("yyyyMMdd HH:mm:ss"), "", 1000, "TRADES", 1, true, null);
                DataCollectionCompleted = false;
            }
        }

        public void GetHistoricalDataPolygon(DateTime startDateTime, DateTime endDateTime)
        {
            var currentDateTime = startDateTime;
            var listOfDates = new List<DateTime>();
            while (currentDateTime  <= endDateTime)
            {
                listOfDates.Add(currentDateTime);
                currentDateTime = currentDateTime.AddDays(1);
            }

            Parallel.ForEach(listOfDates, new ParallelOptions { MaxDegreeOfParallelism = 8 }, curr =>
            {
                var trades = GetTrades(curr);
                var bars = GetBars(curr);

                if (trades.Count > 0)
                {
                    JsonHandler.WriteToJsonFile(RootFilePath + @$"{symbol}/historicalTicks/{curr.ToString("yyyy-MM-dd")}.txt", trades);
                }
                if (bars.Count > 0)
                {
                    JsonHandler.WriteToJsonFile(RootFilePath + @$"{symbol}/historicalBars2mins/{curr.ToString("yyyy-MM-dd")}.txt", bars);
                }
                Console.WriteLine($"Completed fetching data for {curr.ToString("yyyy-MM-dd")}");
            });
        }

        private List<Bar> GetBars(DateTime dateTime)
        {
            var allBars = new List<Bar>();
            var baseUrl = "https://api.polygon.io/v2/aggs/ticker/";
            var urlParams = $"{symbol.ToUpper()}/range/2/minute/{dateTime.ToString("yyyy-MM-dd")}/{dateTime.ToString("yyyy-MM-dd")}?adjusted=false&sort=asc&limit=50000&apiKey={EnvironmentVariables.PolygonApiKey}";

            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(urlParams).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<BarsEntity>(responseString);
            if (result.Results != null)
            {
                foreach (var bar in result.Results)
                {
                    allBars.Add(new Bar((bar.Time / 1000).ToString(), bar.Open, bar.High, bar.Low, bar.Close, bar.Volume, 0, bar.Vwap));
                }
            }
            return allBars;
        }

        private List<Tick> GetTrades(DateTime dateTime)
        {
            var allTicks = new List<Tick>();
            var baseUrl = "https://api.polygon.io/v2/ticks/stocks/trades/";
            var urlParams = $"{symbol.ToUpper()}/{dateTime.ToString("yyyy-MM-dd")}?reverse=false&limit=50000&apiKey={EnvironmentVariables.PolygonApiKey}";

            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(urlParams).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<TradesEntity>(responseString);
            var currentDateTime = dateTime;
            long currentTimestamp = 0;
            var marketEnd = dateTime.Date + new TimeSpan(16, 0, 0);

            while (result != null && result.Results != null)
            {
                if (currentDateTime > marketEnd)
                {
                    break;
                }
                foreach (var trade in result.Results)
                {
                    var time = trade.NanoParticipantTimestamp / 1000000000;
                    allTicks.Add(new Tick(time, trade.Price, symbol));
                    currentDateTime = convertUtcTimeStampToEst(time);
                    currentTimestamp = trade.NanoParticipantTimestamp;
                }
                urlParams = $"{symbol.ToUpper()}/{dateTime.ToString("yyyy-MM-dd")}?timestamp={currentTimestamp}&reverse=false&limit=50000&apiKey={EnvironmentVariables.PolygonApiKey}";
                response = client.GetAsync(urlParams).Result;
                responseString = response.Content.ReadAsStringAsync().Result;
                result = JsonConvert.DeserializeObject<TradesEntity>(responseString);
            }

            return allTicks;
        }

        public void DownloadHistoricalTicks()
        {
            JsonHandler.WriteToJsonFile<List<Tick>>(RootFilePath + ".txt", HistoricalTicks);

            var prevDate = convertUtcTimeStampToEst(Convert.ToInt64(HistoricalTicks[0].Time)).ToString("yyyy-MM-dd");
            List<Tick> ticks = new List<Tick>();
            string date = "";

            foreach (var tick in HistoricalTicks)
            {
                date = convertUtcTimeStampToEst(Convert.ToInt64(tick.Time)).ToString("yyyy-MM-dd");
                if (date == prevDate)
                {
                    ticks.Add(tick);
                }
                else
                {
                    JsonHandler.WriteToJsonFile<List<Tick>>(RootFilePath + String.Format(@"historicalBars\{0}.txt", prevDate), ticks);
                    prevDate = date;
                    ticks = new List<Tick>();
                }
            }
            JsonHandler.WriteToJsonFile<List<Tick>>(RootFilePath + String.Format(@"historicalBars\{0}.txt", date), ticks);
        }

        private DateTime convertUtcTimeStampToEst(double unixTimeStamp)
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime utcDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            utcDateTime = utcDateTime.AddSeconds(unixTimeStamp);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, easternZone);
        }
    }
}