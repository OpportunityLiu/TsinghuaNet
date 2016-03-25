using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace Web
{
    class SslVpnHelper : IHttpFilter
    {
        public SslVpnHelper()
        {
            inner = new HttpBaseProtocolFilter();
        }

        IHttpFilter inner;

        public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
        {
            return inner.SendRequestAsync(request);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    inner.Dispose();
                }
                inner = null;
                disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
