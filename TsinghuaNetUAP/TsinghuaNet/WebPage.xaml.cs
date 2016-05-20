using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace TsinghuaNet
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage
    {
        public WebPage()
        {
            this.InitializeComponent();
            this.Bindings.Initialize();
        }

        private readonly ObservableCollection<WebContent> webViewCollection = new ObservableCollection<WebContent>();

        private WebContent NewWebContent(Uri uri)
        {
            var newView = uri == null ? new WebContent() : new WebContent(uri);
            newView.NewWindowRequested += this.webView_NewWindowRequested;
            return newView;
        }

        private void AddEmptyView()
        {
            var account = Settings.AccountManager.Account;
            account.RetrievePassword();
            var webView = this.NewWebContent(null);
            this.webViewCollection.Add(webView);
            this.listView.SelectedItem = webView;
        }

        private void webView_NewWindowRequested(WebContent sender, WebViewNewWindowRequestedEventArgs args)
        {
            var oldIndex = this.webViewCollection.IndexOf(sender);
            this.webViewCollection.Insert(oldIndex + 1, this.NewWebContent(args.Uri));
            args.Handled = true;
            this.listView.SelectedIndex = oldIndex + 1;
        }

        private readonly CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        private readonly SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();

        protected override Size MeasureOverride(Size availableSize)
        {
            var height = availableSize.Height;
            var width = availableSize.Width;
            if(width >= 1024)
            {
                this.webViewBorder.Height = height - 32;
                this.webViewBorder.Width = width;
            }
            else
            {
                this.webViewBorder.Width = 1024;
                this.webViewBorder.Height = (height - 32) / width * 1024;
            }
            if(width >= 500)
            {
                this.systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                this.coreTitleBar.ExtendViewIntoTitleBar = true;
            }
            else
            {
                this.systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                this.coreTitleBar.ExtendViewIntoTitleBar = false;
            }
            return base.MeasureOverride(availableSize);
        }

        private void CloseViewButton_Click(object sender, RoutedEventArgs e)
        {
            var selectingIndex = this.listView.SelectedIndex;
            var s = (FrameworkElement)sender;
            var view = (WebContent)s.DataContext;
            var closingIndex = this.webViewCollection.IndexOf(view);
            this.webViewCollection.Remove(view);
            if(selectingIndex == closingIndex)
            {
                if(selectingIndex == 0)
                {
                    if(this.webViewCollection.Count != 0)
                        this.listView.SelectedIndex = 0;
                    else
                    {
                        this.Frame.GoBack();
                    }
                }
                else
                    this.listView.SelectedIndex = closingIndex - 1;
            }
        }

        private void NewViewButton_Click(object sender, RoutedEventArgs e)
        {
            this.AddEmptyView();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await StatusBar.GetForCurrentView().HideAsync();
            }

            this.coreTitleBar.LayoutMetricsChanged += this.CoreTitleBar_LayoutMetricsChanged;
            this.systemNavigationManager.BackRequested += this.SystemNavigationManager_BackRequested;

            Window.Current.SetTitleBar(this.titleBar);

            if(this.webViewCollection.Count == 0)
                this.AddEmptyView();

            base.OnNavigatedTo(e);
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            this.Frame.GoBack();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await StatusBar.GetForCurrentView().ShowAsync();
            }

            this.coreTitleBar.LayoutMetricsChanged -= this.CoreTitleBar_LayoutMetricsChanged;
            this.coreTitleBar.ExtendViewIntoTitleBar = false;

            this.systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            this.systemNavigationManager.BackRequested -= this.SystemNavigationManager_BackRequested;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            this.colL.Width = new GridLength(sender.SystemOverlayLeftInset);
            this.colR.Width = new GridLength(sender.SystemOverlayRightInset);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
