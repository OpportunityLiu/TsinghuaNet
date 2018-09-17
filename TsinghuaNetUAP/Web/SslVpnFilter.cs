using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Web
{
    public class SslVpnFilter : IHttpFilter
    {
        private static int sslVpnFilterCount = 0;

        public SslVpnFilter()
        {
            this.inner = new HttpBaseProtocolFilter();
            sslVpnFilterCount++;
        }

        private static readonly Uri logOffUri = new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/logout.cgi");
        private static readonly Uri logOnUri = new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/login.cgi");
        private static readonly Uri welcomUri = new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/url_default/welcome.cgi");
        private static readonly IHttpContent logOnRequest = getLogOnRequest();

        private static IHttpContent getLogOnRequest()
        {
            if (string.IsNullOrEmpty(Settings.AccountManager.ID))
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
            if (logOnRequest is null)
                return this.inner.SendRequestAsync(request);
            if (!this.initialized)
            {
                this.initialized = true;
                return Run<HttpResponseMessage, HttpProgress>(async (token, progress) =>
                {
                    var req = this.inner.SendRequestAsync(new HttpRequestMessage(HttpMethod.Post, logOnUri)
                    {
                        Content = logOnRequest
                    });
                    token.Register(() => req.Cancel());
                    var res = await req;
                    if (welcomUri.IsBaseOf(res.RequestMessage.RequestUri))
                    {
                        var dataMatch = Regex.Match(await res.Content.ReadAsStringAsync(), @"<input.+?name=""FormDataStr"".+?value=""([^""]+?)"">");
                        if (!dataMatch.Success)
                            throw new InvalidOperationException("can't log on sslvpn");
                        req = this.inner.SendRequestAsync(new HttpRequestMessage(HttpMethod.Post, logOnUri)
                        {
                            Content = new HttpFormUrlEncodedContent(new Dictionary<string, string>
                            {
                                ["FormDataStr"] = dataMatch.Groups[1].Value,
                                ["btnContinue"] = ""
                            })
                        });
                        res = await req;
                    }
                    req = this.SendRequestAsync(request);
                    req.Progress = (sender, prog) => progress.Report(prog);
                    return await req;
                });
            }
            else if (needVpn(request.RequestUri))
            {
                request.RequestUri = EncodeForVpn(request.RequestUri, true);
                return this.inner.SendRequestAsync(request);
            }
            else
            {
                return this.inner.SendRequestAsync(request);
            }
        }

        public static Uri EncodeForVpn(Uri uri, bool raw)
        {
            return new Uri(
            $"https://sslvpn.tsinghua.edu.cn{uri.AbsolutePath},DanaInfo={uri.Authority},{(raw ? "CT=sxml," : "")}{(uri.Port == 80 ? "" : $"Port={uri.Port},")}+{uri.Query}"
            );
        }

        private static readonly HashSet<string> noVpnSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
            if (!match.Success)
                return false;
            if (noVpnSet.Contains(match.Groups[2].Value))
                return false;
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual async void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        sslVpnFilterCount--;
                        if (sslVpnFilterCount == 0)
                            await this.inner.SendRequestAsync(new HttpRequestMessage(HttpMethod.Get, logOffUri));
                    }
                    catch (Exception) { }
                    finally
                    {
                        this.inner.Dispose();
                    }
                }
                this.inner = null;
                this.disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
