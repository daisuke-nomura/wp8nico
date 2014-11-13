using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WP8Nico.nomula
{
    public partial class Setting : PhoneApplicationPage
    {
        bool logout = false;

        public Setting()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (logout)//ID or パスワードが書き換わったのでログアウトする
                UserSetting.Logout();

            base.OnNavigatingFrom(e);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            tbxUserID.Text = LocalSetting.ID;
            pbxUserPass.Password = LocalSetting.Password;
            //checkBox2.IsChecked = LocalSetting.RoamingWatchedSetting;
            checkBox5.IsChecked = LocalSetting.SuggestSetting;
            checkBox6.IsChecked = LocalSetting.VideoHub;
            checkBox7.IsChecked = LocalSetting.AutoPlaySetting;
            checkBox8.IsChecked = LocalSetting.RepeatSetting;
            checkBox9.IsChecked = LocalSetting.IYAYO184Setting;
            //languagePicker.SelectedIndex = LocalSetting.LanguageSetting;
        }

        private void ShowErrorMessage(string str)
        {
            Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(str);
                });
        }

        private void tbxUserID_LostFocus(object sender, RoutedEventArgs e)
        {
            LocalSetting.ID = tbxUserID.Text;
        }

        private void pbxUserPass_LostFocus(object sender, RoutedEventArgs e)
        {
            LocalSetting.Password = pbxUserPass.Password;
        }

        private void tbxUserID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                checkBox5.Focus();//loginButton.Focus();

            logout = true;
        }

        private void pbxUserPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                checkBox5.Focus(); //loginButton.Focus();

            logout = true;
        }

        //private void checkBox2_Checked(object sender, RoutedEventArgs e)
        //{
        //    LocalSetting.RoamingWatchedSetting = true;
        //}

        //private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    LocalSetting.RoamingWatchedSetting = false;
        //}

        //private async void loginButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID))
        //    {
        //        if (await App.ViewModel.UserSetting.LightLogin())
        //        {
        //            //ログイン成功
        //            //loginButton.Content = StringResources.Logout;
        //        }
        //        else
        //        {
        //            //ログイン失敗
        //        }
        //    }
        //    else
        //    {
        //        //ログアウトした
        //        App.ViewModel.UserSetting.Logout();
        //        //loginButton.Content = StringResources.Login;
        //    }
        //}

        private void checkBox5_Checked(object sender, RoutedEventArgs e)
        {
            LocalSetting.SuggestSetting = true;
        }

        private void checkBox5_Unchecked(object sender, RoutedEventArgs e)
        {
            LocalSetting.SuggestSetting = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           SearchWord.RemoveData();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WatchedMovie.RemoveItems();
        }

        private void tbxUserID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LocalSetting.ID = tbxUserID.Text;
        }

        private void pbxUserPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LocalSetting.Password = pbxUserPass.Password;
        }

        private void checkBox6_Checked(object sender, RoutedEventArgs e)
        {
            LocalSetting.VideoHub = true;
        }

        private void checkBox6_Unchecked(object sender, RoutedEventArgs e)
        {
            LocalSetting.VideoHub = false;
        }

        private void checkBox7_Checked(object sender, RoutedEventArgs e)
        {
            LocalSetting.AutoPlaySetting = true;
        }

        private void checkBox7_Unchecked(object sender, RoutedEventArgs e)
        {
            LocalSetting.AutoPlaySetting = false;
        }

        private void checkBox8_Checked(object sender, RoutedEventArgs e)
        {
            LocalSetting.RepeatSetting = true;
        }

        private void checkBox8_Unchecked(object sender, RoutedEventArgs e)
        {
            LocalSetting.RepeatSetting = false;
        }

        private void checkBox9_Checked(object sender, RoutedEventArgs e)
        {
            LocalSetting.IYAYO184Setting = true;
        }

        private void checkBox9_Unchecked(object sender, RoutedEventArgs e)
        {
            LocalSetting.IYAYO184Setting = false;
        }

        //private void languagePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (languagePicker != null)
        //    {
        //        WP8Nico.nomula.Language.SaveLanguage((byte)languagePicker.SelectedIndex);
        //    }
        //}
    }
}