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
using Windows.Security.Credentials;
using Web;

namespace BackgroundLogOnTask
{
    public sealed class Task : IBackgroundTask
    {
        private readonly string logOnSucessful;
        private readonly string used;

        public Task()
        {
            //加载资源
            var l = ResourceLoader.GetForViewIndependentUse("BackgroundLogOnTask/Resources");
            used = l.GetString("Used");
            logOnSucessful = l.GetString("LogOnSucessful");
        }

        #region IBackgroundTask 成员

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            PasswordCredential account;
            //初始化信息存储区
            try
            {
                var passVault = new PasswordVault();
                account = passVault.FindAllByResource("TsinghuaAllInOne").First();
            }
            // 未找到储存的密码
            catch(Exception ex) when (ex.HResult == -2147023728)
            {
                return;
            }
            var connection = NetworkInformation.GetInternetConnectionProfile();
            if(connection == null)
                return;
            if(connection.IsWwanConnectionProfile)
                return;
            var d = taskInstance.GetDeferral();
            try
            {
                var http = new HttpClient();
                if(await http.CheckLinkAvailable())
                {
                    d.Complete();
                    return;
                }
                var client = new WebConnect(account);
                await client.LogOnAsync();
                await client.RefreshAsync();
                await TileUpdater.Updater.UpdateTile(client);
                if(!client.IsOnline)
                    return;
                SendToastNotification(logOnSucessful, string.Format(CultureInfo.CurrentCulture, used, client.WebTrafficExact));
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
            ToastNotificationManager.History.Clear();
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toast));
        }
    }
}
