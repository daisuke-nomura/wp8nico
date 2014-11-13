using System;
using System.Text.RegularExpressions;
using System.Windows.Navigation;
using NicoLibrary.nomula;

namespace WP8Nico.nomula
{
    class WP8NicoURIMapper : UriMapperBase
    {
        string str = null;

        public override Uri MapUri(Uri uri)
        {
            str = System.Net.HttpUtility.UrlDecode(uri.ToString());

            if (str.Contains("wp8nico:") || str.Contains("id"))
            {
                Match m = Regex.Match(str, @"\d+.$|sm(\d+)|nm(\d+)|so(\d+)");

                if (m.Success)
                {
                    App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.ID, Parameter = m.Value });
                    uri = new Uri("/Player.xaml", UriKind.Relative);
                }
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }
}
