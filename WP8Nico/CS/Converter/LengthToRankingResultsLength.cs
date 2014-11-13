using System;
#if WINDOWS_PHONE
using System.Windows.Data;
#else
using Windows.UI.Xaml.Data;
#endif

#if WINDOWS_PHONE
namespace WP8Nico.nomula
#else
namespace Nico.nomula
#endif
{
    class LengthToRankingResultsLength : IValueConverter
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
                str = string.Format("{0:D2}:{1:D2}", length.Minutes, length.Seconds);
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
