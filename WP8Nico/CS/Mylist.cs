using NicoLibrary.nomula;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WP8Nico.nomula.Resources;

namespace WP8Nico.nomula
{
    /// <summary>
    /// マイリストの一覧用クラス
    /// </summary>
    public class Mylist : Common.BindableBase
    {
        public uint ID { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Thumbnail { get; set; }//先頭の動画のサムネイル
        const char detail = '1';//detailを取得する場合は1、しない場合は0

        public async static Task<IEnumerable<Mylist>> ReadMylistListAsync()
        {
            const string url = "http://i.nicovideo.jp/v3/mylistgroup.get?sid={0}&detail={1}";
            IList<Mylist> list = new List<Mylist>();
            IEnumerable<Mylist> result = null;

            if (!string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID))
            {
                list.Add(new Mylist() { ID = 0, Name = AppResources.Watched });
                list.Add(new Mylist() { ID = 1, Name = AppResources.ShareWatched });
                list.Add(new Mylist() { ID = 2, Name = AppResources.MyUploads });
                list.Add(new Mylist() { ID = 3, Name = AppResources.ToriaezuMylist });//とりあえずマイリスト追加
                //App.ViewModel.UserSetting.Mylist.Add(toriaezu);

                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(url, App.ViewModel.UserSetting.SessionID, detail), UriKind.Absolute));
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                            result = from item in xml.Descendants("mylistgroup")
                                            where item.Parent.Attribute("status").Value == "ok"
                                            select new Mylist
                                            {
                                                ID = uint.Parse(item.Element("id").Value),
                                                Name = item.Element("name").Value,
                                                Thumbnail = item.Element("video").Element("thumbnail_url").Value
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
            }

            return result != null ? list.Concat(result) : list;
        }

        public async static Task<IEnumerable<RankingResults>> ReadItemsAsync(uint id, ushort from = 0)//取得開始番号
        {
            const string toriaezuUrl = "http://i.nicovideo.jp/v3/deflist.list?sort={0}&sid={1}&id=0&v=1%2E20&limit={2}&from={3}";
            const string otherUrl = "http://i.nicovideo.jp/v3/mylistvideo.get?sort={0}&sid={1}&id={2}&v=1%2E20&limit={3}&from={4}";
            IEnumerable<RankingResults> items = null;
            const char sort = '1';//マイリストのソート順
            const byte limit = 50;//取得数

            if (id == 3)//とりあえずマイリスト
            {
                items = await RankingResults.ReadItemsAsync(string.Format(toriaezuUrl, sort, App.ViewModel.UserSetting.SessionID, limit, from));
            }
            else
            {
                items = await RankingResults.ReadItemsAsync(string.Format(otherUrl, sort, App.ViewModel.UserSetting.SessionID, id, limit, from));
            }

            return items;
        }

        public async static Task<IEnumerable<RankingResults>> ReadUserUploadedItemsAsync()
        {
            const string arrayUrl = "http://i.nicovideo.jp/v3/video.array?v=";
            const string requestFormat = "http://www.nicovideo.jp/user/{0}/video?rss=2.0";
            StringBuilder sb = null;
            IEnumerable<RankingResults> items = null;

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(requestFormat, App.ViewModel.UserSetting.UserNumber)));
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                        var result = from item in xml.Descendants("item")
                                     select Function.ExtractLink(item.Element("link").Value)[0];

                        if (result != null && result.Any())
                        {
                            sb = new StringBuilder();
                            sb.Append(arrayUrl);

                            foreach (string str in result)
                                sb.AppendFormat("{0},", str);

                            sb.Append("&api_version=1%2E20");

                            items = await RankingResults.ReadItemsAsync(sb.ToString());
                        }

                        xml = null;
                        sr.Dispose();
                    }

                    sb = null;
                }

                res.Dispose();
                res = null;
                req = null;
            }
            catch (Exception)
            { }

            return items;
        }

        public async static Task<IEnumerable<RankingResults>> ReadPublicMylistItemsAsync(string id)
        {
            const string arrayUrl = "http://i.nicovideo.jp/v3/video.array?v=";
            const string requestFormat = "http://www.nicovideo.jp/mylist/{0}?rss=2.0";
            StringBuilder sb = null;
            IEnumerable<RankingResults> items = null;

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(requestFormat, id)));
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                        var result = from item in xml.Descendants("item")
                                     select Function.ExtractLink(item.Element("link").Value)[0];

                        if (result != null && result.Any())
                        {
                            sb = new StringBuilder();
                            sb.Append(arrayUrl);

                            foreach (string str in result)
                                sb.AppendFormat("{0},", str);

                            sb.Append("&api_version=1%2E20");

                            items = await RankingResults.ReadItemsAsync(sb.ToString());
                        }

                        xml = null;
                        sr.Dispose();
                    }
                }

                res.Dispose();
                res = null;
                req = null;

                sb = null;
            }
            catch (Exception)
            { }

            return items;
        }

        public async static Task<bool> AddItemAsync(uint id, string name, string videoId)
        {
            const string toriaezuUrl = "http://i.nicovideo.jp/v3/deflist.add?sid={0}&vid={1}";
            const string otherUrl = "http://i.nicovideo.jp/v3/mylistvideo.add?sid={0}&v={1}&id={2}";
            bool resp = false;
            string descendant = null;
            Uri uri = null;

            if (id == 3 && string.Equals(AppResources.ToriaezuMylist, name))
            {
                System.Diagnostics.Debug.Assert(id <= 3);
                uri = new Uri(string.Format(toriaezuUrl, App.ViewModel.UserSetting.SessionID, videoId), UriKind.Absolute);
                descendant = "nicovideo";
            }
            else//とりあえずマイリスト以外の全てのマイリスト
            {
                System.Diagnostics.Debug.Assert(id > 3);
                uri = new Uri(string.Format(otherUrl, App.ViewModel.UserSetting.SessionID, videoId, id), UriKind.Absolute);
                descendant = "nicovideo_mylist_response";
            }

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(uri);
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                        var result = from item in xml.Descendants(descendant)
                                     where item.Attribute("status").Value == "ok"
                                     select item;

                        if (result != null && result.Any())
                            resp = true;

                        result = null;
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

            return resp;
        }

        //public static List<string> GetItemsTitleWithoutAppSpecific()
        //{
        //    List<Mylist> list = App.ViewModel.UserSetting.Mylist.ToList();
        //    List<string> title = new List<string>();

        //    if (list[0].ID == 0)//視聴履歴(アプリ内)の表示を削除
        //        list.RemoveAt(0);
        //    if (list[0].ID == 1)//視聴履歴(ネットワーク)の表示を削除
        //        list.RemoveAt(0);
        //    if (list[0].ID == 2)//投稿動画の表示を削除
        //        list.RemoveAt(0);

        //    for (byte i = 0; i < list.Count; i++)
        //        title.Add(list[i].Name);

        //    return title;
        //}

        public static IEnumerable<string> GetItemsTitleWithoutAppSpecific()
        {
            return from item in App.ViewModel.UserSetting.Mylist
                   where item.ID >= 3
                   select item.Name;
        }

        public static Mylist ResolveMylistFromName(string str)
        {
            var result = from item in App.ViewModel.UserSetting.Mylist
                         where item.Name == str
                         select item;

            return result != null && result.Any() ? result.First() : null;
        }
    }
}
