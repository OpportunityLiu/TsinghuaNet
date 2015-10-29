//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Windows.Foundation;
//using Windows.Security.Credentials;
//using Windows.System;
//using Windows.Web.Http;
//using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

//namespace TsinghuaNet
//{
//    public static class SiteLauncher
//    {

//        public static Task LanunchWebLearning(PasswordCredential account)
//        {
//            if(account == null)
//                throw new ArgumentNullException(nameof(account));
//            account.RetrievePassword();
//            return WebPage.Launch(new Uri($"https://learn.tsinghua.edu.cn/MultiLanguage/lesson/teacher/loginteacher.jsp?userid={account.UserName}&userpass={account.Password}"));
//        }

//        public static IAsyncAction LanunchNewWebLearning(PasswordCredential account)
//        {
//            return Run(async token =>
//            {
//                IAsyncInfo action = null;
//                token.Register(() => action?.Cancel());
//                if(account == null)
//                    throw new ArgumentNullException(nameof(account));
//                var http = new HttpClient(new Windows.Web.Http.Filters.HttpBaseProtocolFilter() { AllowAutoRedirect = false });
//                account.RetrievePassword();
//                action= http.PostAsync(new Uri("https://id.tsinghua.edu.cn/do/off/ui/auth/login/post/fa8077873a7a80b1cd6b185d5a796617/0?/j_spring_security_thauth_roaming_entry"), new HttpFormUrlEncodedContent(new Dictionary<string, string>()
//                {
//                    ["i_user"] = account.UserName,
//                    ["i_pass"] = account.Password
//                }));
//                var result = await (IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress>)action;
//                await WebPage.Launch(result.Headers.Location);
//            });
//        }

//        public static Task LanunchInfo(PasswordCredential account)
//        {
//            if(account == null)
//                throw new ArgumentNullException(nameof(account));
//            account.RetrievePassword();
//            return WebPage.Launch(new Uri($"https://info.tsinghua.edu.cn:443/Login?userName={account.UserName}&password={account.Password}"));
//        }
//    }
//}
