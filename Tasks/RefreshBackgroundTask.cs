using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using System.Net.Http;
using System;

namespace Tasks
{
    public sealed class RefreshBackgroundTask : IBackgroundTask
    {
        public RefreshBackgroundTask()
        {
            // 准备Toast通知
            toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
            toastTitle = toastXml.CreateTextNode("");
            toastText1 = toastXml.CreateTextNode("");
            toastText2 = toastXml.CreateTextNode("");
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastTitle);
            stringElements[1].AppendChild(toastText1);
            stringElements[2].AppendChild(toastText2);
        }

        #region IBackgroundTask 成员

        public void Run(IBackgroundTaskInstance taskInstance)
        {
        }

        #endregion

        private void logOn()
        {
        }

        /// <summary>
        /// 发送 Toast 通知。
        /// </summary>
        /// <param name="title">标题，加粗显示。</param>
        /// <param name="text1">第一行内容。</param>
        /// <param name="text2">第二行内容。</param>
        public void SendToastNotification(string title, string text1, string text2)
        {
            toastTitle.NodeValue = title;
            toastText1.NodeValue = text1;
            toastText2.NodeValue = text2;
            notifier.Show(new ToastNotification(toastXml));
        }

        private XmlDocument toastXml;
        private XmlText toastTitle, toastText1, toastText2;
        private ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
    }

}
