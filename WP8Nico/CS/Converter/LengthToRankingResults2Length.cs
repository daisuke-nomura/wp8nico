using System;
#if WINDOWS_PHONE
using System.Windows.Data;
using WP8Nico.nomula.Resources;
#else
using Windows.UI.Xaml.Data;
#endif

#if WINDOWS_PHONE
namespace WP8Nico.nomula
#else
namespace Nico.nomula
#endif
{
    class LengthToRankingResults2Length : IValueConverter
    {

#if WINDOWS_PHONE
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            TimeSpan length;
            string str = null;

            if (TimeSpan.TryParse(value.ToString(), out length))
            {
#if WINDOWS_PHONE
                str = string.Format("{0}: {1:D2}:{2:D2}:{3:D2}", AppResources.VideoLength, length.Hours, length.Minutes, length.Seconds);
#else
                str = string.Format("再生時間: {0:D2}:{1:D2}:{2:D2}", length.Hours, length.Minutes, length.Seconds);
#endif
            }

            return str;
        }

#if WINDOWS_PHONE
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#endif
        {
            throw new NotImplementedException();
        }
    }
}
