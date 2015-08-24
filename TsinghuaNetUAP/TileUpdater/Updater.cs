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

namespace TileUpdater
{
    public static class Updater
    {
        private static readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("TileUpdater/Resources");

        private static readonly string noDevices = loader.GetString("NoDevices");
        private static readonly string usageFormat = loader.GetString("Usage");

        public static async Task UpdateTile(WebConnect connect)
        {
            await Task.Run(() =>
            {
                var manager = TileUpdateManager.CreateTileUpdaterForApplication();
                if(connect == null)
                    manager.Clear();
                var usage = connect.WebTrafficExact.ToString();
                manager.EnableNotificationQueue(true);
                var devices = connect.DeviceList.ToArray();
                if(devices.Length == 0)
                {
                    XmlDocument tile = new XmlDocument();
                    tile.LoadXml($@"
<tile>
    <visual branding='name'>
        <binding template='TileMedium'>
            <text hint-style='body'>{usage}</text>
            <text hint-style='caption' hint-wrap='true'>{noDevices}</text>
        </binding>
        <binding template='TileWide'>
            <text hint-style='body'>{string.Format(CultureInfo.CurrentCulture, usageFormat, usage)}</text>
            <text hint-style='caption' hint-wrap='true'>{noDevices}</text>
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
                    XmlDocument tile = new XmlDocument();
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
            <text hint-style='body'>{string.Format(CultureInfo.CurrentCulture, usageFormat, usage)}</text>
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
            });

        }

    }
}
