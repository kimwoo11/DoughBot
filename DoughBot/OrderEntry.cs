using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoughBot
{
    public class OrderEntry
    {
        public string DateTime { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public double Price { get; set; }
        public string Action { get; set; }
        public string Right
        {
            get; set;
        }
        public double Atr { get; set; }
        public OrderEntry(long time, double price, double atr, string action, string right)
        {
            var datetime = convertUtcTimeStampToEst(time);
            DateTime = datetime;
            Date = datetime.Split(' ')[0];
            Time = datetime.Split(' ')[1];
            Price = price;
            Action = action;
            Right = right;
            Atr = atr;
        }
        private string convertUtcTimeStampToEst(double unixTimeStamp)
        {
            DateTime utcDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            utcDateTime = utcDateTime.AddSeconds(unixTimeStamp);
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, easternZone).ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
