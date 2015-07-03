using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace TsinghuaNet.Web
{
    public static class HttpHelper
    {
        public static async Task<string> PostStrAsync(this HttpClient httpCilent, Uri uri, string request)
        {
            using(var re = new HttpStringContent(request))
            {
                re.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                using(var get = await httpCilent.PostAsync(uri, re))
                {
                    if(!get.IsSuccessStatusCode)
                        throw new System.Net.Http.HttpRequestException(get.StatusCode.ToString());
                    else
                        return await get.Content.ReadAsStringAsync();
                }
            }
        }

        public static async Task<string> GetStrAsync(this HttpClient httpClient, Uri uri)
        {
            using(var get = await httpClient.PostAsync(uri, null))
            {
                if(!get.IsSuccessStatusCode)
                    throw new System.Net.Http.HttpRequestException(get.StatusCode.ToString());
                else
                    return await get.Content.ReadAsStringAsync();
            }
        }
    }
}
