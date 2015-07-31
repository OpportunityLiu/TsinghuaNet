﻿using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Web.Http;
using System;
using System.Text.RegularExpressions;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using System.Threading.Tasks;

namespace Tasks
{
    public sealed class RefreshBackgroundTask : IBackgroundTask
    {
        private readonly string userName = (string)ApplicationData.Current.RoamingSettings.Values["UserName"];
        private readonly string passwordMd5 = (string)ApplicationData.Current.RoamingSettings.Values["PasswordMD5"];

        private readonly string logOnSucessful;
        private readonly string used;

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
                    catch(AggregateException)
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
                return await check("username=" + userName + "&password=" + passwordMd5 + "&mac=" + MacAddress.Current + "&drop=0&type=1&n=100");
            }
        }

        private Size traffic;

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
        }

        #region IBackgroundTask 成员

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var d = taskInstance.GetDeferral();
            if(await logOn())
                SendToastNotification(logOnSucessful, string.Format(CultureInfo.CurrentCulture, used, traffic));
            d.Complete();
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