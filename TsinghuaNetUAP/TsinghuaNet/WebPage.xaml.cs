using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace TsinghuaNet
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage : Page
    {
        private static Window webPageWindow;
        private static int webPageViewId;

        public static async Task Launch(Uri uri)
        {
            if(webPageWindow == null)
            {
                var view = Windows.ApplicationModel.Core.CoreApplication.CreateNewView();
                ApplicationView appView = null;
                await view.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var frame = new Frame();
                    frame.Navigate(typeof(WebPage), uri);
                    webPageWindow = Window.Current;
                    webPageWindow.Content = frame;
                    webPageWindow.Activate();
                    appView = ApplicationView.GetForCurrentView();
                    appView.SetPreferredMinSize(new Size(320, 400));
                    webPageViewId = appView.Id;
                });
                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(webPageViewId);
                await view.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    appView.TryResizeView(new Size(1000, 600));
                });
            }
            else
            {
                await webPageWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ((Frame)webPageWindow.Content).Navigate(typeof(WebPage), uri);
                    webPageWindow.Activate();
                });
                await ApplicationViewSwitcher.SwitchAsync(webPageViewId);
            }
        }

        public WebPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(webView == null)
            {
                webView = new WebView();
                webViewPlaceholder.Child = webView;
            }
            var uri = e.Parameter as Uri;
            if(uri != null)
            {
                webView.Navigate(uri);
            }
        }

        WebView webView;

        private void root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = e.NewSize.Width;
            var height = e.NewSize.Height;
            this.stackPanelLable.Width = width;
            this.webViewPlaceholder.Width = width;
            this.webViewPlaceholder.Height = height - 40;
            if(width > 1024)
            {
                this.webView.Height = height - 40;
                this.webView.Width = width;
            }
            else
            {
                webView.Width = 1024;
                this.webView.Height = (height - 40) / width * 1024;
            }
        }
    }
}
