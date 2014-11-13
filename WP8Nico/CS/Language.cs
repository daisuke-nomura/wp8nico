
namespace WP8Nico.nomula
{
    public class Language
    {
        public enum Languages : byte
        {
            English = 0,
            Japanese = 1,
        }

        public static byte ReadLanguage()
        {
            return LocalSetting.LanguageSetting;
        }

        public static void SaveLanguage(byte index)
        {
            LocalSetting.LanguageSetting = index;
        }
    }
}
