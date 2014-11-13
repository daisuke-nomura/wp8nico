using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WP8Nico.nomula
{

    public class WatchedMovie//視聴履歴保存クラス　Historyだと歴史カテゴリと被るため
    {
        const byte count = 30;//規定数

        public string ID { get; set; }
        //public uint Position { get; set; }
        public DateTime LastWatched { get; set; }
        public ushort Count { get; set; }
        
        public async static Task<IEnumerable<RankingResults>> ReadItemsAsync()
        {
            const string arrayUrl = "http://i.nicovideo.jp/v3/video.array?v=";
            IEnumerable<RankingResults> items = null;
            StringBuilder sb = null;

            var result = ReadData();

            if (result != null && result.Any())
            {
                sb = new StringBuilder();
                sb.Append(arrayUrl);

                foreach (var obj in result)
                    sb.AppendFormat("{0},", obj.ID);

                sb.Append("&api_version=1%2E20");

                items = await RankingResults.ReadItemsAsync(sb.ToString(), false);

                sb = null;
            }

            return items != null ? items.Reverse() : items;
        }

        public static IEnumerable<WatchedMovie> ReadData()
        {
            IEnumerable<WatchedMovie> result = null;
            string data = LocalSetting.ReadData(LocalSetting.Keys.WATCHED);

            if (!string.IsNullOrEmpty(data))
            {
                JArray parsed = JArray.Parse(data);

                result = from item in parsed
                         select new WatchedMovie()
                         {
                             ID = item["id"].ToString(),
                             LastWatched = DateTime.Parse(item["lastwatch"].ToString()),
                             Count = item["count"].ToObject<ushort>(),
                         };

                parsed = null;
                data = null;
            }

            return result;
        }

        public static void AddData(string id)
        {
            List<WatchedMovie> watched = new List<WatchedMovie>();
            string data = null;
            ushort watchedCount = 1;

            var items = WatchedMovie.ReadData();

            if (items != null)
                watched.AddRange(items);

            for (int i = 0; i < watched.Count; i++)
            {
                if (watched[i].ID.Contains(id))
                {
                    watchedCount = watched[i].Count;
                    watchedCount++;
                    watched.RemoveAt(i);
                    break;
                }
            }

            watched.Add(new WatchedMovie() { ID = id, Count = watchedCount, LastWatched = DateTime.Now });

            if (watched.Count >= count)
            {
                //規定数を超えていたら、先頭の1件を削除
                watched.RemoveAt(0);
            }

            //JSON化
            JArray array = new JArray();

            for (int i = 0, c = watched.Count; i < c; i++)
            {
                var json = new JObject();
                json.Add("id", JToken.FromObject(watched[i].ID));
                json.Add("lastwatch", JToken.FromObject(watched[i].LastWatched.ToString()));
                json.Add("count", JToken.FromObject(watched[i].Count));

                array.Add(json);
            }

            data = array.ToString();

            LocalSetting.SaveData(LocalSetting.Keys.WATCHED, data);
        }

        public static void RemoveItems()
        {
            RemoveData();
        }

        private static void RemoveData()
        {
            LocalSetting.RemoveData(LocalSetting.Keys.WATCHED);
        }
    }
}
