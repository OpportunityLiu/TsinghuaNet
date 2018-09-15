using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Web;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI.Notifications;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace NotificationService
{
    public static class NotificationService
    {
        public static void SetBadgeNumber(int num)
        {
            var badgeXml = new XmlDocument();
            badgeXml.LoadXml($@"<badge value='{num}'/>");
            var badge = new BadgeNotification(badgeXml);
            badge.ExpirationTime = DateTimeOffset.Now.AddDays(1);
            var badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            badgeUpdater.Update(badge);

        }

        public static async void UpdateTile(object sender, PropertyChangedEventArgs e)
        {
            if (sender is WebConnect c && e.PropertyName == nameof(WebConnect.UpdateTime))
                await UpdateTile(c);
        }

        public static IAsyncAction UpdateTile(WebConnect connect)
        {
            return Task.Run(() =>
            {
                var manager = TileUpdateManager.CreateTileUpdaterForApplication();
                manager.Clear();
                var usage = connect.WebTrafficExact.ToString();
                manager.EnableNotificationQueue(true);
                var devices = connect.DeviceList.ToArray();
                SetBadgeNumber(devices.Length);
                if (devices.Length == 0)
                {
                    addTile(manager, $@"
<tile>
    <visual branding='name'>
        <binding template='TileMedium'>
            <text hint-style='body'>
                <![CDATA[{usage}]]>
            </text>
            <text hint-style='caption' hint-wrap='true'>
                <![CDATA[{LocalizedStrings.Resources.NoDevices}]]>
            </text>
        </binding>
        <binding template='TileWide'>
            <text hint-style='body'>
                <![CDATA[{string.Format(CultureInfo.CurrentCulture, LocalizedStrings.Resources.Usage, usage)}]]>
            </text>
            <text hint-style='caption' hint-wrap='true'>
                <![CDATA[{LocalizedStrings.Resources.NoDevices}]]>
            </text>
        </binding>
    </visual>
</tile>");
                    return;
                }
                foreach (var item in devices)
                {
                    addTile(manager, $@"
<tile>
    <visual branding='name'>
        <binding template='TileMedium'>
            <text hint-style='body'>
                <![CDATA[{usage}]]>
            </text>
            <text hint-style='caption'>
                <![CDATA[{item.Name}]]>
            </text>
            <text hint-style='captionsubtle'>
                <![CDATA[{item.LogOnDateTime.TimeOfDay}]]>
            </text>
            <text hint-style='captionsubtle'>
                <![CDATA[{item.IPAddress}]]>
            </text>
        </binding>
        <binding template='TileWide'>
            <text hint-style='body'>
                <![CDATA[{string.Format(CultureInfo.CurrentCulture, LocalizedStrings.Resources.Usage, usage)}]]>
            </text>
            <text hint-style='caption'>
                <![CDATA[{item.Name}]]>
            </text>
            <text hint-style='captionsubtle'>
                <![CDATA[{item.LogOnDateTime}]]>
            </text>
            <text hint-style='captionsubtle'>
                <![CDATA[{item.IPAddress}]]>
            </text>
        </binding>
    </visual>
</tile>");
                }
            }).AsAsyncAction();
        }

        private static void addTile(TileUpdater manager, string content)
        {
            var tile = new XmlDocument();
            tile.LoadXml(content);
            var tileNotification = new TileNotification(tile);
            tileNotification.ExpirationTime = DateTimeOffset.Now.AddDays(1);
            manager.Update(tileNotification);
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
            if (handler != null)
            {
                if (!handler.IsStatic)
                    throw new ArgumentException("Must be static method", nameof(handler));
                aName = handler.DeclaringType.GetTypeInfo().Assembly.FullName;
                cName = handler.DeclaringType.FullName;
                mName = handler.Name;
            }
            toast.LoadXml($@"
<toast launch='{(handler != null ? $@"a={aName};c={cName};m={mName};p={param}" : "")}'>
    <visual>
        <binding template='ToastGeneric'>
            <text>
                <![CDATA[{title}]]>
            </text>
            <text>
                <![CDATA[{text}]]>
            </text>
        </binding>
    </visual>
</toast>");
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
            if (param == "App")
                return;
            var match = Regex.Match(param ?? "", @"^\s*a=(?<aName>.+?)\s*;\s*c=(?<cName>.+?)\s*;\s*m=(?<mName>.+?)\s*;\s*p=(?<param>.+?)\s*$");
            if (match.Success)
            {
                var aName = new AssemblyName(match.Groups["aName"].Value);
                var a = Assembly.Load(aName);
                var c = a.GetType(match.Groups["cName"].Value);
                var m = c.GetMethod(match.Groups["mName"].Value);
                var p = match.Groups["param"].Value;
                object[] pa = null;
                if (string.IsNullOrEmpty(p))
                    pa = new object[0];
                else
                    pa = new object[] { p };
                m.Invoke(null, pa);
            }
        }
    }
}
