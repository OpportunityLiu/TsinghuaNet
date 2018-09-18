using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Threading;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace Web
{
    public static class HttpHelper
    {
        public static HttpClient WithHeaders(this HttpClient client)
        {
            client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Mozilla", "5.0"));
            if(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Windows Phone 10.0"));
            else
                client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Windows NT 10.0"));
            client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("AppleWebKit", "537.36"));
            client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("KHTML, like Gecko"));
            client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Chrome", "48.0.2564.82"));
            client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Safari", "537.36"));
            client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Edge", "14.14332"));
            client.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("*/*"));
            return client;
        }

        public static IAsyncOperation<string> PostStrAsync(this HttpClient httpClient, Uri uri, string request)
        {
            if(httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            return Run(async token =>
            {
                using(var re = new HttpStringContent(request))
                {
                    re.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                    var postTask = httpClient.PostAsync(uri, re);
                    token.Register(() => postTask.Cancel());
                    using(var get = await postTask)
                    {
                        if(!get.IsSuccessStatusCode)
                            throw new System.Net.Http.HttpRequestException(get.StatusCode.ToString());
                        else
                            return await get.Content.ReadAsStringAsync();
                    }
                }
            });
        }

        public static IAsyncOperation<string> GetStrAsync(this HttpClient httpClient, Uri uri)
        {
            if(httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            return Run(async token =>
            {
                var postTask = httpClient.PostAsync(uri, null);
                token.Register(() => postTask.Cancel());
                using(var get = await postTask)
                {
                    if(!get.IsSuccessStatusCode)
                        throw new System.Net.Http.HttpRequestException(get.StatusCode.ToString());
                    else
                        return await get.Content.ReadAsStringAsync();
                }
            });
        }

        public static IAsyncOperation<bool> NeedSslVpn()
        {
            return Run(async token =>
            {
                try
                {
                    using(var client = new HttpClient(new HttpBaseProtocolFilter() { AllowAutoRedirect = false}).WithHeaders())
                    {
                        var request = client.GetAsync(new Uri("http://info.tsinghua.edu.cn"), HttpCompletionOption.ResponseHeadersRead);
                        token.Register(request.Cancel);
                        var result = await request;
                        var sc = (int)result.StatusCode;
                        if(200 <= sc && sc < 300)
                            return false;
                        if(300 <= sc && sc < 400)
                        {
                            if (!result.Headers.TryGetValue("location", out var loc))
                                goto err;
                            var locUri = new Uri(loc);
                            if(locUri.Host == "info.tsinghua.edu.cn")
                                return true;
                            else
                                return false;
                        }
                    err:
                        throw new InvalidOperationException("Status code of info is out of expected range.");
                    }

                }
                catch(Exception)
                {

                    throw;
                }
            });
        }
    }
}
