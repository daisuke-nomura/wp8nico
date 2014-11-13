using System.Collections.Generic;

namespace NicoLibrary.nomula
{
    public class Filter
    {
        private static string[] ngWords = new string[] { "R-18", "Ｒ－１８", "R-１８", "エロ", "エッチ", "おっぱい" };

        public static bool FilterVideo(string title, IList<string> tagList)
        {
            bool res = true;

            if (!string.IsNullOrEmpty(title))
            {
                for (int i = 0; i < ngWords.Length; i++)
                {
                    if (title.Contains(ngWords[i]))
                        res = false;
                }
            }

            if (res && tagList != null)
            {
                for (int i = 0; i < ngWords.Length; i++)
                {
                    if (tagList.Contains(ngWords[i]))
                        res = false;
                }
            }

            return res;
        }
    }
}
