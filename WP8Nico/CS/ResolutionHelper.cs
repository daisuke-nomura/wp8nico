using System;

namespace WP8Nico.nomula
{
    public static class ResolutionHelper
    {
        public enum Resolutions 
        {
            WVGA,
            WXGA,
            HD720p
        };

        public static double ActualWidth
        {
            get
            {
                return App.Current.Host.Content.ActualHeight;//コメント衝突計算時のみに使う　プレイヤーがPortraitなので縦横逆になる
            }
        }

        public static double ActualHeight
        {
            get
            {
                return App.Current.Host.Content.ActualWidth;//コメント衝突計算時のみに使う　プレイヤーがPortraitなので縦横逆になる
            }
        }

        private static bool IsWvga
        {
            get
            {
                return App.Current.Host.Content.ScaleFactor == 100;
            }
        }

        private static bool IsWxga
        {
            get
            {
                return App.Current.Host.Content.ScaleFactor == 160;
            }
        }

        private static bool Is720p
        {
            get
            {
                return App.Current.Host.Content.ScaleFactor == 150;
            }
        }

        public static Resolutions CurrentResolution
        {
            get
            {
                if (IsWvga) return Resolutions.WVGA;
                else if (IsWxga) return Resolutions.WXGA;
                else if (Is720p) return Resolutions.HD720p;
                else
                    throw new InvalidOperationException("Unknown resolution");
            }
        }
    }
}
