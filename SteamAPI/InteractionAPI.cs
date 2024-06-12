using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;

namespace SteamAPI
{
    public class InteractionAPI
    {
        public class MyDataModel
        {
            public bool success { get; set; }
            public string lowest_price { get; set; }
            public string volume { get; set; }
            public string median_price { get; set; }
        }

        public static void Main()
        {
            string url = "https://steamcommunity.com/market/priceoverview/?currency=5&appid=730&market_hash_name=Dual%20Berettas%20%7C%20Contractor%20%28Battle-Scarred%29";
            var parsed = JsonConvert.DeserializeObject<MyDataModel>(new WebClient().DownloadString(url));
            Console.WriteLine(parsed.median_price);
        }

    }
}

