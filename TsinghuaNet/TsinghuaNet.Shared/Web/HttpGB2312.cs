using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Linq;

namespace TsinghuaNet.Web
{
    public static class HttpGb2312
    {
#if WINDOWS_PHONE_APP
        private static Encoding gb2312Encoding = GB2312Encoding.GB2312Encoding.Gb2312;
#endif

        public static string Post(this HttpClient httpCilent, string uri, string request)
        {
            using(var re = new StringContent(request))
            {
                re.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                using(var get = httpCilent.PostAsync(uri, re).Result)
                {
#if WINDOWS_PHONE_APP
                    if(get.Content.Headers.ContentType.CharSet=="gb2312")
                        return new System.IO.StreamReader(get.Content.ReadAsStreamAsync().Result, gb2312Encoding).ReadToEnd();
                    else
#endif
                        return get.Content.ReadAsStringAsync().Result;
                }
            }
        }

        public static string Get(this HttpClient httpClient, string uri)
        {
            using(var get = httpClient.GetAsync(uri).Result)
            {
#if WINDOWS_PHONE_APP
                if(get.Content.Headers.ContentType.CharSet == "gb2312")
                    return new System.IO.StreamReader(get.Content.ReadAsStreamAsync().Result, gb2312Encoding).ReadToEnd();
                else
#endif
                    return get.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
