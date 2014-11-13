using System.IO.IsolatedStorage;

namespace WP8Nico.nomula
{

    public class LocalSetting//データ保存クラス
    {
        static IsolatedStorageSettings localSettings = IsolatedStorageSettings.ApplicationSettings;

        public static string ReadData(Keys key)
        {
            string data = null;

            if (!localSettings.TryGetValue(key.ToString(), out data))
                data = string.Empty;

            return data;
        }

        public static void SaveData(Keys key, string data)
        {
            if (data == null)
                data = string.Empty;

            if (localSettings.Contains(key.ToString()))
                localSettings[key.ToString()] = data;
            else
                localSettings.Add(key.ToString(), data);
        }

        public static void RemoveData(Keys key)
        {
            if (localSettings.Contains(key.ToString()))
                localSettings.Remove(key.ToString());
        }

        public enum Keys
        {
            ID,
            PWD,
            WATCHED,
            QUALITY,
            SHAREWATCHED,
            SUGGEST,
            SEARCHWORD,
            ROAMINGWATCHED,
            NSENWATCHED,
            CATEGORY,
            SEARCHSETTING,
            SEARCHSORTSETTING,
            VIDEOHUB,
            NICOREPO,
            CATEGORYSETTING,
            CATEGORYSPANSETTING,
            NICOCHART,
            VOLUME,
            AUTOPLAY,
            REPEAT,
            IYAYO184,
            LANGUAGE,
        }

        public static string ID
        {
            get
            {
                return LocalSetting.ReadData(Keys.ID);
            }
            set
            {
                LocalSetting.SaveData(Keys.ID, value);
            }
        }

        public static string Password
        {
            get
            {
                return LocalSetting.ReadData(Keys.PWD);
            }
            set
            {
                LocalSetting.SaveData(Keys.PWD, value);
            }
        }

        public static string Quality
        {
            get
            {
                return LocalSetting.ReadData(Keys.QUALITY);
            }
            set
            {
                LocalSetting.SaveData(Keys.QUALITY, value);
            }
        }

        public static bool SuggestSetting
        {
            get
            {
                bool res = false;
                bool.TryParse(LocalSetting.ReadData(LocalSetting.Keys.SUGGEST), out res);

                return res;
            }
            set
            {
                LocalSetting.SaveData(Keys.SUGGEST, value.ToString());
            }
        }

        public static bool RoamingWatchedSetting
        {
            get
            {
                bool res = false;
                bool.TryParse(LocalSetting.ReadData(LocalSetting.Keys.ROAMINGWATCHED), out res);

                return res;
            }
            set
            {
                LocalSetting.SaveData(Keys.ROAMINGWATCHED, value.ToString());
            }
        }

        //public static string CategoryString
        //{
        //    get
        //    {
        //        string data = LocalSetting.ReadData(Keys.CATEGORY);
        //        WP8Nico.nomula.Category.ReadCategory();
        //        return data;
        //    }
        //    set
        //    {
        //        LocalSetting.SaveData(Keys.CATEGORY, value);
        //    }
        //}

        //public static Category[] Category
        //{
        //    get
        //    {
        //        IEnumerable<Category> result = null;
        //        string categories = CategoryString;

        //        if (!string.IsNullOrEmpty(categories))
        //        {
        //            try
        //            {
        //                using (StringReader sr = new StringReader(categories))
        //                {
        //                    XDocument xml = XDocument.Parse(sr.ReadToEndAsync().GetAwaiter().GetResult());

        //                    result = from item in xml.Descendants("genre")
        //                                 where item.Parent.Attribute("status").Value == "ok"
        //                                 select new Category
        //                                 {
        //                                     Key = item.Element("key").Value,
        //                                     Tag = item.Element("tag").Value
        //                                 };

        //                    xml = null;
        //                    sr.Dispose();
        //                }
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }

        //        return result != null ? result.ToArray() : new Category[0];
        //    }
        //    set
        //    {
        //    }
        //}

        public static sbyte SearchSetting
        {
            get
            {
                sbyte s;
                string data = LocalSetting.ReadData(Keys.SEARCHSETTING);

                if (!sbyte.TryParse(data, out s))
                    s = 0;

                return s;
            }
            set
            {
                LocalSetting.SaveData(Keys.SEARCHSETTING, value.ToString());
            }
        }

        public static sbyte SearchSortSetting
        {
            get
            {
                sbyte s;
                string data = LocalSetting.ReadData(Keys.SEARCHSORTSETTING);

                if (!sbyte.TryParse(data, out s))
                    s = 0;

                return s;
            }
            set
            {
                LocalSetting.SaveData(Keys.SEARCHSORTSETTING, value.ToString());
            }
        }

        public static bool VideoHub
        {
            get
            {
                bool res = false;
                bool.TryParse(LocalSetting.ReadData(LocalSetting.Keys.VIDEOHUB), out res);

                return res;
            }
            set
            {
                LocalSetting.SaveData(Keys.VIDEOHUB, value.ToString());
            }
        }

        public static sbyte CategoryTypeSetting
        {
            get
            {
                sbyte s;
                string data = LocalSetting.ReadData(Keys.CATEGORYSETTING);

                if (!sbyte.TryParse(data, out s))
                    s = 0;

                return s;
            }
            set
            {
                LocalSetting.SaveData(Keys.CATEGORYSETTING, value.ToString());
            }
        }

        public static sbyte CategorySpanSetting
        {
            get
            {
                sbyte s;
                string data = LocalSetting.ReadData(Keys.CATEGORYSPANSETTING);

                if (!sbyte.TryParse(data, out s))
                    s = 0;

                return s;
            }
            set
            {
                LocalSetting.SaveData(Keys.CATEGORYSPANSETTING, value.ToString());
            }
        }

        public static bool NicoChartSetting
        {
            get
            {
                bool res = false;
                bool.TryParse(LocalSetting.ReadData(LocalSetting.Keys.NICOCHART), out res);

                return res;
            }
            set
            {
                LocalSetting.SaveData(Keys.NICOCHART, value.ToString());
            }
        }

        public static byte VolumeSetting
        {
            get
            {
                byte s;
                string data = LocalSetting.ReadData(Keys.VOLUME);

                if (!byte.TryParse(data, out s))
                    s = 100;

                return s;
            }
            set
            {
                LocalSetting.SaveData(Keys.VOLUME, value.ToString());
            }
        }

        public static bool AutoPlaySetting
        {
            get
            {
                bool res = true;

                if (!string.IsNullOrEmpty(LocalSetting.ReadData(LocalSetting.Keys.AUTOPLAY)))
                    bool.TryParse(LocalSetting.ReadData(LocalSetting.Keys.AUTOPLAY), out res);

                return res;
            }
            set
            {
                LocalSetting.SaveData(Keys.AUTOPLAY, value.ToString());
            }
        }

        public static bool RepeatSetting
        {
            get
            {
                bool res = false;
                bool.TryParse(LocalSetting.ReadData(LocalSetting.Keys.REPEAT), out res);

                return res;
            }
            set
            {
                LocalSetting.SaveData(Keys.REPEAT, value.ToString());
            }
        }

        public static bool IYAYO184Setting
        {
            get
            {
                bool res = true;

                if (!string.IsNullOrEmpty(LocalSetting.ReadData(LocalSetting.Keys.IYAYO184)))
                    bool.TryParse(LocalSetting.ReadData(LocalSetting.Keys.IYAYO184), out res);

                return res;
            }
            set
            {
                LocalSetting.SaveData(Keys.IYAYO184, value.ToString());
            }
        }

        public static byte LanguageSetting
        {
            get
            {
                byte res = 0;

                if (!byte.TryParse(LocalSetting.ReadData(LocalSetting.Keys.LANGUAGE), out res))
                    res = 0;

                return res;
            }
            set
            {
                LocalSetting.SaveData(Keys.LANGUAGE, value.ToString());
            }
        }

        //public static bool ShareWatchSetting
        //{
        //    get
        //    {
        //        return bool.Parse(LocalSetting.ReadData(Keys.SHAREWATCHED));
        //    }
        //    set
        //    {
        //        if (bool.Parse(value))
        //            LocalSetting.SaveData(Keys.SHAREWATCHED, bool.TrueString);
        //        else
        //            LocalSetting.SaveData(Keys.SHAREWATCHED, bool.FalseString);
        //    }
        //}
    }
}
