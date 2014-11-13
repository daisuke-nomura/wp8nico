using System;
using System.Net;
using System.Threading.Tasks;

namespace NicoLibrary.nomula
{
    public class SynchronizeWatchedMovie
    {
        const ushort count = 30;//規定数

        public string ID { get; set; }
        //public uint Position { get; set; }
        public DateTime LastWatched { get; set; }
        public ushort Count { get; set; }

        public async static Task<SynchronizeWatchedMovie[]> ReadItemsAsync()
        {
            throw new NotImplementedException();
        }

        public async static Task<SynchronizeWatchedMovie[]> ReadDataAsync()
        {
            throw new NotImplementedException();
        }

        public async static Task AddDataAsync(string id, CookieContainer cc)
        {
            //視聴履歴の保存はサーバ側で行われるので、watchページをリクエストすればいい
            const string watchPageUrl = "http://www.nicovideo.jp/watch/{0}";

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(watchPageUrl, id), UriKind.Absolute));
                req.CookieContainer = cc;
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
