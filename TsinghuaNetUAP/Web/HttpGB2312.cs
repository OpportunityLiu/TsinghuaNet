using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;

namespace TsinghuaNet.Web
{
    public static class HttpGB2312
    {
        private static Encoding gb2312Encoding = GB2312.Encoding.GB2312;

        public static async Task<string> PostStrAsync(this HttpClient httpCilent, string uri, string request)
        {
            using(var re = new StringContent(request))
            {
                re.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                using(var get = await httpCilent.PostAsync(uri, re))
                {
                    if(!get.IsSuccessStatusCode)
                        throw new HttpRequestException(get.StatusCode.ToString());
                    if(get.Content.Headers.ContentType != null && get.Content.Headers.ContentType.CharSet == "gb2312")
                        return new System.IO.StreamReader(await get.Content.ReadAsStreamAsync(), gb2312Encoding).ReadToEnd();
                    else
                        return await get.Content.ReadAsStringAsync();
                }
            }
        }

        public static async Task<string> GetStrAsync(this HttpClient httpClient, string uri)
        {
            using(var get = await httpClient.PostAsync(uri, null))
            {
                if(!get.IsSuccessStatusCode)
                    throw new HttpRequestException(get.StatusCode.ToString());
                if(get.Content.Headers.ContentType.CharSet == "gb2312")
                    return new System.IO.StreamReader(await get.Content.ReadAsStreamAsync(), gb2312Encoding).ReadToEnd();
                else
                    return await get.Content.ReadAsStringAsync();
            }
        }
    }
}
