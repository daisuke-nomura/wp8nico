using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WP8Nico.nomula.Resources;

namespace WP8Nico.nomula
{
/// <summary>
    /// 検索タイプ
    /// </summary>
    public class Search
    {
        public static string[] search = new string[] {
            AppResources.SearchKeywordNewAsc,
            AppResources.SearchKeywordNewDesc,
            AppResources.SearchKeywordViewMany,
            AppResources.SearchKeywordViewLittle,
            AppResources.SearchKeywordCommentNew,
            AppResources.SearchKeywordCommentOld,
            AppResources.SearchKeywordCommentMany,
            AppResources.SearchKeywordCommentLittle,
            AppResources.SearchKeywordMylistMany,
            AppResources.SearchKeywordMylistLittle,
            AppResources.SearchKeywordLengthLong,
            AppResources.SearchKeywordLengthShort,
            AppResources.SearchTagNewAsc,
            AppResources.SearchTagNewDesc,
            AppResources.SearchTagViewMany,
            AppResources.SearchTagViewLittle,
            AppResources.SearchTagCommentNew,
            AppResources.SearchTagCommentOld,
            AppResources.SearchTagCommentMany,
            AppResources.SearchTagCommentLittle,
            AppResources.SearchTagMylistMany,
            AppResources.SearchTagMylistLittle,
            AppResources.SearchTagLengthLong,
            AppResources.SearchTagLengthShort
        };

        public string Name { get; set; }
        public ushort Number { get; set; }

        public enum Type : sbyte
        {
            NoSetting = -1,
            Keyword = 0,
            Tag = 20
        }

        public enum Asc : sbyte
        {
            NoSetting = -1,
            Newpost = 0,
            Oldpost = 1,
            Manyview = 2,
            Littleview = 3,
            Newcom = 4,
            Oldcom = 5,
            Manycom = 6,
            Littlecom = 7,
            Manymy = 8,
            Littlemy = 9,
            Longtime = 10,
            Shorttime = 11
        }

        public async static Task<IEnumerable<RankingResults>> ReadItemsAsync(string id, Type type = Type.NoSetting, ushort from = 0, Asc asc = Asc.NoSetting)//取得開始番号
        {
            const string tagUrl = "http://i.nicovideo.jp/v3/tag.search?k={0}&from={1}&tag={2}&limit={3}&sid={4}&v=3%2E00&o={5}";
            const string keywordUrl = "http://i.nicovideo.jp/v3/video.search?k={0}&from={1}&str={2}&limit={3}&sid={4}&v=3%2E00&o={5}";
            IEnumerable<RankingResults> items = null;
            const byte limit = 30;//取得数

            if (type == Type.NoSetting)
                type = ReadSearchTypeSetting();

            char[] setting = SetSearchSortSetting(asc);

            if (type == Type.Tag)//タグ検索
            {
                items = await RankingResults.ReadItemsAsync(string.Format(tagUrl, setting[0], from, id, limit, App.ViewModel.UserSetting.SessionID, setting[1]));
            }
            else if (type == Type.Keyword)
            {
                items = await RankingResults.ReadItemsAsync(string.Format(keywordUrl, setting[0], from, id, limit, App.ViewModel.UserSetting.SessionID, setting[1]));
            }
            else
            {
                throw new Exception("想定外の検索タイプです");
            }

            return items;
        }

        private static void SaveSearchTypeSetting(Type type)
        {
            LocalSetting.SearchSetting = (sbyte)type;
        }

        private static void SaveSearchSortSetting(Asc asc)
        {
            LocalSetting.SearchSortSetting = (sbyte)asc;
        }

        public static char[] SetSearchSortSetting(Asc asc)
        {
            char[] setting = new char[2] { 'f', 'd' };

            if (asc == Asc.NoSetting)
                asc = ReadSearchSortSetting();

            switch (asc)
            {
                case Asc.Newpost: setting[0] = 'f'; setting[1] = 'd'; break;
                case Asc.Oldpost: setting[0] = 'f'; setting[1] = 'a'; break;
                case Asc.Manyview: setting[0] = 'v'; setting[1] = 'd'; break;
                case Asc.Littleview: setting[0] = 'v'; setting[1] = 'a'; break;
                case Asc.Newcom: setting[0] = 'n'; setting[1] = 'd'; break;
                case Asc.Oldcom: setting[0] = 'n'; setting[1] = 'a'; break;
                case Asc.Manycom: setting[0] = 'r'; setting[1] = 'd'; break;
                case Asc.Littlecom: setting[0] = 'r'; setting[1] = 'a'; break;
                case Asc.Manymy: setting[0] = 'm'; setting[1] = 'd'; break;
                case Asc.Littlemy: setting[0] = 'm'; setting[1] = 'a'; break;
                case Asc.Longtime: setting[0] = 'l'; setting[1] = 'd'; break;
                case Asc.Shorttime: setting[0] = 'l'; setting[1] = 'a'; break;
                default: break;
            }

            return setting;
        }

        public static int ReadSearchSetting()
        {
            return (int)ReadSearchTypeSetting() + (int)ReadSearchSortSetting();
        }

        private static Type ReadSearchTypeSetting()
        {
            return (Type)LocalSetting.SearchSetting;
        }

        private static Asc ReadSearchSortSetting()
        {
            return (Asc)LocalSetting.SearchSortSetting;
        }

        public static void SaveSearchSetting(int s)
        {
            if (s >= 12)
            {
                SaveSearchTypeSetting(Type.Tag);
                s -= 12;
            }
            else
                SaveSearchTypeSetting(Type.Keyword);

            SaveSearchSortSetting((Asc)s);
        }
    }
}
