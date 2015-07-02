using System;
using System.Threading.Tasks;
using TsinghuaNet.Web;
using Windows.ApplicationModel.Resources;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TsinghuaNet
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static readonly string logOnFailed = ResourceLoader.GetForViewIndependentUse().GetString("ToastFailed");

        public MainPage()
        {
            var resource = ResourceLoader.GetForCurrentView();
            this.InitializeComponent();
            this.dropDialog = new DropDialog();
            this.renameDialog = new RenameDialog();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.New)
            {
                if(WebConnect.Current == null)
                {
                    await new SignInDialog().ShowAsync();
                    this.DataContext = WebConnect.Current;
                }
                else
                {
                    this.DataContext = WebConnect.Current;
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

        DropDialog dropDialog;

        RenameDialog renameDialog;

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
                await refresh();
            }
        }

        private async Task refresh()
        {
            try
            {
                progressBarUsage.IsIndeterminate = true;
                await WebConnect.Current.RefreshAsync();
            }
            catch(LogOnException)
            {
            }
        }

        private void appBarButtonAbout_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void changeUser_Click(object sender, RoutedEventArgs e)
        {
            var signIn = new SignInDialog();
            var t = signIn.ShowAsync();
            signIn.Closed += (s, args) => this.DataContext = WebConnect.Current;
            await t;
        }

        private async void refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await WebConnect.Current.LogOnAsync();
            }
            catch(LogOnException ex)
            {
                App.Current.SendToastNotification(logOnFailed, ex.Message);
            }
            await refresh();
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
    }
}
