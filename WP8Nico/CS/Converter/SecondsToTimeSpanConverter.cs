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
    public class SecondsToTimeSpanConverter : IValueConverter
    {
#if WINDOWS_PHONE
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            string str = null;
            double p;

            if (double.TryParse(value.ToString(), out p))
            {
                try
                {
                    TimeSpan time = TimeSpan.FromSeconds(p);
                    str = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
                }
                catch (Exception)
                { }
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
