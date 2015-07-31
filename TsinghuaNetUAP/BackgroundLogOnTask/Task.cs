using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Web.Http;
using Windows.Networking.Connectivity;

namespace BackgroundLogOnTask
{
    public sealed class Task : IBackgroundTask
    {
        private readonly string userName;
        private readonly string passwordMD5;

        private readonly string logOnSucessful;
        private readonly string used;

        private readonly bool run;

        /// <summary>
        /// 登陆网络。
        /// </summary>
        /// <exception cref="System.InvalidOperationException">在登陆过程中发生错误。</exception>
        /// <returns>是否发生登陆。</returns>
        private async Task<bool> logOn()
        {
            using(var http = new HttpClient())
            {
                string res = null;
                Func<string, Task<bool>> check = async toPost =>
                {
                    try
                    {
                        res = await http.PostStrAsync(new Uri("http://net.tsinghua.edu.cn/cgi-bin/do_login"), toPost);
                    }
                    catch(Exception ex) when (ex.HResult == -2147012867)
                    {
                        throw new InvalidOperationException();
                    }
                    catch(Exception)
                    {
                        return false;
                    }
                    if(Regex.IsMatch(res, @"^\d+,"))
                    {
                        var a = res.Split(',');
                        traffic = new Size(ulong.Parse(a[2], CultureInfo.InvariantCulture));
                        return true;
                    }
                    return false;
                };
                if(await check("action=check_online"))
                    return false;
                return await check("username=" + userName + "&password=" + passwordMD5 + "&mac=" + MacAddress.Current + "&drop=0&type=1&n=100");
            }
        }

        private Size traffic;

        public Task()
        {
            //加载资源
            var l = ResourceLoader.GetForViewIndependentUse("BackgroundLogOnTask/Resources");
            used = l.GetString("Used");
            logOnSucessful = l.GetString("LogOnSucessful");

            //初始化信息存储区
            try
            {
                var passVault = new Windows.Security.Credentials.PasswordVault();
                var pass = passVault.FindAllByResource("TsinghuaAccount").First();
                pass.RetrievePassword();
                userName = pass.UserName;
                passwordMD5 = pass.Password;
                run = true;
            }
            // 未找到储存的密码
            catch(Exception ex) when (ex.HResult == -2147023728)
            {
                run = false;
            }
        }

        #region IBackgroundTask 成员

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            if(!run)
                return;
            var connection = NetworkInformation.GetInternetConnectionProfile();
            if(connection == null)
                return;
            if(connection.IsWwanConnectionProfile)
                return;
            var d = taskInstance.GetDeferral();
            try
            {
                if(await logOn())
                    SendToastNotification(logOnSucessful, string.Format(CultureInfo.CurrentCulture, used, traffic));
            }
            finally
            {
                d.Complete();
            }
        }

        #endregion

        /// <summary>
        /// 发送 Toast 通知。
        /// </summary>
        /// <param name="title">标题，加粗显示。</param>
        /// <param name="text">内容。</param>
        internal void SendToastNotification(string title, string text)
        {
            XmlDocument toast = new XmlDocument();
            toast.LoadXml($@"
<toast>
    <visual>
        <binding template='ToastGeneric'>
            <text>{title}</text>
            <text>{text}</text>
        </binding>
    </visual>
</toast>");
           ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toast));
        }
    }
}
