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
            View.Navigate(uri);
            View.DOMContentLoaded += View_DOMContentLoaded;
            View.NewWindowRequested += View_NewWindowRequested;
            View.NavigationStarting += View_NavigationStarting;
            View.NavigationFailed += View_NavigationFailed;
            View.NavigationCompleted += View_NavigationCompleted;
            View.UnviewableContentIdentified += View_UnviewableContentIdentified;
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

        }

        private void View_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {

        }

        public event TypedEventHandler<WebContent, WebViewNewWindowRequestedEventArgs> NewWindowRequested;

        private void View_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            NewWindowRequested?.Invoke(this, args);
        }

        private void View_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
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
