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
            var client = new Web.WebConnect(userName, passwordMD5);
            try
            {
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
