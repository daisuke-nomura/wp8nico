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
    class ViewCountToRankingResultsViewCount : IValueConverter
    {
#if WINDOWS_PHONE
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            string str = null;
            uint count;

            if (uint.TryParse(value.ToString(), out count))
            {
#if WINDOWS_PHONE
                str = string.Format("{0}: {1:#,0}", AppResources.ViewCount, count);
#else
                str = string.Format("再生: {0:#,0}", count);
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
