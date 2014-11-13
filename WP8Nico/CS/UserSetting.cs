using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WP8Nico.nomula
{
    /// <summary>
    /// ユーザーのID(メールアドレス)とパスワード保存用クラス
    /// </summary>
    public class UserSetting : Common.BindableBase
    {
        protected const string lightLoginUrl = "https://secure.nicovideo.jp/secure/login?site=nicoiphone";
        protected const string ticketLoginUrl = "http://i.nicovideo.jp/v3/login?ticket={0}";
        protected const string deepLoginUrl = "https://secure.nicovideo.jp/secure/login?site=niconico";
        const string unknownName = "unknown user";

        public string SessionID { get; set; }
        public CookieContainer cc { get; set; }
        public bool IsPremium { get; set; }
        public bool Logined { get; set; }
        public bool pcLogined { get; set; }
        public uint UserNumber { get; set; }
        public PCKeys PCLoginStatus { get; set; }
        public SPKeys SPLoginStatus { get; set; }
        public ObservableCollection<Mylist> Mylist { get; set; }

        private string _nickname;
        public string Nickname
        {
            get { return _nickname; }
            set { SetProperty(ref _nickname, value); }
        }

        private string _description;
        public string Description
        {
            get { return WebUtility.HtmlDecode(_description); }
            set { SetProperty(ref _description, value); }
        }

        private string _thumbnail;
        public string Thumbnail
        {
            get { return _thumbnail; }
            set { SetProperty(ref _thumbnail, value); }
        }

        public enum PCKeys : byte
        {
            NotLogin,
            LoginAttempting,
            Logined,
        }

        public enum SPKeys : byte
        {
            NotLogin,
            LoginAttempting,
            Logined,
        }

        public enum MetroKeys : byte
        {
            NotLogin,
            LoginAttempting,
            Logined,
        }

        public UserSetting()
        {
            SessionID = null;
            cc = new CookieContainer();
            IsPremium = false;
            pcLogined = false;
            Mylist = new ObservableCollection<Mylist>();
            Nickname = unknownName;
        }

        public async static Task<bool?> LightLoginAsync()
        {
            bool? login = null;//ログイン成否
            string ticket = null;//ログインチケット

            if (!string.IsNullOrEmpty(LocalSetting.ID) && !string.IsNullOrEmpty(LocalSetting.Password))
            {
                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(lightLoginUrl, UriKind.Absolute));
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    using (StreamWriter sw = new StreamWriter(await req.GetRequestStreamAsync()))
                    {
                        await sw.WriteAsync(string.Format("mail={0}&password={1}", LocalSetting.ID, LocalSetting.Password));
                        await sw.FlushAsync();
                        sw.Dispose();
                    }

                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;
                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                            var result = from item in xml.Descendants("nicovideo_user_response")
                                         where item.Attribute("status").Value == "ok"
                                         select item.Element("ticket").Value;

                            if (result != null && result.Any())
                                ticket = result.First();

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

                if (!string.IsNullOrEmpty(ticket))
                {
                    try
                    {
                        HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(ticketLoginUrl, ticket), UriKind.Absolute));
                        req.CookieContainer = App.ViewModel.UserSetting.cc;
                        HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                        if (res != null && res.StatusCode == HttpStatusCode.OK)
                        {
                            using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                            {
                                XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                                var result = from item in xml.Descendants("nicovideo_user_response")
                                             where item.Attribute("status").Value == "ok"
                                             select new UserSetting
                                             {
                                                 SessionID = item.Element("user").Element("session_id").Value,
                                                 IsPremium = item.Element("user").Element("premium").Value == "1" ? App.ViewModel.UserSetting.IsPremium = true : App.ViewModel.UserSetting.IsPremium = false,
                                                 UserNumber = uint.Parse(item.Element("user").Element("id").Value),
                                                 Nickname = item.Element("user").Element("nickname").Value,
                                                 Description = item.Element("user").Element("description").Value,
                                                 Thumbnail = item.Element("user").Element("thumbnail_url").Value
                                             };

                                if (result != null && result.Any())
                                {
                                    App.ViewModel.UserSetting.SessionID = result.First().SessionID;
                                    App.ViewModel.UserSetting.UserNumber = result.First().UserNumber;
                                    App.ViewModel.UserSetting.Nickname = result.First().Nickname;
                                    App.ViewModel.UserSetting.Description = result.First().Description;
                                    App.ViewModel.UserSetting.Thumbnail = result.First().Thumbnail;

                                    login = true;
                                    App.ViewModel.UserSetting.Logined = true;
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
                    catch (WebException)
                    { }
                    catch (Exception)
                    { }
                }
                else
                    login = false;
            }

            return login;
        }

        public async static Task<bool?> DeepLoginAsync()
        {
            const string iphoneUA = "Mozilla/5.0 (iPhone; CPU iPhone OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A403 Safari/8536.25";
            bool? login = null;//ログイン成否

            if (!string.IsNullOrEmpty(LocalSetting.ID) && !string.IsNullOrEmpty(LocalSetting.Password))
            {
                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(deepLoginUrl, UriKind.Absolute));
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    req.UserAgent = iphoneUA;//iPhoneに化ける
                    req.AllowAutoRedirect = false;
                    using (StreamWriter sw = new StreamWriter(await req.GetRequestStreamAsync()))
                    {
                        await sw.WriteAsync(string.Format("mail={0}&password={1}", LocalSetting.ID, LocalSetting.Password));
                        await sw.FlushAsync();
                        sw.Dispose();
                    }

                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;
                    if (res != null && (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Found))
                    {
                        if ((res.StatusCode == HttpStatusCode.OK && res.ResponseUri.Host != "secure.nicovideo.jp") || (res.StatusCode == HttpStatusCode.Found && res.ResponseUri.Host == "secure.nicovideo.jp"))
                        {
                            login = true;
                            App.ViewModel.UserSetting.pcLogined = true;
                        }
                        else
                            login = false;
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

            return login;
        }

        public static void Logout()
        {
            App.ViewModel.UserSetting = new UserSetting();
            App.ViewModel.Cache.Reset();
        }
    }
}
