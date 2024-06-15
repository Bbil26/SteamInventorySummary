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
    public class ItemMarket
    {
        public double id { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public int count { get; set; }

        public ItemMarket(double id)
        {
            this.id = id;
        }

        public ItemMarket(string name)
        {
            this.name = name;
        }

        public void AddCount()
        {
            this.count++;
        }
    }

    public class InteractionAPI
    {
        public static void Main()
        {

            List<ItemMarket> listItems = GetClassid();      // Создание Листа классов и заполнение его ID'шниками

            listItems = GetListNameItem(listItems);

            foreach (ItemMarket item in listItems)
            {
                Console.WriteLine("id: " + item.id + " | name: " + item.name);
                GetPrice(market_hash_name: item.name);
            }

        }

        // Получаю ID всех предметов в инвенторе стим за один запрос
        private static List<ItemMarket> GetClassid(string steam_id = "76561198088144226", string appid = "730")
        {
            string url = "https://steamcommunity.com/inventory/" + steam_id + "/" + appid + "/2";       //Собираем url
            
            var parsed = JToken.Parse(new WebClient().DownloadString(url))  //Парсим json
            .SelectTokens("assets[*].classid")  
            .Select(token => token.ToString())  //Все элементы конвертируем в string (До этого был JToken) 
            .ToList();

            List<ItemMarket> listI = new List<ItemMarket>();            //Создаем вспомогательный лист
            
            foreach (var item in parsed)                         //Проходимся по элементам листа
            {
                double id = double.Parse(item.ToString());
                if (!listI.Any(i => i.id == id)) {                      //Если записи с id нет - добавляем новую
                    listI.Add(new ItemMarket(id) { count = 1 });
                }
                else
                {
                    listI.Find(x => x.id == id).AddCount();             //Если она есть, то увеличиваем количество
                }
            }
            return listI;
        }

        //Получение цены *?*
        private static void GetPrice(string currency = "5", string appid = "730", string market_hash_name = "Dual Berettas | Contractor (Battle-Scarred)")
        {
            //Преобразование названия предмета к ГОСТу :)
            market_hash_name = market_hash_name.Replace(" ", "%20").Replace("|", "%7C").Replace("(", "%28").Replace(")", "%29");
            string url = "https://steamcommunity.com/market/priceoverview/?currency=" + currency + "&appid=" + appid + "&market_hash_name=" + market_hash_name;

            try
            {
                //var parsed = JsonConvert.DeserializeObject<SkinInfoMarket>(new WebClient().DownloadString(url));

                var parsed = JToken.Parse(new WebClient().DownloadString(url))  //Парсим json
                .SelectToken("lowest_price")
                .ToString();

                Console.WriteLine(parsed);
            }
            
            catch
            {
                Console.WriteLine("-");
            }
        }

        // Получаю все Hash-name'ы по ID за один запрос
        private static List<ItemMarket> GetListNameItem(List<ItemMarket> listI, string key = "D25806D7EFF96E1AD56CCE7F3E8E6A5A", string appid = "730")
        {
            int n = 0;                                                  // Переменная для модификатора 250 * n 
            List<string> data = new List<string>();                     // Лист для парсинга
            List<ItemMarket> partListI = new List<ItemMarket>();        // Лист для разбиения основного на части
            bool flag = true;
            while (flag)                                //Обрабатываем json по частям
            {
                if ((n + 1) * 250 > listI.Count)        //Логика разбиения основного листа
                {
                    partListI = listI.GetRange(n * 250, listI.Count - n * 250);
                    flag = false;
                }
                else
                    partListI = listI.GetRange(n * 250, 250);

                string url = "https://api.steampowered.com/ISteamEconomy/GetAssetClassInfo/v1/?key=" + key + "&appid=" + appid + "&class_count=" + partListI.Count;

                int count = 0;                          //Формируем url порционно по 250 параметров
                foreach (var item in partListI) 
                {
                    url += $"&classid{count}={item.id}";
                    count++;
                }

                data = JToken.Parse(new WebClient().DownloadString(url))   //составляем лист имен
                .SelectTokens("result.*.market_hash_name")
                .Select(token => token.ToString())  //Все элементы конвертируем в string (До этого был JToken) 
                .ToList();  

                int i = 0;
                foreach (var item in data)
                {
                    listI[n * 250 + i].name = item;
                    i++;
                }

                n++;
            }
            return listI;
        }
    }
}
