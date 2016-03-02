﻿using System;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace TsinghuaNet
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage : Page
    {
        internal static Window webPageWindow;
        private static int webPageViewId;
        
        public static async Task Launch()
        {
            if(webPageWindow == null)
            {
                var view = CoreApplication.CreateNewView();
                ApplicationView appView = null;
                await view.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var page = new WebPage();
                    webPageWindow = Window.Current;
                    webPageWindow.Content = page;
                    webPageWindow.Activate();
                    appView = ApplicationView.GetForCurrentView();
                    appView.TitleBar.BackgroundColor = (Color)page.Resources["SystemChromeMediumLowColor"];
                    appView.TitleBar.ButtonBackgroundColor = (Color)page.Resources["SystemChromeMediumLowColor"];
                    webPageViewId = appView.Id;
                    webPageWindow.Activated += page.WebPageWindow_Activated;
                    appView.Consolidated += async (sender, e) =>
                    {
                        await webPageWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            var p = (WebPage)webPageWindow.Content;
                            webPageWindow.Content = null;
                            webPageWindow.Activated -= p.WebPageWindow_Activated;
                        });
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

        private void WebPageWindow_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if(e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                titleBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                titleBar.Visibility = Visibility.Visible;
            }
        }
        
        public WebPage()
        {
            this.InitializeComponent();
            Bindings.Initialize();
            if(webViewCollection.Count == 0)
                AddEmptyView();
        }

        private ObservableCollection<WebContent> webViewCollection = new ObservableCollection<WebContent>();

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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            Window.Current.SetTitleBar(titleBar);
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            colL.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            colR.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
        }
    }
}
