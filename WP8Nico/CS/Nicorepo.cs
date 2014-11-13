using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WP8Nico.nomula
{
    public class Nicorepo : Common.BindableBase
    {
        public string UserName { get; set; }
        public string UserID { get; set; }
        public Uri UserIcon { get; set; }
        public string VideoTitle { get; set; }
        public string VideoID { get; set; }
        public Uri VideoThumbnail { get; set; }
        public Uri VideoUrl { get; set; }
        public DateTime Date { get; set; }

        public async static Task<IEnumerable<Nicorepo>> ReadItems()
        {
            IEnumerable<Nicorepo> nicorepo = null;

            if (App.ViewModel.UserSetting.pcLogined || await UserSetting.DeepLoginAsync() == true)//ログイン済み
            {
                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri("http://www.nicovideo.jp/my/top", UriKind.Absolute));//ニコレポURLは全ユーザ固定
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            //パース
                            nicorepo = ReadNicorepo(await sr.ReadToEndAsync());
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

            return nicorepo;
        }

        private static IEnumerable<Nicorepo> ReadNicorepo(string str)
        {
            IList<Nicorepo> nicorepo = null;

            try
            {
                //マイリスト登録された動画を取り出す
                Regex regex = new Regex("<img src=\"(.*)\" alt=\"\" class=\"nicorepo-lazyimage\" data-src=\"(?<user_icon>.*?)\"></a>\\s*</div>\\s*<div class=\"log-content\">\\s*<div class=\"log-body\">\\s*<a href=\"(?<user_page>.*?)\" class=\"author-user\">(?<user_name>.*?)</a> (?<content>.*?)\\s*</div>\\s*<div class=\"log-details log-target log-target-video\">\\s*<!--  -->\\s*<div class=\"log-target-thumbnail\">\\s*<a href=\"(?<watch_page>.*?)\">\\s*<img src=\"(.*)\" alt=\"\" class=\"nicorepo-lazyimage video\" data-src=\"(?<thumbnail_url>.*?)\">\\s*</a>\\s*</div>\\s*<div class=\"log-target-info\">\\s*<span class=\"log-target-type-video\">\\w*</span>\\s*<a href=\"(?<watch_page2>.*?)\">(?<movie_title>.*?)</a>\\s*</div>\\s*</div><!-- .log-details -->\\s*<div class=\"log-footer\">\\s*<a href=\"(?<nicorepo_url>.*?)\" class=\"log-footer-date \">\\s*<time class=\"relative\" datetime=\"(?<date>.*?)\">", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                MatchCollection mc = regex.Matches(str);

                if (mc != null && mc.Count > 0)
                {
                    nicorepo = new List<Nicorepo>();

                    for (byte i = 0, c = (byte)mc.Count; i < c; i++)
                    {
                        nicorepo.Add(new Nicorepo() 
                        {
                            UserIcon = new Uri(mc[i].Groups["user_icon"].Value, UriKind.Absolute),
                            UserName = mc[i].Groups["user_name"].Value,
                            VideoTitle = mc[i].Groups["movie_title"].Value,
                            VideoThumbnail = new Uri(mc[i].Groups["thumbnail_url"].Value, UriKind.Absolute),
                            VideoUrl = new Uri(mc[i].Groups["watch_page"].Value, UriKind.Absolute),
                            Date = DateTime.Parse(mc[i].Groups["date"].Value),
                            UserID = string.Empty,
                            VideoID = string.Empty
                        });
                    }
                }

                regex = null;
                mc = null;
            }
            catch (Exception)
            { }

            return nicorepo;
        }


        //if (!App.ViewModel.UserSetting.pcLogined)
        //{
        //    await App.ViewModel.UserSetting.DeepLogin();
        //}
        //if (!App.ViewModel.UserSetting.Logined && await App.ViewModel.UserSetting.LightLogin())
        //{
        //    HttpWebRequest req = WebRequest.CreateHttp(new Uri("http://i.nicovideo.jp/v5//NicoRepo", UriKind.Absolute));
        //    //Cookie c = new Cookie("user_session", App.ViewModel.UserSetting.SessionID, "/", "nicovideo.jp");
        //    //App.ViewModel.UserSetting.cc.Add(new Uri("http://www.nicovideo.jp/", UriKind.Absolute), c);
        //    //req.CookieContainer = App.ViewModel.UserSetting.cc;

        //    foreach (Cookie c in App.ViewModel.UserSetting.cc.GetCookies(new Uri("http://www.nicovideo.jp")))
        //    {
        //        System.Diagnostics.Debug.WriteLine(c.Value);
        //    }

        //    CookieCollection ccc = new CookieCollection();
        //    ccc.Add(new Cookie("nicoiphone_app_version", "4.22"));
        //    //ccc.Add(new Cookie("nicoiphone_device", "iPhone4,1"));
        //    ccc.Add(new Cookie("sid", App.ViewModel.UserSetting.SessionID));
        //    App.ViewModel.UserSetting.cc.Add(new Uri("http://i.nicovideo.jp", UriKind.Absolute), ccc);

        //    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;
        //    CookieContainer cookieJar = new CookieContainer();
        //    HttpClientHandler handler = new HttpClientHandler()
        //    {
        //        CookieContainer = App.ViewModel.UserSetting.cc
        //    };
        //    HttpClient hc = new HttpClient(handler as HttpClientHandler);
        //    hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Mobile/10A5376e");

        //    string response = await hc.GetStringAsync(new Uri("http://i.nicovideo.jp/v5//NicoRepo", UriKind.Absolute));

        //    if (res != null && res.StatusCode == HttpStatusCode.OK)
        //    {
        //        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
        //        {
        //            nicorepo = new ObservableCollection<Nicorepo>();

        //            //Regex regex = new Regex("<a href=\"(?<user_id>.*?)\" class=\"author-user\">(?<user_name>.*?)</a> さんが <strong>動画を投稿しました。</strong>", RegexOptions.IgnoreCase);//"<div class=\"log-target-thumbnail\">\n \t\t\t\t\t\t<a href=\"(?<url>.*?)\">"\n\t\t\t\t\t\t\t\t<img src=\"http://uni.res.nimg.jp/img/x.gif\" alt=\"\" class=\"nicorepo-lazyimage video\" data-src=\"(?<img>.*?)\">\n\t\t\t\t\t\t\t</a>\n\t\t\t\t\t\t</div>\n\t\t\t\t\t\t\t\t\t\t<div class=\"log-target-info\">\n\n\t\t\t\t\t\t\t\t\t\t\t\t\t<span class=\"log-target-type-video\">動画</span>\n\t\t\t\t\t\t\t\t\t\t\t\t<a href=\"(?<url2>.*?)\">(?<title>.*?)</a>\n\t\t\t\t\t</div>\n\t\t\t\t\n\n\t\t\t\t\t\t\t</div><!-- .log-details -->\n\n\t\t\t\t\t\t\t<div class=\"log-footer\">\n\n\t\t\t\t\t<a href=\"(?<nicorepo>.*?)\" class=\"log-footer-date \">\n\t\t\t\t\t\t<time class=\"relative\" datetime=\"(?<time>.*?)\">\n\\t\t\t\t\t\t\t(?<time2>.*?)\n\t\t\t\t\t\t</time>", RegexOptions.IgnoreCase);
        //            //Regex regex = new Regex("<div class=\"log-target-thumbnail\">\r\n \t\t\t\t\t\t<a href=\"(?<url>.*?)\">\r\n\t\t\t\t\t\t\t\t<img src=\"http://uni.res.nimg.jp/img/x.gif\" alt=\"\" class=\"nicorepo-lazyimage video\" data-src=\"(?<thumbnail>.*?)\\r\n\t\t\t\t\t\t\t</a>\r\n\t\t\t\t\t\t</div>\r\n\t\t\t\t\t\t\t\t\t\t<div class=\"log-target-info\">\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t<span class=\"log-target-type-video\">動画</span>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<a href=\"(?<url2>.*?)\">(?<title>.*?)</a>\r\n\t\t\t\t\t</div>\r\n\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t</div><!-- .log-details -->\r\n\r\n\t\t\t\t\t\t\t<div class=\"log-footer\">\r\n\r\n\t\t\t\t\t<a href=\"(?<url>.*?)\" class=\"log-footer-date \">\r\n\t\t\t\t\t\t<time class=\"relative\" datetime=\"(?<time>.*?)\">\r\n\t\t\t\t\t\t\t(?<time2>.*?)\r\n\t\t\t\t\t\t</time>", RegexOptions.IgnoreCase);
        //            string resp = await sr.ReadToEndAsync();
        //            //MatchCollection mc = regex.Matches(resp);

        //            //foreach (Match m in mc)
        //            //{
        //            //    user = m.Groups["user_name"].Value;
        //            //    id = m.Groups["user_id"].Value;
        //            //    content = m.Groups["content"].Value;
        //            //    url = m.Groups["url"].Value;
        //            //    thumbnail = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase).Match(m.Groups["img"].Value).Value;
        //            //}

        //            //Regex regex = new Regex("<img src=\"http://uni.res.nimg.jp/img/x.gif\" alt=\"\" class=\"nicorepo-lazyimage video\" data-src=\"(?<thumbnail>.*?)\">", RegexOptions.IgnoreCase);
        //            //MatchCollection mc2 = regex.Matches(resp);

        //            //foreach (Match m in mc2)
        //            //{

        //            //    thumbnail = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase).Match(m.Groups["thumbnail"].Value).Value;
        //            //}

        //            //regex = new Regex("<span class=\"log-target-type-video\">動画</span>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<a href=\"(?<url>.*?)\">(?<title>.*?)</a>", RegexOptions.IgnoreCase);
        //            //MatchCollection mc3 = regex.Matches(resp);

        //            //foreach (Match m in mc3)
        //            //{
        //            //    url = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase).Match(m.Groups["url"].Value).Value;
        //            //    title = m.Groups["title"].Value;
        //            //}

        //            //regex = new Regex("<a href=\"(?<url>.*?)\" class=\"log-footer-date \">\r\n\t\t\t\t\t\t<time class=\"relative\" datetime=\"(?<time>.*?)\">\r\n\t\t\t\t\t\t\t(?<time2>.*?)\r\n\t\t\t\t\t\t</time>", RegexOptions.IgnoreCase);
        //            //MatchCollection mc4 = regex.Matches(resp);

        //            //foreach (Match m in mc4)
        //            //{
        //            //    url = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase).Match(m.Groups["url"].Value).Value;
        //            //    time = m.Groups["time"].Value;
        //            //}

        //            response = response.Replace("(", "").Replace(")", "");
        //            Regex regex = new Regex("\t\t<div class=\"usericon\">\n\t\t\t\t\t\t\t<a href=\"(?<url1>.*?)\">\n  \t\t\t\t\t<img src=\"(?<thumbnail>.*?)\" width=\"100\" height=\"100\" />\n\t\t\t\t</a>\n\t\t\t\t\t</div>\n\t\t<div class=\"re_info\">\n\t\t\t<p class=\"date\">\n\t\t\t\t(?<date>.*?) <!---->\n\t\t\t</p>\n\t\t\t<p class=\"main_name\">\n\t\t\t\t\t\t\t\t\t<a href=\"(?<url2>.*?)\">(?<name>.*?)</a>\n\t\t\t\t\t\t\t</p>\n\t\t\t<div class=\"re_detail\">\n\t\t\t\t\t\t\t\t\t<p><span class=\"highlight\">(?<user_name>.*?) <a href=\"javascript:ondemand.play\'(?<video_id>.*?)\'\">(?<content>.*?)</a> (?<content2>.*?)</p>", RegexOptions.IgnoreCase);//\t\t\t\t\t\t\t</div>\n", RegexOptions.IgnoreCase);
        //            MatchCollection mc = regex.Matches(response);

        //            foreach (Match m in mc)
        //            {
        //            }
        //        }
        //    }
        //}

        public static ObservableCollection<Nicorepo> ReadNewItems(ObservableCollection<Nicorepo> nicorepo)
        {
            ObservableCollection<Nicorepo> res = null;
            bool chk = true;

            //前回取得時のニコレポ読み出し
            Nicorepo[] old = ReadData().ToArray();

            if (old != null && old.Length > 0 && nicorepo != null && nicorepo.Count > 0)
            {
                if (res == null)
                    res = new ObservableCollection<Nicorepo>();

                for (int i = 0; i < nicorepo.Count; i++)
                {
                    for (int j = 0; j < old.Length; j++)
                    {
                        if (nicorepo[i].VideoUrl == old[j].VideoUrl && nicorepo[i].UserName == old[j].UserName)
                            chk = false;
                    }

                    if (chk)
                    {
                        res.Add(nicorepo[i]);
                    }

                    chk = true;
                }
            }

            if (res == null)
                res = nicorepo;

            //今回のニコレポを保存
            SaveItems(nicorepo);

            return res;
        }

        public static void SaveItems(ObservableCollection<Nicorepo> nicorepo)
        {
            AddData(nicorepo);
        }

        public static IEnumerable<Nicorepo> ReadData()
        {
            IList<Nicorepo> result = null;
            string data = LocalSetting.ReadData(LocalSetting.Keys.NICOREPO);

            if (!string.IsNullOrEmpty(data))
            {
                JArray parsed = JArray.Parse(data);

                if (parsed.Count > 0)
                {
                    result = new List<Nicorepo>();

                    //配列化
                    for (byte i = 0, c = (byte)parsed.Count; i < c; i++)
                    {
                        var json = JObject.Parse(parsed[i].ToString());

                        result.Add(new Nicorepo()
                        {
                            UserName = json["user_name"].ToString(),
                            UserID = json["user_id"].ToString(),
                            UserIcon = new Uri(json["user_icon"].ToString(), UriKind.Absolute),
                            VideoTitle = json["video_title"].ToString(),
                            VideoID = json["video_id"].ToString(),
                            VideoThumbnail = new Uri(json["video_thumbnail"].ToString(), UriKind.Absolute),
                            VideoUrl = new Uri(json["video_url"].ToString(), UriKind.Absolute),
                            Date = DateTime.Parse(json["date"].ToString())
                        });
                    }
                }
            }
            else
            { }

            return result;
        }

        public static void AddData(ObservableCollection<Nicorepo> nicorepo)
        {
            string data = null;

            //JSON化
            JArray array = new JArray();

            for (byte i = 0, c = (byte)nicorepo.Count; i < c; i++)
            {
                var json = new JObject();
                json.Add("user_name", JToken.FromObject(nicorepo[i].UserName));
                json.Add("user_id", JToken.FromObject(nicorepo[i].UserID));
                json.Add("user_icon", JToken.FromObject(nicorepo[i].UserIcon.ToString()));
                json.Add("video_title", JToken.FromObject(nicorepo[i].VideoTitle));
                json.Add("video_id", JToken.FromObject(nicorepo[i].VideoID));
                json.Add("video_thumbnail", JToken.FromObject(nicorepo[i].VideoThumbnail));
                json.Add("video_url", JToken.FromObject(nicorepo[i].VideoUrl));
                json.Add("date", JToken.FromObject(nicorepo[i].Date));

                array.Add(json);
            }

            data = array.ToString();

            LocalSetting.SaveData(LocalSetting.Keys.NICOREPO, data);
        }

        public static void RemoveItems()
        {
            RemoveData();
        }

        private static void RemoveData()
        {
            LocalSetting.RemoveData(LocalSetting.Keys.NICOREPO);
        }
    }
}
