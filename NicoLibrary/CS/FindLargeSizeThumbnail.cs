using System;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;

namespace Nico.nomula
{
    class FindLargeSizeThumbnail
    {
        static StorageFolder folder = ApplicationData.Current.TemporaryFolder;

        public FindLargeSizeThumbnail()
        {
        }

        public async static Task<string> FindLargeSize(string url)
        {
            //bool result = false;

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format("{0}.L", url)));
                req.Method = "GET";
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    //int c = -1;
                    //byte[] bytes = new byte[1024 * 256];
                    //Stream st = res.GetResponseStream();
                    //StorageFile thumbnail = await folder.CreateFileAsync(string.Format("{0}", videoId), CreationCollisionOption.ReplaceExisting);
                    //IRandomAccessStream stream = await thumbnail.OpenAsync(FileAccessMode.ReadWrite);

                    //using (Stream st2 = stream.GetOutputStreamAt(0).AsStreamForWrite())
                    //{
                    //    while (true)
                    //    {
                    //        c = await st.ReadAsync(bytes, 0, bytes.Length);

                    //        if (c <= 0)
                    //            break;

                    //        await st2.WriteAsync(bytes, 0, c);
                    //    }
                    //}

                    //result = true;

                    url = req.RequestUri.ToString();
                }

                res.Dispose();
                res = null;
                req = null;
            }
            catch (WebException)
            { }
            catch (Exception)
            { }

            return url;
        }
    }
}
