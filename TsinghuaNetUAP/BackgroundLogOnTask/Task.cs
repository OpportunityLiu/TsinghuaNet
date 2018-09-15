using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Web;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Web.Http;
using static NotificationService.NotificationService;

namespace BackgroundLogOnTask
{
    public sealed class Task : IBackgroundTask
    {
        #region IBackgroundTask 成员

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //是否自动登陆
            if (!Settings.SettingsHelper.GetLocal("AutoLogOn", true))
                return;
            if (WebConnect.Current is null)
            {  //初始化信息存储区
                PasswordCredential account;
                try
                {
                    var passVault = new PasswordVault();
                    account = passVault.FindAllByResource("TsinghuaAllInOne").First();
                }
                // 未找到储存的密码
                catch (Exception ex) when (ex.HResult == -2147023728)
                {
                    return;
                }
                var connection = NetworkInformation.GetInternetConnectionProfile();
                if (connection == null)
                    return;
                if (connection.IsWwanConnectionProfile)
                    return;
                WebConnect.Current = new WebConnect(account);
            }

            var d = taskInstance.GetDeferral();
            try
            {
                var connect = WebConnect.Current;
                var rst = false;
                try
                {
                    rst = await connect.LogOnAsync();
                }
                catch (LogOnException) { }
                try
                {
                    await connect.RefreshAsync();
                    var tileTask = UpdateTile(connect);
                    var cacheTask = connect.SaveCache();
                    if (rst)
                        SendToastNotification(LocalizedStrings.Resources.LogOnSucessful, string.Format(CultureInfo.CurrentCulture, LocalizedStrings.Resources.Used, connect.WebTrafficExact));
                    await tileTask;
                    await cacheTask;
                }
                catch (LogOnException) { }
            }
            finally
            {
                d.Complete();
            }
        }

        #endregion
    }
}
