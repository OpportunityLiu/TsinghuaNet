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
using static NotificationService.NotificationService;

namespace BackgroundLogOnTask
{
    public sealed class Task : IBackgroundTask
    {
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
            var client = new WebConnect(account);
            var d = taskInstance.GetDeferral();
            try
            {
                var rst = false;
                try
                {
                    rst = await client.LogOnAsync();
                }
                catch(LogOnException) { }
                try
                {
                    await client.RefreshAsync();
                    var tileTask = UpdateTile(client);
                    var cacheTask = client.SaveCache();
                    if(rst)
                        SendToastNotification(LocalizedStrings.Resources.LogOnSucessful, string.Format(CultureInfo.CurrentCulture, LocalizedStrings.Resources.Used, client.WebTrafficExact));
                    await tileTask;
                    await cacheTask;
                }
                catch(LogOnException) { }
            }
            finally
            {
                d.Complete();
            }
        }

        #endregion
    }
}
