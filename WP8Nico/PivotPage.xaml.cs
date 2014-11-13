using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NicoLibrary.nomula;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WP8Nico.nomula.Resources;

namespace WP8Nico.nomula
{
    public partial class PivotPage : PhoneApplicationPage
    {
        string inputText = null;
        ObservableCollection<string> suggest = null;
        ObservableCollection<string> searched = null;

        // コンストラクター
        public PivotPage()
        {
            InitializeComponent();

            // LongListSelector コントロールのデータ コンテキストをサンプル データに設定します
            DataContext = App.ViewModel;

            // ApplicationBar をローカライズするためのサンプル コード
            BuildLocalizedApplicationBar();

            favPage.DataContext = App.ViewModel.UserSetting;
            //nicorepoPage.DataContext = App.ViewModel;
        }

        // ローカライズされた ApplicationBar を作成するためのサンプル コード
        private void BuildLocalizedApplicationBar()
        {
            // ページの ApplicationBar を ApplicationBar の新しいインスタンスに設定します。
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBar.Opacity = 0.6;

            // AppResources のローカライズされた文字列で、新しいメニュー項目を作成します。
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.Setting);
            appBarMenuItem.Click += ApplicationBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(appBarMenuItem);

            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.About);
            appBarMenuItem2.Click += ApplicationBarMenuItem_Click_1;
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
        }

        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            bool? login = null;

            if (string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID))//未ログイン
            {
                favPage.DataContext = App.ViewModel.UserSetting;
                progressBar1.Visibility = Visibility.Visible;

                if ((login = await UserSetting.LightLoginAsync()) == false)//ログイン失敗
                {
                    MessageBox.Show(AppResources.Login, AppResources.LoginFalse, MessageBoxButton.OK);
                }
                else if (login == null)//アカウント設定なし
                {
                    MessageBoxResult chk = MessageBox.Show(AppResources.AskLogin, AppResources.Login, MessageBoxButton.OKCancel);

                    if (chk == MessageBoxResult.OK)
                        NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
                }

                progressBar1.Visibility = Visibility.Collapsed;
            }
            if (!string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID) && App.ViewModel.UserSetting.Mylist != null && App.ViewModel.UserSetting.Mylist.Count == 0)
            {
                progressBar2.Visibility = Visibility.Visible;

                var items = await Mylist.ReadMylistListAsync();

                if (items != null)
                {
                    foreach (var obj in items)
                        App.ViewModel.UserSetting.Mylist.Add(obj);
                }

                progressBar2.Visibility = Visibility.Collapsed;

                //progressBar5.Visibility = Visibility.Visible;
                //ObservableCollection<Nicorepo> nicorepo = await Nicorepo.ReadItems();
                //NicorepoList.ItemsSource = nicorepo;
                //progressBar5.Visibility = Visibility.Collapsed;

                //WP8Tile.SetNicorepoTile(nicorepo);
            }
        }

        private async void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = App.ViewModel;

            // ViewModel Items のデータを読み込みます
            if (!App.ViewModel.IsDataLoaded)
                App.ViewModel.LoadData();

            if (rankingList.ItemsSource == null || rankingList2.ItemsSource == null)
            {
                //カテゴリ取得
                progressBar3.Visibility = Visibility.Visible;
                rankingList.ItemsSource = await Category.ReadCategoryAsync();
                rankingList2.ItemsSource = Category.ReadRss();
                progressBar3.Visibility = Visibility.Collapsed;
            }

            //検索履歴取得
            searched = new ObservableCollection<string>();
            SearchList.ItemsSource = searched;

            var items = SearchWord.ReadData();

            if (items != null)
            {
                foreach (var obj in items)
                    searched.Add(obj);
            }

            if (suggest == null)
            {
                suggest = new ObservableCollection<string>();
                SuggestList.ItemsSource = suggest;
            }

            //ShellTile更新
            //var items = await Category.ReadItems("all");

            //if (items != null && items.Length > 0)
            //{
            //    for (int i = 0; i < 1; i++)
            //    {
            //        StandardTileData std = new StandardTileData
            //        {
            //            BackContent = items[i].Title,
            //            BackBackgroundImage = new Uri(items[i].Thumbnail, UriKind.Absolute),
            //            Count = 1
            //        };

            //        ShellTile.Create(new Uri("/PivotPage.xaml", UriKind.Relative), std);
            //    }
            //}
        }

        //検索結果取得
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //    SearchBox.Text = inputText;
        }

        private async void SearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                rankingList.Focus();//フォーカスを移す

                //テキストを取り出す
                if (SearchBox.Text != inputText)
                    inputText = SearchBox.Text;

                if (LocalSetting.SuggestSetting)//サジェストへ
                {
                    if (progressBar4.Visibility == Visibility.Collapsed)
                        progressBar4.Visibility = Visibility.Visible;

                    if (suggest.Count > 0)
                        suggest.Clear();

                    var items = await NicoSearch.GetSuggestWordsAsync(inputText);

                    foreach (string obj in items)
                        suggest.Add(obj);

                    progressBar4.Visibility = Visibility.Collapsed;
                }
            }
            else if ((e.Key == System.Windows.Input.Key.Back || e.Key == System.Windows.Input.Key.Delete) && SearchBox.Text == string.Empty)
            {
                if (suggest.Count > 0)
                    suggest.Clear();
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
        }

        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private async void Search_ActionIconTapped(object sender, EventArgs e)
        {
            string item = SearchBox.Text;

            if (!string.IsNullOrEmpty(item))
            {
                bool? login = null;

                if (string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID))//未ログイン
                {
                    if ((login = await UserSetting.LightLoginAsync()) == false)//ログイン失敗
                    {
                        MessageBox.Show(AppResources.Login, AppResources.LoginFalse, MessageBoxButton.OK);
                        return;
                    }
                    else if (login == null)//アカウント設定なし
                    {
                        MessageBoxResult chk = MessageBox.Show(AppResources.AskLogin, AppResources.Login, MessageBoxButton.OKCancel);

                        if (chk == MessageBoxResult.OK)
                            NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));

                        return;
                    }
                }

                SearchWord.AddData(item);

                if (Regex.IsMatch(item, @"sm(\d+)$") || Regex.IsMatch(item, @"nm(\d+)$") || Regex.IsMatch(item, @"so(\d+)$"))
                {
                    App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.ID, Parameter = item });
                    NavigationService.Navigate(new Uri("/Player.xaml", UriKind.Relative));
                }
                else
                {
                    App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Tag, Parameter = new Tag() { Title = item, Location = null } });
                    NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));
                }
            }
        }

        private void rankingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rankingList.SelectedIndex == -1)
                return;

            App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Category, Parameter = rankingList.SelectedItem });
            NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));

            rankingList.SelectedIndex = -1;
        }

        private void MylistList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MylistList.SelectedIndex == -1)
                return;

            if (MylistList.SelectedIndex == 0 && (MylistList.SelectedItem as Mylist).ID == 0)//視聴履歴
            {
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Watched, Parameter = null });
            }
            else if (MylistList.SelectedIndex == 1 && (MylistList.SelectedItem as Mylist).ID == 1)//視聴履歴(アカウント)
            {
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.ShareWatched, Parameter = null });
            }
            else if (MylistList.SelectedIndex == 2 && (MylistList.SelectedItem as Mylist).ID == 2)//投稿動画
            {
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Uploaded, Parameter = null });
            }
            else//とりあえずマイリスト他
            {
                Mylist ms = MylistList.SelectedItem as Mylist;
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Mylist, Parameter = ms });
            }

            NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));

            MylistList.SelectedIndex = -1;
        }

        private void SearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchList.SelectedIndex == -1)
                return;

            string item = SearchList.SelectedItem as string;
            SearchWord.AddData(item);

            if (Regex.IsMatch(item, @"sm(\d+)$") || Regex.IsMatch(item, @"nm(\d+)$") || Regex.IsMatch(item, @"so(\d+)$"))
            {
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.ID, Parameter = item });
                NavigationService.Navigate(new Uri("/Player.xaml", UriKind.Relative));
            }
            else
            {
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Tag, Parameter = new Tag() { Title = item, Location = null } });
                NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));
            }

            SearchList.SelectedIndex = -1;
        }

        private void SuggestList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestList.SelectedIndex == -1)
                return;

            string item = SuggestList.SelectedItem as string;
            SearchWord.AddData(item);

            if (Regex.IsMatch(item, @"sm(\d+)$") || Regex.IsMatch(item, @"nm(\d+)$") || Regex.IsMatch(item, @"so(\d+)$"))
            {
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.ID, Parameter = item });
                NavigationService.Navigate(new Uri("/Player.xaml", UriKind.Relative));
            }
            else
            {
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Tag, Parameter = new Tag() { Title = item, Location = null } });
                NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));
            }

            SuggestList.SelectedIndex = -1;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //    if (SearchBox.Text == string.Empty && App.ViewModel.Suggest.Count > 0)
            //        App.ViewModel.Suggest.Clear();
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void rankingList2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rankingList2.SelectedIndex == -1)
                return;

            App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Rss, Parameter = rankingList2.SelectedItem });
            NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));

            rankingList2.SelectedIndex = -1;
        }

        //private void NicorepoList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        //{
        //    if (NicorepoList.SelectedIndex == -1)
        //        return;

        //    var item = NicorepoList.SelectedItem as Nicorepo;
        //    string str = item.VideoUrl.ToString();

        //    Match m = Regex.Match(str, @"\d+.$|sm(\d+)|nm(\d+)|so(\d+)");

        //    if (m.Success)
        //    {
        //        App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.ID, Parameter = m.Value });
        //        NavigationService.Navigate(new Uri("/Player.xaml", UriKind.Relative));
        //    }

        //    NicorepoList.SelectedIndex = -1;
        //}
    }
}