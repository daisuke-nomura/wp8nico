using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WP8Nico.nomula
{

    public class SearchWord//検索単語保存クラス
    {
        const byte count = 20;//規定数

        public string Word { get; set; }
        public DateTime Date { get; set; }
        public ushort Count { get; set; }

        public static IEnumerable<string> ReadData()
        {
            IEnumerable<string> result = null;
            string data = LocalSetting.ReadData(LocalSetting.Keys.SEARCHWORD);

            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    JArray parsed = JArray.Parse(data);

                    result = from item in parsed
                             select JObject.Parse(item.ToString())["id"].ToString();
                }
                catch (Exception)
                {
                }
            }

            return result != null && result.Any() ? result.Reverse() : null;
        }

        public static void AddData(string word)
        {
            List<string> searched = new List<string>();
            string data = null;

            var items = SearchWord.ReadData();

            if (items != null)
                searched.AddRange(items);

            if (searched.Contains(word))
            {
                searched.Remove(word);
            }

            searched.Add(word);

            if (searched.Count >= count)
            {
                //規定数を超えていたら、先頭の1件を削除
                searched.RemoveAt(0);
            }

            //JSON化
            JArray array = new JArray();

            for (int i = 0, c = searched.Count; i < c; i++)
            {
                var json = new JObject();
                json.Add("id", JToken.FromObject(searched[i]));

                array.Add(json);
            }

            data = array.ToString();

            LocalSetting.SaveData(LocalSetting.Keys.SEARCHWORD, data);
        }

        public static void RemoveData()
        {
            LocalSetting.RemoveData(LocalSetting.Keys.SEARCHWORD);
        }
    }
}
