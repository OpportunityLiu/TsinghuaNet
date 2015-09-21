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

namespace TsinghuaNet
{
    class WebContent : INotifyPropertyChanged
    {
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
            var toast = new Windows.Data.Xml.Dom.XmlDocument();
            toast.LoadXml($@"
<toast>
    <visual>
        <binding template='ToastGeneric'>
            <text>下载完成</text>
            <text>已经下载</text>
        </binding>
    </visual>
</toast>");
            var d = new BackgroundDownloader();
            d.SuccessToastNotification = new Windows.UI.Notifications.ToastNotification(toast);
            var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("temp", CreationCollisionOption.GenerateUniqueName);
            var o = d.CreateDownload(args.Uri, file);
            await o.StartAsync();
            var resI = o.GetResponseInformation();
            var h = resI.Headers;
            string name = null;
            if(h.TryGetValue("Content-Disposition", out name))
            {
                var filename = Regex.Match(name, @"filename\s?=\s?""(.+)""");
                if(filename.Success)
                {
                    await file.RenameAsync(filename.Groups[1].Value, NameCollisionOption.GenerateUniqueName);
                    return;
                }
            }
            await file.RenameAsync(resI.ActualUri.ToString(), NameCollisionOption.GenerateUniqueName);
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
