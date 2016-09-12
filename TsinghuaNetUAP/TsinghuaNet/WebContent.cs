using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.IO;
using Windows.Storage.Streams;
using Windows.Web.Http;
using System.Reflection;
using Windows.Storage.AccessCache;
using Windows.Web.Http.Headers;

namespace TsinghuaNet
{
    class WebContent : Web.ObservableObject
    {
        private static class downloader
        {
            private static ToastNotification failedToast;

            static downloader()
            {
                var fXml = new XmlDocument();
                fXml.LoadXml(LocalizedStrings.Toast.DownloadFailed);
                failedToast = new ToastNotification(fXml);
            }

            private static string toValidFileName(string raw)
            {
                var split = raw.Trim().Trim(Path.GetInvalidFileNameChars()).Split(Path.GetInvalidFileNameChars(), StringSplitOptions.None);
                if(split.Length == 1)
                    return raw;
                return string.Join(".", split);
            }

            public static IAsyncAction Download(Uri fileUri)
            {
                return Run(async token =>
                {
                    var d = new BackgroundDownloader() { FailureToastNotification = failedToast };
                    var file = await DownloadsFolder.CreateFileAsync($"{fileUri.GetHashCode():X}.TsinghuaNet.temp", CreationCollisionOption.GenerateUniqueName);
                    var o = d.CreateDownload(fileUri, file);
                    var op = o.StartAsync();
                    op.Completed = async (sender, e) =>
                    {
                        var resI = o.GetResponseInformation();
                        string name;
                        if(resI.Headers.TryGetValue("Content-Disposition", out name))
                        {
                            var h = HttpContentDispositionHeaderValue.Parse(name);
                            name = h.FileName;
                            if(string.IsNullOrWhiteSpace(name))
                                name = null;
                        }
                        name = name ?? resI.ActualUri.ToString();
                        name = toValidFileName(name);
                        JYAnalyticsUniversal.JYAnalytics.TrackEvent("DownloadFile", name);
                        await file.RenameAsync(name, NameCollisionOption.GenerateUniqueName);
                        var fToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                        NotificationService.NotificationService.SendToastNotification(LocalizedStrings.Toast.DownloadSucceed, name, handler, fToken);
                    };
                });
            }

            private static MethodInfo handler = typeof(downloader).GetMethod(nameof(OpenDownloadedFile));

            public static IAsyncAction OpenDownloadedFile(string fileToken)
            {
                return Run(async token =>
                {
                    try
                    {
                        var file = await StorageApplicationPermissions.MostRecentlyUsedList.GetFileAsync(fileToken);
                        if(StorageApplicationPermissions.MostRecentlyUsedList.CheckAccess(file))
                            await Launcher.LaunchFileAsync(file);
                    }
                    //没找到就算了
                    catch(Exception)
                    {
                    }
                });
            }
        }

        private static Uri getHomepage()
        {
            var account = Settings.AccountManager.Account;
            if(account == null)
                return new Uri("about:blank");
            account.RetrievePassword();
            return new Uri($"ms-appx-web:///WebPages/HomePage.html?id={account.UserName}&pw={account.Password}");
        }

        public WebContent(Uri uri)
        {
            View.DOMContentLoaded += View_DOMContentLoaded;
            View.NewWindowRequested += View_NewWindowRequested;
            View.NavigationStarting += View_NavigationStarting;
            View.NavigationFailed += View_NavigationFailed;
            View.NavigationCompleted += View_NavigationCompleted;
            View.UnviewableContentIdentified += View_UnviewableContentIdentified;
            View.ScriptNotify += View_ScriptNotify;
            View.Navigate(uri);
        }

        private void View_ScriptNotify(object sender, NotifyEventArgs e)
        {

        }

        public WebContent()
            : this(getHomepage())
        {
        }

        private async void View_UnviewableContentIdentified(WebView sender, WebViewUnviewableContentIdentifiedEventArgs args)
        {
            await downloader.Download(args.Uri);
            Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger.GetDefault().Log("File downloaded");
        }

        private void View_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger.GetDefault().Log("Web page viewed");
        }

        private void View_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if(e.Uri == new Uri("https://sslvpn.tsinghua.edu.cn/dana/home/starter0.cgi"))
            {
                View.Navigate(new Uri("https://sslvpn.tsinghua.edu.cn/dana/home/index.cgi"));
            }
            else if(e.Uri == new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/logout.cgi"))
            {
                View.Navigate(getHomepage());
                logged = false;
            }
        }

        private void View_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
        }

        public event TypedEventHandler<WebContent, WebViewNewWindowRequestedEventArgs> NewWindowRequested;

        private void View_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            NewWindowRequested?.Invoke(this, args);
        }

        private bool logged = false;

        private void View_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if(!logged)
            {
                logged = true;
                var account = Settings.AccountManager.Account;
                var id = account.UserName;
                account.RetrievePassword();
                var pass = account.Password;
                account = null;
                if(args.Uri == new Uri("http://its.tsinghua.edu.cn"))
                {
                    var ignore = View.InvokeScriptAsync("eval", new string[]
                    {
                        $@" $.post(
                                'http://its.tsinghua.edu.cn/loginAjax',
                                'username={id}&password={pass}',
                                function (data) {{
                                    if(data.code != 0)
                                        $.common.message('error', data.msg).follow($('#loginbutton')[0]);
                                    else
                                        location.href = 'http://its.tsinghua.edu.cn/';
                                }}, 'json')"
                    });
                }
                else if(args.Uri == new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/url_default/welcome.cgi"))
                {
                    var ignore = View.InvokeScriptAsync("eval", new string[]
                    {
                        $@"username.value = '{Settings.AccountManager.ID}';
                        password.value = '{pass}';
                        frmLogin_4.submit();"
                    });
                }
                else if(args.Uri == new Uri("http://zhjwxk.cic.tsinghua.edu.cn/xklogin.do"))
                {
                    var ignore = View.InvokeScriptAsync("eval", new string[]
                    {
                        $@"j_username.value = '{id}';
                        j_password.value = '{pass}';"
                    });
                }
                else
                {
                    logged = false;
                }
            }
            UpdateTitle();
        }

        public WebView View
        {
            get;
        } = new WebView(WebViewExecutionMode.SeparateThread);

        private string title;

        public string Title
        {
            get
            {
                return title;
            }
        }

        protected void UpdateTitle()
        {
            Set(ref title, View.DocumentTitle, nameof(Title));
            JYAnalyticsUniversal.JYAnalytics.TrackEvent("LoadWebPage", View.DocumentTitle);
        }
    }
}
