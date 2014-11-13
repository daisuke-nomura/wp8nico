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
    class UploadTimeToRankingResultsUploadTime : IValueConverter
    {
#if WINDOWS_PHONE
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            DateTime time;
            string str = null;

            if (DateTime.TryParse(value.ToString().Replace(' ', '+'), out time))
            {
                str = string.Format("{0}/{1:D2}/{2:D2} {3:D2}:{4:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute);
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
