
namespace NicoLibrary.nomula
{
    public class PlayableQuality
    {
        public PlayableQuality()
        {
            ErrorOrg = false;
            ErrorLow = false;
            ErrorMid = false;
            ErrorOrg = false;
        }

        public enum QualityType
        {
            Enable_org,
            Enable_mid,
            Enable_low,
            Enable_osec
        }

        public string Type { get; set; }
        public byte Enable_osec { get; set; }
        public byte Enable_osec_access { get; set; }
        public byte Enable_low { get; set; }
        public byte Enable_low_access { get; set; }
        public byte Enable_mid { get; set; }
        public byte Enable_mid_access { get; set; }
        public byte Enable_org { get; set; }
        public byte Enable_org_access { get; set; }
        //public string Status { get; set; }

        public short Quality { get; set; }
        public string Playing { get; set; }
        public bool ErrorOrg { get; set; }
        public bool ErrorMid { get; set; }
        public bool ErrorLow { get; set; }
        public bool ErrorOsec { get; set; }

        public string Ckey { get; set; }
        public string Device { get; set; }


        public static PlayableQuality SelectQuality(PlayableQuality playableQuality, bool isPremium = false)
        {
            bool hquality = true;//SetUserSetting.GetLowQualityConfig();

            if (playableQuality.Enable_org == 0)
                playableQuality.ErrorOrg = true;
            if (playableQuality.Enable_mid == 0)
                playableQuality.ErrorMid = true;
            if (playableQuality.Enable_low == 0)
                playableQuality.ErrorLow = true;

            if (hquality || !isPremium)
            {
                if (playableQuality.Enable_org_access == 0)
                    playableQuality.ErrorOrg = true;
                if (playableQuality.Enable_mid_access == 0)
                    playableQuality.ErrorMid = true;
                if (playableQuality.Enable_low_access == 0)
                    playableQuality.ErrorLow = true;
            }

            if (!playableQuality.ErrorOrg)
            {
                playableQuality.Quality = playableQuality.Enable_org;
                playableQuality.Playing = PlayableQuality.QualityType.Enable_org.ToString();
            }
            else if (!playableQuality.ErrorMid)
            {
                playableQuality.Quality = playableQuality.Enable_mid;
                playableQuality.Playing = PlayableQuality.QualityType.Enable_mid.ToString();
            }
            else
            {
                playableQuality.Quality = playableQuality.Enable_low;
                playableQuality.Playing = PlayableQuality.QualityType.Enable_low.ToString();
            }

            return playableQuality;
        }
    }
}
