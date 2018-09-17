using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace TsinghuaNet
{
    internal class WebContent : ObservableObject
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
                if (split.Length == 1)
                    return raw;
                return string.Join(".", split);
            }

            public static IAsyncAction Download(Uri fileUri)
            {
                return Run(async token =>
                {
                    var d = new BackgroundDownloader { FailureToastNotification = failedToast };
                    var file = await DownloadsFolder.CreateFileAsync($"{fileUri.GetHashCode():X}.TsinghuaNet.temp", CreationCollisionOption.GenerateUniqueName);
                    var downloadOperation = d.CreateDownload(fileUri, file);
                    downloadOperation.StartAsync().Completed = async (sender, e) =>
                    {
                        string name = null;
                        var resI = downloadOperation.GetResponseInformation();
                        if (resI != null)
                        {
                            if (resI.Headers.TryGetValue("Content-Disposition", out name))
                            {
                                var h = HttpContentDispositionHeaderValue.Parse(name);
                                name = h.FileName;
                                if (string.IsNullOrWhiteSpace(name))
                                    name = null;
                            }
                            name = name ?? getFileNameFromUri(resI.ActualUri);
                        }
                        name = name ?? getFileNameFromUri(fileUri);
                        name = toValidFileName(name);
                        await file.RenameAsync(name, NameCollisionOption.GenerateUniqueName);
                        var fToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                        NotificationService.NotificationService.SendToastNotification(LocalizedStrings.Toast.DownloadSucceed, name, handler, fToken);
                    };
                });
            }

            private static string getFileNameFromUri(Uri uri)
            {
                if (uri is null)
                    return null;
                return uri.LocalPath.Split(@"/\?".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            }

            private static readonly MethodInfo handler = typeof(downloader).GetMethod(nameof(OpenDownloadedFile));

            public static IAsyncAction OpenDownloadedFile(string fileToken)
            {
                return Run(async token =>
                {
                    try
                    {
                        var file = await StorageApplicationPermissions.MostRecentlyUsedList.GetFileAsync(fileToken);
                        if (StorageApplicationPermissions.MostRecentlyUsedList.CheckAccess(file))
                            await Launcher.LaunchFileAsync(file);
                    }
                    //没找到就算了
                    catch (Exception)
                    {
                    }
                });
            }
        }

        private static Uri getHomepage()
        {
            var account = Settings.AccountManager.Account;
            if (account == null)
                return new Uri("about:blank");
            account.RetrievePassword();
            return new Uri($"ms-appx-web:///WebPages/HomePage.html?id={account.UserName}&pw={account.Password}");
        }

        public WebContent(Uri uri)
        {
            this.View.DOMContentLoaded += this.View_DOMContentLoaded;
            this.View.NewWindowRequested += this.View_NewWindowRequested;
            this.View.NavigationStarting += this.View_NavigationStarting;
            this.View.NavigationFailed += this.View_NavigationFailed;
            this.View.NavigationCompleted += this.View_NavigationCompleted;
            this.View.UnviewableContentIdentified += this.View_UnviewableContentIdentified;
            this.View.ScriptNotify += this.View_ScriptNotify;
            this.View.Navigate(uri);
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
        }

        private void View_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
        }

        private void View_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (e.Uri == new Uri("https://sslvpn.tsinghua.edu.cn/dana/home/starter0.cgi"))
            {
                this.View.Navigate(new Uri("https://sslvpn.tsinghua.edu.cn/dana/home/index.cgi"));
            }
            else if (e.Uri == new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/logout.cgi"))
            {
                this.View.Navigate(getHomepage());
                this.logged = false;
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
            if (!this.logged)
            {
                this.logged = true;
                var account = Settings.AccountManager.Account;
                var id = account.UserName;
                account.RetrievePassword();
                var pass = account.Password;
                account = null;
                if (args.Uri == new Uri("http://its.tsinghua.edu.cn"))
                {
                    var ignore = this.View.InvokeScriptAsync("eval", new string[]
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
                else if (args.Uri == new Uri("https://sslvpn.tsinghua.edu.cn/dana-na/auth/url_default/welcome.cgi"))
                {
                    var ignore = this.View.InvokeScriptAsync("eval", new string[]
                    {
                        $@"username.value = '{Settings.AccountManager.ID}';
                        password.value = '{pass}';
                        frmLogin_4.submit();"
                    });
                }
                else if (args.Uri == new Uri("http://zhjwxk.cic.tsinghua.edu.cn/xklogin.do"))
                {
                    var ignore = this.View.InvokeScriptAsync("eval", new string[]
                    {
                        $@"j_username.value = '{id}';
                        j_password.value = '{pass}';"
                    });
                }
                else
                {
                    this.logged = false;
                }
            }
            this.UpdateTitle();
        }

        public WebView View
        {
            get;
        } = new WebView(WebViewExecutionMode.SeparateThread);

        private string title;

        public string Title => this.title;

        protected void UpdateTitle()
        {
            Set(ref this.title, this.View.DocumentTitle, nameof(this.Title));
        }
    }
}
