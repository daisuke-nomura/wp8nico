using System.Collections.Generic;

namespace WP8Nico.nomula
{
    public class MainViewModel
    {
        public UserSetting UserSetting { get; set; }
        public Cache Cache { get; set; }
        //public ObservableCollection<Nicorepo> Nicorepo { get; set; }

        public MainViewModel()
        {
            UserSetting = new UserSetting();
            Cache = new Cache();
            //Nicorepo = new ObservableCollection<Nicorepo>();
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public void LoadData()
        {
            this.IsDataLoaded = true;
        }
    }
}