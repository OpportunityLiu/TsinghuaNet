using System;
using System.Globalization;
using System.Linq;
using Web;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Networking.Connectivity;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class RefreshBackgroundTask : IBackgroundTask
    {
        private readonly string userName;
        private readonly string passwordMD5;

        private readonly string logOnSucessful;
        private readonly string used;

        private bool run;

        public RefreshBackgroundTask()
        {
            // 准备Toast通知
            toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            toastTitle = toastXml.CreateTextNode("");
            toastText = toastXml.CreateTextNode("");
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastTitle);
            stringElements[1].AppendChild(toastText);

            //加载资源
            var l = ResourceLoader.GetForViewIndependentUse("Tasks/Resources");
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
            var client = new WebConnect(userName, passwordMD5);
            try
            {
                await client.LogOnAsync();
            }
            catch(LogOnException) { }
            try
            {
                await client.RefreshAsync();
                if(!client.IsOnline)
                    return;
                SendToastNotification(logOnSucessful, string.Format(CultureInfo.CurrentCulture, used, client.WebTrafficExact));
            }
            catch(LogOnException) { }
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
            toastTitle.NodeValue = title;
            toastText.NodeValue = text;
            notifier.Show(new ToastNotification(toastXml));
        }

        private XmlDocument toastXml;
        private XmlText toastTitle, toastText;
        private ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
    }

}
