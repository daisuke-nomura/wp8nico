using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WP8Nico.nomula
{
    public class SynchronizeWatchedMovie
    {
        public string ID { get; set; }
        //public uint Position { get; set; }
        public bool Deleted { get; set; }
        public string Title { get; set; }
        public Uri ThumbnailUrl { get; set; }
        public string Length { get; set; }
        public DateTime LastWatched { get; set; }
        public ushort Count { get; set; }
        public ushort Device { get; set; }

        public async static Task<IEnumerable<RankingResults>> ReadItemsAsync()
        {
            const string arrayUrl = "http://i.nicovideo.jp/v3/video.array?v=";
            IEnumerable<RankingResults> items = null;
            StringBuilder sb = null;

            var result = await ReadDataAsync();

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

            return items;
        }

        public async static Task<IEnumerable<SynchronizeWatchedMovie>> ReadDataAsync()
        {
            const string historyUrl = "http://www.nicovideo.jp/api/videoviewhistory/list?format=json";
            IEnumerable<SynchronizeWatchedMovie> result = null;

            if (await UserSetting.DeepLoginAsync() == true)
            {
                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(historyUrl, UriKind.Absolute));
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            JObject parsed = JObject.Parse(await sr.ReadToEndAsync());

                            result = from item in parsed["history"]
                                     where parsed["status"].ToString() == "ok"
                                     select new SynchronizeWatchedMovie()
                                     {
                                         ID = JObject.Parse(item.ToString())["video_id"].ToString(),
                                     };

                            //parsed = null;
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

            return result;
        }

        public async static Task AddDataAsync(string id)
        {
            //視聴履歴の保存はサーバ側で行われるので、watchページをリクエストすればいい
            const string watchPageUrl = "http://www.nicovideo.jp/watch/{0}";

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(watchPageUrl, id), UriKind.Absolute));
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.PartialContent))
                {
                }
            }
            catch (WebException)
            { }
            catch (Exception)
            { }
        }
    }
}
