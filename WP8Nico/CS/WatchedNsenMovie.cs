
//namespace WP8Nico.nomula
//{
//    class WatchedNsenMovie//Nsenの視聴履歴保存クラス
//    {
//        const ushort count = 30;//規定数

//        public string ID { get; set; }
//        public DateTime Date { get; set; }

//        public static string[] ReadData()
//        {
//            string[] movies = null;
//            string data = LocalSetting.ReadData(LocalSetting.Keys.NSENWATCHED);

//            JsonArray parsed = JsonArray.Parse(data);

//            if (parsed.Count > 0)
//            {
//                movies = new string[parsed.Count];

//                //配列化
//                for (int i = 0, c = parsed.Count; i < c; i++)
//                {
//                    movies[i] = parsed.GetStringAt((uint)i);
//                }
//            }
//            else
//            {
//                movies = new string[0];
//            }

//            return movies;
//        }

//        public static void AddData(string id)
//        {
//            List<string> watched = new List<string>();
//            string data = string.Empty;

//            watched.AddRange(WatchedNsenMovie.ReadData());
//            watched.Add(id);

//            if (watched.Count >= count)
//            {
//                //規定数を超えていたら、先頭の1件を削除
//                watched.RemoveAt(0);
//            }

//            //JSON化
//            JsonArray array = new JsonArray();

//            for (int i = 0, c = watched.Count; i < c; i++)
//            {
//                array.Add(JsonValue.Parse(watched[i]));
//            }

//            data = array.Stringify();

//            LocalSetting.SaveData(LocalSetting.Keys.NSENWATCHED, data);
//        }

//        public static void RemoveData()
//        {
//            LocalSetting.RemoveData(LocalSetting.Keys.NSENWATCHED);
//        }
//    }
//}
