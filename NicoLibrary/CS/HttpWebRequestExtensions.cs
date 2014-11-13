using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NicoLibrary.nomula
{
    static class HttpWebRequestExtensions
    {
        internal static Task<WebResponse> GetResponseAsync(this HttpWebRequest request)
        {
            return Task.Run<WebResponse>(() =>
            {
                AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                IAsyncResult asyncResult = request.BeginGetResponse(r => autoResetEvent.Set(), null);
                autoResetEvent.WaitOne();
                return request.EndGetResponse(asyncResult);
            });
        }
    }
}
