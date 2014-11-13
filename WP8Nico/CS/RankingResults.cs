using NicoLibrary.nomula;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using WP8Nico.nomula.Resources;

namespace WP8Nico.nomula
{
    /// <summary>
    /// 選択したカテゴリ・マイリスト・検索結果、履歴を取得する際に用いるクラス
    /// </summary>
    public class RankingResults : Common.BindableBase
    {
        public string ID { get; set; }
        public string Title { get; set; }
        private string _description;
        public string Description
        {
            get
            {
                return Function.RemoveHTMLTag(_description);
            }
            set
            {
                _description = value;
            }
        }
        public string Thumbnail { get; set; }
        public TimeSpan Length { get; set; }
        public MovieTypes? MovieType { get; set; }
        public IEnumerable<Tag> TagList { get; set; }
        public uint CommentCount { get; set; }
        public uint ViewCount { get; set; }
        public uint MylistCount { get; set; }
        public string UploadTime { get; set; }
        public Participations Participation { get; set; }
        public string ThreadId { get; set; }
        public bool? HasMp4 { get; set; }
        public byte Deleted { get; set; }
        public IList<string> LinkList { get; set; }
        public ServiceProvider.Services Service { get; set; }
        //public string LastComment { get; set; }
        public uint? UploaderID { get; set; }
        public ushort? RankingNumber { get; set; }
        public Uri FlvUrl { get; set; }

        public BitmapImage Image
        {
            get
            {
                return new BitmapImage(new Uri(Thumbnail, UriKind.Absolute));
            }
        }

        private string _uploader;
        public string Uploader
        {
            get { return _uploader; }
            set { SetProperty(ref _uploader, value); }
        }

        private string _uploaderIcon;
        public string UploaderIcon
        {
            get { return _uploaderIcon; }
            set { SetProperty(ref _uploaderIcon, value); }
        }


        public string OptionalThreadId { get; set; }
        public string MessageServer { get; set; }
        public string VideoServer { get; set; }
        public int LengthFromGetFlv { get; set; }

        public Visibility ChannelIconVisibility
        {
            get
            {
                return this.Participation == Participations.CHANNEL ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public uint? CommunityId { get; set; }
        public Visibility CommunityIconVisibility
        {
            get
            {
                return this.CommunityId != null && this.CommunityId != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public enum Participations : byte
        {
            REGULAR,//所属なし
            COMMUNITY,//コミュニティ動画　ただし、iPhone APIではチャンネル動画もこちら
            CHANNEL//チャンネル動画
        }

        private static MovieTypes? ReadMovieType(string str)
        {
            MovieTypes? type = null;

            switch (str)
            {
                case "mp4":
                    type = MovieTypes.MP4;
                    break;
                case "swf":
                    type = MovieTypes.SWF;
                    break;
                case "flv":
                    type = MovieTypes.FLV;
                    break;
                case null:
                    type = null;
                    break;
                default:
                    type = MovieTypes.MP4;
                    break;
            }

            return type;
        }

        public enum MovieTypes : byte
        {
            MP4,
            SWF,
            FLV,
            RTMP,
            RTMPE,
            HLS
        }

        public async static Task<IEnumerable<RankingResults>> ReadItemAsync(string videoID)
        {
            const string arrayUrl = "http://i.nicovideo.jp/v3/video.array?v={0}&api_version=1%2E20";
            string uri = string.Format(arrayUrl, videoID);

            var items = await RankingResults.ReadItemsAsync(uri);

            return items;
        }

        public async static Task<IEnumerable<RankingResults>> ReadItemsAsync(string url, bool cache = true)
        {
            IEnumerable<RankingResults> result = null;

            if (cache && App.ViewModel.Cache.ContainsKey(url.GetHashCode()))//キャッシュがある
            {
                if (App.ViewModel.Cache.TryGetValue(url.GetHashCode(), out result))
                    ;
            }
            else
            {
                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(url, UriKind.Absolute));
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());
                            uint community_id = 0;

                            result = from item in xml.Descendants("video_info")
                                         where item.Parent.Attribute("status").Value == "ok" &&
                                         Filter.FilterVideo(item.Element("video").Element("title").Value,
                                         (from tags in (from tag in item.Element("tags").Elements("tag_info")
                                                   where tag != null && tag.HasElements
                                                   select new Tag
                                                   {
                                                       Title = tag.Element("tag").Value,
                                                       Location = tag.Element("area").Value
                                                   })
                                                   select tags.Title).ToList())
                                         select new RankingResults
                                         {
                                             Title = item.Element("video").Element("title").Value,
                                             Description = item.Element("video").Element("description").Value,
                                             Thumbnail = item.Element("video").Element("thumbnail_url").Value,
                                             ID = item.Element("video").Element("id").Value,
                                             Length = TimeSpan.FromSeconds(double.Parse(item.Element("video").Element("length_in_seconds").Value)),
                                             MovieType = ReadMovieType(item.Element("video").Element("movie_type").Value),
                                             CommentCount = item.Element("channel_thread") != null ? uint.Parse(item.Element("channel_thread").Element("num_res").Value) : uint.Parse(item.Element("thread").Element("num_res").Value),
                                             ViewCount = uint.Parse(item.Element("video").Element("view_counter").Value),
                                             MylistCount = uint.Parse(item.Element("video").Element("mylist_counter").Value),
                                             UploadTime = item.Element("video").Element("first_retrieve").Value,
                                             TagList = from tag in item.Element("tags").Elements("tag_info")
                                                   where item.Element("tags").Elements("tag_info") != null && tag.HasElements
                                                   select new Tag
                                                   {
                                                       Title = tag.Element("tag").Value,
                                                       Location = tag.Element("area").Value
                                                   },
                                             Participation = item.Element("channel_thread") != null ? Participations.CHANNEL : Participations.REGULAR,//SeparateParticipation(byte.Parse(item.Element("video").Element("option_flag_community").Value)),
                                             ThreadId = item.Element("channel_thread") != null ? item.Element("channel_thread").Element("id").Value : item.Element("thread").Element("id").Value,
                                             //HasMp4 = item.Element("video").Element("option_flag_economy_mp4").Value,
                                             //HasMP4m = item.Element("video").Element("option_flag_middle_video").Value,
                                             HasMp4 = item.Element("video").Element("option_flag_economy_mp4").Value == "1" || item.Element("video").Element("option_flag_middle_video").Value == "1" ? true : false,
                                             LinkList = Function.ExtractLink(item.Element("video").Element("description").Value, true),
                                             Deleted = byte.Parse(item.Element("video").Element("deleted").Value),
                                             //LastComment = item.Element("thread").Element("summary").Value,
                                             CommunityId = item.Element("thread").Element("community_id") != null && uint.TryParse(item.Element("thread").Element("community_id").Value, out community_id) ? community_id : 0,
                                             //FlvUrl = item.Element("video").Element("flv_url") != null ? new Uri(item.Element("video").Element("flv_url").Value, UriKind.Absolute) : null,
                                         };
                            
                            xml = null;
                            sr.Dispose();
                        }

                        if (cache && result != null && !App.ViewModel.Cache.ContainsKey(url.GetHashCode()))
                            App.ViewModel.Cache.Add(url.GetHashCode(), result);
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

        public async static Task<IEnumerable<RankingResults>> ReadItemsByFeedAsync(string url)
        {
            IEnumerable<RankingResults> result = null;
            Regex regex = new Regex("http://www.nicovideo.jp/watch/(?<id>.*?)$", RegexOptions.IgnoreCase);
            Regex regex2 = new Regex("第[0-9]+位：(?<title>.*?)$", RegexOptions.IgnoreCase);
            Regex regex3 = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url, UriKind.Absolute));
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                        result = from item in xml.Descendants("item")
                                     select new RankingResults
                                     {
                                         ID = regex.IsMatch(item.Element("link").Value) ? regex.Match(item.Element("link").Value).Groups["id"].Value : null,
                                         Title = regex2.IsMatch(item.Element("title").Value) ? regex2.Match(item.Element("title").Value).Value : null,
                                         Thumbnail = regex3.IsMatch(item.Element("description").Value) ? regex3.Match(item.Element("description").Value).Value : null
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

        public static Participations SeparateParticipation(byte flag)
        {
            Participations participate = Participations.REGULAR;

            if (flag == 0)
                participate = Participations.REGULAR;
            else if (flag == 1)
                participate = Participations.COMMUNITY;

            return participate;
        }

        public async static Task<RankingResults> ReadUploaderInfoAsync(RankingResults video)
        {
            const string videoInfoUrl = "http://api.ce.nicovideo.jp/nicoapi/v1/video.info?v={0}";
            const string userInfoUrl = "http://api.ce.nicovideo.jp/api/v1/user.info?user_id={0}";

            if (!string.IsNullOrEmpty(video.ID))
            {
                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(videoInfoUrl, video.ID), UriKind.Absolute));
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                            var result = from item in xml.Descendants("video")
                                         where item.Parent.Attribute("status").Value == "ok"
                                         select new RankingResults
                                         {
                                             UploaderID = uint.Parse(item.Element("user_id").Value)
                                         };

                            if (result != null)
                                video.UploaderID = result.First().UploaderID;

                            result = null;
                            xml = null;
                            sr.Dispose();
                        }
                    }

                    res.Dispose();
                    res = null;
                    req = null;

                    if (video.UploaderID != null)
                    {
                        req = WebRequest.CreateHttp(new Uri(string.Format(userInfoUrl, video.UploaderID), UriKind.Absolute));
                        req.CookieContainer = App.ViewModel.UserSetting.cc;
                        res = await req.GetResponseAsync() as HttpWebResponse;

                        if (res != null && res.StatusCode == HttpStatusCode.OK)
                        {
                            using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                            {
                                XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                                var result = from item in xml.Descendants("user")
                                             where item.Parent.Attribute("status").Value == "ok"
                                             select new RankingResults
                                             {
                                                 UploaderID = uint.Parse(item.Element("id").Value),
                                                 Uploader = item.Element("nickname").Value,
                                                 UploaderIcon = item.Element("thumbnail_url").Value
                                             };

                                if (result != null && result.Any())
                                {
                                    video.UploaderID = result.First().UploaderID;
                                    video.Uploader = result.First().Uploader;
                                    video.UploaderIcon = result.First().UploaderIcon;
                                }

                                result = null;
                                xml = null;
                                sr.Dispose();
                            }
                        }

                        res.Dispose();
                        res = null;
                        req = null;
                    }
                }
                catch (WebException)
                { }
                catch (Exception e)
                { }
            }

            return video;
        }
        
        public async static Task<RankingResults> ReadGetFlvAsync(RankingResults video, bool as3 = false)
        {
            const string getflv = "http://flapi.nicovideo.jp/api/getflv/";

            if (video != null)
            {
                //getflvにアクセス
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(getflv, UriKind.Absolute));
                req.Method = "POST";
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                req.ContentType = "application/x-www-form-urlencoded";
                using (StreamWriter sw = new StreamWriter(await req.GetRequestStreamAsync()))
                {
                    string data = null;
                    data = !as3 ? string.Format("v={0}", video.ID) : string.Format("v={0}&as3=1", video.ID);//&as3=1
                    //data = data.Replace(".", "%2E");
                    await sw.WriteAsync(data);
                    await sw.FlushAsync();
                    sw.Dispose();
                }

                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        var result = from item in (await sr.ReadToEndAsync()).Split('&')
                                     select new
                                     {
                                         Key = item.Split('=')[0],
                                         Value = item.Split('=')[1]
                                     };

                        foreach (var obj in result)
                        {
                            switch (obj.Key)
                            {
                                case "thread_id":
                                    video.ThreadId = obj.Value;
                                    break;

                                case "ms":
                                    video.MessageServer = WebUtility.UrlDecode(obj.Value);
                                    break;

                                case "l":
                                    video.LengthFromGetFlv = Int32.Parse(obj.Value);
                                    break;

                                case "url":
                                    video.VideoServer = WebUtility.UrlDecode(obj.Value);
                                    break;

                                case "optional_thread_id":
                                    video.OptionalThreadId = obj.Value;
                                    break;
                            }
                        }

                        result = null;
                        sr.Dispose();
                    }
                }
            }

            return video;
        }

        public async static Task<RankingResults> ReadGetFlviPhoneAsync(RankingResults video, PlayableQuality playableQuality)
        {
            const string getflv = "http://flapi.nicovideo.jp/api/getflv";
            const string requestFormat = "v={0}&device={1}&eco={2}&ckey={3}";

            if (video != null)
            {
                try
                {
                    //getflvにアクセス
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(getflv, UriKind.Absolute));
                    req.Method = "POST";
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    req.ContentType = "application/x-www-form-urlencoded";

                    using (StreamWriter sw = new StreamWriter(await req.GetRequestStreamAsync()))
                    {
                        string data = null;
                        playableQuality = PlayableQuality.SelectQuality(playableQuality, App.ViewModel.UserSetting.IsPremium);

                        if (playableQuality.Quality == 0)
                        {
                            //再生不可
                            return null;
                        }

                        if (video.Participation == Participations.CHANNEL)
                            data = string.Format(requestFormat, video.ThreadId, playableQuality.Device, playableQuality.Quality, playableQuality.Ckey);
                        else
                            data = string.Format(requestFormat, video.ID, playableQuality.Device, playableQuality.Quality, playableQuality.Ckey);

                        //data = data.Replace(".", "%2E");
                        await sw.WriteAsync(data);
                        await sw.FlushAsync();
                        sw.Dispose();
                    }

                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            var result = from item in (await sr.ReadToEndAsync()).Split('&')
                                           select new
                                           {
                                               Key = item.Split('=')[0],
                                               Value = item.Split('=')[1]
                                           };
                            
                            foreach (var obj in result)
                            {
                                switch (obj.Key)
                                {
                                    case "thread_id":
                                        video.ThreadId = obj.Value;
                                        break;

                                    case "ms":
                                        video.MessageServer = WebUtility.UrlDecode(obj.Value);
                                        break;

                                    case "l":
                                        video.LengthFromGetFlv = Int32.Parse(obj.Value);
                                        break;

                                    case "url":
                                        video.VideoServer = WebUtility.UrlDecode(obj.Value);
                                        break;

                                    case "optional_thread_id":
                                        video.OptionalThreadId = video.ThreadId;
                                        video.ThreadId = obj.Value;
                                        break;
                                }
                            }

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

            return video;
        }

        public static async Task<PlayableQuality> GetAvailableVideoQualityAsync(RankingResults video)
        {
            IEnumerable<PlayableQuality> result = null;

            //video.playへアクセス
            try
            {
                string community = null;

                if (video.Participation == Participations.REGULAR)
                    community = "default";
                else if (video.Participation == Participations.COMMUNITY || video.Participation == Participations.CHANNEL)
                    community = "community";

                Uri uri = new Uri(string.Format("http://i.nicovideo.jp/v3/video.play?thread={0}&vid={1}&network=wifi&v=3%2E00&sid={2}&provider={3}&device=iphone4", video.ThreadId, video.ID,
                    App.ViewModel.UserSetting.SessionID, community), UriKind.Absolute);
                HttpWebRequest req = WebRequest.CreateHttp(uri);
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                        result = from item in xml.Descendants("nicolive_videoplay_response")
                                     where item.Attribute("status").Value == "ok" && item.Element("getflv").Element("ckey") != null && item.Element("getflv").Element("device") != null
                                     select new PlayableQuality
                                     {
                                         Type = item.Element("type").Value,
                                         Enable_osec = byte.Parse(item.Element("enable_osec").Value),
                                         Enable_osec_access = byte.Parse(item.Element("enable_osec").Attribute("premiumonly").Value),
                                         Enable_low = byte.Parse(item.Element("enable_low").Value),
                                         Enable_low_access = byte.Parse(item.Element("enable_low").Attribute("premiumonly").Value),
                                         Enable_mid = byte.Parse(item.Element("enable_mid").Value),
                                         Enable_mid_access = byte.Parse(item.Element("enable_mid").Attribute("premiumonly").Value),
                                         Enable_org = byte.Parse(item.Element("enable_org").Value),
                                         Enable_org_access = byte.Parse(item.Element("enable_org").Attribute("premiumonly").Value),
                                         Ckey = item.Element("getflv").Element("ckey").Value,
                                         Device = item.Element("getflv").Element("device").Value
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

            return result != null && result.Any() ? result.First() : null;
        }

        public static async Task<RankingResults> GetRealMovieIdFromChannelCommunityWatchPageNumberAsync(RankingResults video)
        {
            const string watchPageUrl = "http://www.nicovideo.jp/watch/{0}";
            string[] segments = null;

            try
            {
                //動画IDを求める
                Uri uri = new Uri(string.Format(watchPageUrl, video.ID), UriKind.Absolute);
                HttpWebRequest req = WebRequest.CreateHttp(uri);
                req.Method = "HEAD";//転送先の動画IDだけ分かればいいので
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.PartialContent)
                {
                    segments = res.ResponseUri.Segments;//転送先の動画IDだけ分かればいいので中身は読まない

                    //    //Uri uri3 = new Uri(string.Format("http://www.nicovideo.jp/watch/{0}", segments[2]), UriKind.Absolute);
                    //    //HttpWebRequest req3 = WebRequest.CreateHttp(uri3);
                    //    //req3.CookieContainer = App.ViewModel.UserSetting.cc;
                    //    //req3.Method = "GET";
                    //    //HttpWebResponse res3 = await req3.GetResponseAsync() as HttpWebResponse;

                    //    //if (res3.StatusCode != HttpStatusCode.OK)
                    //    //    segments = null;
                }

                res.Dispose();
                res = null;
                req = null;

                if (segments != null)
                    video.ID = segments[segments.Length - 1];
            }
            catch (WebException)
            { }
            catch (Exception)
            { }

            return video;
        }

        public static async Task<IEnumerable<RankingResults>> ReadRelatedItemsAsync(RankingResults video)
        {
            const string url = "http://api.ce.nicovideo.jp/nicoapi/v1/video.relation?from=0&limit=10&order=d&sort=v&v={0}";
            IEnumerable<RankingResults> result = null;

            try
            {
                Uri uri = new Uri(string.Format(url, video.ID), UriKind.Absolute);
                HttpWebRequest req = WebRequest.CreateHttp(uri);
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                        result = from item in xml.Descendants("video_info")
                                 where item.Parent.Attribute("status").Value == "ok"
                                 select new RankingResults
                                 {
                                     Title = item.Element("video").Element("title").Value,
                                     Thumbnail = item.Element("video").Element("thumbnail_url").Value,
                                     ID = item.Element("video").Element("id").Value,
                                     Length = TimeSpan.FromSeconds(double.Parse(item.Element("video").Element("length_in_seconds").Value)),
                                     CommentCount = uint.Parse(item.Element("thread").Element("num_res").Value),
                                     ViewCount = uint.Parse(item.Element("video").Element("view_counter").Value),
                                     MylistCount = uint.Parse(item.Element("video").Element("mylist_counter").Value),
                                     UploadTime = item.Element("video").Element("upload_time").Value,
                                     Participation = SeparateParticipation(byte.Parse(item.Element("video").Element("option_flag_community").Value)),
                                     ThreadId = item.Element("thread").Element("id").Value,
                                     Deleted = byte.Parse(item.Element("video").Element("deleted").Value),
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

        public async static Task<IEnumerable<RankingResults>> ReadItemsByRssFeedAsync(string url)
        {
            IEnumerable<RankingResults> items = null, result = null;
            const string regexFormat = "http://www.nicovideo.jp/watch/(?<id>.*?)$";
            const string arrayUrl = "http://i.nicovideo.jp/v3/video.array?v=";
            Regex regex = new Regex(regexFormat, RegexOptions.IgnoreCase);

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url, UriKind.Absolute));
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                        result = from item in xml.Descendants("item")
                                     select new RankingResults
                                     {
                                         Title = item.Element("title").Value,
                                         Description = item.Element("description").Value,
                                         ID = regex.IsMatch(item.Element("link").Value) ? regex.Match(item.Element("link").Value).Groups["id"].Value : null
                                     };

                        xml = null;
                        sr.Dispose();
                    }
                }

                res.Dispose();
                res = null;
                req = null;

                if (result != null && result.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(arrayUrl);

                    foreach (var obj in result)
                        sb.AppendFormat("{0},", obj.ID);

                    sb.Append("&api_version=1%2E20");

                    items = await RankingResults.ReadItemsAsync(sb.ToString(), false);

                    sb = null;
                }

                result = null;
            }
            catch (WebException)
            { }
            catch (Exception)
            { }

            return items;
        }

        //以下はConverterで置き換え予定
        public string UploadTime1
        {
            get
            {
                DateTime time = DateTime.Parse(UploadTime.Replace(' ', '+'));
                return string.Format("{0}/{1:D2}/{2:D2} {3:D2}:{4:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute);
            }
        }

        public string Length1
        {
            get
            {
                TimeSpan length1 = Length;
                return string.Format("{0:D2}:{1:D2}", length1.Minutes, length1.Seconds);
            }
        }

        public string ViewCount1
        {
            get
            {
                return AppResources.ViewCount + ": " + string.Format("{0:#,0}", ViewCount);
            }
        }

        public string CommentCount1
        {
            get
            {
                return AppResources.CommentCount + ": " + string.Format("{0:#,0}", CommentCount);
            }
        }

        public string MylistCount1
        {
            get
            {
                return AppResources.MylistCount + ": " + string.Format("{0:#,0}", MylistCount);
            }
        }

        //public Visibility SWFVisibility
        //{
        //    get
        //    {
        //        Visibility visibility = Windows.UI.Xaml.Visibility.Collapsed;

        //        //if (MovieType == "swf")
        //        //    visibility = Windows.UI.Xaml.Visibility.Visible;//再生不可であることを表示する

        //        return visibility;
        //    }
        //}

        public string ID2
        {
            get
            {
                return "ID: " + ID;
            }
        }

        public string ViewCount2
        {
            get
            {
                return AppResources.ViewCount + ": " + string.Format("{0:#,0}", ViewCount);
            }
        }

        public string CommentCount2
        {
            get
            {
                return AppResources.CommentCount + ": " + string.Format("{0:#,0}", CommentCount);
            }
        }

        public string MylistCount2
        {
            get
            {
                return AppResources.MylistCount + ": " + string.Format("{0:#,0}", MylistCount);
            }
        }

        public string Length2
        {
            get
            {
                TimeSpan length1 = Length;
                return AppResources.VideoLength + ": " + string.Format("{0:D2}:{1:D2}:{2:D2}", length1.Hours, length1.Minutes, length1.Seconds);
            }
        }

        public string UploadTime2
        {
            get
            {
                DateTime time = DateTime.Parse(UploadTime.Replace(' ', '+'));
                return AppResources.PostedTime + ": " + string.Format("{0}/{1:D2}/{2:D2} {3:D2}:{4:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute);
            }
        }

        public string Description2
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
    }
}