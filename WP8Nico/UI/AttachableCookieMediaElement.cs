using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Controls;
using Windows.Networking.Sockets;

namespace WP8Nico.nomula
{
    class AttachableCookieMediaElement : IDisposable
    {
        private CookieContainer _cc;
        private Uri _uri;
        private MediaElement mediaElement;
        private readonly Uri listenAddress = new Uri("http://localhost:81", UriKind.Absolute);
        private byte port = 81;
        StreamSocketListener listener;
        StreamSocket socket;

        public AttachableCookieMediaElement()
        {
            listener = new StreamSocketListener();
            listener.ConnectionReceived += listner_ConnectionReceived;
        }

        public void SetCookieContainer(CookieContainer cc)
        {
            _cc = cc;
        }

        public CookieContainer GetCookieContainer()
        {
            return _cc;
        }

        public Uri Source
        {
            get
            {
                return _uri;
            }
            set
            {
                _uri = value;

                if (mediaElement == null)
                    mediaElement = new MediaElement();

                mediaElement.Source = _uri;
            }
        }

        public async void SetSource(Uri uri)
        {
            _uri = uri;
            
            if (mediaElement == null)
                mediaElement = new MediaElement();

            await listener.BindServiceNameAsync(port.ToString());
            mediaElement.Source = listenAddress;
        }

        private async void listner_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            socket = args.Socket;
            byte[] bytes = new byte[4096];
            Stream input = socket.InputStream.AsStreamForRead();

            //StringBuilder sb = new StringBuilder();

            int c;
            if (input.CanRead)
            {
                c = await input.ReadAsync(bytes, 0, bytes.Length);
                //if (c <= 0) break;

                //sb.Append(Encoding.UTF8.GetString(bytes, 0, c));
            }

            input.Dispose();
            bytes = null;
            //System.Diagnostics.Debug.WriteLine(sb.ToString());
            //sb = null;
            Stream st = socket.OutputStream.AsStreamForWrite();

            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(_uri);
                req.CookieContainer = _cc;
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
                            bytes = Encoding.UTF8.GetBytes(string.Concat("HTTP/1.0 200 OK\r\nContent-Type: video/mp4\r\nContent-Length: ", res.ContentLength, "\r\nConnection: Close\r\n\r\n"));
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
                                //Dispatcher.InvokeAsync(() =>//UIのスレッドを待たない
                                //{
                                //    slider2.Value = read / tick;
                                //});
                            }

                            st.Dispose();
                            st = null;

                            stz.Dispose();
                            stz = null;

                            socket.Dispose();
                            socket = null;

                            res.Dispose();
                            res = null;
                            req = null;
                        }
                        else
                        {
                            res = null;
                            req = null;
                        }
                    }
                    catch (Exception e2)
                    {
                        System.Diagnostics.Debug.WriteLine(e2.Message);
                    }
                }), req);
            }
            catch (Exception)
            { }
        }

        public void Dispose()
        {
            if (listener != null)
            {
                listener.ConnectionReceived -= listner_ConnectionReceived;
                listener.Dispose();
                listener = null;
            }
        }
    }
}
