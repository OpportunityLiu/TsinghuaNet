using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Background;

namespace TsinghuaNet
{
    public sealed class SharedUI : IDisposable
    {
        public SharedUI()
        {
            var a = false;
            //注册后台任务
            foreach(var cur in BackgroundTaskRegistration.AllTasks)
            {
                if(cur.Value.Name == "RefreshBackgroundTask")
                    a = true;
            }
            if(!a)
            {
                var builder = new BackgroundTaskBuilder();
                builder.Name = "RefreshBackgroundTask";
                builder.TaskEntryPoint = "TsinghuaNet.RefreshBackgroundTask";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.InternetAvailable, false));
                builder.Register();
            }

            //初始化信息存储区
            try
            {
                ApplicationData.Current.LocalSettings.Values.Add("PasswordLength", 0);
                ApplicationData.Current.LocalSettings.Values.Add("UserName", "");
                ApplicationData.Current.LocalSettings.Values.Add("PasswordMD5", "");
            }
            catch(ArgumentException)
            {
                //已经添加字段
                if(!string.IsNullOrEmpty((string)ApplicationData.Current.LocalSettings.Values["UserName"]))
                    Connect = new WebConnect((string)ApplicationData.Current.LocalSettings.Values["UserName"], (string)ApplicationData.Current.LocalSettings.Values["PasswordMD5"]);
            }

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

        public WebConnect Connect
        {
            get;
            set;
        }

        private bool useSavedPassword;

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

        #region IDisposable 成员

        public void Dispose()
        {
            this.Connect.Dispose();
        }

        #endregion
    }

    public class RefreshBackgroundTask : IBackgroundTask
    {
        #region IBackgroundTask 成员

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferral = taskInstance.GetDeferral();
            defferral.Complete();
        }

        #endregion
    }

}
