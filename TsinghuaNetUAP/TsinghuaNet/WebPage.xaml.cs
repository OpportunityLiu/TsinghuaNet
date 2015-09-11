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
using System.Collections.ObjectModel;

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

        public static async Task Launch()
        {
            if(webPageWindow == null)
            {
                var view = Windows.ApplicationModel.Core.CoreApplication.CreateNewView();
                ApplicationView appView = null;
                await view.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var frame = new Frame();
                    frame.Navigate(typeof(WebPage));
                    webPageWindow = Window.Current;
                    webPageWindow.Content = frame;
                    webPageWindow.Activate();
                    appView = ApplicationView.GetForCurrentView();
                    appView.SetPreferredMinSize(new Size(320, 400));
                    webPageViewId = appView.Id;
                    appView.Consolidated += async (sender, e) =>
                    {
                        await webPageWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            webPageWindow.Content = null;
                        });
                        webPageWindow.Close();
                        webPageWindow = null;
                    };
                });
                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(webPageViewId);
                await view.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    appView.TryResizeView(new Size(1000, 600));
                });
            }
            else
            {
                await ApplicationViewSwitcher.SwitchAsync(webPageViewId);
            }
        }

        public WebPage()
        {
            this.InitializeComponent();
        }

        private ObservableCollection<WebContent> webViewCollection = new ObservableCollection<WebContent>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Bindings.Initialize();
            if(webViewCollection.Count == 0)
                AddEmptyView();
        }

        private WebContent NewWebContent(Uri uri)
        {
            var newView = new WebContent(uri);
            newView.NewWindowRequested += webView_NewWindowRequested;
            return newView;
        }

        private void AddEmptyView()
        {
            var account = Web.WebConnect.Current.Account;
            account.RetrievePassword();
            var webView = NewWebContent(new Uri($"ms-appx-web:///WebPages/HomePage.html?id={account.UserName}&pw={account.Password}"));
            webViewCollection.Add(webView);
            listView.SelectedItem = webView;
        }

        private void webView_NewWindowRequested(WebContent sender, WebViewNewWindowRequestedEventArgs args)
        {
            var oldIndex = webViewCollection.IndexOf(sender);
            webViewCollection.Insert(oldIndex + 1, NewWebContent(args.Uri));
            args.Handled = true;
            listView.SelectedIndex = oldIndex + 1;
        }

        private void root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = e.NewSize.Width;
            var height = e.NewSize.Height;
            if(width > 1024)
            {
                webViewBorder.Height = height - 32;
                webViewBorder.Width = width;
            }
            else
            {
                webViewBorder.Width = 1024;
                webViewBorder.Height = (height - 32) / width * 1024;
            }
        }

        private void CloseViewButton_Click(object sender, RoutedEventArgs e)
        {
            var selectingIndex = listView.SelectedIndex;
            var s = (FrameworkElement)sender;
            var view = (WebContent)s.DataContext;
            var closingIndex = webViewCollection.IndexOf(view);
            webViewCollection.Remove(view);
            if(selectingIndex == closingIndex)
            {
                if(selectingIndex == 0)
                {
                    if(webViewCollection.Count != 0)
                        listView.SelectedIndex = 0;
                    else
                        AddEmptyView();
                }
                else
                    listView.SelectedIndex = closingIndex - 1;
            }
        }

        private void NewViewButton_Click(object sender, RoutedEventArgs e)
        {
            AddEmptyView();
        }
    }
}
