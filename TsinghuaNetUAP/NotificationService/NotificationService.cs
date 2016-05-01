using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using Web;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Resources;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NotificationService
{
    public static class NotificationService
    {
        public static async void UpdateTile(object sender, PropertyChangedEventArgs e)
        {
            var c = sender as WebConnect;
            if(c != null && e.PropertyName == nameof(WebConnect.UpdateTime))
                await UpdateTile(c);
        }

        public static IAsyncAction UpdateTile(WebConnect connect)
        {
            return Task.Run(() =>
            {
                var manager = TileUpdateManager.CreateTileUpdaterForApplication();
                if(connect == null)
                    manager.Clear();
                var usage = connect.WebTrafficExact.ToString();
                manager.EnableNotificationQueue(true);
                var devices = connect.DeviceList.ToArray();
                if(devices.Length == 0)
                {
                    var tile = new XmlDocument();
                    tile.LoadXml($@"
<tile>
    <visual branding='name'>
        <binding template='TileMedium'>
            <text hint-style='body'>{usage}</text>
            <text hint-style='caption' hint-wrap='true'>{LocalizedStrings.Resources.NoDevices}</text>
        </binding>
        <binding template='TileWide'>
            <text hint-style='body'>{string.Format(CultureInfo.CurrentCulture, LocalizedStrings.Resources.Usage, usage)}</text>
            <text hint-style='caption' hint-wrap='true'>{LocalizedStrings.Resources.NoDevices}</text>
        </binding>
    </visual>
</tile>");
                    var tileNotification = new TileNotification(tile);
                    tileNotification.ExpirationTime = new DateTimeOffset(DateTime.Now.AddDays(1));
                    manager.Update(tileNotification);
                    return;
                }
                foreach(var item in devices)
                {
                    var tile = new XmlDocument();
                    tile.LoadXml($@"
<tile>
    <visual branding='name'>
        <binding template='TileMedium'>
            <text hint-style='body'>{usage}</text>
            <text hint-style='caption'>{item.Name}</text>
            <text hint-style='captionsubtle'>{item.LogOnDateTime.TimeOfDay}</text>
            <text hint-style='captionsubtle'>{item.IPAddress}</text>
        </binding>
        <binding template='TileWide'>
            <text hint-style='body'>{string.Format(CultureInfo.CurrentCulture, LocalizedStrings.Resources.Usage, usage)}</text>
            <text hint-style='caption'>{item.Name}</text>
            <text hint-style='captionsubtle'>{item.LogOnDateTime}</text>
            <text hint-style='captionsubtle'>{item.IPAddress}</text>
        </binding>
    </visual>
</tile>");
                    var tileNotification = new TileNotification(tile);
                    tileNotification.ExpirationTime = new DateTimeOffset(DateTime.Now.AddDays(1));
                    manager.Update(tileNotification);
                }
            }).AsAsyncAction();
        }

        /// <summary>
        /// 发送 Toast 通知。
        /// </summary>
        /// <param name="title">标题，加粗显示。</param>
        /// <param name="text">内容。</param>
        /// <param name="param">启动参数</param>
        public static void SendToastNotification(string title, string text, MethodInfo handler, string param)
        {
            var toast = new XmlDocument();
            string aName = null, cName = null, mName = null;
            if(handler != null)
            {
                if(!handler.IsStatic)
                    throw new ArgumentException("Must be static method", nameof(handler));
                aName = handler.DeclaringType.GetTypeInfo().Assembly.FullName;
                cName = handler.DeclaringType.FullName;
                mName = handler.Name;
            }
            toast.LoadXml($@"
<toast launch='{(handler != null ? $@"a={aName};c={cName};m={mName};p={param}" : "")}'>
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

        /// <summary>
        /// 发送 Toast 通知。
        /// </summary>
        /// <param name="title">标题，加粗显示。</param>
        /// <param name="text">内容。</param>
        public static void SendToastNotification(string title, string text)
        {
            SendToastNotification(title, text, null, "");
        }

        private static Dictionary<int, object> _LaunchInstanceDictionry = new Dictionary<int, object>();

        public static void HandleLaunching(string param)
        {
            if(param == "App")
                return;
            var match = Regex.Match(param ?? "", @"^\s*a=(?<aName>.+?)\s*;\s*c=(?<cName>.+?)\s*;\s*m=(?<mName>.+?)\s*;\s*p=(?<param>.+?)\s*$");
            if(match.Success)
            {
                var aName = new AssemblyName(match.Groups["aName"].Value);
                var a = Assembly.Load(aName);
                var c = a.GetType(match.Groups["cName"].Value);
                var m = c.GetMethod(match.Groups["mName"].Value);
                var p = match.Groups["param"].Value;
                object[] pa = null;
                if(string.IsNullOrEmpty(p))
                    pa = new object[0];
                else
                    pa = new object[] { p };
                m.Invoke(null, pa);
            }
        }
    }
}
