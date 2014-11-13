using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using WP8Nico.nomula.Resources;

namespace WP8Nico.nomula
{

    /// <summary>
    /// カテゴリ一覧取得クラス
    /// </summary>
    public class Category
    {
        public static string[] category = new string[] {
            AppResources.CategoryTypeHourlyAll,
            AppResources.CategoryTypeHourlyView,
            AppResources.CategoryTypeHourlyRes,
            AppResources.CategoryTypeHourlyMylist,
            AppResources.CategoryTypeDailyAll,
            AppResources.CategoryTypeDailyView,
            AppResources.CategoryTypeDailyRes,
            AppResources.CategoryTypeDailyMylist,
            AppResources.CategoryTypeWeeklyAll,
            AppResources.CategoryTypeWeeklyView,
            AppResources.CategoryTypeWeeklyRes,
            AppResources.CategoryTypeWeeklyMylist,
            AppResources.CategoryTypeMonthlyAll,
            AppResources.CategoryTypeMonthlyView,
            AppResources.CategoryTypeMonthlyRes,
            AppResources.CategoryTypeMonthlyMylist,
            AppResources.CategoryTypeTotalAll,
            AppResources.CategoryTypeTotalView,
            AppResources.CategoryTypeTotalRes,
            AppResources.CategoryTypeTotalMylist,
        };

        public string Key { get; set; }
        public string Tag { get; set; }

        public async static Task<IEnumerable<Category>> ReadCategoryAsync()
        {
            IEnumerable<Category> result = null;
            const string url = "http://i.nicovideo.jp/v3/genre.list?overseas=0&official=0";

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url, UriKind.Absolute));
                //req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                        result = from item in xml.Descendants("genre")
                                 select new Category
                                 {
                                     Key = item.Element("key").Value,
                                     Tag = item.Element("tag").Value
                                 };

                        xml = null;
                        sr.Dispose();
                    }
                }

                res.Dispose();
                res = null;
                req = null;
            }
            catch (WebException)
            { }
            catch (Exception)
            { }

            return result;
        }

        public async static Task<IEnumerable<RankingResults>> ReadItemsAsync(string id, ushort from = 0, Types type = Types.NOTSET, Spans span = Spans.NOTSET)//取得開始番号
        {
            const string hourlyUrl = "http://i.nicovideo.jp/v3/video.ranking?type=view&offset={0}&v=1%2E20&length={1}&span={2}&genre={3}";
            const string hourlyUrl2 = "http://i.nicovideo.jp/v3/video.ranking?type=view&offset={0}&v=1%2E20&length={1}&span={2}&genre={3}&type={4}";
            //const string dailyUrl = "http://i.nicovideo.jp/v3/video.ranking?type=view&offset={0}&v=1%2E20&length={1}&span=daily&genre={2}";
            //const string dailyUrl2 = "http://i.nicovideo.jp/v3/video.ranking?type=view&offset={0}&v=1%2E20&length={1}&span=daily&genre={2}&type={3}";
            //const string monthlyUrl = "";
            //const string untilUrl = "";
            IEnumerable<RankingResults> items = null;
            const ushort limit = 50;//取得数
            //StringBuilder sb = new StringBuilder();

            if (type == Types.NOTSET) type = ReadCategoryTypeSetting();
            if (span == Spans.NOTSET) span = ReadCategorySpanSetting();

            //if (id == "all")//総合カテゴリ
            //{
                if (type == Types.NONE)
                    items = await RankingResults.ReadItemsAsync(string.Format(hourlyUrl, from, limit, GetStringBySpan(span), id));
                else
                    items = await RankingResults.ReadItemsAsync(string.Format(hourlyUrl2, from, limit, GetStringBySpan(span), id, GetStringByType(type)));
            //}
            //else
            //{
            //    if (type == Types.NONE)
            //        items = await RankingResults.ReadItems(string.Format(dailyUrl, from, limit, id));
            //    else
            //    {
            //        string str = GetStringByType(type);
            //        items = await RankingResults.ReadItems(string.Format(dailyUrl2, from, limit, id, str));
            //    }
            //}

            //items = RankingResults.ReadItems(id, url, sort, limit, from);


            return items;
        }

        public async static Task<IEnumerable<RankingResults>> ReadBigItemsAsync(BigCategory id)
        {
            const string url = "http://www.nicovideo.jp/ranking/fav/hourly/{0}?rss=2.0";
            string[] category = new string[] { "g_ent2", "g_life2", "g_politics", "g_tech", "g_culture2", "g_other" };
            IEnumerable<RankingResults> items = null;

            items = await RankingResults.ReadItemsByFeedAsync(string.Format(url, category[(int)id]));

            return items;
        }

        public enum BigCategory
        {
            EntertainmentMusic,
            LifeGeneralSports,
            Politics,
            ScienceTechnology,
            AnimeGameDraw,
            Other
        }

        public enum Types : sbyte
        {
            NOTSET = -1,
            NONE = 0,
            VIEW = 1,
            RES = 2,
            MYLIST = 3
        }

        public enum Spans : sbyte
        {
            NOTSET = -1,
            HOURLY = 0,
            DAILY = 10,
            WEEKLY = 20,
            MONTHLY = 30,
            TOTAL = 40
        }

        private static string GetStringByType(Types type)
        {
            string str = string.Empty;

            switch (type)
            {
                case Types.VIEW: str = "view"; break;
                case Types.RES: str = "res"; break;
                case Types.MYLIST: str = "mylist"; break;
                default: str = string.Empty; break;
            }

            return str;
        }

        private static string GetStringBySpan(Spans span)
        {
            string str = string.Empty;

            switch (span)
            {
                case Spans.HOURLY: str = "hourly"; break;
                case Spans.DAILY: str = "daily"; break;
                case Spans.WEEKLY: str = "weekly"; break;
                case Spans.MONTHLY: str = "monthly"; break;
                case Spans.TOTAL: str = "total"; break;
                default: str = string.Empty; break;
            }

            return str;
        }

        private static void SaveCategoryTypeSetting(Types type)
        {
            LocalSetting.CategoryTypeSetting = (sbyte)type;
        }

        private static void SaveCategorySpanSetting(Spans asc)
        {
            LocalSetting.CategorySpanSetting = (sbyte)asc;
        }

        public static int ReadSearchSetting()
        {
            return (int)ReadCategoryTypeSetting() + (int)ReadCategorySpanSetting();
        }

        private static Types ReadCategoryTypeSetting()
        {
            return (Types)LocalSetting.CategoryTypeSetting;
        }

        private static Spans ReadCategorySpanSetting()
        {
            return (Spans)LocalSetting.CategorySpanSetting;
        }

        public static void SaveSearchSetting(int s)
        {
            if (s >= 16) s += 6;
            if (s >= 12) s += 6;
            if (s >= 8) s += 6;
            if (s >= 4) s += 6;

            if (s % 10 == 0)
                SaveCategoryTypeSetting(Types.NONE);
            else if (s % 10 == 3)
            {
                SaveCategoryTypeSetting(Types.MYLIST);
            }
            else if (s % 10 == 2)
            {
                SaveCategoryTypeSetting(Types.RES);
            }
            else if (s % 10 == 1)
            {
                SaveCategoryTypeSetting(Types.VIEW);
            }
            else
                SaveCategoryTypeSetting(Types.NONE);

            if (s / 10 == 0)
                SaveCategorySpanSetting(Spans.HOURLY);
            else if (s / 10 == 1)
                SaveCategorySpanSetting(Spans.DAILY);
            else if (s / 10 == 2)
                SaveCategorySpanSetting(Spans.WEEKLY);
            else if (s / 10 == 3)
                SaveCategorySpanSetting(Spans.MONTHLY);
            else if (s / 10 == 4)
                SaveCategorySpanSetting(Spans.TOTAL);
            else
                SaveCategorySpanSetting(Spans.HOURLY);
        }

        public async static Task<IEnumerable<RankingResults>> ReadRssItemsAsync(string id)
        {
            const string url = "http://www.nicovideo.jp/{0}rss=2.0&lang=ja-jp";
            string[] category = new string[] { "newarrival?", "recent?", "tag/公式?sort=f&" };
            IEnumerable<RankingResults> items = null;
            int i = 0;

            if (int.TryParse(id, out i))
                items = await RankingResults.ReadItemsByRssFeedAsync(string.Format(url, category[i]));

            return items;
        }

        public enum RssFeed : byte
        {
            NewestPosted,
            NewestCommentPosted,
            NewestOfficialMovie
        }

        public static IEnumerable<Category> ReadRss()
        {
            IList<Category> result = new List<Category>();
            result.Add(new Category() { Key = "0", Tag = "新着投稿動画" });
            result.Add(new Category() { Key = "1", Tag = "新着コメント動画" });
            result.Add(new Category() { Key = "2", Tag = "新着公式動画" });

            return result;
        }
    }
}
