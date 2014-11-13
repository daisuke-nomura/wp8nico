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
    class UploadTimeToRankingResults2UploadTime : IValueConverter
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
#if WINDOWS_PHONE
                str = string.Format("{0}: {1}/{2:D2}/{3:D2} {4:D2}:{5:D2}", AppResources.PostedTime, time.Year, time.Month, time.Day, time.Hour, time.Minute);
#else
                str = string.Format("投稿日時: {0}/{1:D2}/{2:D2} {3:D2}:{4:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute);
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
