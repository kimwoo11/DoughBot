using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using IBApi;
using DoughBot.Strategies;
using System.IO;
using CsvHelper;
using System.Globalization;
using Telegram.Bot;

namespace DoughBot
{
    public class Bot
    {
        public readonly int TickRequestIdBase = 2000;
        public readonly int StockRequestIdBase = 3000;
        public readonly int OptionRequestIdBase = 4000;
        public bool IsBotRunning = false;

        public IBWrapper IbWrapper;
        private EClientSocket ibClient;
        private Dictionary<string, Strategy> watchDictionary;
        private DateTime currentDateTime;
        private bool isBacktest = false;
        private bool isSendText = false;

        public int Id;
        public string Host;
        public int Port;
        public Dictionary<string, SecurityData> DataDictionary;
        public Dictionary<int, string> IdSymbolDictionary;
        public List<OrderEntry> OrderEntries = new List<OrderEntry>();

        public Bot(int id, string host, int port, Dictionary<string, Strategy> watchDictionary, bool isSendText)
        {
            Id = id;
            Host = host;
            Port = port;
            this.watchDictionary = watchDictionary;
            this.isSendText = isSendText;
        }

        public void Run()
        {
            InitializeTwsConnection();
            SendText("Turning on bot!");
            InitializeDataDictionary();
            RequestDataAndRunLive();
        }

        public void Run(bool isBacktest)
        {
            this.isBacktest = isBacktest;

            if (isBacktest)
            {
                this.IbWrapper = new IBWrapper(this);
                InitializeDataDictionary();
            }
            else
            {
                InitializeTwsConnection();
                InitializeDataDictionary();
                RequestDataAndRunLive();
            }
        }

        public void OnTickUpdate(int reqId, long time, double tickPrice)
        {
            if (!IsBotRunning)
            {
                return;
            }

            currentDateTime = ConvertUtcTimeStampToEst(time);
            var security = DataDictionary[IdSymbolDictionary[reqId - TickRequestIdBase]];

            security.Add(time, tickPrice);

            //Console.WriteLine($"Symbol: {security.Name}, Time: {currentDateTime}, Tick Price: {tickPrice}");
            var strategy = security.Strategy;
            var signal = strategy.GetSignal(security);

            if (ZoneAnalysis.GetZone(time) == MarketZones.TradingOpen)
            {

                if (signal != SignalType.None)
                {
                    if (signal == SignalType.BuyCall || signal == SignalType.BuyPut)
                    {
                        Buy(time, tickPrice, security, signal);
                        security.CurrentSignal = signal;
                    }
                    else
                    {
                        Sell(time, tickPrice, security);
                        security.CurrentSignal = SignalType.None;
                    }
                }
            }
            else if (ZoneAnalysis.GetZone(time) == MarketZones.TradingClose)
            {
                if (security.CurrentSignal == SignalType.BuyCall || security.CurrentSignal == SignalType.BuyPut)
                {
                    if (signal == SignalType.SellCall || signal == SignalType.SellPut)
                    {
                        Sell(time, tickPrice, security);
                        security.CurrentSignal = SignalType.None;
                        this.Disconnect(); // only disconnect if we sell
                    }
                }
            }
            else if (ZoneAnalysis.GetZone(time) == MarketZones.MarketClose)
            {
                if (security.CurrentSignal == SignalType.BuyCall || security.CurrentSignal == SignalType.BuyPut)
                {
                    Sell(time, tickPrice, security);
                    security.CurrentSignal = SignalType.None;
                }
                this.Disconnect(); // disconnect if no positions held 
            }
        }

        public void OnBarUpdate(int reqId, Bar bar)
        {
            if (!IsBotRunning)
            {
                return;
            }

            var security = DataDictionary[IdSymbolDictionary[reqId - StockRequestIdBase]];
            security.Add(bar);
        }

        private void Buy(long time, double tickPrice, SecurityData sd, SignalType signal)
        {
            sd.OptionContract.Strike = sd.StrikePrices.Count > 0 ? sd.GetStrikePrice(tickPrice, signal) : 0;
            sd.OptionContract.Right = signal == SignalType.BuyCall ? "C" : "P";

            var order = new Order();
            order.Action = "BUY";
            order.TotalQuantity = 2;
            order.OrderType = "MKT";
            if (!isBacktest)
            {
                int waitCount = 0;
                ibClient.placeOrder(IbWrapper.NextOrderId, sd.OptionContract, order);
                IbWrapper.OrderStatus.Add(IbWrapper.NextOrderId, 0.0);
                while (IbWrapper.OrderStatus[IbWrapper.NextOrderId] > 0)
                {
                    Thread.Sleep(500);
                    waitCount++;

                    if (waitCount >= 15)
                    {
                        SendText("Order wait count exceeded. Disconnecting bot. Please check to see if any positions have been bought.");
                        Disconnect();
                        break;
                    }
                }
                var currentDateTime = ConvertUtcTimeStampToEst(time);
                SendText($"{currentDateTime} BOUGHT {sd.Name} {sd.OptionContract.Strike} {sd.OptionContract.Right} @ {tickPrice}; Take Profit = {sd.CurrentTakeProfit}");
            }
            OrderEntries.Add(new OrderEntry(time, sd.PurchasePrice, sd.PurchaseAtr, "BUY", sd.OptionContract.Right));
            IbWrapper.NextOrderId++;
        }

        public void Sell(long time, double tickPrice, SecurityData sd)
        {
            var order = new Order();
            order.Action = "SELL";
            order.TotalQuantity = 2;
            order.OrderType = "MKT";
            if (!isBacktest)
            {
                int waitCount = 0;
                ibClient.placeOrder(IbWrapper.NextOrderId, sd.OptionContract, order);
                IbWrapper.OrderStatus.Add(IbWrapper.NextOrderId, 0.0);
                while (IbWrapper.OrderStatus[IbWrapper.NextOrderId] > 0)
                {
                    Thread.Sleep(500);
                    waitCount++;

                    if (waitCount >= 15)
                    {
                        SendText("Order wait count exceeded. Disconnecting bot. Please check to see if any positions have been bought.");
                        Disconnect();
                        break;
                    }
                }
                var currentDateTime = ConvertUtcTimeStampToEst(time);
                SendText($"{currentDateTime} SOLD {sd.Name} {sd.OptionContract.Strike} {sd.OptionContract.Right} @ {tickPrice}");
            }
            OrderEntries.Add(new OrderEntry(time, tickPrice, sd.PurchaseAtr, "SELL", sd.OptionContract.Right));
            IbWrapper.NextOrderId++;
        }

        private void InitializeTwsConnection()
        {
            this.IbWrapper = new IBWrapper(this);
            EReaderSignal readerSignal = IbWrapper.Signal;
            this.ibClient = IbWrapper.ClientSocket;

            // Connect to TWS
            this.ibClient.eConnect(Host, Port, Id);

            // Create reader to consume messages from the TWS
            EReader reader = new EReader(this.ibClient, readerSignal);
            reader.Start();

            // New thread is started to fetch messages coming into queue
            new Thread(() => { while (this.ibClient.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            // Wait until nextOrderId is populated/the client is ready
            while (IbWrapper.NextOrderId <= 0) { Thread.Sleep(1000); }
        }

        public void InitializeDataDictionary()
        {
            IdSymbolDictionary = new Dictionary<int, string>();
            DataDictionary = new Dictionary<string, SecurityData>();

            int id = 0;
            foreach (var item in watchDictionary)
            {
                var symbol = item.Key;
                var strategy = item.Value;

                IdSymbolDictionary.Add(id, symbol);
                DataDictionary.Add(symbol, new SecurityData(id, symbol, strategy));
                id++;
            }
        }

        private void RequestDataAndRunLive()
        {
            // Request and sort option strike prices
            foreach (var item in DataDictionary)
            {
                SecurityData security = item.Value;
                int reqId = security.Id + OptionRequestIdBase;
                IbWrapper.ContractDetailEnd.Add(reqId, false);
                ibClient.reqContractDetails(reqId, security.OptionContract);
                while (!IbWrapper.ContractDetailEnd[reqId])
                {
                    Thread.Sleep(10);
                }
                security.StrikePrices.Sort();
            }

            this.IsBotRunning = true;

            // Request historical & realtime data
            foreach (var item in DataDictionary)
            {
                Contract contract = item.Value.StockContract;
                var id = item.Value.Id;
                ibClient.reqHistoricalData(id + StockRequestIdBase, contract, "", "1 D", "2 mins", "TRADES", 0, 2, true, null);
                ibClient.reqTickByTickData(id + TickRequestIdBase, contract, "Last", 0, false);
                Thread.Sleep(1000);
            }
        }
        
        public async void SendText(string msg)
        {
            Console.WriteLine(msg);
            var token = "1837108062:AAEZmsQABhAx9tN7TtbWF8kneC4Cbt7qwMY";
            var botClient = new TelegramBotClient(token);
            await botClient.SendTextMessageAsync(
                chatId: -1001511668831,
                text: msg
                );
        }

        public void Disconnect()
        {
            if (!isBacktest)
            {
                foreach (var item in DataDictionary)
                {
                    var id = item.Value.Id;
                    ibClient.cancelHistoricalData(id + StockRequestIdBase);
                    ibClient.cancelTickByTickData(id + TickRequestIdBase);
                }
            }
            this.IsBotRunning = false;
            SendText("Bot disconnected.");
            Thread.Sleep(5000);
            if (!isBacktest)
            {
                Program.automater.Stop();
                Environment.Exit(0);
            }
        }

        public static DateTime ConvertUtcTimeStampToEst(long unixTimeStamp)
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime utcDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            utcDateTime = utcDateTime.AddSeconds(unixTimeStamp);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, easternZone);
        }
    }
}
