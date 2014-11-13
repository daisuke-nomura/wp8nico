using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace WP8Nico.nomula
{

    /// <summary>
    /// コメント用クラス
    /// </summary>
    public class Comment
    {
        private const string threadRequestFormat = "<thread thread=\"{0}\" res_from=\"{1}\" version=\"20061206\" />";
        private const string threadRequestFormat2 = "<packet><thread thread=\"{0}\" version=\"20061206\" res_from=\"-1000\" user_id=\"{1}\" force_184=\"1\" scores=\"1\" nicoru=\"1\" /></packet>";
        private const string threadRequestFormat3 = "<packet><thread thread=\"{0}\" version=\"20061206\" res_from=\"-1000\" user_id=\"{1}\" threadkey=\"{2}\" force_184=\"1\" scores=\"1\" nicoru=\"1\" /></packet>";

        protected static IList<CommentDataFlow> naka = null;
        protected static IList<CommentData> shita = null, ue = null;
        public static IList<Comment> Comments = null;

        private static Random random = new Random();

        protected const byte interval = 10;
        protected const ushort maxDisplayTime = 4000, maxDisplayTime3 = 3000;

        private static readonly short _count = -200;
        public static short Count
        {
            get
            {
                return _count;
            }
        }

        private uint _vpos;
        private Color _color = Colors.White;
        private CommentSize _size = CommentSize.Normal;
        private Position _point = Position.Naka;
        private bool _iyayo = false, _full = false;

        public string Text { get; set; }
        public uint Vpos
        {
            get
            {
                return _point == Position.Naka ? _vpos : _vpos + 1000;
            }
            set
            {
                _vpos = value * 10;
            }
        }
        public string Mail
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                string[] mail = value.Split(' ');

                foreach (var obj in mail)
                {
                    switch (obj)
                    {
                        case "white": _color = Colors.White; continue;
                        case "red": _color = Colors.Red; continue;
                        case "pink": _color = Color.FromArgb(0xFF, 0xFF, 0x80, 0x80); continue;
                        case "orange": _color = Color.FromArgb(0xFF, 0xFF, 0xC0, 0x00); continue;
                        case "yellow": _color = Colors.Yellow; continue;
                        case "green": _color = Color.FromArgb(0xFF, 0x00, 0xFF, 0x00); continue;
                        case "cyan": _color = Colors.Cyan; continue;
                        case "blue": _color = Colors.Blue; continue;
                        case "purple": _color = Color.FromArgb(0xFF, 0xC0, 0x00, 0xFF); continue;
                        case "black": _color = Colors.Black; continue;
                        case "#cccccc": _color = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC); continue;
                        case "#cc0033": _color = Color.FromArgb(0xFF, 0xCC, 0x00, 0x33); continue;
                        case "#ff33cc": _color = Color.FromArgb(0xFF, 0xFF, 0x33, 0xCC); continue;
                        case "#ff6633": _color = Color.FromArgb(0xFF, 0xFF, 0x66, 0x33); continue;
                        case "#cccc00": _color = Color.FromArgb(0xFF, 0xCC, 0xCC, 0x00); continue;
                        case "#00cc66": _color = Color.FromArgb(0xFF, 0x00, 0xCC, 0x66); continue;
                        case "#33cccc": _color = Color.FromArgb(0xFF, 0x33, 0xCC, 0xCC); continue;
                        case "#3399ff": _color = Color.FromArgb(0xFF, 0x33, 0x99, 0xFF); continue;
                        case "#9900ff": _color = Color.FromArgb(0xFF, 0x99, 0x00, 0xFF); continue;
                        case "#666666": _color = Color.FromArgb(0xFF, 0x66, 0x66, 0x66); continue;

                        case "big": _size = CommentSize.Big; continue;
                        case "small": _size = CommentSize.Small; continue;

                        case "shita": _point = Position.Shita; continue;
                        case "ue": _point = Position.Ue; continue;

                        case "184": _iyayo = true; continue;

                        case "full": _full = true; continue;
                    }
                }
            }
        }

        public CommentSize Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }

        public Position Point
        {
            get
            {
                return _point;
            }
            set
            {
                _point = value;
            }
        }
        
        public Color Color
        {
            get
            {
                return _color;
            }
        }
        public double Speed { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public double ToValue { get; set; }

        public bool IYAYO
        {
            get
            {
                return _iyayo;
            }
            set
            {
                _iyayo = value;
            }
        }

        public bool Full
        {
            get
            {
                return _full;
            }
            set
            {
                _full = value;
            }
        }

        public enum CommentSize : byte
        {
            Big = 50,
            Normal = 40,
            Small = 30
        }

        public enum Position : byte
        {
            Ue, Shita, Naka
        }

        public async static Task<IList<Comment>> ReadDataAsync(string server, string threadId, string optionalThreadId)
        {
            IEnumerable<Comment> result = null;

            if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(threadId) && string.IsNullOrEmpty(optionalThreadId))//optionalThreadIdがnullの場合、チャンネル・コミュニティ動画ではない
            {
                string str = string.Format(threadRequestFormat, threadId, Count);

                try
                {
                    Uri uri = new Uri(server, UriKind.Absolute);
                    HttpWebRequest req = WebRequest.CreateHttp(uri);
                    req.Method = "POST";
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.AllowReadStreamBuffering = false;
                    using (StreamWriter sw = new StreamWriter(await req.GetRequestStreamAsync()))
                    {
                        await sw.WriteAsync(str);
                        await sw.FlushAsync();
                        sw.Dispose();
                    }

                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            result = ParseComment(await sr.ReadToEndAsync());
                            sr.Dispose();
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
            }
            else if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(threadId) && !string.IsNullOrEmpty(optionalThreadId))
            {
                string str = string.Format(threadRequestFormat2, optionalThreadId, App.ViewModel.UserSetting.UserNumber);

                try
                {
                    Uri uri = new Uri(server, UriKind.Absolute);
                    HttpWebRequest req = WebRequest.CreateHttp(uri);
                    req.Method = "POST";
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.AllowReadStreamBuffering = false;
                    using (StreamWriter sw = new StreamWriter(await req.GetRequestStreamAsync()))
                    {
                        await sw.WriteAsync(str);
                        await sw.FlushAsync();
                        sw.Dispose();
                    }

                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    //using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    //{
                    //    string responseBody = await sr.ReadToEndAsync();
                    //}
                    res.Dispose();
                    res = null;
                    req = null;

                    string threadKey = await GetThreadKeyAsync(threadId);

                    if (!string.IsNullOrEmpty(threadId))
                    {
                        string str2 = string.Format(threadRequestFormat3, threadId, App.ViewModel.UserSetting.UserNumber, threadKey);

                        uri = new Uri(server, UriKind.Absolute);
                        req = WebRequest.CreateHttp(uri);
                        req.Method = "POST";
                        req.CookieContainer = App.ViewModel.UserSetting.cc;
                        req.ContentType = "application/x-www-form-urlencoded";
                        req.AllowReadStreamBuffering = false;
                        using (StreamWriter sw = new StreamWriter(await req.GetRequestStreamAsync()))
                        {
                            await sw.WriteAsync(str2);
                            await sw.FlushAsync();
                            sw.Dispose();
                        }

                        res = await req.GetResponseAsync() as HttpWebResponse;

                        if (res != null && res.StatusCode == HttpStatusCode.OK)
                        {
                            using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                            {
                                result = ParseComment(await sr.ReadToEndAsync());
                                sr.Dispose();
                            }
                        }

                        res.Dispose();
                        res = null;
                        req = null;
                    }
                }
                catch (WebException)
                { }
                catch (Exception)
                { }
            }

            return result != null ? result.ToList() : null;
        }

        private static async Task<string> GetThreadKeyAsync(string threadId)
        {
            const string getFlvUrl = "http://flapi.nicovideo.jp/api/getthreadkey?thread={0}";
            IEnumerable<string> result = null;

            if (!string.IsNullOrEmpty(threadId))
            {
                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(getFlvUrl, threadId), UriKind.Absolute));
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            result = from item in (await sr.ReadToEndAsync()).Split('&')
                                     where (item.Split('='))[0] == ("threadkey")
                                     select (item.Split('='))[1];

                            sr.Dispose();
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
            }

            return result != null && result.Any() ? result.First() : null;
        }

        private static IEnumerable<Comment> ParseComment(string data)
        {
            IEnumerable<Comment> result = null;
 
            try
            {
                //パースし、コメントの衝突判定を行う
                XDocument xml = XDocument.Parse(data);

                result = from item in xml.Descendants("chat")
                             orderby uint.Parse(item.Attribute("vpos").Value) ascending
                             select new Comment
                             {
                                 Text = item.Value,
                                 Vpos = uint.Parse(item.Attribute("vpos").Value),
                                 Mail = item.Attribute("mail") == null ? null : item.Attribute("mail").Value//mailがnullだったら、nullを追加する
                             };

                xml = null;
            }
            catch (Exception)
            { }

            return result;
        }

        public static Canvas CollisionDetect(ref Canvas commentPanel, Comment target, int i)
        {
            double fontSize = (double)target.Size;
            double speed = 0, top = 0, left = 0, toValue = 0;
            string name = i.ToString();

            // テキスト生成
            Canvas grid = GenerateComment(target, name);
            commentPanel.Children.Add(grid);
            commentPanel.UpdateLayout();

            TextBlock textBlock = grid.Children[0] as TextBlock;
            TextBlock textBlock2 = grid.Children[1] as TextBlock;

            if (target.Point == Comment.Position.Naka)
            {
                // テキストの位置を指定
                speed = ResolutionHelper.ActualWidth + textBlock.ActualWidth;//0.2秒(200ミリ秒)ごとに動くピクセル数(速度)
                top = Comment.GetCommentLane(speed, ResolutionHelper.ActualWidth, ResolutionHelper.ActualHeight, (int)textBlock.ActualHeight, ref naka);
                left = ResolutionHelper.ActualWidth;//CommentPanel.ActualWidth;
                toValue = 0 - textBlock.ActualWidth;
                if (ResolutionHelper.CurrentResolution == ResolutionHelper.Resolutions.HD720p) toValue -= 27;
                naka.Add(new CommentDataFlow { Width = (ushort)textBlock.ActualWidth, Top = (ushort)top, ShowedTime = 0, Speed = speed, textBlockHeight = (ushort)textBlock.ActualHeight, Name = name });
                target.Speed = speed;
            }
            else
            {
                while (fontSize > 1 && textBlock.ActualWidth > 640)//(ResolutionHelper.ActualWidth * 0.75)))
                {
                    fontSize--;
                    textBlock.FontSize = fontSize;
                    textBlock2.FontSize = fontSize;
                    grid.UpdateLayout();
                }

                left = (/*player.CommentPanel.ActualWidth ResolutionHelper.ActualWidth */800 - textBlock.ActualWidth) * 0.5;
                toValue = left;


                if (target.Point == Comment.Position.Shita)
                {
                    top = Comment.GetUeShita(ref shita, (ushort)textBlock.ActualHeight);
                    shita.Add(new CommentData { Width = (ushort)textBlock.ActualWidth, Top = (ushort)top, textBlockHeight = (ushort)textBlock.ActualHeight, Name = name });
                    top = ResolutionHelper.ActualHeight - textBlock.ActualHeight - top;
                }
                else
                {
                    top = Comment.GetUeShita(ref ue, (ushort)textBlock.ActualHeight);
                    ue.Add(new CommentData { Width = (ushort)textBlock.ActualWidth, Top = (ushort)top, textBlockHeight = (ushort)textBlock.ActualHeight, Name = name });
                }
            }

            commentPanel.Children.Remove(grid);

            //grid.SetValue(Canvas.LeftProperty, left);
            //grid.SetValue(Canvas.TopProperty, top);

            target.Top = top;
            target.Left = left;
            target.ToValue = toValue;
            target.Mail = null;
            //target.Point = target.Point;

            return grid;
        }

        public static int GetCommentLane(double speed, double canvasWidth, double canvasHeight, int textBlockHeight, ref IList<CommentDataFlow> naka)
        {
            int top, count = naka.Count, spc = 10;
            int canvasHeightMinusFontsize = (int)canvasHeight - textBlockHeight;
            if (canvasHeightMinusFontsize <= 0) return 0;
            //double next = canvasWidth / speed;//配置しようとしているテキスト
            double next = canvasWidth / speed * 4;

            for (top = 0; count > 0 && top < canvasHeightMinusFontsize; top += spc)//10pxずつチェックしていく      && canvasHeightMinusFontsize - top > 0
            {
                CommentDataFlow cd = null;
                int topAndFontsize = top + textBlockHeight;

                var result = from item in naka
                     where item != null && topAndFontsize - item.Top > 0 && item.Top + item.textBlockHeight > top
                     orderby item.ShowedTime ascending
                     select item;

                if (result != null && result.Any())
                    cd = result.First();
                else
                    return top;
                //表示しようとするコメントが左端に到達するまでどれくらいの時間がかかるかを計算し、
                //表示されているコメントが表示し終わるよりも早ければ配置不可能、
                //表示されているコメントが表示し終わってから左端に来るなら配置可能
                //同じレーン且つ同じレーンの中で一番最後の要素に対して衝突判定を行う

                //double prev = ((cd.Width + canvasWidth) - cd.Speed * (cd.ShowedTime / 1000)) / cd.Speed;//同じレーンの一番最後のテキスト
                //double prev =　(cd.Speed - cd.Speed * (cd.ShowedTime / 1000)) / cd.Speed;    
                //double prev = ((4000 - cd.ShowedTime) / 4000 * (canvasWidth / cd.Speed));
                double prev = 4 * (double)(4000 - cd.ShowedTime) * 0.00025;

                //配置する要素が左端に来る時間 - 前の要素が消える時間
                if (next - prev > 0.0F)//衝突しない
                {
                    return top;
                }
                else if (cd.textBlockHeight > 0)
                {
                    spc = cd.textBlockHeight;
                }
            }

            if (count > 0)
                top = random.Next(0, canvasHeightMinusFontsize);//弾幕

            return top;
        }

        public static int GetUeShita(ref IList<CommentData> target, ushort fontSize)
        {
            ushort top = 0;
            ushort count = (ushort)target.Count;

            for ( ; count > 0; top += 5)
            {
                var result = from item in target
                             where (top + fontSize - item.Top) > 0 && (item.Top + item.textBlockHeight) > top
                             select item;

                if (result != null && !result.Any())
                    break;
            }

            return top;
        }

        public static Canvas GenerateComment(Comment target, string name)
        {
            // テキスト生成
            Canvas grid = new Canvas()
            {
                Margin = new Thickness(0, 0, 0, 0),
                Name = name,
            };

            TextBlock textBlock = new TextBlock()
            {
                Text = target.Text,
                FontSize = (double)target.Size,
                Foreground = new SolidColorBrush() { Color = target.Color },
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("メイリオ"),
                Opacity = 1.0,
                Margin = new Thickness(0, 0, 0, 0)
            };

            TextBlock textBlock2 = new TextBlock()
            {
                Text = target.Text,
                FontSize = (double)target.Size,
                Foreground = new SolidColorBrush() { Color = target.Color == Colors.Black ? Colors.White : Colors.Black },//黒の場合は影を白にする
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("メイリオ"),
                Opacity = 1.0,
                Margin = new Thickness(1, 1, 0, 0)
            };

            grid.Children.Add(textBlock2);
            grid.Children.Add(textBlock);

            return grid;
        }

        public static void FlowComment(Canvas CommentPanel, int c, Comment target, bool iyayo, int j)
        {
            if (!iyayo && target.IYAYO)
                return;//匿名(184)コメントを表示がOFF(false)でMailに184が入っていた場合、表示しない


            Task t = new Task(() =>
            {
                Canvas grid = CollisionDetect(ref CommentPanel, target, j);

                if (grid != null && !CommentPanel.Children.Contains(grid))
                {
                    grid.SetValue(Canvas.LeftProperty, target.Left);
                    grid.SetValue(Canvas.TopProperty, target.Top);

                    try
                    {
                        CommentPanel.Children.Add(grid);

                        // テキストのアニメーション
                        DoubleAnimation myDoubleAnimation = new DoubleAnimation()
                            {
                                From = target.Left,
                                To = target.ToValue,
                            };

                        if (target.Point == Position.Naka)
                            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(maxDisplayTime));
                        else
                            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(maxDisplayTime3));

                        Storyboard.SetTargetName(myDoubleAnimation, grid.Name);
                        Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Canvas.LeftProperty));
                        Storyboard sb = new Storyboard();
                        sb.Children.Add(myDoubleAnimation);
                        sb.Completed += ((sender, e) =>
                        {
                            sb.Stop();
                            sb = null;

                            if (CommentPanel.Children.Contains(grid))
                                CommentPanel.Children.Remove(grid);
                            if (CommentPanel.Resources.Contains(grid.Name))
                                CommentPanel.Resources.Remove(grid.Name);

                            grid = null;
                        });

                        CommentPanel.Resources.Add(grid.Name, sb);

                        if (c < 2000)
                            sb.Seek(TimeSpan.FromMilliseconds(target.Vpos));

                        sb.Begin();
                    }
                    catch (ArgumentException)
                    { }
                    catch (Exception)
                    { }
                }
            });

            t.Start(TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void Init()
        {
            naka = new List<CommentDataFlow>();
            shita = new List<CommentData>();
            ue = new List<CommentData>();
        }

        public static void Dispose()
        {
            naka = null;
            shita = null;
            ue = null;
            Comments = null;
        }

        public static void BeforeCalc2()
        {
            //コメントの衝突判定
            int count, i, c;

            try
            {
                count = naka.Count;
                for (i = 0, c = 0; i < count; i++)
                {
                    naka[i].ShowedTime += interval;

                    if (naka[i].ShowedTime >= maxDisplayTime)
                        c++;
                }

                for (i = 0; i < c; i++)
                    naka.RemoveAt(0);

                count = ue.Count;
                for (i = 0, c = 0; i < count; i++)
                {
                    ue[i].ShowedTime += interval;

                    if (ue[i].ShowedTime >= maxDisplayTime3)
                        c++;
                }

                for (i = 0; i < c; i++)
                    ue.RemoveAt(0);

                count = shita.Count;
                for (i = 0, c = 0; i < count; i++)
                {
                    shita[i].ShowedTime += interval;

                    if (shita[i].ShowedTime >= maxDisplayTime3)
                        c++;
                }

                for (i = 0; i < c; i++)
                    shita.RemoveAt(0);

            }
            catch (NullReferenceException)
            { }
            catch (Exception)
            { }
        }

        public async static Task<bool?> PostCommentAsync(string comment, string Postkey)
        {
            bool? post = false;

            if (!string.IsNullOrEmpty(comment))
            {
                try
                {
                    HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format(""), UriKind.Absolute));
                    req.CookieContainer = App.ViewModel.UserSetting.cc;
                    HttpWebResponse res = await req.GetResponseAsync() as HttpWebResponse;

                    if (res != null && res.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                        {
                            XDocument xml = XDocument.Parse(await sr.ReadToEndAsync());

                            var result = from item in xml.Descendants()
                                         select item;
                            
                            if (result != null)
                            {
                                post = true;
                            }

                            result = null;
                            xml = null;
                            sr.Dispose();
                        }
                    }

                    res.Dispose();
                    res = null;
                    req = null;
                }
                catch (WebException)
                {
                    post = false;
                }
                catch (Exception)
                {
                    post = false;
                }
            }
            else
                post = null;

            return post;
        }
    }

    /// <summary>
    /// 上下コメ用クラス
    /// </summary>
    public class CommentData
    {
        public string Name { get; set; }
        public ushort Width { get; set; }
        public ushort Top { get; set; }
        public ushort textBlockHeight { get; set; }
        public ushort ShowedTime { get; set; }
    }

    /// <summary>
    /// 中コメ用のクラス
    /// </summary>
    public class CommentDataFlow : CommentData
    {
        public double Speed { get; set; }
    }
}
