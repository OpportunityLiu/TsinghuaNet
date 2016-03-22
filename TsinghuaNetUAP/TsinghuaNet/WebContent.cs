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

namespace TsinghuaNet
{
    class WebContent : INotifyPropertyChanged
    {
        private static class downloader
        {
            private static ToastNotification succeedToast, failedToast;

            static downloader()
            {
                var sXml = new XmlDocument();
                sXml.LoadXml(LocalizedStrings.Toast.DownloadSucceed);
                succeedToast = new ToastNotification(sXml);
                var fXml = new XmlDocument();
                fXml.LoadXml(LocalizedStrings.Toast.DownloadFailed);
                failedToast = new ToastNotification(fXml);
            }

            private static string toValidFileName(string raw)
            {
                var split = raw.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.None);
                if(split.Length == 1)
                    return raw;
                return string.Join(".", split);
            }

            public static IAsyncAction Download(Uri fileUri)
            {
                return Run(async token =>
                {
                    var d = new BackgroundDownloader()
                    {
                        SuccessToastNotification = succeedToast,
                        FailureToastNotification = failedToast
                    };
                    var file = await DownloadsFolder.CreateFileAsync($"{fileUri.GetHashCode():X}.TsinghuaNet.temp");
                    var o = d.CreateDownload(fileUri, file);
                    var op = o.StartAsync();
                    op.Completed = async (sender, e) =>
                    {
                        var resI = o.GetResponseInformation();
                        string name;
                        if(resI == null)
                        {
                            name = file.Name;
                        }
                        else if(resI.Headers.TryGetValue("Content-Disposition", out name))
                        {
                            var filename = Regex.Match(name, @"filename\s?=\s?""(.+)""");
                            if(filename.Success)
                            {
                                name = filename.Groups[1].Value;
                            }
                        }
                        name = name ?? resI.ActualUri.ToString();
                        await file.RenameAsync(toValidFileName(name));
                    };
                });
            }
        }

        public WebContent(Uri uri)
        {
            View.DOMContentLoaded += View_DOMContentLoaded;
            View.NewWindowRequested += View_NewWindowRequested;
            View.NavigationStarting += View_NavigationStarting;
            View.NavigationFailed += View_NavigationFailed;
            View.NavigationCompleted += View_NavigationCompleted;
            View.UnviewableContentIdentified += View_UnviewableContentIdentified;
            View.Navigate(uri);
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
            switch(e.WebErrorStatus)
            {
            case Windows.Web.WebErrorStatus.Unknown:
                break;
            case Windows.Web.WebErrorStatus.CertificateCommonNameIsIncorrect:
                break;
            case Windows.Web.WebErrorStatus.CertificateExpired:
                break;
            case Windows.Web.WebErrorStatus.CertificateContainsErrors:
                break;
            case Windows.Web.WebErrorStatus.CertificateRevoked:
                break;
            case Windows.Web.WebErrorStatus.CertificateIsInvalid:
                break;
            case Windows.Web.WebErrorStatus.ServerUnreachable:
                break;
            case Windows.Web.WebErrorStatus.Timeout:
                break;
            case Windows.Web.WebErrorStatus.ErrorHttpInvalidServerResponse:
                break;
            case Windows.Web.WebErrorStatus.ConnectionAborted:
                break;
            case Windows.Web.WebErrorStatus.ConnectionReset:
                break;
            case Windows.Web.WebErrorStatus.Disconnected:
                break;
            case Windows.Web.WebErrorStatus.HttpToHttpsOnRedirection:
                break;
            case Windows.Web.WebErrorStatus.HttpsToHttpOnRedirection:
                break;
            case Windows.Web.WebErrorStatus.CannotConnect:
                break;
            case Windows.Web.WebErrorStatus.HostNameNotResolved:
                break;
            case Windows.Web.WebErrorStatus.OperationCanceled:
                break;
            case Windows.Web.WebErrorStatus.RedirectFailed:
                break;
            case Windows.Web.WebErrorStatus.UnexpectedStatusCode:
                break;
            case Windows.Web.WebErrorStatus.UnexpectedRedirection:
                break;
            case Windows.Web.WebErrorStatus.UnexpectedClientError:
                break;
            case Windows.Web.WebErrorStatus.UnexpectedServerError:
                break;
            case Windows.Web.WebErrorStatus.MultipleChoices:
                break;
            case Windows.Web.WebErrorStatus.MovedPermanently:
                break;
            case Windows.Web.WebErrorStatus.Found:
                break;
            case Windows.Web.WebErrorStatus.SeeOther:
                break;
            case Windows.Web.WebErrorStatus.NotModified:
                break;
            case Windows.Web.WebErrorStatus.UseProxy:
                break;
            case Windows.Web.WebErrorStatus.TemporaryRedirect:
                break;
            case Windows.Web.WebErrorStatus.BadRequest:
                break;
            case Windows.Web.WebErrorStatus.Unauthorized:
                break;
            case Windows.Web.WebErrorStatus.PaymentRequired:
                break;
            case Windows.Web.WebErrorStatus.Forbidden:
                break;
            case Windows.Web.WebErrorStatus.NotFound:
                break;
            case Windows.Web.WebErrorStatus.MethodNotAllowed:
                break;
            case Windows.Web.WebErrorStatus.NotAcceptable:
                break;
            case Windows.Web.WebErrorStatus.ProxyAuthenticationRequired:
                break;
            case Windows.Web.WebErrorStatus.RequestTimeout:
                break;
            case Windows.Web.WebErrorStatus.Conflict:
                break;
            case Windows.Web.WebErrorStatus.Gone:
                break;
            case Windows.Web.WebErrorStatus.LengthRequired:
                break;
            case Windows.Web.WebErrorStatus.PreconditionFailed:
                break;
            case Windows.Web.WebErrorStatus.RequestEntityTooLarge:
                break;
            case Windows.Web.WebErrorStatus.RequestUriTooLong:
                break;
            case Windows.Web.WebErrorStatus.UnsupportedMediaType:
                break;
            case Windows.Web.WebErrorStatus.RequestedRangeNotSatisfiable:
                break;
            case Windows.Web.WebErrorStatus.ExpectationFailed:
                break;
            case Windows.Web.WebErrorStatus.InternalServerError:
                break;
            case Windows.Web.WebErrorStatus.NotImplemented:
                break;
            case Windows.Web.WebErrorStatus.BadGateway:
                break;
            case Windows.Web.WebErrorStatus.ServiceUnavailable:
                break;
            case Windows.Web.WebErrorStatus.GatewayTimeout:
                break;
            case Windows.Web.WebErrorStatus.HttpVersionNotSupported:
                break;
            default:
                break;
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
                var account = Settings.AccountManager.Account;
                var id = account.UserName;
                account.RetrievePassword();
                var pass = account.Password;
                account = null;
                if(args.Uri == new Uri("http://its.tsinghua.edu.cn"))
                {
                    logged = true;
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
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<T>(ref T field,T newValue, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field,newValue))
                return;
            field = newValue;
            RaisePropertyChanged(propertyName);
        }
    }
}
