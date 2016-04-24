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

namespace Web
{
    public static class HttpHelper
    {
        public static IAsyncOperation<string> PostStrAsync(this HttpClient httpClient, Uri uri, string request)
        {
            if(httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            return Run(async token =>
            {
                using(var re = new HttpStringContent(request))
                {
                    re.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
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
                using(var client = new HttpClient(new HttpBaseProtocolFilter() { AllowAutoRedirect = false }))
                {
                    var request = client.GetAsync(new Uri("http://info.tsinghua.edu.cn"), HttpCompletionOption.ResponseHeadersRead);
                    token.Register(request.Cancel);
                    var result = await request;
                    var sc = (int)result.StatusCode;
                    if(200 <= sc && sc < 300)
                        return false;
                    if(300 <= sc && sc < 400)
                    {
                        string loc;
                        if(!result.Headers.TryGetValue("location", out loc))
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
            });
        }
    }
}
