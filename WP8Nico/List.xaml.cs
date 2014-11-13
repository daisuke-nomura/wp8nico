using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using NicoLibrary.nomula;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using WP8Nico.nomula.Resources;

namespace WP8Nico.nomula
{
    public partial class List : PhoneApplicationPage
    {
        ObservableCollection<RankingResults> list = null;
        IEnumerable<RankingResults> res = null;
        NavigationParameter param = null;
        ushort count = 0;
        VisualStateGroup vgroup = new VisualStateGroup();
        const string titleFormat = "{0}: {1}";

        // コンストラクター
        public List()
        {
            InitializeComponent();
        }

        // ローカライズされた ApplicationBar を作成するためのサンプル コード
        private void BuildLocalizedApplicationBar(NavigationParameter.Types type)
        {
            // ページの ApplicationBar を ApplicationBar の新しいインスタンスに設定します。
            ApplicationBar = new ApplicationBar();

            if (param.Type == NavigationParameter.Types.Category)
            {
                int searchSetting = Category.ReadSearchSetting();//現在の並び替えの設定と表示する並び替えの名前が一致している場合、IsEnabled = falseにする
                if (searchSetting >= 40) searchSetting -= 6;
                if (searchSetting >= 30) searchSetting -= 6;
                if (searchSetting >= 20) searchSetting -= 6;
                if (searchSetting >= 10) searchSetting -= 6;

                for (int i = 0, c = Category.category.Length; i < c; i++)//検索並び替えを全表示
                {
                    ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem();
                    menuItem.Text = Category.category[i];

                    if (i == searchSetting)
                        menuItem.IsEnabled = false;
                    else
                        menuItem.Click += menuItem_Click_3;

                    ApplicationBar.MenuItems.Add(menuItem);
                }
            }
            else if (param.Type == NavigationParameter.Types.Tag)
            {
                int searchSetting = Search.ReadSearchSetting();//現在の並び替えの設定と表示する並び替えの名前が一致している場合、IsEnabled = falseにする
                if (searchSetting >= 20) searchSetting -= 8;

                for (int i = 0, c = Search.search.Length; i < c; i++)//検索並び替えを全表示
                {
                    ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem();
                    menuItem.Text = Search.search[i];

                    if (i == searchSetting)
                        menuItem.IsEnabled = false;
                    else
                        menuItem.Click += menuItem_Click;

                    ApplicationBar.MenuItems.Add(menuItem);
                }
            }

            ApplicationBar.Opacity = 0.9;
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            int i = ApplicationBar.MenuItems.IndexOf((sender as ApplicationBarMenuItem));

            if (i > -1)
            {
                Search.SaveSearchSetting(i);

                //progressBar.Margin = new Thickness(63, 268, 0, 0);
                ((ScrollViewer)ResultList.Parent).ScrollToVerticalOffset(0);
                vgroup.CurrentStateChanging -= (s, e2) => { };
                list = null;
                count = 0;
                LayoutRoot_Loaded(null, null);
            }
        }

        void menuItem_Click_3(object sender, EventArgs e)
        {
            int i = ApplicationBar.MenuItems.IndexOf((sender as ApplicationBarMenuItem));

            if (i > -1)
            {
                Category.SaveSearchSetting(i);

                //progressBar.Margin = new Thickness(63, 268, 0, 0);
                ((ScrollViewer)ResultList.Parent).ScrollToVerticalOffset(0);
                vgroup.CurrentStateChanging -= (s, e2) => { };
                list = null;
                count = 0;
                LayoutRoot_Loaded(null, null);
            }
        }

        private async void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            bool? login = null;

            if (string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID))//未ログイン
            {
                if ((login = await UserSetting.LightLoginAsync()) == false)//ログイン失敗
                {
                    MessageBox.Show(AppResources.Login, AppResources.LoginFalse, MessageBoxButton.OK);

                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();

                    return;
                }
                else if (login == null)//アカウント設定なし
                {
                    MessageBoxResult chk = MessageBox.Show(AppResources.AskLogin, AppResources.Login, MessageBoxButton.OKCancel);

                    if (chk == MessageBoxResult.OK)
                        NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
                    else
                    {
                        if (NavigationService.CanGoBack)
                            NavigationService.GoBack();
                    }
                    return;
                }
            }

            string str = null;
            IDictionary<object, object> pageState = null;
            object navigationParameter = null;

            if (App.pageState.Count > 0)
            {
                navigationParameter = App.pageState.Peek();
                param = App.pageState.Peek() as NavigationParameter;
            }

            if (list == null || list.Count == 0)
            {
                list = new ObservableCollection<RankingResults>();
                ResultList.ItemsSource = list;

                if (progressBar.Visibility == Visibility.Collapsed)
                    progressBar.Visibility = Visibility.Visible;

                if (navigationParameter != null && navigationParameter.GetType() == typeof(NavigationParameter))
                {
                    Task t = null;

                    if (param.Type == NavigationParameter.Types.Category)
                    {
                        var category = param.Parameter as Category;
                        str = string.Format(titleFormat, AppResources.Ranking, category.Tag);

                        t = new Task(async () =>
                        {
                            //カテゴリの場合はApplicationBarを表示する
                            BuildLocalizedApplicationBar(param.Type);

                            if (pageState == null)
                            {
                                res = await Category.ReadItemsAsync(category.Key);
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    obj.RankingNumber = ++count;
                                    list.Add(obj);
                                }
                            }

                            progressBar.Visibility = Visibility.Collapsed;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (param.Type == NavigationParameter.Types.Tag)
                    {
                        var item = param.Parameter as Tag;
                        str = string.Format(titleFormat, AppResources.Search2, item.Title);

                        t = new Task(async () =>
                        {
                            //検索の場合はApplicationBarを表示する
                            BuildLocalizedApplicationBar(param.Type);

                            if (pageState == null)
                            {
                                res = await Search.ReadItemsAsync(item.Title, Search.Type.NoSetting);
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    list.Add(obj);
                                    count++;
                                }
                            }

                            progressBar.Visibility = Visibility.Collapsed;
                            //searchSetting.ItemsSource = Search.search;

                            //int s = Search.ReadSearchSetting();
                            //if (s >= 20)
                            //    s -= 8;
                            //searchSetting.SelectedIndex = s;
                            //searchSetting.Visibility = Visibility.Visible;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (param.Type == NavigationParameter.Types.Mylist)
                    {
                        var ms = param.Parameter as Mylist;
                        str = string.Format(titleFormat, AppResources.Mylist2, ms.Name);

                        t = new Task(async () =>
                        {
                            if (pageState == null)
                            {
                                res = await Mylist.ReadItemsAsync(ms.ID);
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    list.Add(obj);
                                    count++;
                                }
                            }

                            progressBar.Visibility = Visibility.Collapsed;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (param.Type == NavigationParameter.Types.Watched)
                    {
                        str = string.Format(AppResources.Watched2);

                        t = new Task(async () =>
                        {
                            if (pageState == null)
                            {
                                res = await WatchedMovie.ReadItemsAsync();
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    list.Add(obj);
                                    count++;
                                }
                            }

                            progressBar.Visibility = Visibility.Collapsed;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (param.Type == NavigationParameter.Types.ShareWatched)
                    {
                        str = string.Format(AppResources.ShareWatched2);

                        t = new Task(async () =>
                        {
                            if (pageState == null)
                            {
                                res = await SynchronizeWatchedMovie.ReadItemsAsync();
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    list.Add(obj);
                                    count++;
                                }
                            }

                            //group.Items.Add(new NicoDataItem(readmore);
                            progressBar.Visibility = Visibility.Collapsed;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    //else if (param.Type == NavigationParameter.Types.BigCategoryFeed)//大カテゴリのランキング
                    //{
                    //    Category2 cat = param.Parameter as Category2;
                    //    str = string.Format(titleFormat, AppResources.Ranking, cat.GroupTitle);

                    //    t = new Task(async () =>
                    //    {
                    //        if (pageState == null)
                    //            res.AddRange(await Category.ReadBigItems(cat.Group));
                    //        else
                    //            res = pageState["items"] as List<RankingResults>;


                    //        foreach (var obj in res)
                    //        {
                    //            list.Add(obj);
                    //            count++;
                    //        }

                    //        //group.Items.Add(new NicoDataItem(readmore);
                    //        progressBar.Visibility = Visibility.Collapsed;
                    //    });

                    //    t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    //}
                    else if (param.Type == NavigationParameter.Types.PublicMylist)
                    {
                        var ms = param.Parameter as Mylist;
                        str = string.Format(titleFormat, AppResources.PublicMylist, ms.ID);

                        t = new Task(async () =>
                        {
                            if (pageState == null)
                            {
                                res = await Mylist.ReadPublicMylistItemsAsync(ms.ID.ToString());
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    list.Add(obj);
                                    count++;
                                }
                            }

                            progressBar.Visibility = Visibility.Collapsed;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (param.Type == NavigationParameter.Types.ID)//動画ID
                    {
                        string id = param.Parameter as string;
                        str = string.Format("ID: {0}", id);

                        t = new Task(async () =>
                        {
                            if (pageState == null)
                            {
                                res = await RankingResults.ReadItemAsync(id);
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    list.Add(obj);
                                    count++;
                                }
                            }

                            progressBar.Visibility = Visibility.Collapsed;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (param.Type == NavigationParameter.Types.Uploaded)
                    {
                        str = string.Format(AppResources.PostedVideo);

                        t = new Task(async () =>
                        {
                            if (pageState == null)
                            {
                                res = await Mylist.ReadUserUploadedItemsAsync();
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    list.Add(obj);
                                    count++;
                                }
                            }

                            progressBar.Visibility = Visibility.Collapsed;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (param.Type == NavigationParameter.Types.RankingResult)//動画情報
                    {
                        var rs = param.Parameter as RankingResults;
                        str = string.Format("ID: {0}", rs.ID);

                        t = new Task(() =>
                            {
                                list.Add(rs);
                                progressBar.Visibility = Visibility.Collapsed;
                            });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (param.Type == NavigationParameter.Types.RankingResults)//複数動画情報
                    {
                        var rs = (param.Parameter as RankingResults[]);

                        if (rs != null && rs.Length > 0)
                        {
                            str = rs[0].ID;

                            foreach (var obj in rs)
                                list.Add(obj);
                        }

                        progressBar.Visibility = Visibility.Collapsed;
                    }
                    else if (param.Type == NavigationParameter.Types.Rss)
                    {
                        var category = param.Parameter as Category;
                        str = string.Format(titleFormat, AppResources.Ranking, category.Tag);

                        t = new Task(async () =>
                        {
                            if (pageState == null)
                            {
                                res = await Category.ReadRssItemsAsync(category.Key);
                            }
                            else
                                res = pageState["items"] as List<RankingResults>;

                            if (res != null)
                            {
                                foreach (var obj in res)
                                {
                                    obj.RankingNumber = ++count;
                                    list.Add(obj);
                                }
                            }

                            progressBar.Visibility = Visibility.Collapsed;
                        });

                        t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    //else if (param.Type == NavigationParameter.Types.WatchedNsen)
                    //{
                    //}
                    else
                    {
                        //ありえないパラメータ
                        throw new Exception(AppResources.UndefinedMovie);
                    }
                }

                VideoList.Header = str;
            }


            //視聴履歴・共有視聴履歴でない場合は一番下まで読んだらリロードする
            if (param.Type != NavigationParameter.Types.Watched || param.Type != NavigationParameter.Types.ShareWatched)//視聴履歴かどうかの判断は、8と同等に
            {
                try
                {
                    // ListBox の初めに定義されている ScrollViewerを取り出す 
                    ScrollViewer ListboxScrollViewer = (ScrollViewer)ResultList.Parent;//(ScrollViewer)VisualTreeHelper.GetChild(ResultList, 0);

                    // Visual State はコントロールテンプレートの常に最上位に定義されている 
                    FrameworkElement element = (FrameworkElement)VisualTreeHelper.GetChild(ListboxScrollViewer, 0);
                    // Visual State を取り出しその中から 縦横Compression のVisualStateを取り出す 
                    foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(element))
                    {
                        if (group.Name == "ScrollStates")
                            vgroup = group;
                    }

                    bool load = true;

                    vgroup.CurrentStateChanging += (s, e2) =>
                        {
                            switch (e2.NewState.Name)
                            {
                                case "Scrolling":
                                case "NotScrolling":
                                default:
                                    //System.Diagnostics.Debug.WriteLine(ListboxScrollViewer.ScrollableHeight - ListboxScrollViewer.VerticalOffset);

                                    if (load && ListboxScrollViewer.ScrollableHeight - ListboxScrollViewer.VerticalOffset <= 1000)//縦の残りスクロール量が1000pxを切ったら追加読み込みする
                                    {
                                        res = null;

                                        if (progressBar.Visibility == Visibility.Collapsed)
                                        {
                                            progressBar.Visibility = Visibility.Visible;


                                            if (navigationParameter != null && navigationParameter.GetType() == typeof(NavigationParameter))
                                            {
                                                Task t = null;

                                                if (param.Type == NavigationParameter.Types.Category)
                                                {
                                                    Category category = param.Parameter as Category;

                                                    t = new Task(async () =>
                                                    {
                                                        if (pageState == null)
                                                        {
                                                            res = await Category.ReadItemsAsync(category.Key, count);
                                                        }
                                                        else
                                                            res = pageState["items"] as List<RankingResults>;

                                                        if (res != null)
                                                        {
                                                            foreach (var obj in res)
                                                            {
                                                                obj.RankingNumber = ++count;
                                                                list.Add(obj);
                                                            }
                                                        }
                                                        else
                                                            load = false;

                                                        progressBar.Visibility = Visibility.Collapsed;
                                                    });

                                                    t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                                                }
                                                else if (param.Type == NavigationParameter.Types.Tag)
                                                {
                                                    var item = param.Parameter as Tag;

                                                    t = new Task(async () =>
                                                    {
                                                        if (pageState == null)
                                                        {
                                                            res = await Search.ReadItemsAsync(item.Title, Search.Type.NoSetting, count);
                                                        }
                                                        else
                                                            res = pageState["items"] as List<RankingResults>;

                                                        if (res != null)
                                                        {
                                                            foreach (var obj in res)
                                                            {
                                                                list.Add(obj);
                                                                count++;
                                                            }
                                                        }
                                                        else
                                                            load = false;

                                                        progressBar.Visibility = Visibility.Collapsed;
                                                        //searchSetting.ItemsSource = Search.search;

                                                        //int s = Search.ReadSearchSetting();
                                                        //if (s >= 20)
                                                        //    s -= 8;
                                                        //searchSetting.SelectedIndex = s;
                                                        //searchSetting.Visibility = Visibility.Visible;
                                                    });

                                                    t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                                                }
                                                else if (param.Type == NavigationParameter.Types.Mylist)
                                                {
                                                    var ms = param.Parameter as Mylist;

                                                    t = new Task(async () =>
                                                    {
                                                        if (pageState == null)
                                                        {
                                                            res = await Mylist.ReadItemsAsync(ms.ID, count);
                                                        }
                                                        else
                                                            res = pageState["items"] as List<RankingResults>;

                                                        if (res != null)
                                                        {
                                                            foreach (var obj in res)
                                                            {
                                                                list.Add(obj);
                                                                count++;
                                                            }
                                                        }
                                                        else
                                                            load = false;

                                                        progressBar.Visibility = Visibility.Collapsed;
                                                    });

                                                    t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                                                }
                                                else
                                                {
                                                    progressBar.Visibility = Visibility.Collapsed;
                                                }
                                            }
                                        }
                                    }
                                    break;
                            };
                        };
                }
                catch (Exception)
                { }
            }
        }

        private void ShowErrorMessage(string str)
        {
            Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(str);
                });
        }

        private async void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultList.SelectedIndex == -1)
                return;

            RankingResults video = ResultList.SelectedItem as RankingResults;
            bool? login = null;

            if (video != null && !string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID) || ((login = await UserSetting.LightLoginAsync()) == true))
            {
                if (video.MovieType == RankingResults.MovieTypes.MP4 || video.HasMp4 == true)
                {
                    App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.RankingResult, Parameter = video });
                    NavigationService.Navigate(new Uri("/Player.xaml", UriKind.Relative));
                }
                else if (video.MovieType == RankingResults.MovieTypes.SWF)
                {
                    MessageBox.Show(AppResources.CannotPlaySWF, AppResources.CannotPlaySWFDesc, MessageBoxButton.OK);
                }
                else if (video.MovieType == RankingResults.MovieTypes.FLV)
                {
                    MessageBox.Show(AppResources.CannotPlayFLV, AppResources.CannotPlayFLVDesc, MessageBoxButton.OK);
                }
                else//MP4でもSWFでもFLVでもない
                {
                    MessageBox.Show(AppResources.CannotPlayUnknown, AppResources.CannotPlayUnknownDesc, MessageBoxButton.OK);
                }
            }
            else if (login == false)//ログイン失敗
            {
                MessageBox.Show(AppResources.Login, AppResources.LoginFalse, MessageBoxButton.OK);
            }
            else//アカウント設定なし
            {
                MessageBoxResult chk = MessageBox.Show(AppResources.AskLogin, AppResources.Login, MessageBoxButton.OKCancel);

                if (chk == MessageBoxResult.OK)
                    NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
            }

            ResultList.SelectedIndex = -1;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && App.pageState != null && App.pageState.Count > 0)
                App.pageState.Pop();

            base.OnNavigatingFrom(e);
        }

        private async void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            RankingResults video = (sender as MenuItem).DataContext as RankingResults;

            if (!string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID))
            {
                if (App.ViewModel.UserSetting.Mylist != null && App.ViewModel.UserSetting.Mylist.Count == 0)
                {
                    foreach (var obj in await Mylist.ReadMylistListAsync())
                        App.ViewModel.UserSetting.Mylist.Add(obj);
                }

                bool IsOpen = false;

                ListPicker listPicker = new ListPicker()
                {
                    Header = AppResources.AddTargetMylist,
                    ItemsSource = Mylist.GetItemsTitleWithoutAppSpecific(),
                    Margin = new Thickness(12, 42, 24, 18),
                    FullModeItemTemplate = this.Resources["favoriteTemplate"] as DataTemplate
                };

                CustomMessageBox messageBox = new CustomMessageBox()
                {
                    Title = AppResources.Mylist,
                    Caption = video.Title,
                    Message = AppResources.SelectMylist,
                    Content = listPicker,
                    LeftButtonContent = AppResources.Add,
                    RightButtonContent = AppResources.Cancel
                };

                Task t = new Task(async () =>
                {
                    //マイリスト追加処理
                    Mylist mylist = null;

                    if ((mylist = Mylist.ResolveMylistFromName(listPicker.SelectedItem as string)) != null)//名前からマイリストの情報取得
                    {
                        if (!await Mylist.AddItemAsync(mylist.ID, mylist.Name, video.ID))//追加失敗
                        {
                            MessageBox.Show(string.Format(AppResources.CannotAddMylist, mylist.Name));
                        }
                    }
                });

                messageBox.Dismissed += (s2, e2) =>
                {
                    switch (e2.Result)
                    {
                        case CustomMessageBoxResult.LeftButton://追加
                            {
                                t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                            }
                            break;
                        case CustomMessageBoxResult.RightButton:
                        case CustomMessageBoxResult.None:
                        default:
                            break;
                    }
                };

                listPicker.SelectionChanged += (s3, e3) =>
                {
                    if (!IsOpen)//最初に開いた時
                    {
                        IsOpen = true;
                        return;
                    }

                    t.Start(TaskScheduler.FromCurrentSynchronizationContext());
                };

                messageBox.Show();
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            RankingResults video = (sender as MenuItem).DataContext as RankingResults;
            const string sltFormat = "{0} ({1}) #niconico #{2}";

            if (video != null)
            {
                ShareLinkTask sls = new ShareLinkTask();
                sls.Title = video.Title;
                sls.LinkUri = new Uri(string.Format("http://nico.ms/{0}", video.ID), UriKind.Absolute);
                sls.Message = string.Format(sltFormat, video.Title, new LengthToRankingResultsLength().Convert(video.Length, null, null, null), video.ID);
                sls.Show();
            }
        }
    }
}