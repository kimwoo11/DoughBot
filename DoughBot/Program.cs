using System;
using System.IO;
using QuantConnect.IBAutomater;
using System.Collections.Generic;
using DoughBot.Strategies;
using Telegram.Bot;
using System.Threading.Tasks;
namespace DoughBot
{
    class Program
    {
        private static int ibPort;
        private static string ibUserName;
        private static string ibPassword;
        private static string ibTradingMode;
        private static string ibVersion;
        public static IBAutomater automater;
        private static Settings settings;

        static void Main(string[] args)
        {
            var workingDirectory = Environment.CurrentDirectory;
            settings = JsonHandler.ReadFromJsonFile<Settings>(workingDirectory + @"\settings.json");
            ibUserName = settings.IbUserName;
            ibPassword = settings.IbPassword;
            ibTradingMode = settings.IbTradingMode;
            ibPort = settings.IbPort;
            ibVersion = settings.IbVersion;

            Dictionary<string, Strategy> watchDictionary = new Dictionary<string, Strategy>
            {
                { "TSLA", new EmaStrictBreakout(9, 21, 50, 0.0015, 0.0015, 0.0015) }
            };

            RunBacktesting();
            //RunLiveTrading(watchDictionary, true);
            Console.ReadLine();
        }

        private static void RunLiveTrading(Dictionary<string, Strategy> watchDictionary, bool sendText)
        {
            StartIbGateway();
            var bot = new Bot(1, "127.0.0.1", ibPort, watchDictionary, settings);
            bot.Run();

            while (true)
            {
                var shutdown = Console.ReadLine();
                if (shutdown == "shutdown")
                { 
                    bot.Disconnect();
                    Console.WriteLine("Disconnecting...");
                    break;
                }
            }
            Console.ReadLine();
        }
        private static void RunBacktesting()
        {
            var rrs = new List<double> { 0.00075, 0.001, 0.0015, 0.002, 0.0025, 0.003, 0.0035};

            foreach (var rr in rrs)
            {
                Console.WriteLine($"Starting backtesting for RR: {rr}");
                var backtestEngine = new BacktestEngine(rr, $"{rr}_backtestResults", "2 mins");
                backtestEngine.Run("AMZN");
            }
            Console.ReadLine();
        }

        private static void StartIbGateway()
        {
            // IBAutomater settings
            var ibDirectory = IsLinux ? "~/Jts" : "C:\\Jts";

            // Create a new instance of the IBAutomater class
            automater = new IBAutomater(ibDirectory, ibVersion, ibUserName, ibPassword, ibTradingMode, ibPort, false);

            // Attach the event handlers
            //automater.OutputDataReceived += (s, e) => Console.WriteLine($"{DateTime.UtcNow:O} {e.Data}");
            //automater.ErrorDataReceived += (s, e) => Console.WriteLine($"{DateTime.UtcNow:O} {e.Data}");
            automater.Exited += (s, e) => Console.WriteLine($"{DateTime.UtcNow:O} IBAutomater exited [ExitCode:{e.ExitCode}]");

            // Start the IBAutomater
            var result = automater.Start(false);
            if (result.HasError)
            {
                Console.WriteLine($"Failed to start IBAutomater - Code: {result.ErrorCode}, Message: {result.ErrorMessage}");
                automater.Stop();
                return;
            }

            
            // Stop the IBAutomater
            //automater.Stop();
            //Console.WriteLine("IBAutomater stopped");
        }
        private static bool IsLinux
        {
            get
            {
                var p = (int)Environment.OSVersion.Platform;
                return p == 4 || p == 6 || p == 128;
            }
        }
    }
}
