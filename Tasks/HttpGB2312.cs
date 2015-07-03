using System;
using System.Collections.Generic;
using System.Text;
using Windows.Web.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Tasks
{
    internal static class HttpGB2312
    {
        //private static Encoding gb2312Encoding = GB2312.Encoding.GB2312;

        public static async Task<string> PostStrAsync(this HttpClient httpCilent, Uri uri, string request)
        {
            using(var re = new HttpStringContent(request))
            {
                re.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                using(var get = await httpCilent.PostAsync(uri, re))
                {
                    if(!get.IsSuccessStatusCode)
                        throw new System.Net.Http.HttpRequestException(get.StatusCode.ToString());
                    //if(get.Content.Headers.ContentType != null && get.Content.Headers.ContentType.CharSet == "gb2312")
                    //    return new System.IO.StreamReader(get.Content.ReadAsStreamAsync().Result, gb2312Encoding).ReadToEnd();
                    else
                        return await get.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
