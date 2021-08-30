using System;
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
        public static IBAutomater automater;
        
        static void Main(string[] args)
        {
            ibUserName = args[0];
            ibPassword = args[1];
            ibTradingMode = args[2];

            Dictionary<string, Strategy> watchDictionary = new Dictionary<string, Strategy>
            {
                { "TSLA", new EmaStrictBreakout(9, 21, 50, 0.0015, 0.0015, 0.0015) }
            };

            //RunBacktesting();
            RunLiveTrading(watchDictionary);
        }

        private static void RunLiveTrading(Dictionary<string, Strategy> watchDictionary)
        {
            StartIbGateway();
            var bot = new Bot(1, "127.0.0.1", ibPort, watchDictionary);
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
            var rrs = new List<double> { 0.001, 0.0015 , 0.002, 0.0025, 0.003 };

            foreach (var rr in rrs)
            {
                Console.WriteLine($"Starting backtesting for RR: {rr}");
                var backtestEngine = new BacktestEngine(rr, $"{rr}_backtestResults", "2 mins");
                backtestEngine.Run("AMC");
            }
            Console.ReadLine();
        }

        private static void StartIbGateway()
        {
            switch (ibTradingMode)
            {
                case "paper":
                    ibPort = 4003;
                    break;
                case "live":
                    ibPort = 4001;
                    break;
            }

            // IBAutomater settings
            var ibDirectory = IsLinux ? "~/Jts" : "C:\\Jts";
            var ibVersion = "981";

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
