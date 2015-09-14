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

namespace NotificationService
{
    public static class NotificationService
    {
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
            <text hint-style='caption' hint-wrap='true'>{Strings.Resources.NoDevices}</text>
        </binding>
        <binding template='TileWide'>
            <text hint-style='body'>{string.Format(CultureInfo.CurrentCulture, Strings.Resources.Usage, usage)}</text>
            <text hint-style='caption' hint-wrap='true'>{Strings.Resources.NoDevices}</text>
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
            <text hint-style='body'>{string.Format(CultureInfo.CurrentCulture, Strings.Resources.Usage, usage)}</text>
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
        public static void SendToastNotification(string title, string text)
        {
            var toast = new XmlDocument();
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
