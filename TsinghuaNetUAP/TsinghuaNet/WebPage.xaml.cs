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
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace TsinghuaNet
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage : Page
    {
        public WebPage()
        {
            this.InitializeComponent();
            Bindings.Initialize();
        }

        private ObservableCollection<WebContent> webViewCollection = new ObservableCollection<WebContent>();

        private WebContent NewWebContent(Uri uri)
        {
            var newView = uri == null ? new WebContent() : new WebContent(uri);
            newView.NewWindowRequested += webView_NewWindowRequested;
            return newView;
        }

        private void AddEmptyView()
        {
            var account = Settings.AccountManager.Account;
            account.RetrievePassword();
            var webView = NewWebContent(null);
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
            if(width >= 1024)
            {
                webViewBorder.Height = height - 32;
                webViewBorder.Width = width;
            }
            else
            {
                webViewBorder.Width = 1024;
                webViewBorder.Height = (height - 32) / width * 1024;
            }
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            if(width >= 500)
            {
                coreTitleBar.ExtendViewIntoTitleBar = true;
            }
            else
            {
                coreTitleBar.ExtendViewIntoTitleBar = false;
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
                    {
                        Frame.GoBack();
                    }
                }
                else
                    listView.SelectedIndex = closingIndex - 1;
            }
        }

        private void NewViewButton_Click(object sender, RoutedEventArgs e)
        {
            AddEmptyView();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await StatusBar.GetForCurrentView().HideAsync();
            }

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;

            Window.Current.SetTitleBar(titleBar);

            if(webViewCollection.Count == 0)
                AddEmptyView();
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            Frame.GoBack();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await StatusBar.GetForCurrentView().ShowAsync();
            }

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged -= CoreTitleBar_LayoutMetricsChanged;

            coreTitleBar.ExtendViewIntoTitleBar = false;

            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            systemNavigationManager.BackRequested -= SystemNavigationManager_BackRequested;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
                colL.Width = new GridLength(sender.SystemOverlayLeftInset);
                colR.Width = new GridLength(sender.SystemOverlayRightInset);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
