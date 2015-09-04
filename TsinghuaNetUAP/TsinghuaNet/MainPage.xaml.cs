using System;
using System.Threading.Tasks;
using Web;
using Windows.ApplicationModel.Resources;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.ApplicationModel;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.ViewManagement;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var version = Package.Current.Id.Version;
            textBlockVersion.Text = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.AppVersionFormat, version.Major, version.Minor, version.Build, version.Revision);
            App.Current.Resuming += (sender, e) => refresh();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.New)
            {
                if(WebConnect.Current == null)
                {
                    this.appBarButtonChangeUser.Visibility = Visibility.Collapsed;
                    changeUser_Click(this, new RoutedEventArgs());
                }
                else
                {
                    this.DataContext = WebConnect.Current;
                    refresh();
                }
            }
        }

        private WebDevice selectedDevice;

        private async void drop_Confirmed(IUICommand sender)
        {
            await selectedDevice.DropAsync();
            try
            {
                await WebConnect.Current.RefreshAsync();
            }
            catch(LogOnException)
            {
            }
        }

        DropDialog dropDialog=new DropDialog();

        RenameDialog renameDialog = new RenameDialog();

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            renameDialog.NewName = selectedDevice.Name;
            if(await renameDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                selectedDevice.Name = renameDialog.NewName;
            }
        }

        private async void Drop_Click(object sender, RoutedEventArgs e)
        {
            dropDialog.Title = selectedDevice.Name;
            if(await dropDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await selectedDevice.DropAsync();
                refresh();
            }
        }

        private IAsyncAction currentAction;

        public void refresh()
        {
            if((this.DataContext = WebConnect.Current) == null)
                return;
            if(currentAction?.Status == AsyncStatus.Started)
                currentAction.Cancel();
            currentAction = Run(async token =>
            {
                //防止进度条闪烁
                var progressTokens = new System.Threading.CancellationTokenSource();
                var finished = false;
                var progress = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    if(!finished)
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => progressBarUsage.IsIndeterminate = true);
                });
                IAsyncAction action = null;
                token.Register(() => action?.Cancel());
                try
                {
                    await WebConnect.Current.LogOnAsync();
                }
                catch(LogOnException)
                {
                }
                try
                {
                    await WebConnect.Current.RefreshAsync();
                }
                catch(LogOnException)
                {
                }
                finished = true;
                progressBarUsage.IsIndeterminate = false;
                progressBarUsage.Value = WebConnect.Current.WebTrafficExact.TotalGB;
            });
        }

        private async void changeUser_Click(object sender, RoutedEventArgs e)
        {
            var signIn = new SignInDialog();
            signIn.Closed += (s, args) =>
            {
                if(WebConnect.Current != null)
                    this.appBarButtonChangeUser.Visibility = Visibility.Visible;
                refresh();
            };
            await signIn.ShowAsync();
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            refresh();
        }

        private void StackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if(e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                return;
            var s = (FrameworkElement)sender;
            selectedDevice = (WebDevice)s.DataContext;
            var p = e.GetPosition(s);
            p.Y = s.ActualHeight;
            ((MenuFlyout)FlyoutBase.GetAttachedFlyout(s)).ShowAt(s, p);
        }

        private void StackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            switch(e.HoldingState)
            {
            case HoldingState.Canceled:
                FlyoutBase.GetAttachedFlyout((FrameworkElement)sender).Hide();
                break;
            case HoldingState.Started:
                var s = (FrameworkElement)sender;
                selectedDevice = (WebDevice)s.DataContext;
                var p = e.GetPosition(s);
                p.Y = s.ActualHeight;
                ((MenuFlyout)FlyoutBase.GetAttachedFlyout(s)).ShowAt(s,p);
                break;
            }
        }

        private void Page_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if(args.NewValue == null)
                return;
            var dList = ((WebConnect)args.NewValue).DeviceList;
            ((INotifyPropertyChanged)dList).PropertyChanged += deviceListChanged;
            deviceListChanged(dList, null);
        }

        private void deviceListChanged(object sender, PropertyChangedEventArgs e)
        {
            textBlockNoDevices.Visibility = ((IList<WebDevice>)sender).Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9NBLGGGZ5Q4J"));
        }

        private async void l_webLearning_Click(object sender, RoutedEventArgs e)
        {
            await SiteLauncher.LanunchWebLearning(WebConnect.Current.Account);
        }

        private async void l_webLearningNew_Click(object sender, RoutedEventArgs e)
        {
            await SiteLauncher.LanunchNewWebLearning(WebConnect.Current.Account);
        }

        private async void l_info_Click(object sender, RoutedEventArgs e)
        {
            await SiteLauncher.LanunchInfo(WebConnect.Current.Account);
        }
    }
}
