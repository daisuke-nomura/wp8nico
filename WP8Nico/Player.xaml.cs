using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using NicoLibrary.nomula;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using WP8Nico.nomula.Resources;

namespace WP8Nico.nomula
{
    public partial class Player : PhoneApplicationPage
    {
        int c = 0, j = 0, z = 0;
        ThreadPoolTimer timer = null;
        RankingResults video = null;

        PlayableQuality playableQuality = null;
        StreamSocket socket = null;
        bool resume = false;
        bool holding = false;

        private const string proxyServerUrl = "http://localhost:81";
        private const string watchPageUrl = "http://www.nicovideo.jp/watch/{0}";
        private const string getFlvUrl = "http://flapi.nicovideo.jp/api/getflv";
        private const byte port = 81;
        //private Stack<NavigationParameter> InboundNavigationParam = null;

        string totalTime = null;
        bool sliderView = true;
        byte sliderViewCount = 0;
        bool fixPortrait = true;
        PageOrientation landscape = PageOrientation.LandscapeLeft;
        bool iyayo = true;

        byte reloadCount = 0;
        const string countFormat = "{0:D2}:{1:D2}:{2:D2}{3}";

        public Player()
        {
            // コンストラクター
            this.InitializeComponent();

            // ApplicationBar をローカライズするためのサンプル コード
            BuildLocalizedApplicationBar();
        }

        // ローカライズされた ApplicationBar を作成するためのサンプル コード
        private void BuildLocalizedApplicationBar()
        {
            // ページの ApplicationBar を ApplicationBar の新しいインスタンスに設定します。
            ApplicationBar = new ApplicationBar();

            // 新しいボタンを作成し、テキスト値を AppResources のローカライズされた文字列に設定します。
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/appbar.favs.addto.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.Favorite;
            appBarButton.Click += appbar_button1_Click;
            ApplicationBar.Buttons.Add(appBarButton);

            ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("/Assets/Icons/share.png", UriKind.Relative));
            appBarButton2.Text = AppResources.Share;
            appBarButton2.Click += appbar_button2_Click;
            ApplicationBar.Buttons.Add(appBarButton2);

            //ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("/Assets/Icons/share.png", UriKind.Relative));
            //appBarButton3.Text = AppResources.Share;
            //appBarButton3.Click += appBarButton3_Click;
            //ApplicationBar.Buttons.Add(appBarButton3);

            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.Reload);
            appBarMenuItem.Click += appBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(appBarMenuItem);

            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem(AppResources.GoHome);
            appBarMenuItem2.Click += appBarMenuItem2_Click;
            ApplicationBar.MenuItems.Add(appBarMenuItem2);

            ApplicationBar.Opacity = 0.6;
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainHeader.Text = "nico";
            mediaElement.AutoPlay = LocalSetting.AutoPlaySetting;
            bool? login = null;

            if (string.IsNullOrEmpty(App.ViewModel.UserSetting.SessionID))//未ログイン
            {
                if ((login = await UserSetting.LightLoginAsync()) == false)//ログイン失敗
                {
                    MessageBox.Show(AppResources.Login, AppResources.LoginFalse, MessageBoxButton.OK);
                    NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));

                    return;
                }
                else if (login == null)//アカウント設定なし
                {
                    MessageBoxResult chk = MessageBox.Show(AppResources.AskLogin, AppResources.Login, MessageBoxButton.OKCancel);

                    if (chk == MessageBoxResult.OK)
                        NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
                    else
                    {
                        MessageBox.Show(AppResources.CannotPlay, AppResources.NotSetAccount, MessageBoxButton.OK);
                        App.Quit();
                    }

                    return;
                }
            }

            object navigationParameter = App.pageState.Peek();//e.Content;

            if (navigationParameter != null && navigationParameter.GetType() == typeof(NavigationParameter))
            {
                NavigationParameter param = navigationParameter as NavigationParameter;

                //if (InboundNavigationParam == null)
                //    InboundNavigationParam = new Stack<NavigationParameter>();

                //InboundNavigationParam.Push(param);

                if (param.Type == NavigationParameter.Types.ID)//動画ID
                {
                    video = new RankingResults();
                    video.ID = param.Parameter.ToString();
                    var items = await RankingResults.ReadItemAsync(video.ID);
                    if (items != null) video = items.First();

                    //動画情報がない
                    if (video == null || string.IsNullOrEmpty(video.ID))
                    {
                        MessageBox.Show(AppResources.CannotPlay, AppResources.CannotPlayThisApp, MessageBoxButton.OK);
                        return;
                    }
                }
                else if (param.Type == NavigationParameter.Types.RankingResult)//動画情報
                {
                    video = param.Parameter as RankingResults;
                }
                else if (param.Type == NavigationParameter.Types.RankingResults)//複数動画情報
                {
                    video = (param.Parameter as RankingResults[])[0];
                }
                else
                {
                    //ありえないパラメータ
                    throw new Exception(AppResources.UndefinedMovie);
                }
            }
            else
            {
                throw new Exception(AppResources.UndefinedMovie);
            }

            LayoutRoot.DataContext = video;

            if (video.TagList != null)
            {
                var tags = (from item in video.TagList select item.Title).ToArray();
                
                if (tags.Length > 1)
                    mainHeader.Text = tags[1];
            }

            Task.Run(() =>
                {
                    WatchedMovie.AddData(video.ID);
                });

            if (LocalSetting.VideoHub)
            {
                await Task.Run(async() =>
                    {
                        try
                        {
                            HttpWebRequest req = WebRequest.CreateHttp(new Uri(video.Thumbnail, UriKind.Absolute));
                            req.CookieContainer = App.ViewModel.UserSetting.cc;
                            HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                            if (res != null && res.StatusCode == HttpStatusCode.OK)
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    await res.GetResponseStream().CopyToAsync(ms);
                                    ms.Seek(0, SeekOrigin.Begin);

                                    MediaHistoryItem mediaHistoryItem = new MediaHistoryItem();
                                    mediaHistoryItem.ImageStream = ms;
                                    mediaHistoryItem.Title = video.Title;
                                    mediaHistoryItem.PlayerContext.Add("id", video.ID);
                                    MediaHistory.Instance.WriteRecentPlay(mediaHistoryItem);
                                }
                            }

                            res.Dispose();
                            res = null;
                            req = null;
                        }
                        catch (WebException)
                        { }
                        catch (Exception)
                        { }
                    });
            }

            iyayo = LocalSetting.IYAYO184Setting;

            if (video.MovieType == RankingResults.MovieTypes.MP4 || video.HasMp4 == true)
            {
                if (LocalSetting.AutoPlaySetting)
                {
                    Task.Run(() =>
                        {
                            RequestAndResponse();
                        });
                }
            }
            else if (video.MovieType == RankingResults.MovieTypes.SWF)
            {
                if (LocalSetting.AutoPlaySetting)
                    RequestAndResponse();

                //MessageBox.Show(AppResources.CannotPlaySWF, AppResources.CannotPlaySWFDesc, MessageBoxButton.OK);
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Comment.Dispose();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }

            if (timer != null)
            {
                timer.Cancel();
                timer = null;
            }

            if (mediaElement.CurrentState == MediaElementState.Playing)
            {
                mediaElement.Stop();
            }

            //base.OnNavigatingFrom(e);

            if (e.NavigationMode == NavigationMode.Back)
                App.pageState.Pop();
        }

        private async void SetCommentAsync(ThreadPoolTimer timer)
        {
            Comment.BeforeCalc2();

            await Dispatcher.InvokeAsync((() =>
            {
                if (mediaElement.CurrentState != MediaElementState.Playing)
                    return;

                c = (int)mediaElement.Position.TotalMilliseconds;

                if (sliderView && sliderViewCount++ % 50 == 0)
                {
                    if (!holding)
                        slider.Value = mediaElement.Position.TotalSeconds;

                    playerCount.Text = string.Format(countFormat, mediaElement.Position.Hours, mediaElement.Position.Minutes, mediaElement.Position.Seconds, totalTime);
                }


                if (Comment.Comments != null)
                {
                    for (z = c + 1000; j < Comment.Comments.Count && Comment.Comments[j].Vpos < z; j++)
                    {
                        Comment.FlowComment(CommentPanel, c, Comment.Comments[j], iyayo, j);
                    }
                }
            }));
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            slider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            timer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(SetCommentAsync), TimeSpan.FromMilliseconds(10));
            j = 0;
            //playb.Visibility = Visibility.Visible;
            totalTime = string.Format("/{0:D2}:{1:D2}:{2:D2}", mediaElement.NaturalDuration.TimeSpan.Hours, mediaElement.NaturalDuration.TimeSpan.Minutes, mediaElement.NaturalDuration.TimeSpan.Seconds);
            play.Source = new BitmapImage(new Uri("/Assets/Icons/appbar.transport.pause.rest.png", UriKind.Relative));
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Cancel();
            timer = null;
            mediaElement.Position = TimeSpan.Zero;


            j = 0;
            c = 0;

            if (LocalSetting.RepeatSetting)
                mediaElement.Play();
            else
            {
                mediaElement.Stop();
                //slider.Value = 0;
                //play.Source = new BitmapImage(new Uri("/Assets/Icons/appbar.transport.play.rest.png", UriKind.Relative));
            }

            timer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(SetCommentAsync), TimeSpan.FromMilliseconds(10));
        }

        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (reloadCount <= 2)
            {
                reloadCount++;
                OnNavigatedTo(new NavigationEventArgs(null, null));
            }
            else
            {
                MessageBox.Show(e.ToString());
            }
        }

        private async void RequestAndResponse()
        {
            bool? login = null;

            if (video.MovieType == RankingResults.MovieTypes.MP4 || video.HasMp4 == true)
            {
                if (video.Participation == RankingResults.Participations.REGULAR)//コミュニティ・チャンネルに属さない
                {
                    playableQuality = await RankingResults.GetAvailableVideoQualityAsync(video);

                    if (playableQuality != null)
                    {
                        if ((login = await UserSetting.DeepLoginAsync()) == true)
                        {
                            if (!string.IsNullOrEmpty(playableQuality.Type))
                            {
                                try
                                {
                                    #region
                                    //Cookie appInfo = new Cookie("nicoiphone_app_version", "4.22");
                                    //Cookie deviceInfo = new Cookie("nicoiphone_device", "iPhone4%2E1");
                                    //CookieCollection cc = new CookieCollection();
                                    //cc.Add(appInfo);
                                    //cc.Add(deviceInfo);
                                    //App.ViewModel.UserSetting.cc.Add(new Uri("http://www.nicovideo.jp/", UriKind.Absolute), cc);
                                    #endregion
                                    video = await RankingResults.ReadGetFlviPhoneAsync(video, playableQuality);

                                    Dispatcher.InvokeAsync(async () =>
                                    {
                                        try
                                        {
                                            StreamSocketListener listener = new StreamSocketListener();
                                            listener.ConnectionReceived += listner_ConnectionReceived;
                                            await listener.BindServiceNameAsync(port.ToString());

                                            mediaElement.Source = new Uri(proxyServerUrl, UriKind.Absolute);
                                        }
                                        catch (Exception)
                                        { }
                                    });

                                    Task.Run(async () =>
                                    {
                                        Comment.Init();
                                        Comment.Comments = await Comment.ReadDataAsync(video.MessageServer, video.ThreadId, video.OptionalThreadId);

                                        //commentList.ItemsSource = Comment.AllComment;
                                    });
                                }
                                catch (WebException)
                                { }
                                catch (Exception)
                                { }
                            }
                        }
                        else if (login == false)
                        {
                            await Dispatcher.InvokeAsync(() =>
                                {
                                    MessageBox.Show(AppResources.Login, AppResources.LoginFalse, MessageBoxButton.OK);
                                });
                        }
                        else//login == null
                        {
                            await Dispatcher.InvokeAsync(() =>
                                {
                                    MessageBoxResult chk = MessageBox.Show(AppResources.AskLogin, AppResources.Login, MessageBoxButton.OKCancel);

                                    if (chk == MessageBoxResult.OK)
                                        NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
                                });
                        }
                    }
                    else if (!App.ViewModel.UserSetting.IsPremium)
                    {
                        ShowErrorMessage(AppResources.UnavailableForNotPremium);
                    }
                    else
                    {
                        //再生不可
                    }
                }
                else
                {
                    if ((login = await UserSetting.DeepLoginAsync()) == true)
                    {
                        try
                        {
                            video = await RankingResults.GetRealMovieIdFromChannelCommunityWatchPageNumberAsync(video);
                            video = await RankingResults.ReadGetFlvAsync(video);

                            Dispatcher.InvokeAsync(async () =>
                            {
                                StreamSocketListener listener = new StreamSocketListener();
                                listener.ConnectionReceived += listner_ConnectionReceived;
                                await listener.BindServiceNameAsync(port.ToString());

                                mediaElement.Source = new Uri(proxyServerUrl, UriKind.Absolute);
                            });

                            Task.Run(async () =>
                            {
                                Comment.Init();
                                Comment.Comments = await Comment.ReadDataAsync(video.MessageServer, video.ThreadId, video.OptionalThreadId);

                                //commentList.ItemsSource = Comment.AllComment;
                            });
                        }
                        catch (WebException)
                        { }
                        catch (Exception)
                        { }
                    }
                    else if (login == false)
                    {
                        await Dispatcher.InvokeAsync(() =>
                            {
                                MessageBox.Show(AppResources.Login, AppResources.LoginFalse, MessageBoxButton.OK);
                            });
                    }
                    else//login == null
                    {
                        await Dispatcher.InvokeAsync(() =>
                            {
                                MessageBoxResult chk = MessageBox.Show(AppResources.AskLogin, AppResources.Login, MessageBoxButton.OKCancel);

                                if (chk == MessageBoxResult.OK)
                                    NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
                            });
                    }
                }
            }
            else if (video.MovieType == RankingResults.MovieTypes.SWF)
            {
                if ((login = await UserSetting.DeepLoginAsync()) == true)
                {
                    try
                    {
                        video = await RankingResults.GetRealMovieIdFromChannelCommunityWatchPageNumberAsync(video);
                        video = await RankingResults.ReadGetFlvAsync(video, true);

                        Task.Run(async () =>
                        {
                            //var st = await Swf.GetSwfStreamAsync(video.VideoServer);
                            //var st3 = await Swf.DecompressionSwfStream(st);
                            //var st2 = Swf.ParseSwf(st3);
                        });

                        Task.Run(async () =>
                        {
                            Comment.Init();
                            Comment.Comments = await Comment.ReadDataAsync(video.MessageServer, video.ThreadId, video.OptionalThreadId);

                            //commentList.ItemsSource = Comment.AllComment;
                        });
                    }
                    catch (WebException)
                    { }
                    catch (Exception)
                    { }
                }
                else if (login == false)
                {
                    await Dispatcher.InvokeAsync(() =>
                        {
                            MessageBox.Show(AppResources.Login, AppResources.LoginFalse, MessageBoxButton.OK);
                        });
                }
                else//login == null
                {
                    await Dispatcher.InvokeAsync(() =>
                        {
                            MessageBoxResult chk = MessageBox.Show(AppResources.AskLogin, AppResources.Login, MessageBoxButton.OKCancel);

                            if (chk == MessageBoxResult.OK)
                                NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
                        });
                }
            }
            else
            {
                //再生不可
            }

            Dispatcher.InvokeAsync(async() =>
                {
                    video = await RankingResults.ReadUploaderInfoAsync(video);
                    ResultList.ItemsSource = await RankingResults.ReadRelatedItemsAsync(video);
                    progressBar1.Visibility = Visibility.Collapsed;
                });
        }

        async void listner_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
        {
            const string responseFormat = "HTTP/1.0 200 OK\r\nContent-Type: video/mp4\r\nContent-Length: {0}\r\nConnection: Close\r\n\r\n";
            socket = args.Socket;
            byte[] bytes = new byte[4096];
            int c = 0;
            Stream input = socket.InputStream.AsStreamForRead();

            if (input.CanRead)
            {
                c = await input.ReadAsync(bytes, 0, bytes.Length);
                //if (c <= 0) break;
            }

            input.Dispose();
            bytes = null;
            Stream st = socket.OutputStream.AsStreamForWrite();

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(video.VideoServer, UriKind.Absolute));
                req.CookieContainer = App.ViewModel.UserSetting.cc;
                req.AllowReadStreamBuffering = false;
                req.BeginGetResponse(new AsyncCallback(async (ar) =>
                {
                    try
                    {
                        req = ar.AsyncState as HttpWebRequest;
                        HttpWebResponse res = req.EndGetResponse(ar) as HttpWebResponse;

                        if (res != null && res.StatusCode == HttpStatusCode.OK)
                        {
                            long read = 0, tick = res.ContentLength / 200;

                            //MediaElementのリクエストに応答
                            bytes = Encoding.UTF8.GetBytes(string.Format(responseFormat, res.ContentLength));
                            await st.WriteAsync(bytes, 0, bytes.Length);
                            bytes = null;

                            byte[] buff = new byte[256 * 1024];
                            int length = buff.Length;
                            Stream stz = res.GetResponseStream();

                            while (true)
                            {
                                c = await stz.ReadAsync(buff, 0, length);
                                if (c <= 0) break;

                                await st.WriteAsync(buff, 0, c);

                                read += c;
                                Dispatcher.InvokeAsync(() =>//UIのスレッドを待たない
                                    {
                                        slider.ProgressValue = read / tick;
                                    });
                            }

                            st.Dispose();
                            st = null;

                            stz.Dispose();
                            stz = null;

                            socket.Dispose();
                            socket = null;

                        }

                        res.Dispose();
                        res = null;
                        req = null;
                    }
                    catch (Exception e2)
                    {
                        //System.Diagnostics.Debug.WriteLine(e2.Message);
                    }
                }), req);
            }
            catch (Exception)
            { }
        }

        private void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            MediaElementState mes = ((MediaElement)sender).CurrentState;

            if (mes == MediaElementState.Paused || mes == MediaElementState.Buffering)
            {
                for (int i = 0, c = CommentPanel.Children.Count; i < c; i++)
                {
                    Canvas obj = CommentPanel.Children[i] as Canvas;

                    if (obj != null)
                    {
                        Storyboard sb = CommentPanel.Resources[obj.Name] as Storyboard;

                        if (sb != null)
                            sb.Pause();
                    }
                }

                resume = true;
            }
            else if (mes == MediaElementState.Playing && resume)
            {
                for (int i = 0, c = CommentPanel.Children.Count; i < c; i++)
                {
                    Canvas obj = CommentPanel.Children[i] as Canvas;

                    if (obj != null)
                    {
                        Storyboard sb = CommentPanel.Resources[obj.Name] as Storyboard;

                        if (sb != null)
                            sb.Resume();
                    }
                }

                resume = false;
            }
        }

        private void slider1_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void slider1_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            holding = true;

            //うまく動いてないので直す必要あり
            mediaElement.Pause();
            double value = slider.Value;
            c = (int)value;
            int cc = c * 1000;

            //foreach (var target in CommentPanel.Children)
            //{
            //    Canvas obj = target as Canvas;

            //    if (obj != null)
            //    {
            //        Storyboard sb = CommentPanel.Resources[obj.Name] as Storyboard;

            //        if (sb != null)
            //        {
            //            sb.Stop();
            //            sb = null;
            //            CommentPanel.Resources.Remove(obj.Name);
            //        }

            //        CommentPanel.Children.RemoveAt(0);
            //    }
            //}

            Comment.Init();

            //var result = from item in Comment.AllComment
            //             where item.Vpos > cc
            //             orderby item.Vpos ascending
            //             select item;

            if (Comment.Comments != null)
            {
                for (int i = 0, z = Comment.Comments.Count; i < z; i++)
                {
                    if (Comment.Comments[i].Vpos > cc)
                    {
                        j = i;
                        break;
                    }
                }
            }

            mediaElement.Position = TimeSpan.FromSeconds(value);
            mediaElement.Play();
            holding = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Source == null && !LocalSetting.AutoPlaySetting)
            {
                mediaElement.AutoPlay = true;
                RequestAndResponse();
                return;
            }

            if (mediaElement.CurrentState == MediaElementState.Playing)
            {
                mediaElement.Pause();
                play.Source = new BitmapImage(new Uri("/Assets/Icons/appbar.transport.play.rest.png", UriKind.Relative));
            }
            else
            {
                mediaElement.Play();
                play.Source = new BitmapImage(new Uri("/Assets/Icons/appbar.transport.pause.rest.png", UriKind.Relative));
            }
        }

        private async void appbar_button1_Click(object sender, EventArgs e)
        {
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
                    FullModeItemTemplate = favoriteTemplate
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

        private void appbar_button2_Click(object sender, EventArgs e)
        {
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

        private void tagList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tagList.SelectedIndex == -1)
                return;

            Tag tag = tagList.SelectedItem as Tag;
            App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Tag, Parameter = new Tag() { Title = tag.Title } });
            NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));

            tagList.SelectedIndex = -1;
        }

        private void linkList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (linkList.SelectedIndex == -1)
                return;

            string str = linkList.SelectedItem.ToString();

            if (str.Contains("youtube.com") || str.Contains("youtu.be"))
            {
                Match m = Regex.Match(str, @"(\?v=(?<id>[a-zA-Z0-9_-]+)|/(?<id>[a-zA-Z0-9_-]+$))");

                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri(string.Format("http://m.youtube.com/#/watch?v={0}", m.Groups["id"].Value), UriKind.Absolute);
                task.Show();
            }
            else if (Regex.IsMatch(str, @"sm(\d+)$") || Regex.IsMatch(str, @"nm(\d+)$") || Regex.IsMatch(str, @"so(\d+)$"))
            {
                App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.ID, Parameter = str });
                NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));
            }
            else if (str.Contains("mylist/"))
            {
                Match m = Regex.Match(str, @"(\d+)$");

                if (m.Success)
                {
                    App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.PublicMylist, Parameter = new Mylist() { ID = uint.Parse(m.Value) } });
                    NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));
                }
            }
            else if (Regex.IsMatch(str, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri(Regex.Match(str, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase | RegexOptions.Multiline).Value, UriKind.Absolute);
                task.Show();
            }

            linkList.SelectedIndex = -1;
        }

        private void CommentPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (playerControl.Visibility == Visibility.Visible)
            {
                playerControl.Visibility = Visibility.Collapsed;
                //ApplicationBar.IsVisible = false;
                sliderView = false;
            }
            else
            {
                playerControl.Visibility = Visibility.Visible;
                //ApplicationBar.IsVisible = true;
                sliderView = true;
            }
        }

        private void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultList.SelectedIndex == -1)
                return;

            RankingResults item = ResultList.SelectedItem as RankingResults;
            App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.ID, Parameter = item.ID });//Vita APIだと情報が少ないので、関連動画はIDだけを渡す
            //OnNavigatedTo(new NavigationEventArgs(null, null));
            //NavigationService.Navigate(new Uri(string.Format("/Player.xaml?{0}", new Random().Next(0, 1000).ToString()), UriKind.Relative));
            NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));

            ResultList.SelectedIndex = -1;
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
                landscape = e.Orientation;

            //if (fixPortrait)
            //    return;

            if (e.Orientation == PageOrientation.Portrait || e.Orientation == PageOrientation.PortraitUp || e.Orientation == PageOrientation.PortraitDown)
            {
                ApplicationBar.IsVisible = true;
                Portrait.Storyboard.Begin();
                //this.Orientation = PageOrientation.Portrait;
                mediaElement.Projection = null;
                mediaElement.Margin = new Thickness(0);
                //BuildLocalizedApplicationBar();
            }
            else if (e.Orientation == PageOrientation.Landscape || e.Orientation == PageOrientation.LandscapeLeft)
            {
                ApplicationBar.IsVisible = false;
                Landscape.Storyboard.Begin();
                //this.Orientation = PageOrientation.Landscape;
                mediaElement.Projection = new PlaneProjection() { RotationZ = -90, GlobalOffsetX = -160 };
                mediaElement.Margin = new Thickness(0, 160, -320, 160);
                //BuildLocalizedApplicationBarLandscape();
                VideoPlayer.Projection = new PlaneProjection() { RotationZ = -90, GlobalOffsetX = -160 };

                //if (ResolutionHelper.CurrentResolution == ResolutionHelper.Resolutions.HD720p)
                //{
                //    mediaElement.Width = 853;
                //    //VideoPlayer.Width = 853;
                //    mediaElement.Projection = new PlaneProjection() { RotationZ = -90, GlobalOffsetX = -185 };
                //    //VideoPlayer.Projection = new PlaneProjection() { RotationZ = -90, GlobalOffsetX = -160, GlobalOffsetY = 30 };
                //    mediaElement.Margin = new Thickness(0, 0, -320, 0);
                //}
            }
            else if (e.Orientation == PageOrientation.Landscape || e.Orientation == PageOrientation.LandscapeRight)
            {
                Landscape.Storyboard.Begin();
                ApplicationBar.IsVisible = false;
                //this.Orientation = PageOrientation.Landscape;
                mediaElement.Projection = new PlaneProjection() { RotationZ = 90, GlobalOffsetX = -160 };
                mediaElement.Margin = new Thickness(0, 160, -320, 160);
                //BuildLocalizedApplicationBarLandscape();
                VideoPlayer.Projection = new PlaneProjection() { RotationZ = 90, GlobalOffsetX = -160 };

                //if (ResolutionHelper.CurrentResolution == ResolutionHelper.Resolutions.HD720p)
                //{
                //    mediaElement.Width = 853;
                //    VideoPlayer.Width = 853;
                //    mediaElement.Projection = new PlaneProjection() { RotationZ = 90, GlobalOffsetX = -160 };
                //    VideoPlayer.Projection = new PlaneProjection() { RotationZ = 90, GlobalOffsetX = -160 };
                //}
            }
        }

        private void Button_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Tag, Parameter = new Tag() { Title = video.Uploader } });
            NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));
        }

        private void Button_Tap_2(object sender, System.Windows.Input.GestureEventArgs e)
        {
            appbar_button1_Click(sender, e);
        }

        private void Button_Tap_3(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.pageState.Push(new NavigationParameter() { Type = NavigationParameter.Types.Tag, Parameter = new Tag() { Title = video.Title } });
            NavigationService.Navigate(new Uri("/List.xaml", UriKind.Relative));
        }

        //private void commentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (commentList.SelectedIndex == -1)
        //        return;

        //    //選択されたアイテムのVposまで動画をシークする


        //    commentList.SelectedIndex = -1;
        //}

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
                    FullModeItemTemplate =　this.Resources["favoriteTemplate"] as DataTemplate
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
            const string sltFormat = "{0} ({1}) #niconico #{2}";
            RankingResults video = (sender as MenuItem).DataContext as RankingResults;

            if (video != null)
            {
                ShareLinkTask sls = new ShareLinkTask();
                sls.Title = video.Title;
                sls.LinkUri = new Uri(string.Format("http://nico.ms/{0}", video.ID), UriKind.Absolute);
                sls.Message = string.Format(sltFormat, video.Title, new LengthToRankingResultsLength().Convert(video.Length, null, null, null), video.ID);
                sls.Show();
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //sliderTooltip.Text = e.NewValue.ToString();
        }

        private void mediaElement_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
        }

        private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            CommentPanel.Opacity = 0.0;
        }

        private void ToggleButton_Unchecked_1(object sender, RoutedEventArgs e)
        {
            CommentPanel.Opacity = 1.0;
        }

        void appBarButton3_Click(object sender, EventArgs e)
        {
            fixPortrait = !fixPortrait;
        }

        private void Grid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //fixPortrait = false;
            //OnOrientationChanged(new OrientationChangedEventArgs(landscape));
        }

        void appBarMenuItem_Click(object sender, EventArgs e)
        {
            OnNavigatedTo(new NavigationEventArgs(null, null));
        }

        private void CommentPanel_DoubleTap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //OnOrientationChanged(new OrientationChangedEventArgs(PageOrientation.PortraitUp));
            //fixPortrait = true;
        }

        void appBarMenuItem2_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PivotPage.xaml", UriKind.Relative));
        }

        private void ShowErrorMessage(string str)
        {
            Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(str);
                });
        }
    }
}