using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Text.RegularExpressions;

namespace Web
{
    public class SslVpnFilter : IHttpFilter
    {
        public SslVpnFilter()
        {
            inner = new HttpBaseProtocolFilter();
        }

        private static Uri logOffUri = new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/logout.cgi");
        private static Uri logOnUri = new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/login.cgi");
        private static Uri welcomUri = new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/url_default/welcome.cgi");
        private static IHttpContent logOnRequest = getLogOnRequest();

        private static IHttpContent getLogOnRequest()
        {
            if(string.IsNullOrEmpty(Settings.AccountManager.ID))
                return null;
            var pass = Settings.AccountManager.Account;
            pass.RetrievePassword();
            return new HttpFormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = Settings.AccountManager.ID,
                ["password"] = pass.Password,
                ["realm"] = "ldap"
            });
        }

        private IHttpFilter inner;

        private bool initialized;

        public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
        {
            if(logOnRequest == null)
                return inner.SendRequestAsync(request);
            if(!initialized)
            {
                initialized = true;
                return Run<HttpResponseMessage, HttpProgress>(async (token, progress) =>
                {
                    var req = inner.SendRequestAsync(new HttpRequestMessage(HttpMethod.Post, logOnUri)
                    {
                        Content = logOnRequest
                    });
                    token.Register(() => req.Cancel());
                    var res = await req;
                    if(welcomUri.IsBaseOf(res.RequestMessage.RequestUri))
                        throw new InvalidOperationException("SslVpn is using by othe program.");
                    req = SendRequestAsync(request);
                    req.Progress = (sender, prog) => progress.Report(prog);
                    return await req;
                });
            }
            else if(needVpn(request.RequestUri))
            {
                request.RequestUri = encodeForVpn(request.RequestUri);
                return inner.SendRequestAsync(request);
            }
            else
            {
                return inner.SendRequestAsync(request);
            }
        }

        private static Uri encodeForVpn(Uri uri)
        {
            return new Uri(
            $"https://sslvpn.tsinghua.edu.cn{uri.AbsolutePath},DanaInfo={uri.Authority},CT=sxml,{(uri.Port == 80 ? "" : $"Port={uri.Port},")}+{uri.Query}"
            );
        }

        static readonly HashSet<string> noVpnSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "learn",
            "sslvpn",
            "lib",
            "mail",
            "mails",
            "net"
        };

        private static bool needVpn(Uri uri)
        {
            var match = Regex.Match(uri.Authority, @"^(.*?\.|)(.+?)\.tsinghua\.edu\.cn");
            if(!match.Success)
                return false;
            if(noVpnSet.Contains(match.Groups[2].Value))
                return false;
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual async void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    await inner.SendRequestAsync(new HttpRequestMessage(HttpMethod.Get, logOffUri));
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
