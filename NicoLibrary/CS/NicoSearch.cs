using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NicoLibrary.nomula
{
    public class NicoSearch
    {
        public static readonly string nicosearchUrl = "http://nicotools.com/nicosearch/mysqli.php?p&inp=";

        public async static Task<IEnumerable<string>> GetSuggestWordsAsync(string str)
        {
            IEnumerable<string> result = null;

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Concat(nicosearchUrl, str), UriKind.Absolute));
                HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                    {
                        result = from item in JArray.Parse(await sr.ReadToEndAsync()) select item.ToString();
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
    }
}
