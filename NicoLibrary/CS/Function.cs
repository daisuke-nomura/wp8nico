using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NicoLibrary.nomula
{
    public class Function
    {
        public static string RemoveHTMLTag(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                Regex regex = new Regex("<.*?>", RegexOptions.Singleline);
                description = regex.Replace(description, string.Empty);
                regex = null;
            }

            return description;
        }

        public static IList<string> ExtractLink(string str, bool youtube = false)
        {
            IList<string> result = null;

            if (str.Contains("sm") || str.Contains("nm") || str.Contains("so") || str.Contains("mylist/"))
            {
                if (result == null)
                    result = new List<string>();

                MatchCollection mc = Regex.Matches(str, @"sm(\d+)");

                foreach (Match m in mc)
                    result.Add(m.Value);


                mc = Regex.Matches(str, @"nm(\d+)");

                foreach (Match m in mc)
                    result.Add(m.Value);


                mc = Regex.Matches(str, @"so(\d+)");

                foreach (Match m in mc)
                    result.Add(m.Value);


                mc = Regex.Matches(str, @"mylist/(\d+)");

                foreach (Match m in mc)
                    result.Add(m.Value);

                mc = null;
            }

            if (youtube && str.Contains("www.youtube.com"))
            {
                if (result == null)
                    result = new List<string>();

                MatchCollection mc = Regex.Matches(str, @"www.youtube.com\/watch\?([a-zA-Z0-9_=-]+)");

                foreach (Match m in mc)
                    result.Add(m.Value);

                mc = null;
            }

            return result;
        }
    }
}
