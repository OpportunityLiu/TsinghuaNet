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
        private static readonly Uri generate204 = new Uri("http://www.v2ex.com/generate_204");

        public static IAsyncOperation<bool> CheckLinkAvailable(this HttpClient httpClient)
        {
            return Run(async token =>
            {
                var postTask = httpClient.GetAsync(generate204, HttpCompletionOption.ResponseHeadersRead);
                var cancel = Task.Delay(2000).ContinueWith(task =>
                {
                    postTask.Cancel();
                });
                token.Register(() => postTask.Cancel());
                try
                {
                    using(var get = await postTask)
                    {
                        return get.StatusCode == HttpStatusCode.NoContent || (get.IsSuccessStatusCode && get.Content.Headers.ContentLength == 0);
                    }
                }
                catch(Exception)
                {
                    return false;
                }
            });
        }

        public static IAsyncOperation<string> PostStrAsync(this HttpClient httpClient, Uri uri, string request)
        {
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
    }
}
