using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Threading;

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

        private static Uri testUri = new Uri("http://info.tsinghua.edu.cn");

        public static IAsyncOperation<bool> NeedSslVpn(this HttpClient httpClient)
        {
            if(httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            return Run(async token =>
            {
                var postTask = httpClient.GetAsync(testUri, HttpCompletionOption.ResponseHeadersRead);
                token.Register(() => postTask.Cancel());
                using(var get = await postTask)
                {
                    var code = (int)get.StatusCode;
                    if(code < 200 || code >= 400)
                        throw new System.Net.Http.HttpRequestException(get.StatusCode.ToString());
                    else if(code < 300)
                        return get.RequestMessage.RequestUri.ToString().EndsWith("indexOutJump.jsp");
                    else
                        return (int)get.StatusCode >= 300;
                }
            });
        }
    }
}
