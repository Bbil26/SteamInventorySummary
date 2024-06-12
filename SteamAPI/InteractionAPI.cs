using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Security.Policy;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Diagnostics;

namespace SteamAPI
{
    public class InteractionAPI
    {

        //Инфа о инвенторе стим
        public class Account
        {
            public Asset[] assets { get; set; }
        }

        public class Asset
        {
            public string classid { get; set; }

        }


        //Инфа о стоимости скина
        public class SkinInfoMarket
        {
            public bool success { get; set; }
            public string lowest_price { get; set; }
            public string volume { get; set; }
            public string median_price { get; set; }
        }

        public static void Main()
        {

            //GetNameItem();
            //GetClassID();
            //GetPrice()
            //GetPrice(market_hash_name: GetNameItem(classid0: GetClassID()));
            int counter = 0;

            List<string> listId = GetClassID();
            List<string> listName = new List<string>();

            float num = GetClassID().Count();
            foreach (var item in listId)
            {
                counter++;
                Console.WriteLine(Math.Round(counter / num * 100) + "%");
                GetPrice(market_hash_name: GetNameItem(classid0: item));
                listName.Add(GetNameItem(classid0: item));
                Console.WriteLine(listName[counter - 1]);
            }
        }

        // classid0=111111&classid1=4901046679
        private static List<string> GetClassID(string steam_id = "76561198088144226", string appid = "730")
        {
            string url = "https://steamcommunity.com/inventory/" + steam_id + "/" + appid + "/2";
            var parsed = JsonConvert.DeserializeObject<Account>(new WebClient().DownloadString(url));
            
            List<string> listId = new List<string>();
            foreach(var item in parsed.assets)
            {
                listId.Add(item.classid);
            }
            return listId;
        }

        private static void GetPrice(string currency = "5", string appid = "730", string market_hash_name = "Dual Berettas | Contractor (Battle-Scarred)")
        {
            //Преобразование названия предмета к ГОСТу :)
            market_hash_name = market_hash_name.Replace(" ", "%20").Replace("|", "%7C").Replace("(", "%28").Replace(")", "%29");
            string url = "https://steamcommunity.com/market/priceoverview/?currency=" + currency + "&appid=" + appid + "&market_hash_name=" + market_hash_name;

            try
            {
                var parsed = JsonConvert.DeserializeObject<SkinInfoMarket>(new WebClient().DownloadString(url));
                Console.WriteLine(parsed.lowest_price);
            }
            
            catch
            {
                Console.WriteLine("-");
            }
        }

        private static string GetNameItem(string key = "D25806D7EFF96E1AD56CCE7F3E8E6A5A", string appid = "730", string classid0 = "310777038")
        {
            string url = "https://api.steampowered.com/ISteamEconomy/GetAssetClassInfo/v1/?key=" + key + "&appid=" + appid + "&class_count=3&classid0=" + classid0;
            var data = JObject.Parse(new WebClient().DownloadString(url))
            .SelectTokens("result.*.market_hash_name")
            .ToArray();
            return data[0].ToString();
        }
    }
}
